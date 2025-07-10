using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Plugins.Email;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Sample.Core.Tools
{
    public class SaveTool : ToolBase
    {
        private readonly bool _autoLoad;
        private readonly Func<string> _fileName;

        public SaveTool(bool autoLoad = true, Func<string> fileName = null ) : base("Save/Load")
        {
            _autoLoad = autoLoad;
            _fileName = fileName ?? (() => "svg_image.svg");
            IconName = "ic_save_white_48dp.png";
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
                        ws.SaveDocumentWithBoundsAsViewbox(stream);
                    }

                }, 
                (obj) => ws.Document != null),
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
                new ToolCommand(this, "Share SVG", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();

                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());

                    if (fs.FileExists(storagePath))
                    {
                        using (var stream = fs.OpenRead(storagePath))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/svg+xml", FileName = "svg_file.svg"} });
                        }
                    }

                }, 
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    
                    //using (var bmp = ws.GetOrCreate(ws.ScreenWidth, ws.ScreenHeight))
                    using (var bmp = ws.Document.DrawAllContents(SvgEngine.Factory.Colors.White)) // 2MP (see https://de.wikipedia.org/wiki/Bildaufl%C3%B6sungen_in_der_Digitalfotografie)
                    {
                        if (bmp.Width == 0 || bmp.Height == 0)
                        {
                            // TODO: notify user that she cannot save an empty document
                            return;
                        }

                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDefaultStoragePath(), "svg_image.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            bmp.SavePng(stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return Canvas.Document.Children.OfType<SvgVisualElement>().Any() && fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG thumb", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    
                    using (var bmp = ws.Document.DrawAllContents(160, SvgEngine.Factory.Colors.White))
                    {
                        if (bmp.Width == 0 || bmp.Height == 0)
                        {
                            // TODO: notify user that she cannot save an empty document
                            return;
                        }

                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDefaultStoragePath(), "svg_image_thumb.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            bmp.SavePng(stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return Canvas.Document.Children.OfType<SvgVisualElement>().Any() && fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG XL", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();

                    using (var bmp = ws.Document.DrawAllContents(2048, SvgEngine.Factory.Colors.White))
                    {
                        if (bmp.Width == 0 || bmp.Height == 0)
                        {
                            // TODO: notify user that she cannot save an empty document
                            return;
                        }

                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDefaultStoragePath(), "svg_image_XL.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            bmp.SavePng(stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return Canvas.Document.Children.OfType<SvgVisualElement>().Any() && fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG Screen", (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();

                    using (var bmp = ws.CaptureScreenBitmap())
                    {
                        if (bmp.Width == 0 || bmp.Height == 0)
                        {
                            // TODO: notify user that she cannot save an empty document
                            return;
                        }

                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDefaultStoragePath(), "svg_image_screen.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            bmp.SavePng(stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png" } });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return Canvas.Document.Children.OfType<SvgVisualElement>().Any() && fs.FileExists(storagePath);
                }),
                new ToolCommand(this, "Share PNG Constraints", obj =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();

                    using (var bmp = ws.CaptureDocumentBitmap())
                    {
                        if (bmp.Width == 0 || bmp.Height == 0)
                        {
                            // TODO: notify user that she cannot save an empty document
                            return;
                        }

                        // now save it as PNG
                        var path = fs.PathCombine(fs.GetDefaultStoragePath(), "svg_image_constraints.png");
                        if (fs.FileExists(path))
                            fs.DeleteFile(path);

                        using (var stream = fs.OpenWrite(path))
                        {
                            bmp.SavePng(stream);
                        }

                        // then share it using MVVMCross plugin
                        using(var stream = fs.OpenRead(path))
                        {
                            var share = Mvx.Resolve<IMvxComposeEmailTaskEx>();
                            share.ComposeEmail(new [] {"someone@somewhere.com"} , subject:$"SVG {DateTime.Now.ToString()}", attachments: new [] {new EmailAttachment {Content=stream, ContentType = "image/png", FileName = "svg_file.png"} });
                        }
                    }
                },
                (obj) =>
                {
                    var fs = SvgEngine.Resolve<IFileSystem>();
                    var path = fs.GetDefaultStoragePath();
                    var storagePath = fs.PathCombine(path, _fileName());
                    return Canvas.Document.Children.OfType<SvgVisualElement>().Any() && fs.FileExists(storagePath);
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
