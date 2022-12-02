using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Svg.Editor.Events;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.Properties;
using Svg.Editor.Tools;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Xamarin.Forms;
using Color = Svg.Interfaces.Color;
using IGestureRecognizer = Svg.Editor.Interfaces.IGestureRecognizer;

namespace Svg.Editor
{
	public sealed class SvgDrawingCanvas : IDisposable, ICanInvalidateCanvas, INotifyPropertyChanged, ISvgDrawingCanvas
	{
		#region Private fields and properties

		private readonly ObservableCollection<SvgVisualElement> _selectedElements;
		private readonly ObservableCollection<ITool> _tools;
		private ObservableCollection<IEnumerable<IToolCommand>> _toolCommands;
		private List<IToolCommand> _toolSelectors;
		private SvgDocument _document;
		private bool _initialized;
		private ITool _activeTool;
		private bool _isDebugEnabled;
		private bool _documentIsDirty;
		private PointF _zoomFocus;
		private IDisposable _onGestureToken;
		private IGestureRecognizer _gestureRecognizer;

		private Subject<string> _propertyChangedSubject = new Subject<string>();
		private readonly ISchedulerProvider _schedulerProvider;
		private readonly object _lockObject = new object();

		private IUndoRedoService UndoRedoService { get; }

		#endregion

		#region Public properties

		public SvgDocument Document
		{
			get
			{
				if (_document == null)
				{
					_document = new SvgDocument();
					_document.ViewBox = SvgViewBox.Empty;

					OnDocumentChanged(null, _document);
				}
				return _document;
			}
			set
			{
				var oldDocument = _document;
				_document = value;
				OnDocumentChanged(oldDocument, _document);
			}
		}

		public bool DocumentIsDirty
		{
			get { return _documentIsDirty; }
			private set
			{
				if (_documentIsDirty != value)
				{
					_documentIsDirty = value;
					OnPropertyChanged();
				}
			}
		}

		public ObservableCollection<SvgVisualElement> SelectedElements => _selectedElements;

		public ObservableCollection<ITool> Tools => _tools;

		public ObservableCollection<IEnumerable<IToolCommand>> ToolCommands
		{
			get
			{
				if (_toolCommands == null)
					ResetToolCommands();
				return _toolCommands;
			}
		}

		public List<Func<SvgVisualElement, Task>> DefaultEditors { get; } = new List<Func<SvgVisualElement, Task>>();

        public PointF Translate { get; set; } = PointF.Create(0, 0);

        public float ZoomFactor { get; set; } = 1f;

		public PointF ZoomFocus
		{
			get { return _zoomFocus ?? (_zoomFocus = PointF.Create(0, 0)); }
			set { _zoomFocus = value; }
		}

		public bool ZoomEnabled { get; set; } = true;

		public int ScreenWidth { get; set; }

		public int ScreenHeight { get; set; }

		public PointF ScreenCenter => PointF.Create((float) ScreenWidth / 2, (float) ScreenHeight / 2);

		public RectangleF Constraints { get; set; }

		public ConstraintsMode ConstraintsMode { get; set; }

		/// <summary>
		/// If enabled, adds a DebugTool that brings some helpful visualizations
		/// </summary>
		public bool IsDebugEnabled
		{
			get { return _isDebugEnabled; }
			set
			{
				_isDebugEnabled = value;

				if (_isDebugEnabled)
				{
					var dt = Tools.OfType<DebugTool>().FirstOrDefault();
					if (dt == null)
						Tools.Add(new DebugTool());
				}
				else
				{
					var dt = Tools.OfType<DebugTool>().FirstOrDefault();
					if (dt != null)
						Tools.Remove(dt);
				}
			}
		}

		public ITool ActiveTool
		{
			get { return _activeTool; }
			set
			{
				_activeTool = value;
				if (_activeTool != null)
				{
					_activeTool.IsActive = true;
				}
				foreach (var otherTool in Tools.Where(t => t != _activeTool && t.ToolUsage == ToolUsage.Explicit))
				{
					otherTool.IsActive = false;
				}
			}
		}

		public Color BackgroundColor { get; set; }

