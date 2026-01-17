using System;

namespace SweetTypeTone.Models;

/// <summary>
/// Represents an input event (keyboard or mouse)
/// </summary>
public class InputEvent
{
    public InputEventType Type { get; set; }
    public InputAction Action { get; set; }
    public int KeyCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public enum InputEventType
{
    Keyboard,
    Mouse
}

public enum InputAction
{
    KeyDown,
    KeyUp,
    MouseDown,
    MouseUp
}
