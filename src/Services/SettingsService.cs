using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SweetTypeTone.Interfaces;
using SweetTypeTone.Models;

namespace SweetTypeTone.Services;

/// <summary>
/// Service for managing application settings
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private AppSettings _currentSettings;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, "SweetTypeTone");
        Directory.CreateDirectory(appDirectory);

        _settingsPath = Path.Combine(appDirectory, "settings.json");
        _currentSettings = new AppSettings();
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                var settings = JsonSerializer.Deserialize(json, AppJsonContext.Default.AppSettings);

                if (settings != null)
                {
                    _currentSettings = settings;
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }

        // Return default settings if load fails
        _currentSettings = new AppSettings();
        await SaveSettingsAsync(_currentSettings);
        return _currentSettings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, AppJsonContext.Default.AppSettings);
            await File.WriteAllTextAsync(_settingsPath, json);
            _currentSettings = settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            throw;
        }
    }

    public AppSettings GetSettings()
    {
        return _currentSettings;
    }

    public async Task UpdateSettingAsync<T>(string key, T value)
    {
        var property = typeof(AppSettings).GetProperty(key);
        if (property != null && property.CanWrite)
        {
            property.SetValue(_currentSettings, value);
            await SaveSettingsAsync(_currentSettings);
        }
    }

    public async Task ResetSettingsAsync()
    {
        _currentSettings = new AppSettings();
        await SaveSettingsAsync(_currentSettings);
    }
}
