using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Events;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Tools
{
    public abstract class ToolBase : ITool, ISerializableTool
    {
        protected ToolBase(string name) : this(name,null)
        {
        }

        protected ToolBase(string name, IDictionary<string, object> properties)
        {
	        Name = name;
			Properties = properties ?? new Dictionary<string, object>();
        }

        protected ISvgDrawingCanvas Canvas { get; private set; }

        #region Custom attributes

        /// <summary>
        /// Add this to mark the element as background, making it immutable and stay in the background.
        /// </summary>
        public const string BackgroundCustomAttributeKey = "iclbackground";
        /// <summary>
        /// Add this to disable snapping operations by <see cref="GridTool"/> for this element.
        /// </summary>
        public const string NoSnappingConstraint = "snapping";
        /// <summary>
        /// Add this to disable the <see cref="ColorTool"/> to fill this element.
        /// </summary>
        public const string NoFillConstraint = "fill";
        /// <summary>
        /// Add this to disable the <see cref="ColorTool"/> to set the stroke for this element.
        /// </summary>
        public const string NoStrokeConstraint = "stroke";
        /// <summary>
        /// Add this to disable text editing for this element.
        /// </summary>
        public const string ImmutableTextConstraint = "text";
        /// <summary>
        /// This attribute is place on an element to determine the constraints for editing tools.
        /// Possible values would be:
        /// <list type="bullet">
        /// <item>text: <see cref="ImmutableTextConstraint"/></item>
        /// <item>font-size</item>
        /// <item>fill: <see cref="NoFillConstraint"/></item>
        /// <item>stroke: <see cref="NoStrokeConstraint"/></item>
        /// <item>snapping: <see cref="NoSnappingConstraint"/></item>
        /// </list>
        /// <remarks>The values are treated as opt-out.</remarks>
        /// </summary>
        public const string ConstraintsCustomAttributeKey = "iclconstraints";
        
        #endregion

        public string Name { get; protected set; }
        public ToolUsage ToolUsage { get; protected set; }
        public ToolType ToolType { get; protected set; }
        public virtual bool IsActive { get; set; } = true;
        public IEnumerable<IToolCommand> Commands { get; protected set; } = Enumerable.Empty<IToolCommand>();
        /// <summary>
        /// Properties for the tool which can be configured in the designer. Key should be lower-case for consistency.
        /// </summary>
        public IDictionary<string, object> Properties { get; }

        public virtual int DrawOrder => 500;
        public virtual int PreDrawOrder => 500;
        public virtual int InputOrder => 500;
        public virtual int GestureOrder => 500;

        /// <summary>
        /// States if the <see cref="OnDrag"/> method should receive <see cref="DragGesture.Enter"/> gestures.
        /// </summary>
        protected bool HandleDragEnter { get; set; }
        /// <summary>
        /// States if the <see cref="OnDrag"/> method should receive <see cref="DragGesture.Exit"/> gestures.
        /// </summary>
        protected bool HandleDragExit { get; set; }

        public string IconName { get; set; }

        /// <summary>
        /// Initializes this tool with a canvas. The <see cref="Canvas"/> property will be set. Call this at first when overriding!
        /// </summary>
        /// <param name="ws">The canvas that this tool should reference.</param>
        /// <returns></returns>
        public virtual Task Initialize(ISvgDrawingCanvas ws)
        {
            Canvas = ws;

            return Task.FromResult(true);
        }

        public virtual Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnPreDraw(IRenderer renderer, ISvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnUserInput(UserInputEvent @event, ISvgDrawingCanvas ws)
        {
            return Task.FromResult(true);
        }

        public virtual async Task OnGesture(UserGesture gesture)
        {
            switch (gesture.Type)
            {
                case GestureType.Tap:
                    await OnTap((TapGesture) gesture);
                    break;
                case GestureType.DoubleTap:
                    await OnDoubleTap((DoubleTapGesture) gesture);
                    break;
                case GestureType.LongPress:
                    await OnLongPress((LongPressGesture) gesture);
                    break;
                case GestureType.Drag:
                    var drag = (DragGesture) gesture;
                    if (drag.State == DragState.Enter && !HandleDragEnter) return;
                    if (drag.State == DragState.Exit && !HandleDragExit) return;
                    await OnDrag((DragGesture) gesture);
                    break;
            }
        }

        protected virtual Task OnTap(TapGesture tap)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnDoubleTap(DoubleTapGesture doubleTap)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnLongPress(LongPressGesture longPress)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnDrag(DragGesture drag)
        {
            return Task.FromResult(true);
        }

        public virtual void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {

        }
        public virtual void Reset()
        {
        }
        public virtual void Dispose()
        {
        }

	    public virtual bool CanSerialize => true;
	    public virtual string SerializationId => Name;
		public virtual IDictionary<string, object> GetPropertiesForSerialization()
        {
            Properties["isselectedtool"] = IsActive && ToolUsage == ToolUsage.Explicit;
            return CanSerialize ? Properties : new Dictionary<string, object>();
	    }

	    public bool DeserializeProperties(IDictionary<string, object> serializedProperties)
	    {
		    if (!CanSerialize)
			    return false;

		    foreach (var property in serializedProperties)
			    Properties[property.Key] = property.Value;
		    return true;
	    }
    }
}
