using System;
using System.Diagnostics;

namespace SweetTypeTone.Services;

/// <summary>
/// Checks and helps setup Linux input permissions
/// </summary>
public static class PermissionChecker
{
    public static bool HasInputPermissions()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "groups",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return output.Contains("input");
        }
        catch
        {
            return false;
        }
    }
    
    public static bool TrySetupPermissions()
    {
        try
        {
            var username = Environment.UserName;
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pkexec",
                    Arguments = $"usermod -aG input {username}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit();
            
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
