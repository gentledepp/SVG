using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Sample.Core.Tools
{
    public class AuxiliaryLineTool : ToolBase
    {
        private bool _showAuxiliaryLines = true;

        public bool ShowAuxiliaryLines
        {
            get { return _showAuxiliaryLines; }
            set
            {
                _showAuxiliaryLines = value;
                if (Canvas != null)
                {
                    ShowHideAuxiliaryLines(Canvas.Document);
                }
            }
        }

        public AuxiliaryLineTool() : base("Auxiliaryline")
        {
            ToolType = ToolType.View;
        }

        public AuxiliaryLineTool(IDictionary<string, object> properties) : base("Auxiliaryline", properties)
        {
        }

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Toogle auxiliary lines", (obj) =>
                {
                    ShowAuxiliaryLines = !ShowAuxiliaryLines;
                    Canvas.FireInvalidateCanvas();
                }, iconName:"ic_code_white_48dp.png", sortFunc: (obj) => 1000)
            };
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            UnWatchDocument(oldDocument);
            WatchDocument(newDocument);
            ShowHideAuxiliaryLines(newDocument);
        }

        private void WatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;
            document.ChildAdded += OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Subscribe(child);
            }
        }

        private void UnWatchDocument(SvgDocument document)
        {
            if (document == null)
                return;

            document.ChildAdded -= OnChildAdded;

            foreach (var child in document.Children.OfType<SvgVisualElement>())
            {
                Unsubscribe(child);
            }
        }

        private void Subscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildAdded += OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
            child.ChildRemoved += OnChildRemoved;
        }

        private void Unsubscribe(SvgElement child)
        {
            if (!(child is SvgVisualElement))
                return;

            child.ChildAdded -= OnChildAdded;
            child.ChildRemoved -= OnChildRemoved;
        }

        private void OnChildAdded(object sender, ChildAddedEventArgs e)
        {
            Subscribe(e.NewChild);
            ShowHideAuxiliaryLines(e.NewChild);
        }

        private void OnChildRemoved(object sender, ChildRemovedEventArgs e)
        {
            Unsubscribe(e.RemovedChild);
        }

        private void ShowHideAuxiliaryLines(SvgElement element)
        {
            var d = element as SvgDocument;
            if (d != null)
            {
                foreach (var vc in d.Children.OfType<SvgVisualElement>())
                {
                    ShowHideAuxiliaryLines(vc);
                }
            }
            else
            {
                var ve = element as SvgVisualElement;
                if (ve == null)
                    return;

                var isAuxLine = ve.CustomAttributes.ContainsKey("iclhelpline");
                if (isAuxLine)
                {
                    ve.Visible = ShowAuxiliaryLines;
                }
                foreach (var vc in ve.Children.OfType<SvgVisualElement>())
                {
                    ShowHideAuxiliaryLines(vc);
                }
            }
        }
    }
}
