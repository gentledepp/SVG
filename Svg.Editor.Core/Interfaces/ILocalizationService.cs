using System.Globalization;

namespace Svg.Editor.Interfaces;

public interface ILocalizationService
{
    public string GetString(string key, CultureInfo culture = null);
}