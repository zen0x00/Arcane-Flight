using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonController : MonoBehaviour
{
    // Linux Ubuntu Python 3.9 path
    public string pythonExePath = "/usr/bin/python3.9";

    // Auto-load main2.py from StreamingAssets
    private string scriptPath;

    private Process pythonProcess;

    void Start()
    {
        // Build path automatically
        scriptPath = Path.Combine(Application.streamingAssetsPath, "Python/posture_streamer.py");
        StartPython();
    }

    void OnApplicationQuit()
    {
        StopPython();
    }

#if UNITY_EDITOR
    void OnDisable()
    {
        StopPython();
    }
#endif

    public void StartPython()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
            return;

        if (!File.Exists(pythonExePath))
        {
            UnityEngine.Debug.LogError("Python executable not found: " + pythonExePath);
            return;
        }

        if (!File.Exists(scriptPath))
        {
            UnityEngine.Debug.LogError("Python script not found in StreamingAssets: " + scriptPath);
            return;
        }

        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonExePath;

        // Wrap script path in quotes
        pythonProcess.StartInfo.Arguments = $"\"{scriptPath}\"";

        pythonProcess.StartInfo.CreateNoWindow = true;
        pythonProcess.StartInfo.UseShellExecute = false;

        pythonProcess.StartInfo.RedirectStandardError = true;
        pythonProcess.StartInfo.RedirectStandardOutput = true;

        pythonProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                UnityEngine.Debug.Log("PY> " + e.Data);
        };

        pythonProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                UnityEngine.Debug.LogError("PY ERR> " + e.Data);
        };

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        UnityEngine.Debug.Log("Python started.");
    }

    public void StopPython()
    {
        try
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess.Dispose();
                pythonProcess = null;
                UnityEngine.Debug.Log("Python process stopped.");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to stop Python: " + ex.Message);
        }
    }
}

