using UnityEngine;
using System.Collections;

public class RoleConf
{
    public int lv;
    public int job;
    public int scale;
    public int defaultScale;
    public int cameraDistance;
    public float bloodOffset;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out lv);
        binayUtil.readUnsignedInt(out job);
        binayUtil.readUnsignedInt(out scale);
        binayUtil.readUnsignedInt(out defaultScale);
        binayUtil.readUnsignedInt(out cameraDistance);
        binayUtil.readFloat(out bloodOffset);
    }
}
