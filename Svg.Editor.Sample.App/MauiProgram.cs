using Microsoft.Maui.Controls.Compatibility.Hosting;
using SkiaSharp.Views.Forms;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SkiaSharp.Views.Maui.Handlers;
using Svg.Editor.Forms;
using Svg.Editor.Samples.Forms;
#if ANDROID
using SkiaSharp.Views.Android;
using SkiaSharp.Views.Forms;
#elif IOS
#else
using SkiaSharp.Views.Forms;
#endif

namespace Svg.Editor.Sample.App
{

    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            SvgEditorForms.Init();
            builder
                .UseMauiApp<Samples.Forms.App>()
                .UseMauiCompatibility()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    handlers.AddHandler(typeof(SvgCanvasEditorView), typeof(DroidCanvasViewHandlerBase));
#elif IOS
                    handlers.AddHandler(typeof(SvgCanvasEditorView), typeof(TouchCanvasViewHandlerBase));
#else
                    handlers.AddHandler(typeof(SvgCanvasEditorView), typeof(UwpCanvasViewHandlerBase));
#endif
                });

            return builder.Build();
        }
    }
}