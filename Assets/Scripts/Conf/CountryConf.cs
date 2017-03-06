using UnityEngine;
using System.Collections;

public class CountryConf
{
    public int id;
    public string country;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUnsignedInt(out id);
        binayUtil.readUTFBytes(out country, 48);
    }
}
