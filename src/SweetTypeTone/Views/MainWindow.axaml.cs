using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SweetTypeTone.ViewModels;
using SweetTypeTone.Core.Models;
using System;
using System.Threading.Tasks;

namespace SweetTypeTone.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Handle keyboard events for tester
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Update keyboard tester
        if (KeyTesterText != null)
        {
            KeyTesterText.Text = $"↓ {e.Key}";
        }
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        // Update keyboard tester
        if (KeyTesterText != null)
        {
            KeyTesterText.Text = $"↑ {e.Key}";
            
            // Reset after a short delay
            Task.Delay(500).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (KeyTesterText != null)
                    {
                        KeyTesterText.Text = "...";
                    }
                });
            });
        }
    }
    
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Enable window dragging
        BeginMoveDrag(e);
    }
    
    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void SoundPackComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        // Move focus away from ComboBox after selection to prevent keyboard interference
        if (sender is ComboBox comboBox)
        {
            // Close the dropdown
            comboBox.IsDropDownOpen = false;
            
            // Clear focus from ComboBox
            var focusManager = TopLevel.GetTopLevel(this)?.FocusManager;
            focusManager?.ClearFocus();
        }
    }
}