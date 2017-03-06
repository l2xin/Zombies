using UnityEngine;
using System.Collections;

public class RoleForTeamConf
{
    public int lv;
    public int job;
    public int scale;
    public int defaultScale;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out lv);
        binayUtil.readUnsignedInt(out job);
        binayUtil.readUnsignedInt(out scale);
        binayUtil.readUnsignedInt(out defaultScale);
    }
}
