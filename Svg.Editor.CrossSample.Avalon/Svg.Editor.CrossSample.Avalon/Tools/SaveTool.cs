using System;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.CrossSample.Avalon.Tools
{
    public class SaveTool : ToolBase
    {
        private readonly bool _autoLoad;
        private readonly Func<string> _fileName;

        public SaveTool(bool autoLoad = true, Func<string> fileName = null ) : base("Save/Load")
        {
            _autoLoad = autoLoad;
            _fileName = fileName ?? (() => "svg_image.svg");
            IconName = "ic_save.svg";
        }

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new[]
            {
                new ToolCommand(this, "Save", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                        fs.DeleteFile(storagePath);
                    }

                    using (var stream = fs.OpenWrite(storagePath))
                    {
                        ws.SaveDocumentWithScreenAsViewbox(stream);
                    }

                }, 
                (obj) => ws.Document != null, sortFunc: (x)=>1, description: "Click to save your SVG file and preserve all changes made to your design."),
                new ToolCommand(this, "Load", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                       ws.Document = SvgDocument.Open<SvgDocument>(storagePath);
                    }
                    ws.FireToolCommandsChanged();

                }, 
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }, description: "Click to load last saved SVG file and continue editing your design."),
                new ToolCommand(this, "Clear", (obj) =>
                {
                    ws.Document = new SvgDocument();
                }, description: "Click to clear the canvas and remove all current design elements.")

            };

            if (_autoLoad)
            {
                Commands.Single(t => t.Name == "Load").Execute(null);
            }
        }
    }
}
