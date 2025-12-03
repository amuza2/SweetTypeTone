using OpenTK.Audio.OpenAL;
using NVorbis;
using NAudio.Wave;
using SweetTypeTone.Core.Interfaces;
using SweetTypeTone.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SweetTypeTone.Core.Services;

/// <summary>
/// OpenAL-based audio service for cross-platform audio playback
/// </summary>
public class OpenALAudioService : IAudioService
{
    private readonly ConcurrentDictionary<int, AudioBuffer> _soundCache = new();
    private readonly ConcurrentDictionary<int, AudioBuffer> _soundUpCache = new();
    private readonly List<int> _availableSources = new();
    private ALDevice _device;
    private ALContext _context;
    private float _volume = 0.5f;
    private bool _isMuted;
    private SoundPack? _currentPack;
    private bool _disposed;
    private const int MaxSources = 32; // Maximum simultaneous sounds
    
    // For sprite-based packs
    private AudioBuffer? _spriteBuffer;
    private bool _isSpritePack;

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                Console.WriteLine("Initializing OpenAL audio...");
                
                // Open default audio device
                _device = ALC.OpenDevice(null);
                if (_device == ALDevice.Null)
                {
                    throw new Exception("Failed to open OpenAL device");
                }

                // Create audio context
                _context = ALC.CreateContext(_device, (int[])null!);
                if (_context == ALContext.Null)
                {
                    throw new Exception("Failed to create OpenAL context");
                }

                ALC.MakeContextCurrent(_context);
                
                // Create audio sources for simultaneous playback
                for (int i = 0; i < MaxSources; i++)
                {
                    int source = AL.GenSource();
                    _availableSources.Add(source);
                }

