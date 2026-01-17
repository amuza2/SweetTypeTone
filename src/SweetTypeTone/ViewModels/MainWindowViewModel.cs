using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetTypeTone.Interfaces;
using SweetTypeTone.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SweetTypeTone.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private readonly IInputMonitorService _inputMonitorService;
    private readonly ISoundPackService _soundPackService;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _title = "SweetTypeTone";

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private float _volume = 0.5f;

    [ObservableProperty]
    private SoundPack? _selectedSoundPack;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<SoundPack> SoundPacks { get; } = new();

    public MainWindowViewModel(
        IAudioService audioService,
        IInputMonitorService inputMonitorService,
        ISoundPackService soundPackService,
        ISettingsService settingsService)
    {
        _audioService = audioService;
        _inputMonitorService = inputMonitorService;
        _soundPackService = soundPackService;
        _settingsService = settingsService;

        _inputMonitorService.InputDetected += OnInputDetected;
    }

    private bool _isInitializing;
    
    public async Task InitializeAsync()
    {
        _isInitializing = true;
        IsLoading = true;
        StatusMessage = "Initializing...";

        try
        {
            // Load settings first (fast)
            var settings = await _settingsService.LoadSettingsAsync();
            Volume = settings.MasterVolume;
            IsMuted = settings.IsMuted;

            // Initialize audio and scan sound packs in parallel
            var audioTask = _audioService.InitializeAsync();
            var scanTask = _soundPackService.GetAllSoundPacksAsync();
            
            await Task.WhenAll(audioTask, scanTask);
            
            var packs = await scanTask;
            
            // Sort by supported status first, then alphabetically
            var sortedPacks = packs
                .OrderByDescending(p => p.IsSupported)  // Supported first
                .ThenBy(p => p.Name)                     // Then alphabetically
                .ToList();
            
            SoundPacks.Clear();
            foreach (var pack in sortedPacks)
            {
                SoundPacks.Add(pack);
            }

            // Select saved sound pack or first available (supported only)
            SoundPack? packToLoad = null;
            if (!string.IsNullOrEmpty(settings.CurrentSoundPackId))
            {
                packToLoad = SoundPacks.FirstOrDefault(p => p.Id == settings.CurrentSoundPackId && p.IsSupported);
            }
            packToLoad ??= SoundPacks.FirstOrDefault(p => p.IsSupported);

            // Start monitoring BEFORE loading sound pack for faster perceived startup
            if (settings.EnableKeyboardSounds || settings.EnableMouseSounds)
            {
                await StartMonitoringAsync();
            }

            // Now load the sound pack
            if (packToLoad != null)
            {
                await LoadSoundPackAsync(packToLoad);
                _selectedSoundPack = packToLoad; // Set backing field directly to avoid triggering OnSelectedSoundPackChanged
                OnPropertyChanged(nameof(SelectedSoundPack));
            }
            else
            {
                StatusMessage = "No sound packs available";
            }

            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            _isInitializing = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartMonitoringAsync()
    {
        if (IsMonitoring)
            return;

        try
        {
            await _inputMonitorService.StartAsync();
            IsMonitoring = true;
            StatusMessage = "Monitoring active";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to start monitoring: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StopMonitoringAsync()
    {
        if (!IsMonitoring)
            return;

        await _inputMonitorService.StopAsync();
        IsMonitoring = false;
        StatusMessage = "Monitoring stopped";
    }

    [RelayCommand]
    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        _audioService.SetMuted(IsMuted);
        
        var settings = _settingsService.GetSettings();
        settings.IsMuted = IsMuted;
        _ = _settingsService.SaveSettingsAsync(settings);
    }

    partial void OnVolumeChanged(float value)
    {
        _audioService.SetVolume(value);
        
        var settings = _settingsService.GetSettings();
        settings.MasterVolume = value;
        _ = _settingsService.SaveSettingsAsync(settings);
    }

    partial void OnSelectedSoundPackChanged(SoundPack? oldValue, SoundPack? newValue)
    {
        // Skip during initialization - we load the pack explicitly
        if (_isInitializing)
            return;
            
        if (newValue != null)
        {
            // Prevent selecting unsupported packs
            if (!newValue.IsSupported)
            {
                StatusMessage = $"Cannot load: {newValue.UnsupportedReason}";
                // Revert to previous selection or first supported pack
                SelectedSoundPack = oldValue?.IsSupported == true 
                    ? oldValue 
                    : SoundPacks.FirstOrDefault(p => p.IsSupported);
                return;
            }
            
            _ = LoadSoundPackAsync(newValue);
        }
    }
    
    private async Task LoadSoundPackAsync(SoundPack soundPack)
    {
        IsLoading = true;
        StatusMessage = $"Loading {soundPack.Name}...";
        
        try
        {
            await _audioService.LoadSoundPackAsync(soundPack);
            await _settingsService.SaveSettingsAsync(new AppSettings
            {
                CurrentSoundPackId = soundPack.Id,
                MasterVolume = Volume,
                IsMuted = IsMuted,
                EnableKeyboardSounds = true,
                EnableMouseSounds = false
            });
            
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading pack: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportSoundPackAsync()
    {
        try
        {
            StatusMessage = "Select a Mechvibes sound pack folder...";
            
            // For now, show instructions for manual import
            var customPacksPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config", "SweetTypeTone", "CustomSoundPacks"
            );
            
            StatusMessage = $"Copy sound pack folders to: {customPacksPath}, then click Refresh";
            
            // TODO: Implement proper folder picker dialog
            // This requires platform-specific file dialogs which Avalonia supports
            // For now, users can manually copy folders
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshSoundPacksAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Refreshing sound packs...";
            
            // Force rescan of sound packs from disk
            await _soundPackService.RefreshSoundPacksAsync();
            var packs = await _soundPackService.GetAllSoundPacksAsync();
            
            // Remember current selection
            var currentPackId = SelectedSoundPack?.Id;
            
            // Sort by supported status first, then alphabetically
            var sortedPacks = packs
                .OrderByDescending(p => p.IsSupported)  // Supported first
                .ThenBy(p => p.Name)                     // Then alphabetically
                .ToList();
            
            // Update collection
            SoundPacks.Clear();
            foreach (var pack in sortedPacks)
            {
                var supportedText = pack.IsSupported ? "" : $" [{pack.UnsupportedReason}]";
                Console.WriteLine($"  - {pack.Name}{supportedText} (ID: {pack.Id})");
                SoundPacks.Add(pack);
            }
            
            // Restore selection if still valid and supported
            if (!string.IsNullOrEmpty(currentPackId))
            {
                var previousPack = SoundPacks.FirstOrDefault(p => p.Id == currentPackId);
                if (previousPack?.IsSupported == true)
                {
                    SelectedSoundPack = previousPack;
                }
            }
            
            // If no valid selection, pick first supported pack
            SelectedSoundPack ??= SoundPacks.FirstOrDefault(p => p.IsSupported);
            
            StatusMessage = $"Refreshed! Found {packs.Count} sound packs";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Refresh error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnInputDetected(object? sender, InputEvent e)
    {
        if (IsMuted || !IsMonitoring)
            return;
        _audioService.PlaySound(e.KeyCode, e.Action);
    }

    public async Task CleanupAsync()
    {
        try
        {
            Console.WriteLine("Cleaning up...");
            
            // Stop monitoring first with timeout
            if (IsMonitoring)
            {
                var stopTask = _inputMonitorService.StopAsync();
                var timeoutTask = Task.Delay(1000); // 1 second timeout
                
                var completedTask = await Task.WhenAny(stopTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("Stop monitoring timed out, forcing cleanup");
                }
                
                IsMonitoring = false;
            }
            
            // Unsubscribe from events
            _inputMonitorService.InputDetected -= OnInputDetected;
            
            // Dispose services (this will force close file handles)
            _inputMonitorService.Dispose();
            _audioService.Dispose();
            
            Console.WriteLine("Cleanup complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex.Message}");
        }
    }
    
    public void Cleanup()
    {
        // Synchronous wrapper for compatibility with timeout
        var task = CleanupAsync();
        if (!task.Wait(2000)) // 2 second total timeout
        {
            Console.WriteLine("Cleanup timed out, exiting anyway");
        }
    }
}
