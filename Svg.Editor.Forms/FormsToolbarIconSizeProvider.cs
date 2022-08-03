using Svg.Editor.Interfaces;
using Svg.Interfaces;
using SizeF = Svg.Interfaces.SizeF;

namespace Svg.Editor.Forms
{
    public class FormsToolBarIconSizeProvider : IToolbarIconSizeProvider
    {
        public SizeF GetSize()
        {
            var current = DeviceInfo.Current.Platform;

            if(current == DevicePlatform.iOS)
                return SizeF.Create(32, 32);

            if (current == DevicePlatform.Android)
                return SizeF.Create(32, 32);

            return SizeF.Create(32, 32);
        }
    }
}
