using System;
using System.Collections.Generic;

[Serializable]
public class Landmark3D
{
    public float x;
    public float y;
    public float z;
    public float visibility;
    public float presence;
}

[Serializable]
public class PosePacket
{
    public long timestamp;
    public List<Landmark3D> landmarks;
    public Dictionary<string, float> angles;
    public Dictionary<string, int> virtual_indices;
}
