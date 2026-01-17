using OpenTK.Audio.OpenAL;
using NLayer;
using NVorbis;
using SweetTypeTone.Interfaces;
using SweetTypeTone.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SweetTypeTone.Services;

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

    // For default/fallback sounds
    private readonly List<AudioBuffer> _defaultSoundBuffers = new();
    private readonly List<AudioBuffer> _defaultUpSoundBuffers = new();
    private readonly Random _random = new();

    // Round-robin source selection for better performance
    private int _sourceIndex = 0;

    // Cache for pre-extracted sprite segment buffers (keyCode -> bufferId)
    private readonly ConcurrentDictionary<int, int> _spriteSoundCache = new();

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            try
            {
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

                CheckALError("Initialize");
            }
            catch
            {
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
            // Check if sprite-based
            var hasSpriteDefinitions = soundPack.KeyDefinitions.Values.Any(d => d.SpriteStart.HasValue);

            if (hasSpriteDefinitions)
            {
                LoadSpritePack(soundPack);
            }
            else
            {
                LoadFilePack(soundPack);
            }

        });
    }

    private void LoadSpritePack(SoundPack soundPack)
    {
        // Get the sprite filename from the first key definition
        string? spriteFileName = soundPack.KeyDefinitions.Values
            .FirstOrDefault(d => d.SpriteStart.HasValue)?.DownSoundPath;

        if (string.IsNullOrEmpty(spriteFileName))
            return;

        // Build full path to sprite file
        var spritePath = Path.Combine(soundPack.FolderPath, spriteFileName);

        if (!File.Exists(spritePath))
            return;

        // Load entire sprite file as raw audio data temporarily
        using var vorbis = new VorbisReader(spritePath);

        var sampleRate = vorbis.SampleRate;
        var channels = vorbis.Channels;

        // Pre-allocate list with estimated capacity for better performance
        var estimatedSamples = (int)(vorbis.TotalSamples * channels);
        var audioDataList = estimatedSamples > 0
            ? new List<float>(estimatedSamples)
            : new List<float>();

        var buffer = new float[sampleRate * channels]; // 1 second buffer
        int samplesRead;

        while ((samplesRead = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
        {
            // Use AddRange for better performance
            if (samplesRead == buffer.Length)
                audioDataList.AddRange(buffer);
            else
                audioDataList.AddRange(new ArraySegment<float>(buffer, 0, samplesRead));
        }

        var audioData = audioDataList.ToArray();
        var format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;

        // Pre-extract ALL sprite segments as OpenAL buffers immediately
        // This uses GPU/OpenAL memory instead of keeping raw audio data in RAM
        int extractedCount = 0;
        foreach (var kvp in soundPack.KeyDefinitions)
        {
            var def = kvp.Value;
            if (!def.SpriteStart.HasValue || !def.SpriteDuration.HasValue)
                continue;

            var startMs = def.SpriteStart.Value;
            var durationMs = def.SpriteDuration.Value;

            // Convert milliseconds to sample index
            long startSampleLong = (long)startMs * sampleRate * channels / 1000;
            long durationSamplesLong = (long)durationMs * sampleRate * channels / 1000;

            int startSample = (int)startSampleLong;
            int durationSamples = (int)durationSamplesLong;

            // Validate bounds
            if (startSample < 0 || startSample >= audioData.Length)
                continue;

            if (startSample + durationSamples > audioData.Length)
                durationSamples = audioData.Length - startSample;

            if (durationSamples <= 0)
                continue;

            // For stereo, ensure even number of samples
            if (channels == 2 && durationSamples % 2 != 0)
                durationSamples--;

            // Convert float segment to 16-bit PCM
            var pcmData = new short[durationSamples];
            for (int i = 0; i < durationSamples; i++)
            {
                pcmData[i] = (short)(Math.Clamp(audioData[startSample + i], -1f, 1f) * short.MaxValue);
            }

            // Create permanent OpenAL buffer for this key
            int bufferId = AL.GenBuffer();
            AL.BufferData(bufferId, format, pcmData, sampleRate);

            var error = AL.GetError();
            if (error == ALError.NoError)
            {
                _spriteSoundCache[kvp.Key] = bufferId;
                extractedCount++;
            }
            else
            {
                AL.DeleteBuffer(bufferId);
            }
        }

        // Store minimal metadata only (no raw audio data!)
        _spriteBuffer = new AudioBuffer
        {
            SampleRate = sampleRate,
            Channels = channels,
            Data = null // Don't keep raw audio data - saves ~50-100MB!
        };

        _isSpritePack = true;
    }

    private void LoadFilePack(SoundPack soundPack)
    {
        int successCount = 0;
        int failCount = 0;

        // Collect all files to load
        var filesToLoad = new List<(int keyCode, string path, bool isUpSound)>();

        foreach (var kvp in soundPack.KeyDefinitions)
        {
            if (!string.IsNullOrEmpty(kvp.Value.DownSoundPath))
            {
                var fullPath = Path.Combine(soundPack.FolderPath, kvp.Value.DownSoundPath);
                if (File.Exists(fullPath))
                {
                    filesToLoad.Add((kvp.Key, fullPath, false));
                }
            }

            if (!string.IsNullOrEmpty(kvp.Value.UpSoundPath))
            {
                var fullPath = Path.Combine(soundPack.FolderPath, kvp.Value.UpSoundPath);
                if (File.Exists(fullPath))
                {
                    filesToLoad.Add((kvp.Key, fullPath, true));
                }
            }
        }

        // Load files in parallel for faster loading
        var loadResults = new ConcurrentBag<(int keyCode, AudioBuffer? buffer, bool isUpSound, bool success)>();

        Parallel.ForEach(filesToLoad, new ParallelOptions { MaxDegreeOfParallelism = 4 }, fileInfo =>
        {
            try
            {
                var buffer = LoadAudioFile(fileInfo.path);
                loadResults.Add((fileInfo.keyCode, buffer, fileInfo.isUpSound, buffer != null));
            }
            catch
            {
                loadResults.Add((fileInfo.keyCode, null, fileInfo.isUpSound, false));
            }
        });

        // Store loaded buffers
        foreach (var result in loadResults)
        {
            if (result.success && result.buffer != null)
            {
                if (result.isUpSound)
                    _soundUpCache[result.keyCode] = result.buffer;
                else
                    _soundCache[result.keyCode] = result.buffer;
                successCount++;
            }
            else
            {
                failCount++;
            }
        }

        // Load default sounds for fallback (when a key doesn't have a specific mapping)
        foreach (var defaultPath in soundPack.DefaultSoundPaths)
        {
            var fullPath = Path.Combine(soundPack.FolderPath, defaultPath);
            if (File.Exists(fullPath))
            {
                try
                {
                    var buffer = LoadAudioFile(fullPath);
                    if (buffer != null)
                    {
                        _defaultSoundBuffers.Add(buffer);
                    }
                }
                catch
                {
                    // Skip failed sounds
                }
            }
        }

        // Load default up sounds
        foreach (var defaultPath in soundPack.DefaultUpSoundPaths)
        {
            var fullPath = Path.Combine(soundPack.FolderPath, defaultPath);
            if (File.Exists(fullPath))
            {
                try
                {
                    var buffer = LoadAudioFile(fullPath);
                    if (buffer != null)
                    {
                        _defaultUpSoundBuffers.Add(buffer);
                    }
                }
                catch { }
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
            Data = null // Free memory - we only need the OpenAL buffer
        };
    }

    private AudioBuffer LoadWavFile(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));

        // Read RIFF header
        var riff = new string(reader.ReadChars(4));
        if (riff != "RIFF")
            throw new Exception($"Invalid WAV file: expected RIFF, got {riff}");

        var fileSize = reader.ReadInt32();
        var wave = new string(reader.ReadChars(4));
        if (wave != "WAVE")
            throw new Exception($"Invalid WAV file: expected WAVE, got {wave}");

        int channels = 0;
        int sampleRate = 0;
        int bitsPerSample = 0;
        short audioFormat = 0;

        // Read chunks until we find fmt and data
        short[]? pcmData = null;

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var chunkId = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            var chunkStart = reader.BaseStream.Position;

            if (chunkId == "fmt ")
            {
                audioFormat = reader.ReadInt16();
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                var byteRate = reader.ReadInt32();
                var blockAlign = reader.ReadInt16();
                bitsPerSample = reader.ReadInt16();

                // Only support PCM format (1)
                if (audioFormat != 1)
                    throw new Exception($"Unsupported WAV format: {audioFormat} (only PCM is supported)");

                // Skip any extra fmt data
                reader.BaseStream.Position = chunkStart + chunkSize;
            }
            else if (chunkId == "data")
            {
                if (bitsPerSample == 16)
                {
                    pcmData = new short[chunkSize / 2];
                    for (int i = 0; i < pcmData.Length; i++)
                    {
                        pcmData[i] = reader.ReadInt16();
                    }
                }
                else if (bitsPerSample == 8)
                {
                    // Convert 8-bit unsigned to 16-bit signed
                    pcmData = new short[chunkSize];
                    for (int i = 0; i < chunkSize; i++)
                    {
                        byte sample = reader.ReadByte();
                        pcmData[i] = (short)((sample - 128) * 256);
                    }
                }
                else
                {
                    throw new Exception($"Unsupported bits per sample: {bitsPerSample}");
                }

                break; // Found data, we're done
            }
            else
            {
                // Skip unknown chunks
                reader.BaseStream.Position = chunkStart + chunkSize;
            }
        }

        if (pcmData == null)
            throw new Exception("No data chunk found in WAV file");

        if (channels == 0 || sampleRate == 0)
            throw new Exception("Invalid WAV file: missing fmt chunk");

        // Create OpenAL buffer
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

    private AudioBuffer LoadMp3File(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536, useAsync: false);
        using var reader = new MpegFile(stream);

        var sampleRate = reader.SampleRate;
        var channels = reader.Channels;

        // Read all samples with optimized buffer (larger buffer = fewer allocations)
        // Pre-allocate list capacity to avoid resizing
        // MP3 compression ratio is typically 10:1, so decoded size ≈ fileSize * 10
        // But we store as float (4 bytes), so: fileSize * 10 / 4 = fileSize * 2.5
        var fileSize = new FileInfo(filePath).Length;
        var estimatedSamples = (int)(fileSize * 2.5);
        var samples = new List<float>(estimatedSamples);
        var buffer = new float[16384]; // 16KB buffer for fewer read operations
        int samplesRead;

        while ((samplesRead = reader.ReadSamples(buffer, 0, buffer.Length)) > 0)
        {
            // Use AddRange with ArraySegment to avoid individual Add calls
            if (samplesRead == buffer.Length)
                samples.AddRange(buffer);
            else
                samples.AddRange(new ArraySegment<float>(buffer, 0, samplesRead));
        }

        var audioData = samples.ToArray();

        // Convert float samples to 16-bit PCM
        var pcmData = new short[audioData.Length];
        for (int i = 0; i < audioData.Length; i++)
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
            Data = null // Free memory - we only need the OpenAL buffer
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

                // Fall back to random default sound if no specific mapping
                if (buffer == null && _defaultSoundBuffers.Count > 0)
                {
                    buffer = _defaultSoundBuffers[_random.Next(_defaultSoundBuffers.Count)];
                }
            }
            else if (action == InputAction.KeyUp || action == InputAction.MouseUp)
            {
                _soundUpCache.TryGetValue(keyCode, out buffer);

                // Fall back to random default up sound if no specific mapping
                if (buffer == null && _defaultUpSoundBuffers.Count > 0)
                {
                    buffer = _defaultUpSoundBuffers[_random.Next(_defaultUpSoundBuffers.Count)];
                }
            }

            if (buffer != null)
            {
                PlayBuffer(buffer.BufferId);
            }
        }
    }

    private void PlaySpriteSound(int keyCode, InputAction action)
    {
        if (_currentPack == null || !_isSpritePack)
            return;

        // Only play key down sounds for now (key up sounds would need separate definitions)
        if (action != InputAction.KeyDown && action != InputAction.MouseDown)
            return;

        // Look up pre-cached buffer for this key
        if (_spriteSoundCache.TryGetValue(keyCode, out int bufferId))
        {
            PlayBuffer(bufferId);
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
        // Round-robin search starting from last used index
        // This avoids always checking from the beginning
        for (int i = 0; i < MaxSources; i++)
        {
            int idx = (_sourceIndex + i) % MaxSources;
            AL.GetSource(_availableSources[idx], ALGetSourcei.SourceState, out int state);
            if (state != (int)ALSourceState.Playing)
            {
                _sourceIndex = (idx + 1) % MaxSources;
                return _availableSources[idx];
            }
        }

        // All sources are busy - reuse oldest one
        int fallbackIdx = _sourceIndex;
        _sourceIndex = (_sourceIndex + 1) % MaxSources;
        return _availableSources[fallbackIdx];
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
        // Stop all sources and detach buffers BEFORE deleting them
        // This prevents OpenAL IllegalCommand errors
        foreach (var source in _availableSources)
        {
            AL.SourceStop(source);
            AL.Source(source, ALSourcei.Buffer, 0); // Detach buffer from source
        }

        // Now safe to delete all buffers
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

        // Delete default sound buffers
        foreach (var buffer in _defaultSoundBuffers)
        {
            if (buffer.BufferId > 0)
                AL.DeleteBuffer(buffer.BufferId);
        }
        foreach (var buffer in _defaultUpSoundBuffers)
        {
            if (buffer.BufferId > 0)
                AL.DeleteBuffer(buffer.BufferId);
        }

        // Delete pre-cached sprite sound buffers
        foreach (var bufferId in _spriteSoundCache.Values)
        {
            if (bufferId > 0)
                AL.DeleteBuffer(bufferId);
        }

        _soundCache.Clear();
        _soundUpCache.Clear();
        _defaultSoundBuffers.Clear();
        _defaultUpSoundBuffers.Clear();
        _spriteSoundCache.Clear();
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
