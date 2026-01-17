using System.Threading.Tasks;
using SweetTypeTone.Models;

namespace SweetTypeTone.Interfaces;

/// <summary>
/// Service for audio playback
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Initialize the audio service
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Load a sound pack
    /// </summary>
    Task LoadSoundPackAsync(SoundPack soundPack);

    /// <summary>
    /// Play sound for a specific key code
    /// </summary>
    void PlaySound(int keyCode, InputAction action);

    /// <summary>
    /// Set master volume (0.0 to 1.0)
    /// </summary>
    void SetVolume(float volume);

    /// <summary>
    /// Mute or unmute audio
    /// </summary>
    void SetMuted(bool muted);

    /// <summary>
    /// Unload current sound pack
    /// </summary>
    void UnloadSoundPack();

    /// <summary>
    /// Dispose resources
    /// </summary>
    void Dispose();
}
