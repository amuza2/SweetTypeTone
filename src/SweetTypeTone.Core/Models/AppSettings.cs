namespace SweetTypeTone.Core.Models;

/// <summary>
/// Application settings
/// </summary>
public class AppSettings
{
    public string CurrentSoundPackId { get; set; } = string.Empty;
    public float MasterVolume { get; set; } = 0.5f;
    public bool IsMuted { get; set; }
    public bool StartMinimized { get; set; }
    public bool StartWithSystem { get; set; }
    public bool ShowTrayIcon { get; set; } = true;
    public bool EnableKeyboardSounds { get; set; } = true;
    public bool EnableMouseSounds { get; set; }
    public bool ActiveVolumeAdjustment { get; set; } = true;
    public ThemeMode Theme { get; set; } = ThemeMode.Dark;
    public string CustomSoundPacksPath { get; set; } = string.Empty;
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
}

public enum ThemeMode
{
    Light,
    Dark,
    System
}
