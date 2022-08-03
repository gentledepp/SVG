using Microsoft.Maui.Controls.Compatibility.Hosting;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
//using Svg.Editor.Forms.UWP;
#if ANDROID
//using Svg.Editor.Forms.Droid;
#endif

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                   // handlers.AddHandler(typeof(SvgCanvasEditorView), typeof(DroidCanvasEditorViewRenderer));
#elif IOS
                    handlers.AddCompatibilityRenderer(typeof(SvgCanvasEditorView), typeof(TouchCanvasEditorViewRenderer));
#else
                    //handlers.AddCompatibilityRenderer(typeof(SvgCanvasEditorView), typeof(UwpCanvasEditorViewRenderer));
#endif
                });

            return builder.Build();
        }
    }
}