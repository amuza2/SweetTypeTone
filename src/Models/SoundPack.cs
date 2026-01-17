using System;
using System.Collections.Generic;

namespace SweetTypeTone.Models;

/// <summary>
/// Represents a sound pack configuration
/// </summary>
public class SoundPack
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string FolderPath { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public bool IsSupported { get; set; } = true;
    public string? UnsupportedReason { get; set; }
    public SoundPackType Type { get; set; } = SoundPackType.Keyboard;
    public Dictionary<int, SoundDefinition> KeyDefinitions { get; set; } = new();
    /// <summary>
    /// Default sounds to play when a key doesn't have a specific definition (one is randomly selected)
    /// </summary>
    public List<string> DefaultSoundPaths { get; set; } = new();
    /// <summary>
    /// Default key-up sounds (one is randomly selected)
    /// </summary>
    public List<string> DefaultUpSoundPaths { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Defines a sound for a specific key or mouse button
/// </summary>
public class SoundDefinition
{
    public int KeyCode { get; set; }
    public string DownSoundPath { get; set; } = string.Empty;
    public string? UpSoundPath { get; set; }
    public float Volume { get; set; } = 1.0f;
    public int? SpriteStart { get; set; } // For sprite-based sounds (milliseconds)
    public int? SpriteDuration { get; set; } // For sprite-based sounds (milliseconds)
}

/// <summary>
/// Type of sound pack
/// </summary>
public enum SoundPackType
{
    Keyboard,
    Mouse,
    Both
}

/// <summary>
/// Legacy Mechvibes config format for compatibility
/// </summary>
public class MechvibesConfig
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? key_define_type { get; set; }
    public bool includes_numpad { get; set; }
    public string? sound { get; set; }
    public string? soundup { get; set; }
    public Dictionary<string, object>? defines { get; set; }
    public int? version { get; set; }
}
