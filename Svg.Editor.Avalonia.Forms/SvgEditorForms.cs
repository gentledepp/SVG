using Avalonia.Controls.Templates;
using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Avalon.Forms.Services;
using Svg.Editor.Avalon.Forms.ToolBar;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using System.Threading;

namespace Svg.Editor.Avalon.Forms;

public class SvgEditorForms
{
    private static bool _initialized;
    private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

    public static void Init(Avalonia.Application app)
    {

        if (_initialized) return;
        Lock.Wait();
        try
        {
            if (_initialized) return;
            SvgPlatform.Init();
            SvgEditor.Init();

            SvgEngine.Register<IColorInputService>(() => new ColorInputService());
            SvgEngine.Register<IMarkerOptionsInputService>(() => new MarkerOptionsInputService());
            SvgEngine.Register<IStrokeStyleOptionsInputService>(() => new StrokeStyleOptionsInputService());
            SvgEngine.Register<ITextInputService>(() => new TextInputService());
            SvgEngine.Register<IPickImageService>(() => new FormsPickImageService());
            SvgEngine.Register<IPinInputService>(() => new PinInputService());
            SvgEngine.Register<IToolTipInfoService>(() => new ToolTipInfoService());
            SvgEngine.RegisterSingleton<IColorPickerState>(() => new ColorPickerState());
            SvgEngine.Register<IUserInteraction>(() => new UserInteractionService());

            app.DataTemplates.Add(new MenuItemHeaderTemplate());

            _initialized = true;
        }
        finally
        {
            Lock.Release();
        }
    }
}