using UnityEngine;
using System.Collections;

public class BuffConf
{ 
    public int id;
    public int speed;
    public float continueTime;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out id);
        binayUtil.readUnsignedInt(out speed);
        binayUtil.readFloat(out continueTime);
    }
}
