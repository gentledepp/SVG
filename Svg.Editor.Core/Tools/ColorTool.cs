using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Editor.Utils;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
	public interface IColorInputService
	{
		Task<string> GetHexaColorFromUserInput(string title);
	}

	public interface ISupportTextColor
	{
		string GetDefaultTextColorIndex(string color);
	}

	/// <summary>
	/// Implemented by a tool that owns a composite element (e.g. a group whose visible shape is a
	/// child) to tell the <see cref="ColorTool"/> which element(s) actually carry the color.
	/// This keeps the <see cref="ColorTool"/> closed for modification: it never needs to know about
	/// any concrete shape - a new shape's tool just implements this and returns its color targets.
	/// Return <c>null</c> for elements this tool does not own.
	/// </summary>
	public interface IColorTargetProvider
	{
		IEnumerable<SvgElement> GetColorTargets(SvgElement element);
	}

	public class ColorTool : UndoableToolBase
	{
		#region Private fields and properties

		private static IColorInputService ColorInputService => SvgEngine.Resolve<IColorInputService>();

		private static ISvgCachingService SvgCachingService => SvgEngine.TryResolve<ISvgCachingService>();

		private static readonly Lazy<IToolbarIconSizeProvider> Tbi =
			new Lazy<IToolbarIconSizeProvider>(SvgEngine.TryResolve<IToolbarIconSizeProvider>);

		private SizeF _iconDimensions;

		#endregion

		#region Public properties

		public const string IconDimensionsKey = "icondimensions";
		public const string HexColorKey = "hexcolor";


        public string HexColor
        {
            get
            {
                object index;
                return Properties.TryGetValue(HexColorKey, out index)
                    ? Convert.ToString(index)
                    : "#000000";
            }
            set
            {
				if(!string.IsNullOrEmpty(value))
                    Properties[HexColorKey] = value;
            }
        }


        #endregion

		public ColorTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Color", properties,
			undoRedoService)
		{
			IconName = "ic_format_color_fill";
			ToolType = ToolType.Modify;
		}

		#region Overrides

		public override async Task Initialize(ISvgDrawingCanvas ws)
		{
			await base.Initialize(ws);

			object iconDimensions;
			if (Properties.TryGetValue(IconDimensionsKey, out iconDimensions))
			{
				_iconDimensions = iconDimensions as SizeF;
			}

			// add tool commands
			Commands = new List<IToolCommand>
			{
				new ChangeColorCommand(ws, this, "Change color", description: LocalizationService.GetString("Svg.Editor.ColorTool.ChangeColor.Description")),
				new ChangeTextColorCommand(ws, this, "Change text color", _ => Canvas.ActiveTool is ISupportTextColor, description: LocalizationService.GetString("Svg.Editor.ColorTool.ChangeTextColor.Description")),
			};

			// initialize with callbacks
			WatchDocument(ws.Document);
		}

		public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
		{
			// add watch for global colorizing
			WatchDocument(newDocument);
			UnWatchDocument(oldDocument);
		}

		#endregion

		#region Private helpers

		private static string StringifyColor(Color color)
		{
			return $"{color.R}_{color.G}_{color.B}";
		}

		/// <summary>
		/// Resolves which element(s) should actually receive the color for a selected element.
		/// Tools that own composite shapes can redirect coloring to a child (e.g. a pin's shape)
		/// by implementing <see cref="IColorTargetProvider"/>; otherwise the element itself is used.
		/// </summary>
		private IEnumerable<SvgElement> ResolveColorTargets(SvgElement element)
		{
			var targets = Canvas.Tools
				.OfType<IColorTargetProvider>()
				.Select(p => p.GetColorTargets(element))
				.FirstOrDefault(t => t != null);

			return targets ?? new[] { element };
		}

		private void ColorizeElement(SvgElement element, string hxColor)
		{
			var noFill = element.Fill == null ||element.Fill == SvgPaintServer.None || element.Fill == SvgColourServer.NotSet || element.HasConstraints(NoFillConstraint);
			var noStroke = element.Stroke == null || element.Stroke == SvgPaintServer.None || element.Stroke == SvgColourServer.NotSet || element.HasConstraints(NoStrokeConstraint);

			// only colorize visual elements
			if (!(element is SvgVisualElement) || noFill && noStroke) return;

			var oldStroke = ((SvgColourServer) element.Stroke)?.ToString();
			var oldFill = ((SvgColourServer) element.Fill)?.ToString();
			UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize element", _ =>
			{
				if (!noStroke)
				{
					element.Stroke?.Dispose();
					element.Stroke = new SvgColourServer(Color.Create(hxColor));
				}
				if (!noFill)
				{
					element.Fill?.Dispose();
					element.Fill = new SvgColourServer(Color.Create(hxColor));
				}
				Canvas.FireInvalidateCanvas();
			}, _ =>
			{
				if (!noStroke)
				{
					element.Stroke?.Dispose();
					element.SvgElementFactory.SetPropertyValue(element, "stroke", oldStroke, element.OwnerDocument);
				}
				if (!noFill)
				{
					element.Fill?.Dispose();
					element.SvgElementFactory.SetPropertyValue(element, "fill", oldFill, element.OwnerDocument);
				}
				Canvas.FireInvalidateCanvas();
			}), hasOwnUndoRedoScope: false);
		}

		/// <summary>
		/// Subscribes to the documentss "Add/RemoveChild" handlers.
		/// </summary>
		/// <param name="document"></param>
		private void WatchDocument(SvgDocument document)
		{
			if (document == null)
				return;

			document.ChildAdded -= OnChildAdded;
			document.ChildAdded += OnChildAdded;
		}

		private void UnWatchDocument(SvgDocument document)
		{
			if (document == null)
				return;

			document.ChildAdded -= OnChildAdded;
		}

		private void OnChildAdded(object sender, ChildAddedEventArgs e)
		{
			if (Canvas?.ActiveTool is ISupportTextColor)
			{
				ColorizeElement(e.NewChild.Children[0], HexColor);
				ColorizeElement(e.NewChild.Children[1], HexColor);
			}
			else
			{
				ColorizeElement(e.NewChild, HexColor);
			}
		}

		#endregion

		#region Inner types

		/// <summary>
		/// This command changes the color of selected items, or the global selected color, if no items are selected.
		/// </summary>
		private class ChangeColorCommand : ToolCommand
		{
			private readonly ISvgDrawingCanvas _canvas;

			public ChangeColorCommand(ISvgDrawingCanvas canvas, ColorTool tool, string name, string description = null)
				: base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500, description: description)
			{
				_canvas = canvas;
			}

			private new ColorTool Tool => (ColorTool) base.Tool;

			public override async void Execute(object parameter)
			{
				var t = Tool;

                string hxColor;

				try
				{
					 hxColor =
						await ColorInputService.GetHexaColorFromUserInput("Choose color");
				}
				catch (TaskCanceledException)
				{
					return;
				}

				if (_canvas.SelectedElements.Any())
				{
					t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize selected elements", o => { }));
					// change the color of all selected items. Which element actually carries the
					// color (the element itself, or a child for composite shapes) is resolved via
					// IColorTargetProvider, so the ColorTool needs no knowledge of concrete shapes.
					foreach (var selectedElement in _canvas.SelectedElements)
					{
						foreach (var target in t.ResolveColorTargets(selectedElement))
						{
							t.ColorizeElement(target, hxColor);
						}
					}
					// don't change the global color when items are selected
					return;
				}

				t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
				{
					t.HexColor = hxColor;
					t.Canvas.FireToolCommandsChanged();
				}, o =>
				{
                    t.HexColor = hxColor;
					t.Canvas.FireToolCommandsChanged();
				}));
			}
		}

		private class ChangeTextColorCommand : ToolCommand
		{
			private readonly ISvgDrawingCanvas _canvas;

			public ChangeTextColorCommand(ISvgDrawingCanvas canvas, ColorTool tool, string name, Func<object, bool> canExecute = null, string description = null)
				: base(tool, name, o => { }, canExecute, iconName: tool.IconName, sortFunc: tc => 500, description:description)
			{
				_canvas = canvas;
			}

			private new ColorTool Tool => (ColorTool)base.Tool;

			public override async void Execute(object parameter)
			{
				var t = Tool;

				string hxColor;

				try
				{
                    hxColor =
						await ColorInputService.GetHexaColorFromUserInput("Choose color");
				}
				catch (TaskCanceledException)
				{
					return;
				}

				if (_canvas.SelectedElements.Any())
				{
					t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize selected element's texts", o => { }));
					// change the color of all selected items
					foreach (var selectedElement in _canvas.SelectedElements)
					{
						t.ColorizeElement(selectedElement.Children[1], hxColor);
					}
					// don't change the global color when items are selected
					return;
				}

                var formerHxColor = t.HexColor;
				t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
                {
                    t.HexColor = hxColor;
                    t.Canvas.FireToolCommandsChanged();
				}, o =>
				{
                    t.HexColor = formerHxColor;
					t.Canvas.FireToolCommandsChanged();
				}));
			}

		}

		#endregion
	}
}