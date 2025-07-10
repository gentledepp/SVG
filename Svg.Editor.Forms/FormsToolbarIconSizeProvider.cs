using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class FormsToolBarIconSizeProvider : IToolbarIconSizeProvider
    {
        public SizeF GetSize()
        {
            switch (Device.RuntimePlatform)
            {

                case Device.iOS:
                    return SizeF.Create(32, 32);
                case Device.Android:
                    return SizeF.Create(32, 32);
                default:
                    break;
            }
            return SizeF.Create(32, 32);
        }
    }
}
