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
        private readonly ILocalizationService _localizationService;
        private readonly IUserInteraction _userInteractionService;

        
        public MarkerOptionsInputService()
        {
            _localizationService = SvgEngine.Resolve<ILocalizationService>();
            _userInteractionService = SvgEngine.Resolve<IUserInteraction>();
        }

        public async Task<int[]> GetUserInput(IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> markerEndOptions,
            int markerEndSelected)
        {

            var mso = markerStartOptions.ToList();
            var start = await _userInteractionService.ActionSheetAsync(_localizationService.GetString("Svg.Editor.Marker.Options.Start"), mso.ToArray(), cancelButton: _localizationService.GetString("Svg.Editor.Global.Cancel"), cancellable: true);


            var startIndex = mso.IndexOf(start);
            if (start == null || startIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };

            var meo = markerEndOptions.ToList();
            var end = await _userInteractionService.ActionSheetAsync(_localizationService.GetString("Svg.Editor.Marker.Options.End"), meo.ToArray(), cancelButton: _localizationService.GetString("Svg.Editor.Global.Cancel"), cancellable: true);

            var endIndex = meo.IndexOf(end);
            if (end == null || endIndex < 0)
                return new[] { markerStartSelected, markerEndSelected };


            return new[] { startIndex, endIndex };
        }
    }
}
