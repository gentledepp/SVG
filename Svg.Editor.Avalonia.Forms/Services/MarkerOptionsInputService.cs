using Svg.Editor.Avalon.Forms.Dialog;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class MarkerOptionsInputService : IMarkerOptionsInputService
    {
        private ILocalizationService _localizationService;

        public MarkerOptionsInputService()
        {
            _localizationService = SvgEngine.Resolve<ILocalizationService>();
        }

        public async Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> markerEndOptions,
            int markerEndSelected)
        {

            var mso = markerStartOptions.ToList();
            var start = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(_localizationService.GetString("Marker.Options.Start"), mso.ToArray(), cancelButton: "Cancel", cancellable: true);


            var startIndex = mso.IndexOf(start);
            if (start == null || startIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };

            var meo = markerEndOptions.ToList();
            var end = await UserInteractionServiceExt.UserInteractionInst.ActionSheetAsync(_localizationService.GetString("Marker.Options.End"), meo.ToArray(), cancelButton: "Cancel", cancellable: true);

            var endIndex = meo.IndexOf(end);
            if (end == null || endIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };


            return new[] { startIndex, endIndex };
        }
    }
}
