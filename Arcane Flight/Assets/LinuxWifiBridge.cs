using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class LinuxWifiLauncher : MonoBehaviour
{
    public void RunWifiScript()
    {
#if UNITY_STANDALONE_LINUX
        // Resolve home directory safely
        string homeDir = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile);

        string scriptPath = System.IO.Path.Combine(
            homeDir, "check-wifi.sh");

        if (!System.IO.File.Exists(scriptPath))
        {
            Debug.LogError("Wi-Fi script not found: " + scriptPath);
            return;
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = scriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using Process p = Process.Start(psi);

        string output = p.StandardOutput.ReadToEnd();
        string error = p.StandardError.ReadToEnd();

        p.WaitForExit();

        Debug.Log("Wi-Fi Script Output:\n" + output);

        if (!string.IsNullOrEmpty(error))
            Debug.LogError("Wi-Fi Script Error:\n" + error);
#endif
    }
}
