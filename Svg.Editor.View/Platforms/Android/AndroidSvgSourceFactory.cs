using Android.Content.Res;
using Svg;
using Svg.Editor.Droid.Services;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Svg.Platform;

[assembly: SvgService(typeof(ISvgSourceFactory), typeof(AndroidSvgSourceFactory))]

namespace Svg.Editor.Droid.Services
{
    public class AndroidSvgSourceFactory : ISvgSourceFactory
    {
        private readonly AssetManager _assets;

        public AndroidSvgSourceFactory()
        {
            _assets = Android.App.Application.Context.Assets;
        }

        public ISvgSource Create(string path)
        {
            return new SvgAssetSource(path, _assets);
        }
    }
}