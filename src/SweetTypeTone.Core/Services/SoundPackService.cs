using SweetTypeTone.Core.Interfaces;
using SweetTypeTone.Core.Models;
using System.Text.Json;

namespace SweetTypeTone.Core.Services;

/// <summary>
/// Service for managing sound packs
/// </summary>
public class SoundPackService : ISoundPackService
{
    private readonly string _soundPacksDirectory;
    private readonly string _customSoundPacksDirectory;
    private readonly List<SoundPack> _soundPacks = new();

    public SoundPackService(string? customDirectory = null)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDirectory = Path.Combine(appDataPath, "SweetTypeTone");
        
        _soundPacksDirectory = Path.Combine(appDirectory, "SoundPacks");
        _customSoundPacksDirectory = customDirectory ?? Path.Combine(appDirectory, "CustomSoundPacks");

        Directory.CreateDirectory(_soundPacksDirectory);
        Directory.CreateDirectory(_customSoundPacksDirectory);
    }

    public async Task<List<SoundPack>> GetAllSoundPacksAsync()
    {
        if (_soundPacks.Count == 0)
        {
            await ScanForSoundPacksAsync();
        }
        return _soundPacks.ToList();
    }

    public async Task<SoundPack?> GetSoundPackByIdAsync(string id)
    {
        var packs = await GetAllSoundPacksAsync();
        return packs.FirstOrDefault(p => p.Id == id);
    }

    public async Task<SoundPack> ImportMechvibesPackAsync(string path)
    {
        if (!Directory.Exists(path) && !File.Exists(path))
        {
            throw new FileNotFoundException($"Sound pack not found at: {path}");
        }

        var configPath = Path.Combine(path, "config.json");
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"config.json not found in: {path}");
        }

        var jsonContent = await File.ReadAllTextAsync(configPath);
        var mechvibesConfig = JsonSerializer.Deserialize<MechvibesConfig>(jsonContent);

        if (mechvibesConfig == null)
        {
            throw new InvalidOperationException("Failed to parse config.json");
        }

        var soundPack = await ConvertMechvibesPackAsync(mechvibesConfig, path);
        
        // Copy to custom directory
        var destPath = Path.Combine(_customSoundPacksDirectory, soundPack.Id);
        await CopySoundPackAsync(path, destPath, soundPack);

        _soundPacks.Add(soundPack);
        return soundPack;
    }

    private async Task<SoundPack> ConvertMechvibesPackAsync(MechvibesConfig config, string sourcePath)
    {
        var soundPack = new SoundPack
        {
            Id = config.id ?? Guid.NewGuid().ToString(),
            Name = config.name ?? "Unnamed Pack",
            FolderPath = sourcePath,
            IsCustom = true,
            Type = SoundPackType.Keyboard
        };

        if (config.defines == null)
        {
            return soundPack;
        }

        // Handle version 1 (sprite-based) and version 2 (multi-file) formats
        if (config.key_define_type == "single" && !string.IsNullOrEmpty(config.sound))
        {
            // Version 1: Extract sprites to individual files
            await ExtractSpritesToFilesAsync(config, sourcePath, soundPack);
        }
        else if (config.key_define_type == "multi")
        {
            // Version 2: Direct file references
            foreach (var kvp in config.defines)
            {
                if (int.TryParse(kvp.Key, out var keyCode))
                {
                    var soundDef = new SoundDefinition
                    {
                        KeyCode = keyCode,
                        DownSoundPath = kvp.Value.ToString() ?? ""
                    };
                    soundPack.KeyDefinitions[keyCode] = soundDef;
                }
                else if (kvp.Key.EndsWith("-up") && int.TryParse(kvp.Key.Replace("-up", ""), out var upKeyCode))
                {
                    if (soundPack.KeyDefinitions.TryGetValue(upKeyCode, out var existing))
                    {
                        existing.UpSoundPath = kvp.Value.ToString();
                    }
                }
            }
        }

        return soundPack;
    }

    private async Task ExtractSpritesToFilesAsync(MechvibesConfig config, string sourcePath, SoundPack soundPack)
    {
        // For now, we'll keep sprite references and handle them in audio service
        // Full extraction would require audio processing libraries
        var soundPath = Path.Combine(sourcePath, config.sound ?? "sound.ogg");
        
        if (config.defines != null)
        {
            foreach (var kvp in config.defines)
            {
                if (int.TryParse(kvp.Key, out var keyCode) && kvp.Value is JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        var array = element.EnumerateArray().ToList();
                        if (array.Count >= 2)
                        {
                            var soundDef = new SoundDefinition
                            {
                                KeyCode = keyCode,
                                DownSoundPath = config.sound ?? "",
                                SpriteStart = array[0].GetInt32(),
                                SpriteDuration = array[1].GetInt32()
                            };
                            soundPack.KeyDefinitions[keyCode] = soundDef;
                        }
                    }
                }
            }
        }

        await Task.CompletedTask;
    }

    private async Task CopySoundPackAsync(string sourcePath, string destPath, SoundPack soundPack)
    {
        Directory.CreateDirectory(destPath);
        soundPack.FolderPath = destPath;

        // Copy all audio files
        var audioExtensions = new[] { ".ogg", ".wav", ".mp3" };
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();
            if (audioExtensions.Contains(extension))
            {
                var destFile = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }

        // Save converted config
        var configPath = Path.Combine(destPath, "soundpack.json");
        var json = JsonSerializer.Serialize(soundPack, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(configPath, json);
    }

    public async Task<SoundPack> CreateSoundPackAsync(SoundPack soundPack)
    {
        soundPack.Id = Guid.NewGuid().ToString();
        soundPack.CreatedAt = DateTime.Now;
        soundPack.ModifiedAt = DateTime.Now;

        var packPath = Path.Combine(_customSoundPacksDirectory, soundPack.Id);
        Directory.CreateDirectory(packPath);
        soundPack.FolderPath = packPath;

        await SaveSoundPackAsync(soundPack);
        _soundPacks.Add(soundPack);

        return soundPack;
    }

    public async Task UpdateSoundPackAsync(SoundPack soundPack)
    {
        soundPack.ModifiedAt = DateTime.Now;
        await SaveSoundPackAsync(soundPack);

        var existing = _soundPacks.FirstOrDefault(p => p.Id == soundPack.Id);
        if (existing != null)
        {
            var index = _soundPacks.IndexOf(existing);
            _soundPacks[index] = soundPack;
        }
    }

    public async Task DeleteSoundPackAsync(string id)
    {
        var soundPack = await GetSoundPackByIdAsync(id);
        if (soundPack != null && soundPack.IsCustom)
        {
            if (Directory.Exists(soundPack.FolderPath))
            {
                Directory.Delete(soundPack.FolderPath, true);
            }
            _soundPacks.Remove(soundPack);
        }
    }

    public async Task ExportSoundPackAsync(string id, string destinationPath)
    {
        var soundPack = await GetSoundPackByIdAsync(id);
        if (soundPack == null)
        {
            throw new InvalidOperationException($"Sound pack {id} not found");
        }

        Directory.CreateDirectory(destinationPath);

        // Copy all files
        foreach (var file in Directory.GetFiles(soundPack.FolderPath))
        {
            var destFile = Path.Combine(destinationPath, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }
    }

    public async Task ScanForSoundPacksAsync()
    {
        _soundPacks.Clear();

        // Scan custom directory
        await ScanDirectoryAsync(_customSoundPacksDirectory, true);

        // Scan default directory
        await ScanDirectoryAsync(_soundPacksDirectory, false);
    }

    private async Task ScanDirectoryAsync(string directory, bool isCustom)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var packDir in Directory.GetDirectories(directory))
        {
            try
            {
                // First check for our native format
                var soundpackPath = Path.Combine(packDir, "soundpack.json");
                
                if (File.Exists(soundpackPath))
                {
                    var json = await File.ReadAllTextAsync(soundpackPath);
                    var soundPack = JsonSerializer.Deserialize<SoundPack>(json);
                    
                    if (soundPack != null)
                    {
                        soundPack.IsCustom = isCustom;
                        soundPack.FolderPath = packDir;
                        _soundPacks.Add(soundPack);
                    }
                }
                else
                {
                    // Check for Mechvibes format (config.json)
                    var mechvibesConfigPath = Path.Combine(packDir, "config.json");
                    
                    if (File.Exists(mechvibesConfigPath))
                    {
                        Console.WriteLine($"Found Mechvibes pack: {packDir}");
                        var json = await File.ReadAllTextAsync(mechvibesConfigPath);
                        var mechvibesConfig = JsonSerializer.Deserialize<MechvibesConfig>(json);
                        
                        if (mechvibesConfig != null)
                        {
                            var soundPack = await ConvertMechvibesPackAsync(mechvibesConfig, packDir);
                            soundPack.IsCustom = isCustom;
                            _soundPacks.Add(soundPack);
                            Console.WriteLine($"Loaded pack: {soundPack.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound pack from {packDir}: {ex.Message}");
            }
        }
    }

    private async Task SaveSoundPackAsync(SoundPack soundPack)
    {
        var configPath = Path.Combine(soundPack.FolderPath, "soundpack.json");
        var json = JsonSerializer.Serialize(soundPack, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(configPath, json);
    }
}
