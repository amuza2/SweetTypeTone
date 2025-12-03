using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using SweetTypeTone.Core.Interfaces;
using SweetTypeTone.Core.Services;
using SweetTypeTone.ViewModels;
using SweetTypeTone.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SweetTypeTone;

public partial class App : Application
{
    private IAudioService? _audioService;
    private IInputMonitorService? _inputMonitorService;
    private ISoundPackService? _soundPackService;
    private ISettingsService? _settingsService;
    private MainWindowViewModel? _mainViewModel;
    private MainWindow? _mainWindow;
    private NativeMenuItem? _toggleMenuItem;
    private const string TrayIconPath = "icons8-key-press-96.png";
    private const string ShowIconPath = "icons8-show-96.png";
    private const string SoundIconPath = "icons8-sound-100.png";
    private const string ExitIconPath = "icons8-exit-24.png";
    private const string GitHubIconPath = "icons8-github-24.png";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Initialize services
            _audioService = new OpenALAudioService();
            _inputMonitorService = new LinuxInputMonitorService();
            _soundPackService = new SoundPackService();
            _settingsService = new SettingsService();

            // Create ViewModel with dependencies
            _mainViewModel = new MainWindowViewModel(
                _audioService,
                _inputMonitorService,
                _soundPackService,
                _settingsService
            );

            _mainWindow = new MainWindow
            {
                DataContext = _mainViewModel
            };

            desktop.MainWindow = _mainWindow;
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Handle window closing (minimize to tray)
            _mainWindow.Closing += OnMainWindowClosing;

            // Create tray icon
            CreateTrayIcon();

            // Initialize async
            _ = _mainViewModel.InitializeAsync();

            // Handle shutdown
            desktop.ShutdownRequested += OnShutdownRequested;
            
            // Update toggle menu text
            if (_toggleMenuItem != null)
                _toggleMenuItem.Header = "Hide";
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        _mainViewModel?.Cleanup();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    // Tray Icon Implementation

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        _mainWindow?.Hide();
        UpdateToggleMenuText();
    }

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        ToggleWindow();
        UpdateToggleMenuText();
    }

    private void UpdateToggleMenuText()
    {
        if (_toggleMenuItem != null)
        {
            _toggleMenuItem.Header = GetToggleMenuText();
        }
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        // Start cleanup in background, don't wait for it
        if (_mainViewModel != null)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    _mainViewModel.Cleanup();
                }
                catch
                {
                    // Ignore cleanup errors on exit
                }
            });
        }

        // Exit immediately
        Environment.Exit(0);
    }

    private void ToggleWindow()
    {
        if (_mainWindow == null) return;

        if (_mainWindow.IsVisible)
        {
            _mainWindow.Hide();
        }
        else
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }
    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        ToggleWindow();
        UpdateToggleMenuText();
    }

    private void OnMuteClicked(object? sender, EventArgs e)
    {
        if (_mainViewModel != null)
        {
            _mainViewModel.ToggleMuteCommand.Execute(null);
        }
    }

    private void OnGitHubClicked(object? sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/yourusername/SweetTypeTone",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening GitHub repository: {ex.Message}");
        }
    }

    private void CreateTrayIcon()
    {
        try
        {
            var statusMenuItem = new NativeMenuItem
            {
                Header = "SweetTypeTone",
                IsEnabled = false
            };

            _toggleMenuItem = new NativeMenuItem
            {
                Header = GetToggleMenuText(),
                Icon = LoadBitmap($"/Assets/{ShowIconPath}")
            };
            _toggleMenuItem.Click += OnToggleClicked;

            var muteMenuItem = new NativeMenuItem
            {
                Header = "Mute",
                ToggleType = NativeMenuItemToggleType.CheckBox,
                Icon = LoadBitmap($"/Assets/{SoundIconPath}")
            };
            muteMenuItem.Click += OnMuteClicked;
            
            // Bind initial mute state
            if (_mainViewModel != null)
            {
                muteMenuItem.IsChecked = _mainViewModel.IsMuted;
            }

            var githubMenuItem = new NativeMenuItem
            {
                Header = "GitHub Repository",
                Icon = LoadBitmap($"/Assets/{GitHubIconPath}")
            };
            githubMenuItem.Click += OnGitHubClicked;

            var exitMenuItem = new NativeMenuItem
            {
                Header = "Exit",
                Icon = LoadBitmap($"/Assets/{ExitIconPath}")
            };
            exitMenuItem.Click += OnExitClicked;

            // Create the native menu
            var menu = new NativeMenu();
            menu.Add(statusMenuItem);
            menu.Add(_toggleMenuItem);
            menu.Add(new NativeMenuItemSeparator());
            menu.Add(muteMenuItem);
            menu.Add(new NativeMenuItemSeparator());
            menu.Add(githubMenuItem);
            menu.Add(new NativeMenuItemSeparator());
            menu.Add(exitMenuItem);

            // Create main tray icon
            var trayIcon = new TrayIcon
            {
                Icon = LoadWindowIcon($"/Assets/{TrayIconPath}"),
                ToolTipText = "SweetTypeTone - Keyboard Sound Effects",
                Menu = menu
            };

            trayIcon.Clicked += OnTrayIconClicked;

            // Set the tray icon on the application
            var trayIcons = new TrayIcons { trayIcon };
            TrayIcon.SetIcons(this, trayIcons);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating tray icon: {ex.Message}");
        }
    }

    private string GetToggleMenuText()
    {
        return (_mainWindow?.IsVisible == true) ? "Hide" : "Show";
    }

    private WindowIcon? LoadWindowIcon(string path)
    {
        try
        {
            var uri = new Uri($"avares://SweetTypeTone{path}");
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);
            return new WindowIcon(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading window icon: {path} - {ex.Message}");
            return null;
        }
    }

    private Avalonia.Media.Imaging.Bitmap? LoadBitmap(string path)
    {
        try
        {
            var uri = new Uri($"avares://SweetTypeTone{path}");
            using var stream = Avalonia.Platform.AssetLoader.Open(uri);
            return new Avalonia.Media.Imaging.Bitmap(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading bitmap: {path} - {ex.Message}");
            return null;
        }
    }
}