using UnityEngine;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class LinuxWifiScanner : MonoBehaviour
{
    public string Scan()
    {
#if UNITY_STANDALONE_LINUX
        string home = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile);

        string script = System.IO.Path.Combine(home, "wifi_scan.sh");

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = script,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using Process p = Process.Start(psi);
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        return output;
#else
        return "";
#endif
    }
}
