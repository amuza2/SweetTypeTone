using SweetTypeTone.Core.Models;

namespace SweetTypeTone.Core.Interfaces;

/// <summary>
/// Service for monitoring keyboard and mouse input
/// </summary>
public interface IInputMonitorService
{
    /// <summary>
    /// Event raised when input is detected
    /// </summary>
    event EventHandler<InputEvent>? InputDetected;

    /// <summary>
    /// Start monitoring input
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop monitoring input
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Check if monitoring is active
    /// </summary>
    bool IsMonitoring { get; }

    /// <summary>
    /// Dispose resources
    /// </summary>
    void Dispose();
}
