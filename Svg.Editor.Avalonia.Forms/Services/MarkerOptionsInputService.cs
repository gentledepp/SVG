using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class MarkerOptionsInputService : IMarkerOptionsInputService
    {
        public async Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> markerEndOptions,
            int markerEndSelected)
        {


            var mso = markerStartOptions.ToList();
            var start = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(title, mso.ToArray());

            await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(title, mso.ToArray());

            var startIndex = mso.IndexOf(start);
            if (start == null || startIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };

            var meo = markerEndOptions.ToList();
            var end = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(title, mso.ToArray());

            var endIndex = meo.IndexOf(end);
            if (end == null || endIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };


            return new[] { startIndex, endIndex };
        }
    }
}
