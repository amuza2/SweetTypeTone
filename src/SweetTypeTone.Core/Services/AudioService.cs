using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NVorbis;
using SweetTypeTone.Core.Interfaces;
using SweetTypeTone.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SweetTypeTone.Core.Services;

/// <summary>
/// Audio service implementation using NAudio
/// </summary>
public class AudioService : IAudioService
{
    private readonly ConcurrentDictionary<int, CachedSound> _soundCache = new();
    private readonly ConcurrentDictionary<int, CachedSound> _soundUpCache = new();
    private IWavePlayer? _wavePlayer;
    private MixingSampleProvider? _mixer;
    private float _volume = 0.5f;
    private bool _isMuted;
    private SoundPack? _currentPack;
    private bool _disposed;

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                // Try to use ALSA on Linux, fallback to WaveOut on other platforms
                if (OperatingSystem.IsLinux())
                {
                    Console.WriteLine("Initializing ALSA audio output for Linux...");
                    // For now, we'll use a simple approach - create player on demand
                    // NAudio's WaveOutEvent doesn't work on Linux, we need a different approach
                }
                else
                {
                    _wavePlayer = new WaveOutEvent();
                }
                
                _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
                {
                    ReadFully = true
                };
                
                if (_wavePlayer != null)
                {
                    _wavePlayer.Init(_mixer);
                    _wavePlayer.Play();
                }
                
                Console.WriteLine("Audio service initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Audio initialization failed: {ex.Message}");
                Console.WriteLine("Audio playback may not work, but the app will continue...");
                // Don't throw - allow the app to continue without audio for now
            }
        });
    }

    public async Task LoadSoundPackAsync(SoundPack soundPack)
    {
        UnloadSoundPack();
        _currentPack = soundPack;

        await Task.Run(() =>
        {
            Console.WriteLine($"Loading sound pack: {soundPack.Name}");
            Console.WriteLine($"  Keys to load: {soundPack.KeyDefinitions.Count}");
            
            // For sprite-based packs, we'll skip loading for now
            // TODO: Implement sprite extraction or on-demand playback
            var hasSpriteDefinitions = soundPack.KeyDefinitions.Values.Any(d => d.SpriteStart.HasValue);
            
            if (hasSpriteDefinitions)
            {
                Console.WriteLine("  ⚠ Sprite-based sound pack detected - audio playback not yet implemented");
                return;
            }
            
            foreach (var kvp in soundPack.KeyDefinitions)
            {
                try
                {
                    // Load key down sound
                    if (!string.IsNullOrEmpty(kvp.Value.DownSoundPath))
                    {
                        var fullPath = Path.Combine(soundPack.FolderPath, kvp.Value.DownSoundPath);
                        if (File.Exists(fullPath))
                        {
                            var sound = new CachedSound(fullPath);
                            _soundCache[kvp.Key] = sound;
                        }
                    }

                    // Load key up sound if exists
                    if (!string.IsNullOrEmpty(kvp.Value.UpSoundPath))
                    {
                        var fullPath = Path.Combine(soundPack.FolderPath, kvp.Value.UpSoundPath);
                        if (File.Exists(fullPath))
                        {
                            var sound = new CachedSound(fullPath);
                            _soundUpCache[kvp.Key] = sound;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error loading sound for key {kvp.Key}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"  ✓ Loaded {_soundCache.Count} key down sounds");
            Console.WriteLine($"  ✓ Loaded {_soundUpCache.Count} key up sounds");
        });
    }

    public void PlaySound(int keyCode, InputAction action)
    {
        if (_isMuted || _mixer == null || _currentPack == null)
            return;

        CachedSound? sound = null;

        if (action == InputAction.KeyDown || action == InputAction.MouseDown)
        {
            _soundCache.TryGetValue(keyCode, out sound);
        }
        else if (action == InputAction.KeyUp || action == InputAction.MouseUp)
        {
            _soundUpCache.TryGetValue(keyCode, out sound);
        }

        if (sound != null)
        {
            var sampleProvider = new CachedSoundSampleProvider(sound);
            var volumeProvider = new VolumeSampleProvider(sampleProvider)
            {
                Volume = _volume
            };
            _mixer.AddMixerInput(volumeProvider);
        }
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0f, 1f);
    }

    public void SetMuted(bool muted)
    {
        _isMuted = muted;
    }

    public void UnloadSoundPack()
    {
        _soundCache.Clear();
        _soundUpCache.Clear();
        _currentPack = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _wavePlayer?.Stop();
        _wavePlayer?.Dispose();
        UnloadSoundPack();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Cached sound in memory for fast playback
/// </summary>
internal class CachedSound
{
    public float[] AudioData { get; private set; }
    public WaveFormat WaveFormat { get; private set; }

    public CachedSound(string audioFilePath)
    {
        AudioData = Array.Empty<float>();
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        
        var extension = Path.GetExtension(audioFilePath).ToLowerInvariant();
        
        if (extension == ".ogg")
        {
            LoadOggFile(audioFilePath);
        }
        else
        {
            LoadAudioFile(audioFilePath);
        }
    }

    private void LoadOggFile(string audioFilePath)
    {
        using var vorbisReader = new VorbisReader(audioFilePath);
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(vorbisReader.SampleRate, vorbisReader.Channels);
        
        var wholeFile = new List<float>();
        var readBuffer = new float[vorbisReader.SampleRate * vorbisReader.Channels];
        int samplesRead;
        
        while ((samplesRead = vorbisReader.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
        {
            wholeFile.AddRange(readBuffer.Take(samplesRead));
        }
        
        AudioData = wholeFile.ToArray();
    }

    private void LoadAudioFile(string audioFilePath)
    {
        using var audioFileReader = new AudioFileReader(audioFilePath);
        WaveFormat = audioFileReader.WaveFormat;
        
        var wholeFile = new List<float>();
        var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
        int samplesRead;
        
        while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            wholeFile.AddRange(readBuffer.Take(samplesRead));
        }
        
        AudioData = wholeFile.ToArray();
    }
}

/// <summary>
/// Extension methods for NVorbis
/// </summary>
internal static class VorbisExtensions
{
    public static ISampleProvider ToSampleProvider(this VorbisReader reader)
    {
        return new VorbisSampleProvider(reader);
    }
}

/// <summary>
/// Sample provider for Vorbis files
/// </summary>
internal class VorbisSampleProvider : ISampleProvider
{
    private readonly VorbisReader _reader;
    private readonly WaveFormat _waveFormat;

    public VorbisSampleProvider(VorbisReader reader)
    {
        _reader = reader;
        _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(reader.SampleRate, reader.Channels);
    }

    public WaveFormat WaveFormat => _waveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        return _reader.ReadSamples(buffer, offset, count);
    }
}

/// <summary>
/// Sample provider for cached sounds
/// </summary>
internal class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound _cachedSound;
    private long _position;

    public CachedSoundSampleProvider(CachedSound cachedSound)
    {
        _cachedSound = cachedSound;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = _cachedSound.AudioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        
        Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
        _position += samplesToCopy;
        
        return (int)samplesToCopy;
    }

    public WaveFormat WaveFormat => _cachedSound.WaveFormat;
}
