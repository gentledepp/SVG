using System.Threading;

namespace Svg.Editor.Avalon.Forms;

public class SvgEditorForms
{
    private static bool _initialized;
    private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

    public static void Init()
    {
        if (_initialized) return;
        Lock.Wait();
        try
        {
            if (_initialized) return;
            SvgPlatform.Init();
            SvgEditor.Init();
            _initialized = true;
        }
        finally
        {
            Lock.Release();
        }
    }
}