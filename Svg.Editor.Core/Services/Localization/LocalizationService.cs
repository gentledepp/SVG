using System;
using System.Globalization;
using System.Resources;
using Avalonia;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Services.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;

    public LocalizationService()
    {
        _resourceManager = new ResourceManager("Svg.Editor.Services.Localization.Resources.String",
            typeof(LocalizationService).Assembly);
    }
    
    public string GetString(string key, CultureInfo culture = null)
    {
        if (key == null)
            throw new ArgumentException("key cannot be null!");
        
        culture ??= CultureInfo.CurrentCulture;
        try
        {
            string value = _resourceManager.GetString(key, culture);
            return string.IsNullOrEmpty(value) ? $"[{key}, {culture.DisplayName}]" : value;
        }
        catch (Exception ex)
        { 
            return $"[{key}, {culture.DisplayName}]";
        }
    }
}