using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Svg.Editor.Events;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Tools
{
    public enum ToolUsage
    {
        Implicit,
        Explicit
    }

    public enum ToolType
    {
        Undefined = 0x00,
        Select = 0x01,
        Create = 0x10,
        Modify = 0x0100,
        View = 0x1000
    }

	public interface ISerializableTool
	{
		/// <summary>
		/// Gets a value indicating whether this tool can serialize.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can serialize; otherwise, <c>false</c>.
		/// </value>
		bool CanSerialize { get; }

		/// <summary>
		/// Gets the serialization identifier for the tool. Should include or be the tool name.
		/// </summary>
		/// <value>
		/// The serialization identifier.
		/// </value>
		string SerializationId { get; }

		/// <summary>
		/// Gets the properties for serialization, removing all properties that cannot be serialized.
		/// </summary>
		/// <returns></returns>
		IDictionary<string, object> GetPropertiesForSerialization();

		/// <summary>
		/// Deserializes the properties.
		/// </summary>
		/// <param name="serializedProperties">The serialized properties.</param>
		/// <returns></returns>
		bool DeserializeProperties(IDictionary<string, object> serializedProperties);
	}

	public interface ITool : IDisposable
	{
		public ILocalizationService LocalizationService { get; }
        string Name { get; }
        ToolUsage ToolUsage { get; }
        /// <summary>
        /// Provides information about the type of action that this <see cref="ITool"/> is performing.
        /// </summary>
        ToolType ToolType { get; }
        bool IsActive { get; set; }
        IEnumerable<IToolCommand> Commands { get; }
        /// <summary>
        /// Properties for the <see cref="ITool"/> that can be configured in the designer. Key should be lower-case for consistency.
        /// </summary>
        IDictionary<string, object> Properties { get; }
        /// <summary>
        /// Defines the order (ascending) in which this <see cref="ITool"/>s <see cref="OnDraw"/> method should be called.
        /// </summary>
        int DrawOrder { get; }
        /// <summary>
        /// Defines the order (ascending) in which this <see cref="ITool"/>s <see cref="OnPreDraw"/> method should be called.
        /// </summary>
        int PreDrawOrder { get; }
        /// <summary>
        /// Defines the order (ascending) in which this <see cref="ITool"/>s <see cref="OnUserInput"/> method should be called.
        /// </summary>
        int InputOrder { get; }
        /// <summary>
        /// Defines the order (ascending) in which this <see cref="ITool"/>s <see cref="OnGesture"/> method should be called.
        /// </summary>
        int GestureOrder { get; }
        string IconName { get; }
	    Task Initialize(ISvgDrawingCanvas ws);
	    Task OnDraw(IRenderer renderer, ISvgDrawingCanvas ws);
	    Task OnPreDraw(IRenderer renderer, ISvgDrawingCanvas ws);
	    Task OnUserInput(UserInputEvent @event, ISvgDrawingCanvas ws);
	    Task OnGesture(UserGesture gesture);
	    void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument);
	    void Reset();
    }

    public interface IToolCommand : ICommand
    {
        string Name { get; }
        string Description { get; }
        ITool Tool { get; }
        string IconName { get; }
        int Sort { get; }
        string GroupName { get; }
        string GroupIconName { get; }
    }

    public class ToolCommand : IToolCommand
    {
        private const int DefaultSortValue = 1000;
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly Func<IToolCommand, int> _sortFunc;
        private string _groupName;
        private string _groupIcon;
        private string _name;
        private string _description;
        private string _iconName;

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public ITool Tool { get; }

        public virtual string IconName
        {
            get { return _iconName; }
            set { _iconName = value; }
        }

        public virtual int Sort => _sortFunc?.Invoke(this) ?? DefaultSortValue;

        public virtual string GroupName
        {
            get { return _groupName ?? Tool.Name; }
            set { _groupName = value; }
        }

        public virtual string GroupIconName
        {
            get { return _groupIcon ?? Tool.IconName; }
            set { _groupIcon = value; }
        }

        public ToolCommand(ITool tool, string name, Action<object> execute, Func<object, bool> canExecute = null, string description = null, string iconName = null, Func<IToolCommand, int> sortFunc = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
            _sortFunc = sortFunc;
            Tool = tool;
            _name = name;
            _description = description;
            _iconName = iconName;
        }

        public virtual bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public virtual void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}