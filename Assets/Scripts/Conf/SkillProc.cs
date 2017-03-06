using UnityEngine;
using System.Collections;

public class SkillProc
{ 
    public int id;
    public int lv;
    public int effect;
    public int projectitle;
    public int camp;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out id);
        binayUtil.readUnsignedInt(out lv);
        binayUtil.readUnsignedInt(out effect);
        binayUtil.readUnsignedInt(out projectitle);
        binayUtil.readUnsignedInt(out camp);
    }
}
