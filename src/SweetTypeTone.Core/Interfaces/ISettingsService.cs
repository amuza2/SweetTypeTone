using SweetTypeTone.Core.Models;

namespace SweetTypeTone.Core.Interfaces;

/// <summary>
/// Service for managing application settings
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Load settings from storage
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// Save settings to storage
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// Get current settings
    /// </summary>
    AppSettings GetSettings();

    /// <summary>
    /// Update a specific setting
    /// </summary>
    Task UpdateSettingAsync<T>(string key, T value);

    /// <summary>
    /// Reset settings to default
    /// </summary>
    Task ResetSettingsAsync();
}
