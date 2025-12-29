using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PoseUdpReceiver : MonoBehaviour
{
    [Header("UDP")]
    public int port = 5052;

    [Header("Logging")]
    public bool logEveryFrame = true;
    public bool logAngles = true;
    public bool logCenters = true;

    private const uint MAGIC = 0x504F5345; // "POSE"

    private UdpClient udp;
    private Thread rxThread;
    private volatile bool running;

    public static PoseFrame latest;
    private static readonly object dataLock = new object();

    // ----------------------------------------------------
    void Start()
    {
        udp = new UdpClient(port);
        running = true;

        rxThread = new Thread(ReceiveLoop)
        {
            IsBackground = true
        };
        rxThread.Start();

        Debug.Log("[UDP] Binary Pose Receiver started");
    }

    void OnDisable()
    {
        running = false;
        try { udp?.Close(); } catch { }
    }

    // ----------------------------------------------------
    void ReceiveLoop()
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);

        while (running)
        {
            try
            {
                byte[] data = udp.Receive(ref ep);
                ParsePacket(data);
            }
            catch { }
        }
    }

    // ----------------------------------------------------
    void ParsePacket(byte[] d)
    {
        int o = 0;

        uint magic = BitConverter.ToUInt32(d, o); o += 4;
        if (magic != MAGIC) return;

        o += 4; // version
        ulong ts = BitConverter.ToUInt64(d, o); o += 8;

        ushort lmCount = BitConverter.ToUInt16(d, o); o += 2;
        Vector3[] landmarks = new Vector3[lmCount];

        for (int i = 0; i < lmCount; i++)
        {
            float x = BitConverter.ToSingle(d, o); o += 4;
            float y = BitConverter.ToSingle(d, o); o += 4;
            float z = BitConverter.ToSingle(d, o); o += 4;
            o += 8; // visibility + presence

            landmarks[i] = new Vector3(x, -y, z);
        }

        ushort angleCount = BitConverter.ToUInt16(d, o); o += 2;
        var angles = new System.Collections.Generic.Dictionary<string, float>();

        for (int i = 0; i < angleCount; i++)
        {
            byte len = d[o++];
            string name = Encoding.ASCII.GetString(d, o, len);
            o += len;

            float val = BitConverter.ToSingle(d, o); o += 4;
            angles[name] = val;
        }

        lock (dataLock)
        {
            latest = new PoseFrame
            {
                timestamp = (long)ts,
                landmarks = landmarks,
                angles = angles
            };
        }
    }

    // ----------------------------------------------------
    void Update()
    {
        if (!logEveryFrame) return;

        PoseFrame frame;
        lock (dataLock)
        {
            frame = latest;
        }

        if (frame == null) return;

        // ---- Frame Header ----
        Debug.Log(
            $"[FRAME] t={frame.timestamp} | landmarks={frame.landmarks.Length}"
        );

        // ---- Virtual Centers ----
        if (logCenters && frame.landmarks.Length >= 35)
        {
            Vector3 shoulderCenter = frame.landmarks[33];
            Vector3 hipCenter = frame.landmarks[34];

            Debug.Log(
                $"[CENTERS] Shoulder={shoulderCenter:F3} | Hip={hipCenter:F3}"
            );
        }

        // ---- Angles ----
        if (logAngles && frame.angles != null)
        {
            foreach (var kv in frame.angles)
            {
                Debug.Log($"[ANGLE] {kv.Key} = {kv.Value:F2}");
            }
        }
    }
}

// ----------------------------------------------------
public class PoseFrame
{
    public long timestamp;
    public Vector3[] landmarks;
    public System.Collections.Generic.Dictionary<string, float> angles;
}
