using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SweetTypeTone.Interfaces;
using SweetTypeTone.Models;

namespace SweetTypeTone.Services;

/// <summary>
/// Linux input monitor using evdev
/// </summary>
public class LinuxInputMonitorService : IInputMonitorService
{
    public event EventHandler<InputEvent>? InputDetected;
    
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _monitoringTask;
    private readonly List<string> _devicePaths = new();
    private bool _disposed;

    public bool IsMonitoring { get; private set; }

    public async Task StartAsync()
    {
        if (IsMonitoring)
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        
        // Find input devices
        await DiscoverInputDevicesAsync();

        if (_devicePaths.Count == 0)
        {
            throw new InvalidOperationException(
                "No input devices found. Make sure you have permission to read from /dev/input/. " +
                "You may need to add your user to the 'input' group: sudo usermod -a -G input $USER");
        }

        IsMonitoring = true;
        _monitoringTask = Task.Run(() => MonitorInputAsync(_cancellationTokenSource.Token));
    }

    public async Task StopAsync()
    {
        if (!IsMonitoring)
            return;

        _cancellationTokenSource?.Cancel();
        
        if (_monitoringTask != null)
        {
            await _monitoringTask;
        }

        IsMonitoring = false;
    }

    private async Task DiscoverInputDevicesAsync()
    {
        await Task.Run(() =>
        {
            _devicePaths.Clear();
            
            var inputDir = "/dev/input";
            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine($"Input directory {inputDir} does not exist");
                return;
            }

            // Look for event devices
            var eventDevices = Directory.GetFiles(inputDir, "event*")
                .Where(path => File.Exists(path))
                .ToList();

            Console.WriteLine($"Found {eventDevices.Count} input devices");

            foreach (var device in eventDevices)
            {
                try
                {
                    // Try to open the device to check if we have permission
                    using var fs = File.OpenRead(device);
                    _devicePaths.Add(device);
                    Console.WriteLine($"✓ Added device: {device}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Cannot access {device}: {ex.Message}");
                }
            }
            
            Console.WriteLine($"Total accessible devices: {_devicePaths.Count}");
            
            // If no devices are accessible, try to setup permissions
            if (_devicePaths.Count == 0 && eventDevices.Count > 0)
            {
                TrySetupPermissions();
            }
        });
    }

    private void TrySetupPermissions()
    {
        try
        {
            Console.WriteLine("\n⚠ No input devices accessible. Attempting to setup permissions...");
            
            // Check if pkexec is available
            var pkexecPath = FindExecutable("pkexec");
            var setupScriptPath = "/usr/local/bin/sweettypetone-setup";
            
            if (pkexecPath != null && File.Exists(setupScriptPath))
            {
                Console.WriteLine("Running permission setup (you may be prompted for your password)...");
                
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pkexecPath,
                        Arguments = $"{setupScriptPath} {Environment.UserName}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                
                process.Start();
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    Console.WriteLine("\n✓ Permissions setup complete!");
                    Console.WriteLine("⚠ IMPORTANT: You must log out and log back in for changes to take effect.");
                }
                else
                {
                    Console.WriteLine("\n✗ Permission setup failed or was cancelled.");
                    ShowManualInstructions();
                }
            }
            else
            {
                ShowManualInstructions();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during permission setup: {ex.Message}");
            ShowManualInstructions();
        }
    }

    private void ShowManualInstructions()
    {
        Console.WriteLine("\n============================================");
        Console.WriteLine("Manual Permission Setup Required:");
        Console.WriteLine("============================================");
        Console.WriteLine("Run these commands in a terminal:");
        Console.WriteLine("");
        Console.WriteLine("  sudo usermod -a -G input $USER");
        Console.WriteLine("  sudo tee /etc/udev/rules.d/99-sweettypetone.rules > /dev/null << 'EOF'");
        Console.WriteLine("  KERNEL==\"event*\", SUBSYSTEM==\"input\", MODE=\"0660\", GROUP=\"input\"");
        Console.WriteLine("  EOF");
        Console.WriteLine("  sudo udevadm control --reload-rules");
        Console.WriteLine("  sudo udevadm trigger");
        Console.WriteLine("");
        Console.WriteLine("Then LOG OUT and LOG BACK IN.");
        Console.WriteLine("============================================\n");
    }

    private string? FindExecutable(string name)
    {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? Array.Empty<string>();
        
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, name);
            if (File.Exists(fullPath))
                return fullPath;
        }
        
        return null;
    }

    private async Task MonitorInputAsync(CancellationToken cancellationToken)
    {
        var tasks = _devicePaths.Select(devicePath => 
            Task.Run(() => MonitorDeviceAsync(devicePath, cancellationToken), cancellationToken)
        ).ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }

    private async Task MonitorDeviceAsync(string devicePath, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new FileStream(devicePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 
                bufferSize: 4096, // Larger buffer for better performance
                useAsync: true);
            var buffer = new byte[24]; // Size of input_event struct on 64-bit Linux

            while (!cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                
                if (bytesRead == 24)
                {
                    ParseInputEvent(buffer);
                }
                else if (bytesRead == 0)
                {
                    // Device disconnected or end of stream
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error monitoring device {devicePath}: {ex.Message}");
        }
    }

    private void ParseInputEvent(byte[] buffer)
    {
        // Parse Linux input_event structure
        // struct input_event {
        //     struct timeval time;  // 16 bytes on 64-bit
        //     __u16 type;           // 2 bytes
        //     __u16 code;           // 2 bytes
        //     __s32 value;          // 4 bytes
        // };

        var type = BitConverter.ToUInt16(buffer, 16);
        var code = BitConverter.ToUInt16(buffer, 18);
        var value = BitConverter.ToInt32(buffer, 20);

        // EV_KEY = 1 (keyboard and mouse buttons)
        if (type == 1)
        {
            var inputEvent = new InputEvent
            {
                KeyCode = code,
                Timestamp = DateTime.Now
            };

            // Determine if it's keyboard or mouse
            // Mouse buttons are typically 0x110-0x117 (BTN_LEFT, BTN_RIGHT, etc.)
            if (code >= 0x110 && code <= 0x117)
            {
                inputEvent.Type = InputEventType.Mouse;
                inputEvent.Action = value == 1 ? InputAction.MouseDown : InputAction.MouseUp;
            }
            else
            {
                inputEvent.Type = InputEventType.Keyboard;
                inputEvent.Action = value == 1 ? InputAction.KeyDown : 
                                   value == 0 ? InputAction.KeyUp : 
                                   InputAction.KeyDown; // value == 2 is key repeat
            }

            // Only fire events for press (1) and release (0), not repeat (2)
            if (value == 0 || value == 1)
            {
                InputDetected?.Invoke(this, inputEvent);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopAsync().Wait();
        _cancellationTokenSource?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
