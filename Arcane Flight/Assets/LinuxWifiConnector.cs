using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class LinuxWifiConnector : MonoBehaviour
{
    public bool Connect(string ssid, string password)
    {
#if UNITY_STANDALONE_LINUX
        string home = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile);

        string script = System.IO.Path.Combine(home, "wifi_connect.sh");

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = $"\"{script}\" \"{ssid}\" \"{password}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process p = Process.Start(psi);
        string result = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        return result.Contains("SUCCESS");
#else
        return false;
#endif
    }
}
