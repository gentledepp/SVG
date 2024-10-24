using System;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Sample.Forms.Tools
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
                (obj) => ws.Document != null, sortFunc: (x)=>1),
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
                }),
                new ToolCommand(this, "Clear", (obj) =>
                {
                    ws.Document = new SvgDocument();
                })

            };

            if (_autoLoad)
            {
                Commands.Single(t => t.Name == "Load").Execute(null);
            }
        }
    }
}
