using System.Threading.Tasks;

namespace Svg.Editor.Interfaces;

public interface IToolTipInfoService
{
    public Task ShowToolTip(string text);

    public void CloseToolTip();
}