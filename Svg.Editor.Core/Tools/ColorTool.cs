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
		Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors, int defaultIndex = 0);
	}

	public interface ISupportTextColor
	{
		int GetDefaultTextColorIndex(int parentColor, string[] selectableColors);
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

		public const string SelectedColorIndexKey = "selectedcolorindex";
		public const string SelectedTextColorIndexKey = "selectedtextcolorindex";
		public const string SelectableColorsKey = "selectablecolors";
		public const string SelectableColorNamesKey = "selectablecolornames";
		public const string IconDimensionsKey = "icondimensions";

		public string[] SelectableColors
		{
			get
			{
				object selectableColors;
				if (!Properties.TryGetValue(SelectableColorsKey, out selectableColors))
					selectableColors = Enumerable.Empty<string>();
				return (string[]) selectableColors;
			}
		}

		public string[] SelectableColorNames
		{
			get
			{
				object selectableColorNames;
				if (!Properties.TryGetValue(SelectableColorNamesKey, out selectableColorNames))
					selectableColorNames = SelectableColors.Clone();
				return (string[]) selectableColorNames;
			}
		}

		public int SelectedColorIndex
		{
			get
			{
				object index;
				return Properties.TryGetValue(SelectedColorIndexKey, out index)
					? Convert.ToInt32(index)
					: 0;
			}
			set { Properties[SelectedColorIndexKey] = value; }
		}

		public int SelectedTextColorIndex
		{
			get
			{
				object index;
				return Properties.TryGetValue(SelectedTextColorIndexKey, out index)
					? Convert.ToInt32(index)
					: 7;
			}
			set { Properties[SelectedTextColorIndexKey] = value; }
		}

		#endregion

		public ColorTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Color", properties,
			undoRedoService)
		{
			IconName = "Svg.Editor.Resources.svg.ic_format_color_fill.svg";
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

			// cache icons
			var cachingService = SvgEngine.TryResolve<ISvgCachingService>();
			if (cachingService != null)
			{
				foreach (var selectableColor in SelectableColors)
				{
					var color = Color.Create(selectableColor);
					var options = new SaveAsPngOptions
					{
						PreprocessAction = SvgProcessingUtil.ColorAction(color),
						CustomPostFix = (key, opt) => StringifyColor(color),
						ImageDimension = Tbi.Value?.GetSize(),
					};
					// global config
					// local config
					if (_iconDimensions != null)
						options.ImageDimension = _iconDimensions;

					cachingService.GetCachedPng(IconName, options);
				}
			}

			// add tool commands
			Commands = new List<IToolCommand>
			{
				new ChangeColorCommand(ws, this, "Change color"),
				new ChangeTextColorCommand(ws, this, "Change text color", _ => Canvas.ActiveTool is ISupportTextColor)
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

		private void ColorizeElement(SvgElement element, int colorIndex)
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
					element.Stroke = new SvgColourServer(Color.Create(SelectableColors.ElementAtOrDefault(colorIndex) ?? "#000000"));
				}
				if (!noFill)
				{
					element.Fill?.Dispose();
					element.Fill = new SvgColourServer(Color.Create(SelectableColors.ElementAtOrDefault(colorIndex) ?? "#000000"));
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
				ColorizeElement(e.NewChild.Children[0], SelectedColorIndex);
				ColorizeElement(e.NewChild.Children[1], SelectedTextColorIndex);
			}
			else
			{
				ColorizeElement(e.NewChild, SelectedColorIndex);
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

			public ChangeColorCommand(ISvgDrawingCanvas canvas, ColorTool tool, string name)
				: base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500)
			{
				_canvas = canvas;
			}

			private new ColorTool Tool => (ColorTool) base.Tool;

			public override async void Execute(object parameter)
			{
				var t = Tool;

				int selectedColorIndex;

				try
				{
					selectedColorIndex =
						await ColorInputService.GetIndexFromUserInput("Choose color", t.SelectableColorNames, t.SelectableColors);
				}
				catch (TaskCanceledException)
				{
					return;
				}

				if (_canvas.SelectedElements.Any())
				{
					t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Colorize selected elements", o => { }));
					// change the color of all selected items
					foreach (var selectedElement in _canvas.SelectedElements)
					{
						if (t.Canvas.ActiveTool is ISupportTextColor)
						{
							// in this case the selected element is a group
							// the first child of the group is the shape itself
							// and the second child is the text contained by the shape
							t.ColorizeElement(selectedElement.Children[0], selectedColorIndex);
						}
						else
						{
							t.ColorizeElement(selectedElement, selectedColorIndex);
						}
					}
					// don't change the global color when items are selected
					return;
				}

				if (t.Canvas.ActiveTool is ISupportTextColor tool)
				{
					t.SelectedTextColorIndex = tool.GetDefaultTextColorIndex(selectedColorIndex, t.SelectableColorNames);
				}

				var formerSelectedColor = t.SelectedColorIndex;
				t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
				{
					t.SelectedColorIndex = selectedColorIndex;
					t.Canvas.FireToolCommandsChanged();
				}, o =>
				{
					t.SelectedColorIndex = formerSelectedColor;
					t.Canvas.FireToolCommandsChanged();
				}));
			}

			public override string IconName => SvgCachingService?.GetCachedPng(Tool.IconName,
				new SaveAsPngOptions()
				{
					CustomPostFix = (key, op) => StringifyColor(Color.Create(Tool.SelectableColors.ElementAtOrDefault(Tool.SelectedColorIndex) ?? "#000000")),
					ImageDimension = Tbi.Value?.GetSize()
				});
		}

		private class ChangeTextColorCommand : ToolCommand
		{
			private readonly ISvgDrawingCanvas _canvas;

			public ChangeTextColorCommand(ISvgDrawingCanvas canvas, ColorTool tool, string name, Func<object, bool> canExecute = null)
				: base(tool, name, o => { }, canExecute, iconName: tool.IconName, sortFunc: tc => 500)
			{
				_canvas = canvas;
			}

			private new ColorTool Tool => (ColorTool)base.Tool;

			public override async void Execute(object parameter)
			{
				var t = Tool;

				int selectedTextColorIndex;

				try
				{
					selectedTextColorIndex =
						await ColorInputService.GetIndexFromUserInput("Choose color", t.SelectableColorNames, t.SelectableColors, 7);
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
						t.ColorizeElement(selectedElement.Children[1], selectedTextColorIndex);
					}
					// don't change the global color when items are selected
					return;
				}

				var formerSelectedColor = t.SelectedTextColorIndex;
				t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
				{
					t.SelectedTextColorIndex = selectedTextColorIndex;
					t.Canvas.FireToolCommandsChanged();
				}, o =>
				{
					t.SelectedTextColorIndex = formerSelectedColor;
					t.Canvas.FireToolCommandsChanged();
				}));
			}

			public override string IconName => SvgCachingService?.GetCachedPng(Tool.IconName,
				new SaveAsPngOptions()
				{
					CustomPostFix = (key, op) => StringifyColor(Color.Create(Tool.SelectableColors.ElementAtOrDefault(Tool.SelectedColorIndex) ?? "#000000")),
					ImageDimension = Tbi.Value?.GetSize()
				});
		}

		#endregion
	}
}