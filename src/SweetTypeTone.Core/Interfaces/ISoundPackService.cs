using SweetTypeTone.Core.Models;

namespace SweetTypeTone.Core.Interfaces;

/// <summary>
/// Service for managing sound packs
/// </summary>
public interface ISoundPackService
{
    /// <summary>
    /// Get all available sound packs
    /// </summary>
    Task<List<SoundPack>> GetAllSoundPacksAsync();

    /// <summary>
    /// Get a specific sound pack by ID
    /// </summary>
    Task<SoundPack?> GetSoundPackByIdAsync(string id);

    /// <summary>
    /// Import a Mechvibes sound pack
    /// </summary>
    Task<SoundPack> ImportMechvibesPackAsync(string path);

    /// <summary>
    /// Create a new sound pack
    /// </summary>
    Task<SoundPack> CreateSoundPackAsync(SoundPack soundPack);

    /// <summary>
    /// Update an existing sound pack
    /// </summary>
    Task UpdateSoundPackAsync(SoundPack soundPack);

    /// <summary>
    /// Delete a sound pack
    /// </summary>
    Task DeleteSoundPackAsync(string id);

    /// <summary>
    /// Export a sound pack
    /// </summary>
    Task ExportSoundPackAsync(string id, string destinationPath);

    /// <summary>
    /// Scan for sound packs in directories
    /// </summary>
    Task ScanForSoundPacksAsync();

    /// <summary>
    /// Refresh sound packs by clearing cache and rescanning
    /// </summary>
    Task RefreshSoundPacksAsync();
}
