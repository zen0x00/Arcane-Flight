using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonManager : MonoBehaviour
{
    [Header("Python")]
    public bool autoStartOnPlay = true;

    [Tooltip("Relative to StreamingAssets/Python/")]
    public string pythonScriptName = "posture_streamer.py";

    [Tooltip("Absolute python path (Linux). Example: /usr/bin/python3")]
    public string linuxPythonPath = "/usr/bin/python3";

    private Process pythonProcess;
    private string pythonExe;
    private string scriptPath;

    // ------------------------------------------------
    void Awake()
    {
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        pythonExe = linuxPythonPath;
#else
        pythonExe = PythonPathResolver.GetPythonPath();
#endif

        UnityEngine.Debug.Log("[PY] Python exe: " + pythonExe);

        if (string.IsNullOrEmpty(pythonExe) || !File.Exists(pythonExe))
        {
            UnityEngine.Debug.LogError("Python executable not found: " + pythonExe);
            enabled = false;
            return;
        }

        scriptPath = Path.Combine(
            Application.streamingAssetsPath,
            "Python",
            pythonScriptName
        );

        UnityEngine.Debug.Log("[PY] Script path: " + scriptPath);

        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError("Python script not found: " + scriptPath);
            enabled = false;
        }
    }

    // ------------------------------------------------
    void Start()
    {
        if (autoStartOnPlay)
            StartPython();
    }

    void OnDisable() => StopPython();
    void OnApplicationQuit() => StopPython();

#if UNITY_EDITOR
    void OnDestroy() => StopPython();
#endif

    // ------------------------------------------------
    public void StartPython()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
            return;

        var psi = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"-u \"{scriptPath}\"",
            WorkingDirectory = Path.GetDirectoryName(scriptPath),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // -------- Linux environment fix --------
        psi.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
        psi.EnvironmentVariables["PATH"] = "/usr/bin:/bin:/usr/local/bin";

        pythonProcess = new Process
        {
            StartInfo = psi,
            EnableRaisingEvents = true
        };

        // STDOUT
        pythonProcess.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.Log("[PY] " + e.Data);
        };

        // STDERR (show everything for Linux debug)
        pythonProcess.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                UnityEngine.Debug.LogError("[PY STDERR] " + e.Data);
        };

        pythonProcess.Exited += (s, e) =>
        {
            UnityEngine.Debug.Log("[PY] Python process exited.");
        };

        try
        {
            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            UnityEngine.Debug.Log("[PY] Python posture streamer started.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[PY] Failed to start Python: " + ex);
        }
    }

    // ------------------------------------------------
    public void StopPython()
    {
        if (pythonProcess == null)
            return;

        try
        {
            if (!pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess.WaitForExit(2000);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning("[PY] Kill failed: " + ex.Message);
        }

        try { pythonProcess.Dispose(); } catch { }

        pythonProcess = null;
        UnityEngine.Debug.Log("[PY] Python posture streamer stopped.");
    }
}
