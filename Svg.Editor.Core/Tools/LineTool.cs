using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Editor.Tools
{
	public interface IMarkerOptionsInputService
	{
		Task<int[]> GetUserInput(
			string title,
			IEnumerable<string> markerStartOptions,
			int markerStartSelected,
			IEnumerable<string> markerEndOptions,
			int markerEndSelected);
	}

	public class LineTool : UndoableToolBase
	{
		#region Private fields

		private const double MaxPointerDistance = 20.0;

		private static IMarkerOptionsInputService MarkerOptionsInputServiceProxy => SvgEngine
			.Resolve<IMarkerOptionsInputService>();

		private SvgLine _currentLine;

		private Brush _brush;

		//private Pen _pen;
		private bool _isActive;

		private MovementHandle _movementHandle;
		private ITool _activatedFrom;
		private PointF _offset;
		private PointF _translate;

		#endregion

		#region Private properties

		private Brush BlueBrush => _brush ?? (_brush =
			                           SvgEngine.Factory.CreateSolidBrush(
				                           SvgEngine.Factory.CreateColorFromArgb(255, 80, 210, 210)));

		//private Pen BluePen => _pen ?? (_pen = SvgEngine.Factory.CreatePen(BlueBrush, 5));

		#endregion

		#region Public properties

		public const string SelectedMarkerEndIndexKey = "selectedmarkerendindex";
		public const string SelectedMarkerStartIndexKey = "selectedmarkerstartindex";
		public const string DefaultStrokeWidthKey = "defaultstrokewidth";
		public const string MarkerStartIdsKey = "markerstartids";
		public const string MarkerStartNamesKey = "markerstartnames";
		public const string MarkerEndIdsKey = "markerendids";
		public const string MarkerEndNamesKey = "markerendnames";

		public string LineStyleIconName { get; set; } = "ic_line_endings.svg";

		public override int InputOrder => 300;

		public string[] MarkerStartIds
		{
			get
			{
				object markerIds;
				if (!Properties.TryGetValue(MarkerStartIdsKey, out markerIds))
					markerIds = Enumerable.Empty<string>();
				return (string[]) markerIds;
			}
		}

		public string[] MarkerStartNames
		{
			get
			{
				object markerNames;
				if (!Properties.TryGetValue(MarkerStartNamesKey, out markerNames))
					markerNames = Enumerable.Empty<string>();
				return (string[]) markerNames;
			}
		}

		public string[] MarkerEndIds
		{
			get
			{
				object markerIds;
				if (!Properties.TryGetValue(MarkerEndIdsKey, out markerIds))
					markerIds = Enumerable.Empty<string>();
				return (string[]) markerIds;
			}
		}

		public string[] MarkerEndNames
		{
			get
			{
				object markerNames;
				if (!Properties.TryGetValue(MarkerEndNamesKey, out markerNames))
					markerNames = Enumerable.Empty<string>();
				return (string[]) markerNames;
			}
		}

		public override bool IsActive
		{
			get { return _isActive; }
			set
			{
				if (_isActive == value) return;
				_isActive = value;
				// if tool was activated, reduce selection to a single line and set it as current line
				_currentLine = _isActive ? Canvas.SelectedElements.OfType<SvgLine>().FirstOrDefault() : null;
				Canvas.SelectedElements.Clear();
				if (_currentLine != null) Canvas.SelectedElements.Add(_currentLine);
				Canvas.FireInvalidateCanvas();
			}
		}

		public int SelectedMarkerStartIndex
		{
			get
			{
				object index;
				return Properties.TryGetValue(SelectedMarkerStartIndexKey, out index)
					? Convert.ToInt32(index)
					: 0;
			}
			set { Properties[SelectedMarkerStartIndexKey] = value; }
		}

		public int SelectedMarkerEndIndex
		{
			get
			{
				object index;
				return Properties.TryGetValue(SelectedMarkerEndIndexKey, out index)
					? Convert.ToInt32(index)
					: 0;
			}
			set { Properties[SelectedMarkerEndIndexKey] = value; }
		}

		public int DefaultStrokeWidth
		{
			get
			{
				object defaultStrokeWidth;
				return Properties.TryGetValue(DefaultStrokeWidthKey, out defaultStrokeWidth)
					? Convert.ToInt32(defaultStrokeWidth)
					: 2;
			}
			set { Properties[DefaultStrokeWidthKey] = value; }
		}

		#endregion

		public LineTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Line", properties,
			undoRedoService)
		{
			IconName = "ic_mode_edit.svg";
			ToolUsage = ToolUsage.Explicit;
			ToolType = ToolType.Create;
			HandleDragExit = true;
		}

		#region Overrides

		public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
		{
			if (oldDocument != null) UnWatchDocument(oldDocument);
			WatchDocument(newDocument);
			InitializeDefinitions(newDocument);
		}

		protected override async Task OnTap(TapGesture tap)
		{
			await base.OnTap(tap);

			if (!IsActive) return;

			_currentLine = Canvas.GetElementsUnder<SvgLine>(Canvas.GetPointerRectangle(tap.Position),
					SelectionType.Intersect)
				.FirstOrDefault();

			if (_currentLine != null)
			{
				Canvas.SelectedElements.Clear();
				Canvas.SelectedElements.Add(_currentLine);
			}
			else
			{
				Canvas.SelectedElements.Clear();

				if (_activatedFrom != null)
				{
					Canvas.ActiveTool = _activatedFrom;
					_activatedFrom = null;
				}
			}

			Canvas.FireToolCommandsChanged();
			Canvas.FireInvalidateCanvas();
		}

		protected override async Task OnDoubleTap(DoubleTapGesture doubleTap)
		{
			await base.OnDoubleTap(doubleTap);

			if (Canvas.ActiveTool.ToolType != ToolType.Select) return;

			var line = Canvas.GetElementsUnderPointer<SvgLine>(doubleTap.Position).FirstOrDefault();
			if (line != null)
			{
				Canvas.SelectedElements.Clear();
				Canvas.SelectedElements.Add(line);

				// save the active tool for restoring later
				_activatedFrom = Canvas.ActiveTool;
				Canvas.ActiveTool = this;
				Canvas.FireInvalidateCanvas();
			}
		}

		protected override async Task OnDrag(DragGesture drag)
		{
			await base.OnDrag(drag);

			if (!IsActive) return;

			if (drag.State == DragState.Exit)
			{
				_movementHandle = MovementHandle.None;
				_offset = null;
				_translate = null;
				return;
			}

			var relativeEnd = Canvas.ScreenToCanvas(drag.Position);

			if (_currentLine == null)
			{
				var relativeStart = Canvas.ScreenToCanvas(drag.Start);

				SelectLine(CreateLine(relativeStart));

				// capture variables for use in lambda
				var children = Canvas.Document.Children;
				var capturedCurrentLine = _currentLine;
				UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add new line", o =>
				{
					children.Add(capturedCurrentLine);
					Canvas.FireInvalidateCanvas();
				}, o =>
				{
					children.Remove(capturedCurrentLine);
					Canvas.FireInvalidateCanvas();
				}));

				_movementHandle = MovementHandle.End;
			}
			else
			{
				var m = _currentLine.Transforms.GetMatrix();
				m.Invert();
				m.TransformPoints(new[] {relativeEnd});

				// capture _currentLine for use in lambda
				var capturedCurrentLine = _currentLine;

				if (_movementHandle == MovementHandle.None)
				{
					var canvasPointer1Position = Canvas.ScreenToCanvas(drag.Start);
					var points = _currentLine.GetTransformedLinePoints();
					_movementHandle = Math.Abs(canvasPointer1Position.X - points[1].X) * Canvas.ZoomFactor <= MaxPointerDistance &&
					                  Math.Abs(canvasPointer1Position.Y - points[1].Y) * Canvas.ZoomFactor <= MaxPointerDistance
						? MovementHandle.End
						: Math.Abs(canvasPointer1Position.X - points[0].X) * Canvas.ZoomFactor <= MaxPointerDistance &&
						  Math.Abs(canvasPointer1Position.Y - points[0].Y) * Canvas.ZoomFactor <= MaxPointerDistance
							? MovementHandle.Start
							: _currentLine.GetBoundingBox().Contains(canvasPointer1Position)
								? MovementHandle.StartEnd
								: MovementHandle.None;

					if (_movementHandle == MovementHandle.None) return;

					UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit line", o => Canvas.FireInvalidateCanvas(),
						o => Canvas.FireInvalidateCanvas()));
				}

				var formerStartY = _currentLine.StartY;
				var formerStartX = _currentLine.StartX;
				var formerEndY = _currentLine.EndY;
				var formerEndX = _currentLine.EndX;

				switch (_movementHandle)
				{
					case MovementHandle.End:
						// capture variables for use in lambda
						UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move line end", o =>
						{
							capturedCurrentLine.EndX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
							capturedCurrentLine.EndY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
						}, o =>
						{
							capturedCurrentLine.EndX = formerEndX;
							capturedCurrentLine.EndY = formerEndY;
						}), hasOwnUndoRedoScope: false);
						break;
					case MovementHandle.Start:
						// capture variables for use in lambda
						UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move line start", o =>
						{
							capturedCurrentLine.StartX = new SvgUnit(SvgUnitType.Pixel, relativeEnd.X);
							capturedCurrentLine.StartY = new SvgUnit(SvgUnitType.Pixel, relativeEnd.Y);
						}, o =>
						{
							capturedCurrentLine.StartX = formerStartX;
							capturedCurrentLine.StartY = formerStartY;
						}), hasOwnUndoRedoScope: false);
						break;
					case MovementHandle.StartEnd:
						var absoluteDeltaX = drag.Delta.Width / Canvas.ZoomFactor;
						var absoluteDeltaY = drag.Delta.Height / Canvas.ZoomFactor;

						// add translation to current line
						var previousDelta = _offset ?? PointF.Create(0, 0);
						var relativeDeltaX = absoluteDeltaX - previousDelta.X;
						var relativeDeltaY = absoluteDeltaY - previousDelta.Y;

						previousDelta.X = absoluteDeltaX;
						previousDelta.Y = absoluteDeltaY;
						_offset = previousDelta;

						AddTranslate(_currentLine, relativeDeltaX, relativeDeltaY);
						break;
				}

				Canvas.FireInvalidateCanvas();
			}
		}

		public override async Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
		{
			await base.OnDraw(renderer, ws);

			if (_currentLine != null)
			{
				renderer.Graphics.Save();

				var radius = (float) MaxPointerDistance / 4 / ws.ZoomFactor;
				var halfRadius = radius / 2;
				var points = _currentLine.GetTransformedLinePoints();
				renderer.FillCircle(points[0].X - halfRadius, points[0].Y - halfRadius, radius, BlueBrush);
				renderer.FillCircle(points[1].X - halfRadius, points[1].Y - halfRadius, radius, BlueBrush);


				renderer.Graphics.Restore();
			}
		}

		public override async Task Initialize(ISvgDrawingCanvas ws)
		{
			await base.Initialize(ws);
			
			Commands = new List<IToolCommand>
			{
				new ChangeLineStyleCommand(ws, this, "Line endings")
			};
		}

		#endregion

		#region Private helpers

		private void UnWatchDocument(SvgDocument svgDocument)
		{
			svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
		}

		private void WatchDocument(SvgDocument svgDocument)
		{
			svgDocument.ChildRemoved -= SvgDocumentOnChildRemoved;
			svgDocument.ChildRemoved += SvgDocumentOnChildRemoved;
		}

		private void SvgDocumentOnChildRemoved(object sender, ChildRemovedEventArgs args)
		{
			if (IsActive && args.RemovedChild == _currentLine)
			{
				_currentLine = null;
				Canvas.FireInvalidateCanvas();
			}
		}

		private void InitializeDefinitions(SvgDocument document)
		{
			var definitions = document.Children.OfType<SvgDefinitionList>().FirstOrDefault();
			if (definitions == null)
			{
				definitions = new SvgDefinitionList();
				document.Children.Insert(0, definitions);
			}

			foreach (var marker in EnumerateMarkers())
			{
				if (document.GetElementById(marker.ID) != null) continue;

				definitions.Children.Add(marker);
			}
		}

		private SvgLine CreateLine(PointF relativeStart)
		{
			var line = new SvgLine
			{
				Stroke = new SvgColourServer(SvgEngine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
				Fill = SvgPaintServer.None,
				StrokeWidth = new SvgUnit(SvgUnitType.Pixel, DefaultStrokeWidth),
				StartX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
				StartY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
				EndX = new SvgUnit(SvgUnitType.Pixel, relativeStart.X),
				EndY = new SvgUnit(SvgUnitType.Pixel, relativeStart.Y),
				MarkerStart = MarkerStartIds.Any() ? CreateUriFromId(MarkerStartIds[SelectedMarkerStartIndex]) : null,
				MarkerEnd = MarkerEndIds.Any() ? CreateUriFromId(MarkerEndIds[SelectedMarkerEndIndex]) : null
			};

			return line;
		}

		private void AddTranslate(SvgVisualElement element, float deltaX, float deltaY)
		{
			// the movetool stores the last translation explicitly for each element
			// that way, if another tool manipulates the translation (e.g. the snapping tool)
			// the movetool is not interfered by that
			var b = element.GetBoundingBox();
			var translate = _translate ?? PointF.Create(b.X, b.Y);

			translate.X += deltaX;
			translate.Y += deltaY;

			_translate = translate;

			var dX = translate.X - b.X;
			var dY = translate.Y - b.Y;

			var m = element.CreateTranslation(dX, dY);
			var formerM = element.Transforms.GetMatrix().Clone();
			UndoRedoService.ExecuteCommand(
				new UndoableActionCommand("Move object", o => { element.SetTransformationMatrix(m); },
					o => { element.SetTransformationMatrix(formerM); }), hasOwnUndoRedoScope: false);
		}

		private void SelectLine(SvgLine line)
		{
			_currentLine = line;
			Canvas.SelectedElements.Clear();
			Canvas.SelectedElements.Add(line);
		}

		private static Uri CreateUriFromId(string markerEndId, string exception = "none")
		{
			return markerEndId != exception ? new Uri($"url(#{markerEndId})", UriKind.Relative) : null;
		}

		private static IEnumerable<SvgMarker> EnumerateMarkers()
		{
			var marker = new SvgMarker
			{
				ID = "arrowStart",
				Orient = new SvgOrient() {IsAuto = true},
				RefX = new SvgUnit(SvgUnitType.Pixel, -2.5f),
				MarkerWidth = 2
			};
			marker.Children.Add(new SvgPath
			{
				PathData = new SvgPathSegmentList(new SvgPathSegment[]
				{
					new SvgMoveToSegment(PointF.Create(0, -2.0f)),
					new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2f)),
					new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(-4.0f, 0)),
					new SvgClosePathSegment()
				}),
				Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
				Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
			});
			yield return marker;
			marker = new SvgMarker
			{
				ID = "arrowEnd",
				Orient = new SvgOrient() {IsAuto = true},
				RefX = new SvgUnit(SvgUnitType.Pixel, 2.5f),
				MarkerWidth = 2
			};
			marker.Children.Add(new SvgPath
			{
				PathData = new SvgPathSegmentList(new SvgPathSegment[]
				{
					new SvgMoveToSegment(PointF.Create(0, -2.0f)),
					new SvgLineSegment(PointF.Create(0, -2.0f), PointF.Create(0, 2.0f)),
					new SvgLineSegment(PointF.Create(0, 2.0f), PointF.Create(4.0f, 0)),
					new SvgClosePathSegment()
				}),
				Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
				Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
			});
			yield return marker;
			marker = new SvgMarker
			{
				ID = "circle",
				Orient = new SvgOrient() {IsAuto = true} /*, RefX = new SvgUnit(SvgUnitType.Pixel, -1.5f)*/,
				MarkerWidth = 2
			};
			marker.Children.Add(new SvgEllipse
			{
				RadiusX = 1.5f,
				RadiusY = 1.5f,
				Stroke = SvgColourServer.ContextStroke, // inherit stroke color from parent/aka context
				Fill = SvgColourServer.ContextFill, // inherit stroke color from parent/aka context
			});
			yield return marker;
		}

		#endregion

		#region Inner types

		/// <summary>
		/// This command changes the line style of selected items, or the global line style, if no items are selected.
		/// </summary>
		private class ChangeLineStyleCommand : ToolCommand
		{
			private readonly ISvgDrawingCanvas _canvas;

			public ChangeLineStyleCommand(ISvgDrawingCanvas canvas, LineTool tool, string name)
				: base(tool, name, o => { }, iconName: tool.LineStyleIconName, sortFunc: tc => 500)
			{
				_canvas = canvas;
			}

			public override async void Execute(object parameter)
			{
				var t = (LineTool) Tool;

				int markerStartIndex;
				int markerEndIndex;

				if (t._currentLine == null)
				{
					markerStartIndex = t.SelectedMarkerStartIndex;
					markerEndIndex = t.SelectedMarkerEndIndex;
				}
				else
				{
					var markerStartId = t._currentLine.MarkerStart?.OriginalString?.Replace("url(#", null)?.TrimEnd(')') ??
					                    "none";
					var markerEndId = t._currentLine.MarkerEnd?.OriginalString?.Replace("url(#", null)?.TrimEnd(')') ?? "none";
					markerStartIndex = Array.IndexOf(t.MarkerStartIds, markerStartId);
					markerEndIndex = Array.IndexOf(t.MarkerEndIds, markerEndId);
				}

				int[] selectedOptions;

				try
				{
					selectedOptions = await MarkerOptionsInputServiceProxy.GetUserInput("Choose line endings",
						t.MarkerStartNames, markerStartIndex,
						t.MarkerEndNames, markerEndIndex);
				}
				catch (TaskCanceledException)
				{
					return;
				}

				if (selectedOptions == null)
					return;

				var startIndex = selectedOptions[0];
				var endIndex = selectedOptions[1];

				if (t._currentLine != null)
				{
					var formerCurrentLine = t._currentLine;
					var formerMarkerStart = t._currentLine.MarkerStart;
					var formerMarkerEnd = t._currentLine.MarkerEnd;
					t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
					{
						// change the line style of all selected items
						formerCurrentLine.MarkerStart = CreateUriFromId(t.MarkerStartIds[startIndex]);
						formerCurrentLine.MarkerEnd = CreateUriFromId(t.MarkerEndIds[endIndex]);
						_canvas.FireInvalidateCanvas();
					}, o =>
					{
						formerCurrentLine.MarkerStart = formerMarkerStart;
						formerCurrentLine.MarkerEnd = formerMarkerEnd;
						_canvas.FireInvalidateCanvas();
					}));
					// don't change the global line style when items are selected
					return;
				}

				var formerSelectedMarkerStartId = t.SelectedMarkerStartIndex;
				var formerSelectedMarkerEndId = t.SelectedMarkerEndIndex;
				t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
				{
					t.SelectedMarkerStartIndex = startIndex;
					t.SelectedMarkerEndIndex = endIndex;
				}, o =>
				{
					t.SelectedMarkerStartIndex = formerSelectedMarkerStartId;
					t.SelectedMarkerEndIndex = formerSelectedMarkerEndId;
				}));
			}

			public override bool CanExecute(object parameter)
			{
				return Tool.IsActive;
			}
		}

		private enum MovementHandle
		{
			None,
			Start,
			End,
			StartEnd
		}

		#endregion
	}
}