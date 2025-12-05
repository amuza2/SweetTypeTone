using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SweetTypeTone.Core.Services;
using System.Threading.Tasks;

namespace SweetTypeTone.Views;

public partial class PermissionSetupWindow : Window
{
    public bool SetupCompleted { get; private set; }
    
    public PermissionSetupWindow()
    {
        InitializeComponent();
    }
    
    private async void SetupButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            button.IsEnabled = false;
            button.Content = "Setting up...";
        }
        
        await Task.Run(() =>
        {
            SetupCompleted = PermissionChecker.TrySetupPermissions();
        });
        
        if (SetupCompleted)
        {
            await ShowSuccessDialog();
        }
        else
        {
            await ShowErrorDialog();
        }
        
        Close();
    }
    
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        SetupCompleted = false;
        Close();
    }
    
    private async Task ShowSuccessDialog()
    {
        var dialog = new Window
        {
            Title = "Setup Complete",
            Width = 450,
            Height = 250,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.Parse("#1a1a1a"))
        };
        
        var content = new StackPanel
        {
            Margin = new Thickness(30),
            Spacing = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        
        content.Children.Add(new TextBlock
        {
            Text = "✅ Setup Complete!",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#4CAF50")),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });
        
        content.Children.Add(new TextBlock
        {
            Text = "Permissions configured successfully!\n\n⚠️ IMPORTANT: You must log out and log back in for changes to take effect.\n\nAfter logging back in, run SweetTypeTone again.",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#EEEEEE")),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        });
        
        var okButton = new Button
        {
            Content = "OK",
            Width = 100,
            Height = 35,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#4CAF50")),
            Foreground = Brushes.White
        };
        okButton.Click += (s, e) => dialog.Close();
        content.Children.Add(okButton);
        
        dialog.Content = content;
        await dialog.ShowDialog(this);
    }
    
    private async Task ShowErrorDialog()
    {
        var dialog = new Window
        {
            Title = "Setup Failed",
            Width = 450,
            Height = 250,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.Parse("#1a1a1a"))
        };
        
        var content = new StackPanel
        {
            Margin = new Thickness(30),
            Spacing = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        
        content.Children.Add(new TextBlock
        {
            Text = "❌ Setup Failed",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B")),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });
        
        content.Children.Add(new TextBlock
        {
            Text = "Failed to configure permissions.\n\nPlease run manually in terminal:\nsudo usermod -aG input $USER\n\nThen log out and log back in.",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#EEEEEE")),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center
        });
        
        var okButton = new Button
        {
            Content = "OK",
            Width = 100,
            Height = 35,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.Parse("#555555")),
            Foreground = Brushes.White
        };
        okButton.Click += (s, e) => dialog.Close();
        content.Children.Add(okButton);
        
        dialog.Content = content;
        await dialog.ShowDialog(this);
    }
}
