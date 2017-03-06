using UnityEngine;
using System.Collections;

public class RankConf
{
    public int lv;
    public string name;
    public int star;
    public int index;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out lv);
        binayUtil.readUTFBytes(out name, 12);
        binayUtil.readUnsignedInt(out star);
        binayUtil.readUnsignedInt(out index);
    }
}
