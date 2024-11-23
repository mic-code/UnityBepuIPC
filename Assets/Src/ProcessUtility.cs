using System.Diagnostics;

public class ProcessUtility
{
    public static Process CreateProcess(string path, string workingDir = null, string arguments = null, bool redirect = false, bool useShellExecute = false)
    {
        if (workingDir == null)
            workingDir = "";

        //logger.LogInformation(command);
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = workingDir,
            FileName = path,
            Arguments = arguments,
            UseShellExecute = useShellExecute,
            CreateNoWindow = true,
            RedirectStandardOutput = redirect,
            RedirectStandardError = redirect,
        };

        var process = new Process
        {
            StartInfo = startInfo
        };

        return process;
    }

    public static Process StartProcess(string path, string workingDir = null, string arguments = null, bool redirect = false, bool useShellExecute = false)
    {
        var process = CreateProcess(path, workingDir, arguments, redirect, useShellExecute);
        process.Start();

        return process;
    }
}