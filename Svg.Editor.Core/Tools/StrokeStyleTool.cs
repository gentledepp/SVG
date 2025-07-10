using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;

namespace Svg.Editor.Tools
{
	public interface IStrokeStyleOptionsInputService
	{
		Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(
			string title,
			IEnumerable<string> strokeDashOptions,
			int strokeDashSelected,
			IEnumerable<string> strokeWidthOptions,
			int strokeWidthSelected);
	}

	public class StrokeStyleTool : UndoableToolBase
	{
		public const string SelectedStrokeDashIndexKey = "selectedstrokedash";
		public const string StrokeDashesKey = "strokedashes";
		public const string StrokeDashNamesKey = "strokedashnames";
		public const string SelectedStrokeWidthIndexKey = "selectedstrokewidth";
		public const string StrokeWidthsKey = "strokewidths";
		public const string StrokeWidthNamesKey = "strokewidthnames";

		private IStrokeStyleOptionsInputService StrokeStyleOptionsInputService
			=> SvgEngine.Resolve<IStrokeStyleOptionsInputService>();

		public string[] StrokeDashes
		{
			get
			{
				object lineStyles;
				if (!Properties.TryGetValue(StrokeDashesKey, out lineStyles))
					lineStyles = Enumerable.Empty<string>();
				return (string[]) lineStyles;
			}
		}

		public string[] StrokeDashNames
		{
			get
			{
				object lineStyleNames;
				if (!Properties.TryGetValue(StrokeDashNamesKey, out lineStyleNames))
					lineStyleNames = Enumerable.Empty<string>();
				return (string[]) lineStyleNames;
			}
		}

		public int[] StrokeWidths
		{
			get
			{
				object strokeWidths;
				if (!Properties.TryGetValue(StrokeWidthsKey, out strokeWidths))
					strokeWidths = Enumerable.Empty<int>();
				return (int[]) strokeWidths;
			}
		}

		public string[] StrokeWidthNames
		{
			get
			{
				object strokeWidthNames;
				if (!Properties.TryGetValue(StrokeWidthNamesKey, out strokeWidthNames))
					strokeWidthNames = Enumerable.Empty<string>();
				return (string[]) strokeWidthNames;
			}
		}

		public int SelectedStrokeDashIndex { get {
			object index;
			return Properties.TryGetValue(SelectedStrokeDashIndexKey, out index)
				? Convert.ToInt32(index)
				: 0;
		} set { Properties[SelectedStrokeDashIndexKey] = value; } }

		public int SelectedStrokeWidthIndex { get {
			object index;
			return Properties.TryGetValue(SelectedStrokeWidthIndexKey, out index)
				? Convert.ToInt32(index)
				: 0;
		} set { Properties[SelectedStrokeWidthIndexKey] = value; } }

		public StrokeStyleOptions SelectedStrokeStyleOptions => new StrokeStyleOptions
		{
			StrokeDashIndex = Convert.ToInt32(SelectedStrokeDashIndex),
			StrokeWidthIndex = Convert.ToInt32(SelectedStrokeWidthIndex)
		};

		public StrokeStyleTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base(
			"Stroke style", properties, undoRedoService)
		{
			IconName = "ic_line_style.svg";
			ToolType = ToolType.Modify;
		}

		public override async Task Initialize(ISvgDrawingCanvas ws)
		{
			await base.Initialize(ws);

			// add tool commands
			Commands = new List<IToolCommand>
			{
				new ChangeStrokeStyleCommand(this, "Change stroke")
			};
		}

		public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
		{
			// add watch for global colorizing
			WatchDocument(newDocument);
			UnWatchDocument(oldDocument);
		}

		#region Private helpers

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
			SetStrokeStyle(e.NewChild);
		}

		private void SetStrokeStyle(SvgElement element, StrokeStyleOptions styleOptions = null)
		{
			var visualElement = element as SvgVisualElement;

			if (visualElement == null || !CanSetStrokeStyle(visualElement))
				return;

			styleOptions = styleOptions ?? new StrokeStyleOptions
			{
				StrokeDashIndex = SelectedStrokeDashIndex,
				StrokeWidthIndex = SelectedStrokeWidthIndex
			};

			var strokeDash = StrokeDashes.ElementAtOrDefault(styleOptions.StrokeDashIndex) ?? "none";
			var strokeWidth = new SvgUnit(SvgUnitType.Pixel,
				Math.Max(StrokeWidths.ElementAtOrDefault(styleOptions.StrokeWidthIndex), 1));
			var formerStrokeDash = visualElement.StrokeDashArray;
			var formerStrokeWidth = visualElement.StrokeWidth;
			UndoRedoService.ExecuteCommand(new UndoableActionCommand
			(
				"Set stroke style",
				_ =>
				{
					element.SvgElementFactory.SetPropertyValue(element, "stroke-dasharray", strokeDash, element.OwnerDocument);
					visualElement.StrokeWidth = strokeWidth;
				},
				_ =>
				{
					visualElement.StrokeDashArray = formerStrokeDash;
					visualElement.StrokeWidth = formerStrokeWidth;
				}
			), hasOwnUndoRedoScope: false);
		}

