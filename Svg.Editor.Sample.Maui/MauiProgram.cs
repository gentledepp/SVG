using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using SkiaSharp.Views.Forms;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Svg.Editor.Forms;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Sample.Maui
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
                    SvgEngine.RegisterSingleton<IToolbarIconSizeProvider>(() => new Views.iOS.TouchToolbarIconSizeProvider());
#else
                    handlers.AddHandler(typeof(SvgCanvasEditorView), typeof(UwpCanvasViewHandlerBase));
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}