                Console.WriteLine($"✓ OpenAL initialized with {MaxSources} audio sources");
                CheckALError("Initialize");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ OpenAL initialization failed: {ex.Message}");
                throw;
            }
        });
    }

    public async Task LoadSoundPackAsync(SoundPack soundPack)
    {
        UnloadSoundPack();
        _currentPack = soundPack;
        _isSpritePack = false;

        await Task.Run(() =>
        {
            Console.WriteLine($"Loading sound pack: {soundPack.Name}");
            Console.WriteLine($"  Keys to load: {soundPack.KeyDefinitions.Count}");
            
            // Check if sprite-based
            var hasSpriteDefinitions = soundPack.KeyDefinitions.Values.Any(d => d.SpriteStart.HasValue);
            
            if (hasSpriteDefinitions)
            {
                Console.WriteLine("  Loading sprite-based sound pack...");
                LoadSpritePack(soundPack);
            }
            else
            {
                Console.WriteLine("  Loading file-based sound pack...");
                LoadFilePack(soundPack);
            }
            
            if (_isSpritePack)
            {
                Console.WriteLine($"  ✓ Sprite pack ready for on-demand playback");
            }
            else
            {
                Console.WriteLine($"  ✓ Loaded {_soundCache.Count} key down sounds");
                Console.WriteLine($"  ✓ Loaded {_soundUpCache.Count} key up sounds");
            }
        });
    }

    private void LoadSpritePack(SoundPack soundPack)
    {
        // Load the main sprite file
        var spritePath = Path.Combine(soundPack.FolderPath, "sound.ogg");
        if (!File.Exists(spritePath))
        {
            Console.WriteLine($"  ✗ Sprite file not found: {spritePath}");
            return;
        }

        // Load entire sprite file as raw audio data (not as OpenAL buffer yet)
        using var vorbis = new VorbisReader(spritePath);
        
        var sampleRate = vorbis.SampleRate;
        var channels = vorbis.Channels;
        
        // Read all samples (NVorbis TotalSamples can be unreliable, so read in chunks)
        var audioDataList = new List<float>();
        var buffer = new float[sampleRate * channels]; // 1 second buffer
        int samplesRead;
        
        while ((samplesRead = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                audioDataList.Add(buffer[i]);
            }
        }
        
        var audioData = audioDataList.ToArray();
        var totalSamples = audioData.Length;
        var durationSeconds = (float)totalSamples / (sampleRate * channels);
        
        // Store the sprite buffer for on-demand playback
        _spriteBuffer = new AudioBuffer
        {
            SampleRate = sampleRate,
            Channels = channels,
            Data = audioData
        };
        
        _isSpritePack = true;
        
        Console.WriteLine($"  ✓ Sprite loaded successfully");
        Console.WriteLine($"  ✓ {soundPack.KeyDefinitions.Count} key definitions ready");
    }

    private void LoadFilePack(SoundPack soundPack)
    {
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
                        var buffer = LoadAudioFile(fullPath);
                        if (buffer != null)
                        {
                            _soundCache[kvp.Key] = buffer;
                        }
                    }
                }

                // Load key up sound
                if (!string.IsNullOrEmpty(kvp.Value.UpSoundPath))
                {
                    var fullPath = Path.Combine(soundPack.FolderPath, kvp.Value.UpSoundPath);
                    if (File.Exists(fullPath))
                    {
                        var buffer = LoadAudioFile(fullPath);
                        if (buffer != null)
                        {
                            _soundUpCache[kvp.Key] = buffer;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error loading sound for key {kvp.Key}: {ex.Message}");
            }
        }
    }

    private AudioBuffer? LoadAudioFile(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (extension == ".ogg")
            {
                return LoadOggFile(filePath);
            }
            else if (extension == ".wav")
            {
                return LoadWavFile(filePath);
            }
            else if (extension == ".mp3")
            {
                return LoadMp3File(filePath);
            }
            
            Console.WriteLine($"  ✗ Unsupported audio format: {extension}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Error loading {filePath}: {ex.Message}");
            return null;
        }
    }

    private AudioBuffer LoadOggFile(string filePath)
    {
        using var vorbis = new VorbisReader(filePath);
        
        var sampleRate = vorbis.SampleRate;
        var channels = vorbis.Channels;
        var totalSamples = (int)vorbis.TotalSamples;
        
        var audioData = new float[totalSamples];
        vorbis.ReadSamples(audioData, 0, totalSamples);
        
        // Convert float samples to 16-bit PCM
        var pcmData = new short[totalSamples];
        for (int i = 0; i < totalSamples; i++)
        {
            pcmData[i] = (short)(audioData[i] * short.MaxValue);
        }
        
        // Create OpenAL buffer
        int bufferId = AL.GenBuffer();
        var format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
        
        AL.BufferData(bufferId, format, pcmData, sampleRate);
        CheckALError("BufferData");
        
        return new AudioBuffer
        {
            BufferId = bufferId,
            SampleRate = sampleRate,
            Channels = channels,
            Data = audioData
        };
    }

    private AudioBuffer LoadWavFile(string filePath)
    {
        // Simple WAV loader (assumes 16-bit PCM)
        using var reader = new BinaryReader(File.OpenRead(filePath));
        
        // Skip RIFF header
        reader.ReadBytes(12);
        
        // Read fmt chunk
        reader.ReadBytes(8);
        int channels = reader.ReadInt16();
        int sampleRate = reader.ReadInt32();
        reader.ReadBytes(6);
        int bitsPerSample = reader.ReadInt16();
        
        // Find data chunk
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            
            if (chunkId == "data")
            {
                var pcmData = new short[chunkSize / 2];
                for (int i = 0; i < pcmData.Length; i++)
                {
                    pcmData[i] = reader.ReadInt16();
                }
                
                int bufferId = AL.GenBuffer();
                var format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                
                AL.BufferData(bufferId, format, pcmData, sampleRate);
                CheckALError("BufferData");
                
                return new AudioBuffer
                {
                    BufferId = bufferId,
                    SampleRate = sampleRate,
                    Channels = channels
                };
            }
            else
            {
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
            }
        }
        
        throw new Exception("No data chunk found in WAV file");
    }

    private AudioBuffer LoadMp3File(string filePath)
    {
        // MP3 decoding on Linux requires external tools
        // For now, skip MP3 files and suggest conversion
        throw new NotSupportedException(
            "MP3 files are not supported on Linux. " +
            "Please convert to OGG or WAV format using: " +
            $"ffmpeg -i \"{filePath}\" \"{Path.ChangeExtension(filePath, ".ogg")}\"");
    }

    private AudioBuffer? ExtractAudioSegment(AudioBuffer source, int startSample, int durationSamples)
    {
        if (source.Data == null)
            return null;
        
        // Validate and clamp indices
        if (startSample < 0 || startSample >= source.Data.Length)
        {
            Console.WriteLine($"  ✗ Invalid start sample: {startSample} (max: {source.Data.Length})");
            return null;
        }
        
        // Clamp duration to available data
        if (startSample + durationSamples > source.Data.Length)
        {
            durationSamples = source.Data.Length - startSample;
        }
        
        if (durationSamples <= 0)
        {
            Console.WriteLine($"  ✗ Invalid duration: {durationSamples}");
            return null;
        }
        
        var extractedData = new float[durationSamples];
        Array.Copy(source.Data, startSample, extractedData, 0, durationSamples);
        
        // Convert to PCM
        var pcmData = new short[durationSamples];
        for (int i = 0; i < durationSamples; i++)
        {
            pcmData[i] = (short)(Math.Clamp(extractedData[i], -1f, 1f) * short.MaxValue);
        }
        
        // Create OpenAL buffer
        int bufferId = AL.GenBuffer();
        var format = source.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
        
        AL.BufferData(bufferId, format, pcmData, source.SampleRate);
        
        var error = AL.GetError();
        if (error != ALError.NoError)
        {
            Console.WriteLine($"  ✗ OpenAL BufferData error: {error}");
            AL.DeleteBuffer(bufferId);
            return null;
        }
        
        return new AudioBuffer
        {
            BufferId = bufferId,
            SampleRate = source.SampleRate,
            Channels = source.Channels
        };
    }

    public void PlaySound(int keyCode, InputAction action)
    {
        if (_isMuted || _currentPack == null)
            return;

        if (_isSpritePack && _spriteBuffer != null)
        {
            // Play from sprite
            PlaySpriteSound(keyCode, action);
        }
        else
        {
            // Play from individual files
            AudioBuffer? buffer = null;

            if (action == InputAction.KeyDown || action == InputAction.MouseDown)
            {
                _soundCache.TryGetValue(keyCode, out buffer);
            }
            else if (action == InputAction.KeyUp || action == InputAction.MouseUp)
            {
                _soundUpCache.TryGetValue(keyCode, out buffer);
            }

            if (buffer != null)
            {
                PlayBuffer(buffer.BufferId);
            }
        }
    }
    
    private void PlaySpriteSound(int keyCode, InputAction action)
    {
        if (_currentPack == null || _spriteBuffer == null)
        {
            Console.WriteLine($"[PlaySprite] No pack or buffer (keyCode={keyCode})");
            return;
        }
        
        // Get the key definition
        if (!_currentPack.KeyDefinitions.TryGetValue(keyCode, out var def))
        {
            Console.WriteLine($"[PlaySprite] Key {keyCode} not found in definitions");
            return;
        }
        
        // Only play key down sounds for now (key up sounds would need separate definitions)
        if (action != InputAction.KeyDown && action != InputAction.MouseDown)
            return;
        
        if (!def.SpriteStart.HasValue || !def.SpriteDuration.HasValue)
        {
            Console.WriteLine($"[PlaySprite] Key {keyCode} missing sprite data");
            return;
        }
        
        // Extract and play the sprite segment on-demand
        var startMs = def.SpriteStart.Value;
        var durationMs = def.SpriteDuration.Value;
        
        Console.WriteLine($"[PlaySprite] Key {keyCode}: start={startMs}ms, duration={durationMs}ms");
        
        // Convert milliseconds to sample index (use long to avoid overflow)
        long startSampleLong = (long)startMs * _spriteBuffer.SampleRate * _spriteBuffer.Channels / 1000;
        long durationSamplesLong = (long)durationMs * _spriteBuffer.SampleRate * _spriteBuffer.Channels / 1000;
        
        int startSample = (int)startSampleLong;
        int durationSamples = (int)durationSamplesLong;
        
        Console.WriteLine($"[PlaySprite] Samples: start={startSample}, duration={durationSamples}, max={_spriteBuffer.Data!.Length}");
        
        // Validate bounds
        if (startSample < 0 || startSample >= _spriteBuffer.Data!.Length)
        {
            Console.WriteLine($"[PlaySprite] ✗ Start sample out of bounds!");
            return;
        }
        
        if (startSample + durationSamples > _spriteBuffer.Data.Length)
        {
            durationSamples = _spriteBuffer.Data.Length - startSample;
            Console.WriteLine($"[PlaySprite] Clamped duration to {durationSamples}");
        }
        
        if (durationSamples <= 0)
        {
            Console.WriteLine($"[PlaySprite] ✗ Invalid duration!");
            return;
        }
        
        // For stereo, ensure even number of samples (pairs of L/R)
        if (_spriteBuffer.Channels == 2 && durationSamples % 2 != 0)
        {
            durationSamples--;
        }
        
        // Extract segment
        var segment = new float[durationSamples];
        Array.Copy(_spriteBuffer.Data, startSample, segment, 0, durationSamples);
        
        // Convert to PCM
        var pcmData = new short[durationSamples];
        for (int i = 0; i < durationSamples; i++)
        {
            pcmData[i] = (short)(Math.Clamp(segment[i], -1f, 1f) * short.MaxValue);
        }
        
        // Create temporary buffer and play
        int bufferId = AL.GenBuffer();
        var format = _spriteBuffer.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
        AL.BufferData(bufferId, format, pcmData, _spriteBuffer.SampleRate);
        
        var error = AL.GetError();
        if (error == ALError.NoError)
        {
            Console.WriteLine($"[PlaySprite] ✓ Playing key {keyCode}");
            PlayBuffer(bufferId);
            
            // Schedule buffer deletion after playback (simple approach: delete after 1 second)
            Task.Delay(1000).ContinueWith(_ => AL.DeleteBuffer(bufferId));
        }
        else
        {
            Console.WriteLine($"[PlaySprite] ✗ OpenAL error: {error}");
            AL.DeleteBuffer(bufferId);
        }
    }

    private void PlayBuffer(int bufferId)
    {
        // Find an available source
        int source = GetAvailableSource();
        if (source == 0)
            return;

        AL.Source(source, ALSourcei.Buffer, bufferId);
        AL.Source(source, ALSourcef.Gain, _volume);
        AL.SourcePlay(source);
        CheckALError("SourcePlay");
    }

    private int GetAvailableSource()
    {
        // Find a source that's not playing
        foreach (var source in _availableSources)
        {
            AL.GetSource(source, ALGetSourcei.SourceState, out int state);
            if (state != (int)ALSourceState.Playing)
            {
                return source;
            }
        }
        
        // If all sources are busy, reuse the first one
        return _availableSources.FirstOrDefault();
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
        // Delete all buffers
        foreach (var buffer in _soundCache.Values)
        {
            if (buffer.BufferId > 0)
                AL.DeleteBuffer(buffer.BufferId);
        }
        foreach (var buffer in _soundUpCache.Values)
        {
            if (buffer.BufferId > 0)
                AL.DeleteBuffer(buffer.BufferId);
        }
        
        _soundCache.Clear();
        _soundUpCache.Clear();
        _spriteBuffer = null;
        _isSpritePack = false;
        _currentPack = null;
        
        CheckALError("UnloadSoundPack");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        UnloadSoundPack();

        // Delete sources
        foreach (var source in _availableSources)
        {
            AL.DeleteSource(source);
        }
        _availableSources.Clear();

        // Cleanup OpenAL
        if (_context != ALContext.Null)
        {
            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(_context);
        }

        if (_device != ALDevice.Null)
        {
            ALC.CloseDevice(_device);
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void CheckALError(string operation)
    {
        var error = AL.GetError();
        if (error != ALError.NoError)
        {
            Console.WriteLine($"OpenAL Error in {operation}: {error}");
        }
    }

    private class AudioBuffer
    {
        public int BufferId { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public float[]? Data { get; set; } // For sprite extraction
    }
}
