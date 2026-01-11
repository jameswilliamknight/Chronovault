using System.Diagnostics;

namespace Chronovault.Packager.Helpers;

/// <summary>
/// Helpers for running external processes.
/// </summary>
public static class ProcessHelpers
{
    /// <summary>
    /// Runs a command asynchronously with output streaming.
    /// </summary>
    public static async Task<int> RunCommandAsync(string command, string[] arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = string.Join(" ", arguments),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null) Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null) Console.Error.WriteLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}

