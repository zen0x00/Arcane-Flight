using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class PythonPathResolver
{
    public static string GetPythonPath()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        return ResolveWindows();
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        return ResolveLinux();
#else
        Debug.LogError("Unsupported platform for Python path resolution.");
        return null;
#endif
    }

    // ---------------- WINDOWS ----------------
    private static string ResolveWindows()
    {
        string pythonHome = Environment.GetEnvironmentVariable("PYTHON_HOME");
        if (!string.IsNullOrEmpty(pythonHome))
        {
            string exe = Path.Combine(pythonHome, "python.exe");
            if (File.Exists(exe)) return exe;
        }

        string where = RunCommand("where", "python");
        if (!string.IsNullOrEmpty(where))
            return where.Split('\n')[0].Trim();

        string[] common =
        {
            @"C:\Python39\python.exe",
            @"C:\Python310\python.exe",
            @"C:\Python311\python.exe",
            @"C:\Program Files\Python39\python.exe",
            @"C:\Program Files\Python310\python.exe"
        };

        foreach (var p in common)
            if (File.Exists(p)) return p;

        UnityEngine.Debug.LogError("Python not found on Windows.");
        return null;
    }

    // ---------------- LINUX ----------------
    private static string ResolveLinux()
    {
        string pythonHome = Environment.GetEnvironmentVariable("PYTHON_HOME");
        if (!string.IsNullOrEmpty(pythonHome))
        {
            string exe = Path.Combine(pythonHome, "bin/python3");
            if (File.Exists(exe)) return exe;
        }

        string py3 = RunCommand("which", "python3");
        if (!string.IsNullOrEmpty(py3)) return py3.Trim();

        string py = RunCommand("which", "python");
        if (!string.IsNullOrEmpty(py)) return py.Trim();

        UnityEngine.Debug.LogError("Python not found on Linux.");
        return null;
    }

    // ---------------- EXEC ----------------
    private static string RunCommand(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
                return p.StandardOutput.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }
}
