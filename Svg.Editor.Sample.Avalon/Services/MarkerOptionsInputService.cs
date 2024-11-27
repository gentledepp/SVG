using Svg.Editor.Tools;
using Xamarin.Forms;

namespace Svg.Editor.Sample.Avalon.Services
{
    public class MarkerOptionsInputService : IMarkerOptionsInputService
    {
        public async Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> markerEndOptions,
            int markerEndSelected)
        {
            var mso = markerStartOptions.ToList();
            var start = await Application.Current.MainPage.DisplayActionSheet("Start", "cancel", null, mso.ToArray());

            var startIndex = mso.IndexOf(start);
            if (start == null || startIndex < 0)
                return new [] {markerStartSelected, markerEndSelected};

            var meo = markerEndOptions.ToList();
            var end = await Application.Current.MainPage.DisplayActionSheet("End", "cancel", null, meo.ToArray());

            var endIndex = meo.IndexOf(end);
            if (end == null || endIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };
            

            return new [] { startIndex, endIndex };
        }
    }
}