		/// <summary>
		/// Allows to specify the default filter used to determine whether some SvgVisualElement takes part in a hit-test or not
		/// </summary>
		public Func<SvgVisualElement, bool> DefaultHitTestFilter { get; set; } = element =>
                            !element.CustomAttributes.ContainsKey(ToolBase.BackgroundCustomAttributeKey) &&
                            !element.CustomAttributes.ContainsKey(ToolBase.HittestInvisibleAttributeKey);

		public IGestureRecognizer GestureRecognizer
		{
			get { return _gestureRecognizer; }
			set
			{
				_onGestureToken?.Dispose();
				if (value == null) return;
				_gestureRecognizer = value;
				_onGestureToken = _gestureRecognizer.RecognizedGestures.Subscribe(async g => await OnGesture(g));
			}
		}

		#endregion

		public event EventHandler CanvasInvalidated;

		[Obsolete("ToolCommandsChanged will be removed, subscribe to PropertyChanged instead.")]
		public event EventHandler ToolCommandsChanged;

		public SvgDrawingCanvas()
		{
			Translate = PointF.Create(0f, 0f);
			ZoomFactor = 1f;

			BackgroundColor = SvgEngine.Factory.Colors.White;

			_selectedElements = new ObservableCollection<SvgVisualElement>();
			_selectedElements.CollectionChanged += OnSelectionChanged;

			UndoRedoService = SvgEngine.Resolve<IUndoRedoService>();
            UndoRedoService.ActionExecuted += UndoRedoServiceOnActionExecuted;
            UndoRedoService.CanRedoChanged += UndoRedoServiceOnCanRedoChanged;
            UndoRedoService.CanUndoChanged += UndoRedoServiceOnCanRedoChanged;

			GestureRecognizer = SvgEngine.Resolve<IGestureRecognizer>();
			_schedulerProvider = SvgEngine.Resolve<ISchedulerProvider>();

			_tools = new ObservableCollection<ITool>();
			_tools.CollectionChanged += OnToolsChanged;

			_propertyChangedSubject.Subscribe(OnPropertyChanged);
		}

        private void UndoRedoServiceOnActionExecuted(object sender, CommandEventArgs e)
        {
            // clear selection when undoing
            if (e.ExecuteAction == ExecuteAction.Undo)
                SelectedElements.Clear();
        }

        private void UndoRedoServiceOnCanRedoChanged(object sender, EventArgs e)
        {
            FireToolCommandsChanged();
        }

        public void LoadTools(params Func<ITool>[] tools)
		{
			_tools.CollectionChanged -= OnToolsChanged;
			_tools.Clear();

			foreach (var tool in tools)
			{
				_tools.Add(tool.Invoke());
			}

			_tools.CollectionChanged += OnToolsChanged;
		}