		private static bool CanSetStrokeStyle(SvgVisualElement visualElement)
		{
			return visualElement is SvgLine || visualElement is SvgPath ||
			       visualElement is SvgEllipse || visualElement is SvgRectangle ||
			       visualElement is SvgPolygon;
		}

		#endregion

		public class StrokeStyleOptions
		{
			public int StrokeDashIndex { get; set; }

			public int StrokeWidthIndex { get; set; }
		}

		/// <summary>
		/// This command changes the color of selected items, or the global selected color, if no items are selected.
		/// </summary>
		private class ChangeStrokeStyleCommand : ToolCommand
		{
			private new StrokeStyleTool Tool => (StrokeStyleTool) base.Tool;

			public ChangeStrokeStyleCommand(StrokeStyleTool tool, string name)
				: base(tool, name, o => { }, iconName: tool.IconName, sortFunc: tc => 500)
			{
			}

			/// <summary>
			/// Changes the selected global stroke style or the stroke style of the selected element.
			/// </summary>
			/// <param name="parameter"></param>
			public override async void Execute(object parameter)
			{
				var t = Tool;

				var selectedElement = t.Canvas.SelectedElements.FirstOrDefault();

				if (selectedElement != null)
				{
					var elementDash = selectedElement.StrokeDashArray.Any()
						? selectedElement.StrokeDashArray.ToString()
						: "none";
					var elementWidth = (int) Math.Round(selectedElement.StrokeWidth.Value);
					var formerStrokeStyle = new StrokeStyleOptions
					{
						StrokeDashIndex = Array.IndexOf(t.StrokeDashes, elementDash),
						StrokeWidthIndex = Array.IndexOf(t.StrokeWidths, elementWidth)
					};

					StrokeStyleOptions selectedStrokeStyleOptions;

					try
					{
						selectedStrokeStyleOptions =
							await
								t.StrokeStyleOptionsInputService.GetUserInput("Change stroke style for selection", t.StrokeDashNames,
									formerStrokeStyle.StrokeDashIndex,
									t.StrokeWidthNames, formerStrokeStyle.StrokeWidthIndex);
					}
					catch (TaskCanceledException)
					{
						return;
					}

					if (selectedStrokeStyleOptions == null)
						return;

					// prepare command for the whole operation
					t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Change stroke style operation",
						_ => t.Canvas.FireInvalidateCanvas(), _ => t.Canvas.FireInvalidateCanvas()));

					// change the stroke style of selected element
					t.SetStrokeStyle(selectedElement, selectedStrokeStyleOptions);
				}
				else
				{
					var formerSelectedOptions = t.SelectedStrokeStyleOptions;
					StrokeStyleOptions selectedStrokeStyleOptions;
					try
					{
						selectedStrokeStyleOptions =
							await
								t.StrokeStyleOptionsInputService.GetUserInput("Select global stroke style", t.StrokeDashNames,
									formerSelectedOptions.StrokeDashIndex,
									t.StrokeWidthNames, formerSelectedOptions.StrokeWidthIndex);
					}
					catch (TaskCanceledException)
					{
						return;
					}

					if (selectedStrokeStyleOptions == null)
						return;

					t.UndoRedoService.ExecuteCommand(new UndoableActionCommand
					(
						"Select global stroke style",
						_ =>
						{
							t.SelectedStrokeDashIndex = selectedStrokeStyleOptions.StrokeDashIndex;
							t.SelectedStrokeWidthIndex = selectedStrokeStyleOptions.StrokeWidthIndex;
						},
						_ =>
						{
							t.SelectedStrokeDashIndex = formerSelectedOptions.StrokeDashIndex;
							t.SelectedStrokeWidthIndex = formerSelectedOptions.StrokeWidthIndex;
						}
					));
				}
			}

			public override bool CanExecute(object parameter)
			{
				return Tool.Canvas.SelectedElements.Count < 2;
			}
		}
	}
}