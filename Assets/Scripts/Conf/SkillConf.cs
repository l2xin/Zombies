using UnityEngine;
using System.Collections;

public class SkillConf
{
    public int id;
    public string name;
    public string spec;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out id);
        binayUtil.readUTFBytes(out name, 24);
        binayUtil.readUTFBytes(out spec, 100);
    }
}