		public void LoadTools(string serializedProperties, params Func<ISerializableTool>[] tools)
		{
			IDictionary<string, IDictionary<string, object>> properties;
			try
			{
				properties = JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, object>>>(serializedProperties,
					new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
			}
			catch (JsonReaderException)
			{
				properties = null;
			}

			if (properties == null)
			{
				LoadTools(tools.Select<Func<ISerializableTool>, Func<ITool>>(t => () => t() as ITool).ToArray());
				return;
			}

			var toolFactories = tools.Select<Func<ISerializableTool>, Func<ITool>>(t =>
			{
				IDictionary<string, object> props;
				var realizedTool = t();
				if (properties.TryGetValue(realizedTool.SerializationId, out props))
					realizedTool.DeserializeProperties(props.ToDictionary(p => p.Key,
						p => (p.Value as JArray)?.ToObject<object[]>() ?? p.Value));
				return () => realizedTool as ITool;
			});
			LoadTools(toolFactories.ToArray());
		}

		public string GetToolPropertiesJson()
		{
			var properties = Tools.Cast<ISerializableTool>().Where(t => t.CanSerialize)
				.ToDictionary(t => t.SerializationId, t => t.GetPropertiesForSerialization());
			return JsonConvert.SerializeObject(properties, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
		}

		/// <summary>
		/// Called by the platform specific input event detector whenever the user interacts with the model
		/// </summary>
		/// <param name="ev"></param>
		public async Task OnEvent(UserInputEvent ev)
		{
			await EnsureInitialized();

			// call gesture recognizer first
			GestureRecognizer?.OnNext(ev);

			foreach (var tool in Tools.OrderBy(t => t.InputOrder))
			{
				await tool.OnUserInput(ev, this);
			}
		}

		/// <summary>
		/// Called when a gesture - like tap, double tap, long press, etc - is recognized.
		/// </summary>
		/// <param name="gesture"></param>
		public async Task OnGesture(UserGesture gesture)
		{
			await EnsureInitialized();

			foreach (var tool in Tools.OrderBy(t => t.GestureOrder))
			{
				await tool.OnGesture(gesture);
			}
		}

        public Action<IRenderer> OnDrawAction;


        /// <summary>
        /// Called by platform specific implementation to allow tools to draw something onto the canvas
        /// </summary>
        /// <param name="renderer"></param>
        public async Task OnDraw(IRenderer renderer)
		{
            // make sure all tools have been initialized successfully
			await EnsureInitialized();

            ScreenWidth = renderer.Width;
            ScreenHeight = renderer.Height;

            OnDrawAction += OnInitialDraw;
            OnDrawAction.Invoke(renderer);

			SetInitialTransformation();

			switch (ConstraintsMode)
			{
				case ConstraintsMode.FillUniform:
					ApplyConstraintsFillUniform();
					break;
				case ConstraintsMode.FitUniform:
					ApplyConstraintsFitUniform();
					break;
			}

			// apply global panning and zooming
			renderer.Translate(Translate.X, Translate.Y);
			renderer.Scale(ZoomFactor, ZoomFocus.X, ZoomFocus.Y);

			// draw default background
			renderer.FillEntireCanvasWithColor(BackgroundColor);

			// prerender step (e.g. gridlines, etc.)
			foreach (var tool in Tools.OrderBy(t => t.PreDrawOrder))
			{
				await tool.OnPreDraw(renderer, this);
			}

			// render svg step
			renderer.Graphics.Save();
			Document.Draw(GetOrCreateRenderer(renderer.Graphics));
			renderer.Graphics.Restore();

			// post render step (e.g. selection borders, etc.)
			foreach (var tool in Tools.OrderBy(t => t.DrawOrder))
			{
				await tool.OnDraw(renderer, this);
			}
        }

        private void OnInitialDraw(IRenderer obj)
        {
            OnDrawAction -= OnInitialDraw;
        }


        private void ApplyConstraintsFillUniform()
		{
			if (Constraints == null || Constraints == RectangleF.Empty) return;

			// if zoom is totally out of bounds, reset
			if (ScreenWidth / ZoomFactor > Constraints.Width || ScreenHeight / ZoomFactor > Constraints.Height)
			{
				ZoomFactor = Math.Max(ScreenWidth / Constraints.Width,
					ScreenHeight / Constraints.Height);
				Translate = PointF.Create(ScreenWidth / ZoomFactor > Constraints.Width ? 0 : Translate.X,
					ScreenHeight / ZoomFactor > Constraints.Height ? 0 : Translate.Y);
			}

			// adjust the translate according to the constraints

			var constraintTopLeft = PointF.Create(Constraints.Left, Constraints.Top) * ZoomFactor;
			var constraintBottomRight = PointF.Create(Constraints.Right, Constraints.Bottom) * ZoomFactor;
			var screenTopLeft = ScreenToCanvas(0, 0) * ZoomFactor;
			var screenBottomRight = ScreenToCanvas(ScreenWidth, ScreenHeight) * ZoomFactor;

			if (screenTopLeft.X < constraintTopLeft.X)
			{
				Translate.X += screenTopLeft.X - constraintTopLeft.X;
			}

			if (screenTopLeft.Y < constraintTopLeft.Y)
			{
				Translate.Y += screenTopLeft.Y - constraintTopLeft.Y;
			}

			if (screenBottomRight.X > constraintBottomRight.X)
			{
				Translate.X += screenBottomRight.X - constraintBottomRight.X;
			}

			if (screenBottomRight.Y > constraintBottomRight.Y)
			{
				Translate.Y += screenBottomRight.Y - constraintBottomRight.Y;
			}
		}

        private bool Loading { get; set; } = true;
        private void ApplyConstraintsFitUniform()
		{
			if (Constraints == null || Constraints == RectangleF.Empty) return;

			// if zoom is totally out of bounds, reset
			// or: when loading, zoom out
			if ((ScreenWidth / ZoomFactor > Constraints.Width && ScreenHeight / ZoomFactor > Constraints.Height)
				|| Loading)
			{
				ZoomFactor = Math.Min(ScreenWidth / Constraints.Width,
					ScreenHeight / Constraints.Height);
                ZoomFocus = PointF.Empty;
				Translate = PointF.Create((ScreenWidth - Constraints.Width * ZoomFactor) / 2,
                    (ScreenHeight - Constraints.Height * ZoomFactor) / 2);
				// this should replace "ZoomFocus = PointF.Empty;" but doesn't work when ZoomFactor < 1
				//+ (ZoomFocus - ScreenToCanvas(0, 0)) * (ZoomFactor - 1);
                Loading = false;
				return;
			}
			var constraintTopLeft = PointF.Create(Constraints.Left, Constraints.Top) * ZoomFactor;
			var constraintBottomRight = PointF.Create(Constraints.Right, Constraints.Bottom) * ZoomFactor;
			var screenTopLeft = ScreenToCanvas(0, 0) * ZoomFactor;
			var screenBottomRight = ScreenToCanvas(ScreenWidth, ScreenHeight) * ZoomFactor;
			var marginX = (ScreenWidth - Constraints.Width * ZoomFactor) / 2;
			var marginY = (ScreenHeight - Constraints.Height * ZoomFactor) / 2;
			if (marginX < 0) marginX = 0;
			if (marginY < 0) marginY = 0;

			// adjust the translate according to the constraints
			if (screenTopLeft.X < constraintTopLeft.X)
			{
				Translate.X += screenTopLeft.X - constraintTopLeft.X + marginX;
			}
			else if (screenBottomRight.X > constraintBottomRight.X)
			{
				Translate.X += screenBottomRight.X - constraintBottomRight.X - marginX;
			}

			if (screenTopLeft.Y < constraintTopLeft.Y)
			{
				Translate.Y += screenTopLeft.Y - constraintTopLeft.Y + marginY;
			}
			else if (screenBottomRight.Y > constraintBottomRight.Y)
			{
				Translate.Y += screenBottomRight.Y - constraintBottomRight.Y - marginY;
			}
		}

		public async Task EnsureInitialized()
		{
			if (!_initialized)
			{
				foreach (var tool in Tools)
					await tool.Initialize(this);

				if(ActiveTool is null)
                {
                    ActiveTool = Tools.FirstOrDefault(t => t.ToolUsage == ToolUsage.Explicit
                                                          && t.Properties.ContainsKey("isselectedtool")
                                                          && t.Properties["isselectedtool"] is true)
                                ?? Tools.FirstOrDefault(t => t.ToolUsage == ToolUsage.Explicit);
                }

				_initialized = true;

				FireToolCommandsChanged();
			}
		}

		private ISvgRenderer GetOrCreateRenderer(Graphics graphics)
		{
			return SvgRenderer.FromGraphics(graphics);
		}

		public Bitmap CreateBitmap(int width, int height)
		{
			return Bitmap.Create(width, height);
		}

		/// <summary>
		/// Returns a rectangle with width and height 20px that surrounds the given point
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public RectangleF GetPointerRectangle(PointF p)
        {
            // "10 pixel fat finger" - but take ZoomFactor into account, as the more you zoom, the larger your finger area on the drawing canvas!
            var halfFingerThickness = 10 * ZoomFactor; 
            var location = PointF.Create(p.X - halfFingerThickness, p.Y - halfFingerThickness);
            var size = SizeF.Create(halfFingerThickness * 2, halfFingerThickness * 2);
            return RectangleF.Create(location, size);
		}

        /// <summary>
        /// the selection rectangle must be in absolute screen coordinates (so not transformed by canvas.Translate or canvas.ZoomFactor)
        /// </summary>
        /// <param name="selectionRectangle"></param>
        /// <param name="selectionType"></param>
        /// <param name="maxItems"></param>
        /// <param name="recursionLevel"></param>
        /// <returns></returns>
        public IList<TElement> GetElementsUnder<TElement>(
            RectangleF selectionRectangle,
            SelectionType selectionType,
			HitTestResultMode resultMode = HitTestResultMode.ReturnRootElementOnly,
            int maxItems = int.MaxValue,
            int recursionLevel = 1)
            where TElement : SvgVisualElement
        {

			return GetElementsUnder<TElement>(selectionRectangle,selectionType, resultMode, DefaultHitTestFilter, maxItems,recursionLevel);
        }

		/// <summary>
		/// the selection rectangle must be in absolute screen coordinates (so not transformed by canvas.Translate or canvas.ZoomFactor)
		/// </summary>
		/// <param name="selectionRectangle"></param>
		/// <param name="selectionType"></param>
		/// <param name="maxItems"></param>
		/// <param name="recursionLevel"></param>
		/// <returns></returns>
		public IList<TElement> GetElementsUnder<TElement>(
			RectangleF selectionRectangle,
			SelectionType selectionType,
            HitTestResultMode resultMode,
            Func<SvgVisualElement, bool> filter,
			int maxItems = int.MaxValue,
			int recursionLevel = int.MaxValue)
			where TElement : SvgVisualElement
		{
			if (selectionRectangle == null)
				return new List<TElement>();
			
			return Document.HitTest<TElement>(selectionRectangle,selectionType,resultMode, GetCanvasTransformationMatrix(),recursionLevel)
                    .Where(c => filter(c))
					.Take(maxItems)
					.ToList();
		}

        /// <summary>
        /// gets all visual elements under the given pointer (a 20px rectangle surrounding the given point to simulate thick finger)
        /// </summary>
        /// <param name="pointer1Position"></param>
        /// <param name="selectionType"></param>
        /// <param name="recursionLevel"></param>
        /// <returns></returns>
        public IList<TElement> GetElementsUnderPointer<TElement>(PointF pointer1Position, 
            SelectionType selectionType = SelectionType.Intersect, 
            int recursionLevel = 1)
			where TElement : SvgVisualElement
        {
            return GetElementsUnder<TElement>(GetPointerRectangle(pointer1Position), selectionType,
                HitTestResultMode.ReturnAllMatchingDescendants,
				recursionLevel: recursionLevel);
		}

		public async Task AddItemInScreenCenter(SvgDocument document)
		{
			var visibleChildren =
				document.Children.OfType<SvgVisualElement>().Where(e => e.Displayable && e.Visible).ToList();

			var element = visibleChildren.First();
			if (visibleChildren.Count > 1)
			{
				var group = new SvgGroup
				{
					Fill = document.Fill,
					Stroke = document.Stroke
				};
				foreach (var visibleChild in visibleChildren)
				{
					group.Children.Add(visibleChild);
				}
				element = group;
			}

			await AddItemInScreenCenter(element);

			MergeSvgDefs(Document, document);
		}

		public async Task AddItemInScreenCenter(SvgVisualElement element)
		{
			var childBounds = element.GetBoundingBox();
			var halfRelChildWidth = childBounds.Width / 2;
			var halfRelChildHeight = childBounds.Height / 2;
			var centerPos = ScreenToCanvas((float) ScreenWidth / 2, (float) ScreenHeight / 2);
			var centerPosX = centerPos.X - halfRelChildWidth;
			var centerPosY = centerPos.Y - halfRelChildHeight;

			// make sure it is centered
			if (Math.Abs(childBounds.X) > float.Epsilon)
				centerPosX -= childBounds.X;
			if (Math.Abs(childBounds.Y) > float.Epsilon)
				centerPosY -= childBounds.Y;

			//SvgTranslate tl = new SvgTranslate(centerPosX, centerPosY);
			//element.Transforms.Add(tl);
			//element.ID = $"{element.ElementName}_{Guid.NewGuid():N}";
			var m = element.CreateTranslation(centerPosX, centerPosY);
			element.SetTransformationMatrix(m);

			UndoRedoService.ExecuteCommand(new UndoableActionCommand
				(
					"Add child element",
					o =>
					{
						Document.Children.Add(element);
						FireInvalidateCanvas();
					},
					o =>
					{
						Document.Children.Remove(element);
						FireInvalidateCanvas();
					})
			);

			// this has to happen after the element is added, because the undoable command of
			// the merge is concatenated to the add child command
			if (element.OwnerDocument != null)
				MergeSvgDefs(Document, element.OwnerDocument);

			// invoke the defaulteditors for the element
			foreach (var defaultEditor in DefaultEditors)
			{
				await defaultEditor.Invoke(element);
			}
		}

		private void MergeSvgDefs(SvgDocument target, SvgDocument source)
		{
			if (target == null) throw new ArgumentNullException(nameof(target));

			if (source == null)
				return;

			if (target == source)
				return;

			var invisibleChildren = source.Children.Where(c => !(c is SvgVisualElement)).ToArray();
			var defs = invisibleChildren.FirstOrDefault(ic => ic.ElementName == "defs");
			if (defs != null)
			{
				var docDefs = target.Children.FirstOrDefault(c => c.ElementName == "defs");
				if (docDefs == null)
					target.Children.Add(defs);
				else
				{
					foreach (var defChild in defs.Children)
					{
						var docDefChild = docDefs.Children.FirstOrDefault(c => c.ID == defChild.ID);
						if (docDefChild == null)
						{
							UndoRedoService.ExecuteCommand(new UndoableActionCommand
							(
								"Add def",
								o => docDefs.Children.Add(defChild),
								o => docDefs.Children.Remove(defChild)
							), hasOwnUndoRedoScope: false);
						}
					}
				}
			}
		}

		public Matrix GetCanvasTransformationMatrix()
		{
			var m1 = SvgEngine.Factory.CreateMatrix();
			m1.Translate(Translate.X, Translate.Y);
			m1.Translate(ZoomFocus.X, ZoomFocus.Y);
			m1.Scale(ZoomFactor, ZoomFactor);
			m1.Translate(-ZoomFocus.X, -ZoomFocus.Y);
			return m1;
		}

		public PointF CanvasToScreen(float x, float y)
		{
			return CanvasToScreen(PointF.Create(x, y));
		}

		public PointF CanvasToScreen(PointF canvasPointF)
		{
			var point = canvasPointF.Clone();
			var m = GetCanvasTransformationMatrix();
			m.TransformPoints(new[] {point});
			return point;
		}

		public PointF ScreenToCanvas(float x, float y)
		{
			return ScreenToCanvas(PointF.Create(x, y));
		}

		public PointF ScreenToCanvas(PointF screenPointF)
		{
			var point = screenPointF.Clone();
			var m = GetCanvasTransformationMatrix();
			m.Invert();
			m.TransformPoints(new[] {point});
			return point;
		}

		/// <summary>
		/// Stores the document with a viewbox that surrounds all contained visual elements
		/// then resets the viewbox
		/// </summary>
		/// <param name="stream"></param>
		public void SaveDocumentWithBoundsAsViewbox(Stream stream)
		{
			var oldWidth = Document.Width;
			var oldHeight = Document.Height;
			var oldViewBox = Document.ViewBox;

			try
			{
				var documentSize = Document.CalculateDocumentBounds();
                Document.SetDocumentViewbox(documentSize, Constraints);
				Document.Write(stream);

				FireToolCommandsChanged();
			}
			finally
			{
				Document.ViewBox = oldViewBox;
				Document.Width = oldWidth;
				Document.Height = oldHeight;
			}

			DocumentIsDirty = false;
		}

		/// <summary>
		/// Stores the document with a viewbox that surrounds the current screen capture
		/// then resets the viewbox
		/// </summary>
		/// <param name="stream"></param>
		public void SaveDocumentWithScreenAsViewbox(Stream stream)
		{
			var oldWidth = Document.Width;
			var oldHeight = Document.Height;
			var oldViewBox = Document.ViewBox;
			var minXminY = ScreenToCanvas(0, 0);
			var drawingClip = RectangleF.Create(minXminY, SizeF.Create(ScreenWidth / ZoomFactor, ScreenHeight / ZoomFactor));

			try
			{
                Document.SetDocumentViewbox(drawingClip, Constraints);
				Document.Write(stream);

				FireToolCommandsChanged();
			}
			finally
			{
				Document.ViewBox = oldViewBox;
				Document.Width = oldWidth;
				Document.Height = oldHeight;
			}

			DocumentIsDirty = false;
		}

		public Bitmap CaptureDocumentBitmap(int maxSize = 4096, Color backgroundColor = null)
        {
            return Document.CaptureDocumentBitmap(Constraints, maxSize, backgroundColor);
        }
        public Bitmap CaptureScreenBitmap(Color backgroundColor = null)
		{
			if (ScreenWidth == 0 || ScreenHeight == 0)
				throw new InvalidOperationException(
					$"Cannot capture screen when {nameof(ScreenWidth)} or {nameof(ScreenHeight)} of {GetType().Name} are not initialized.");

			var drawingWidth = (int) Math.Round(Math.Min(ScreenWidth / ZoomFactor, Constraints?.Width ?? float.MaxValue));
			var drawingHeight = (int) Math.Round(Math.Min(ScreenHeight / ZoomFactor, Constraints?.Height ?? float.MaxValue));
			var drawingClip = RectangleF.Create(ScreenToCanvas(0, 0), SizeF.Create(drawingWidth, drawingHeight));
			var bitmap = Bitmap.Create(drawingWidth, drawingHeight);

			return Document.RenderBitmap(bitmap, backgroundColor, drawingClip, Constraints);
		}
		
		public void FireInvalidateCanvas()
		{
			_schedulerProvider.MainScheduer.Schedule(this, (s, st) =>
				{
					CanvasInvalidated?.Invoke(st, EventArgs.Empty);
					return null;
				}
			);
		}

		public void FireToolCommandsChanged()
		{
			ResetToolCommands();

			_propertyChangedSubject.OnNext(nameof(ToolCommands));
			// remain backwards compatible
			ToolCommandsChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			foreach (var tool in Tools)
				tool.Dispose();

			_onGestureToken?.Dispose();
			_document?.Dispose();

            UndoRedoService.CanRedoChanged -= UndoRedoServiceOnCanRedoChanged;
            UndoRedoService.CanUndoChanged -= UndoRedoServiceOnCanRedoChanged;
            UndoRedoService.ActionExecuted -= UndoRedoServiceOnActionExecuted;
        }

		private IList<IToolCommand> EnsureToolSelectors()
		{
			if (_toolSelectors == null)
			{
				_toolSelectors = Tools.Where(t => t.ToolUsage == ToolUsage.Explicit)
					.Select(t => new SelectToolCommand(this, t, t.Name, t.IconName))
					.OrderBy(c => c.Sort)
					.Cast<IToolCommand>()
					.ToList();
			}
			return _toolSelectors;
		}

		private void OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			FireToolCommandsChanged();
		}

		private void OnToolsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_toolSelectors = null;
			FireToolCommandsChanged();
		}

		private void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
		{
			if (oldDocument != null)
				oldDocument.ContentModified -= OnDocumentContentModified;
			if (newDocument != null)
			{
				newDocument.ContentModified -= OnDocumentContentModified;
				newDocument.ContentModified += OnDocumentContentModified;
			}
			DocumentIsDirty = false;

			// fire document changed
			foreach (var tool in Tools)
				tool.OnDocumentChanged(oldDocument, newDocument);

			oldDocument?.Dispose();

			// selection is not valid anymore
			SelectedElements.Clear();

			// reset canvas properties
            Translate = PointF.Create(0, 0);
            ZoomFocus = PointF.Create(0, 0);
            ZoomFactor = 1f;

            // check if the document has a viewBox and set translate and zoom accordingly
            CalculateInitialTransformation = true;

            // re-render
            FireInvalidateCanvas();
		}

		private bool CalculateInitialTransformation { get; set; }

		public bool ZoomOutOnInitialRenderpass { get; set; }

		private void SetInitialTransformation()
		{
			if (!CalculateInitialTransformation) return;


            // check if canvas is pristine.
            // If not, we want to keep the transform and zoom
            var canvasIsPristine = ZoomFactor == 1f
                                   && ZoomFocus.X == 0 && ZoomFocus.Y == 0
                                   && Translate.X == 0 && Translate.Y == 0;

            if (!canvasIsPristine)
            {
                // we need to reset the viewBox for correct rendering afterwards
                if (Document != null)
                    Document.ViewBox = SvgViewBox.Empty;
                CalculateInitialTransformation = false;
                return;
            }

            if (ZoomOutOnInitialRenderpass && Document != null)
            {
                var worldBounds = Document.CalculateDocumentBounds();
                if (worldBounds.IsEmpty)
                {
                    ZoomFactor = 1;
                    ZoomFocus = PointF.Create(0, 0);
                    Translate = PointF.Create(0, 0);
                }
                else
                {
                    ZoomFactor = Math.Min(ScreenWidth / worldBounds.Width,
                        ScreenHeight / worldBounds.Height);
                    ZoomFocus = PointF.Create(0, 0);
                    var offsetX = -worldBounds.Left * ZoomFactor;
                    var marginX = (ScreenWidth - worldBounds.Width * ZoomFactor) / 2;
                    var offsetY = -worldBounds.Top * ZoomFactor;
                    var marginY = (ScreenHeight - worldBounds.Height * ZoomFactor) / 2;
                    Translate = PointF.Create(offsetX + marginX, offsetY + marginY);
                }
            }
            else if (Document is null || Document.ViewBox.Equals(SvgViewBox.Empty))
            {
                Translate = PointF.Create(0f, 0f);
                ZoomFactor = 1f;
            }
            else if(Document != null)
            {

                float scaleX;
                float scaleY;
                float minX;
                float minY;
                Document.ViewBox.CalculateTransform(Document.AspectRatio, ScreenWidth, ScreenHeight,
                    out scaleX, out scaleY, out minX, out minY);

                ZoomFactor = Math.Min(1 / scaleX, 1 / scaleY);
                ZoomFocus = PointF.Empty;
                Translate = PointF.Create(-Document.ViewBox.MinX * ZoomFactor, -Document.ViewBox.MinY * ZoomFactor);

            }

            // we need to reset the viewBox for correct rendering afterwards
            if(Document != null)
                Document.ViewBox = SvgViewBox.Empty;
            CalculateInitialTransformation = false;
        }

		private void OnDocumentContentModified(object sender, SvgElement e)
		{
			if (e is SvgFragment || !(e is SvgVisualElement)) return;

			DocumentIsDirty = true;
		}

		private IEnumerable<IEnumerable<IToolCommand>> GetCommands()
		{
			// prepare tool commands
			var commands = Tools.Select(t => t.Commands.OrderBy(tc => tc.Sort))
				.OrderBy(t => t.FirstOrDefault()?.Sort ?? int.MaxValue)
				.Cast<IEnumerable<IToolCommand>>()
				.ToList();

			// prepare tool selectors
			var toolSelectors = EnsureToolSelectors().OrderBy(s => s.Sort);

			commands.Insert(0, toolSelectors);

			return commands;
		}

		private void ResetToolCommands()
		{
			if (_toolCommands == null)
				_toolCommands = new ObservableCollection<IEnumerable<IToolCommand>>();

			lock (_lockObject)
			{
				_toolCommands.Clear();
				var cmds = GetCommands();
				foreach (var cmd in cmds)
				{
					_toolCommands.Add(cmd);
				}
			}
		}

		private class SelectToolCommand : ToolCommand
		{
			private readonly ISvgDrawingCanvas _canvas;

			public SelectToolCommand(ISvgDrawingCanvas canvas, ITool tool, string name, string iconName)
				: base(tool, name, _ => { }, iconName: iconName)
			{
				if (canvas == null) throw new ArgumentNullException(nameof(canvas));
				_canvas = canvas;
			}

			public override int Sort => -1;

			public override void Execute(object parameter)
			{
				_canvas.ActiveTool = Tool;
				_canvas.FireToolCommandsChanged();
			}

			public override bool CanExecute(object parameter)
			{
				return _canvas.ActiveTool != Tool;
			}

			public override string GroupIconName
			{
				get { return _canvas.ActiveTool?.IconName; }
				set { }
			}

			public override string GroupName
			{
				get { return _canvas.ActiveTool?.Name; }
				set { }
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			System.Diagnostics.Debug.WriteLine($"Propertychanged: {propertyName}");

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public enum ConstraintsMode
	{
		FitUniform,
		FillUniform
	}

}