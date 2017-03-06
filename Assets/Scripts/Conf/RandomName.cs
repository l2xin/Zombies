using UnityEngine;
using System.Collections;

public class RandomName
{
    public string firstName;
    public string secondName;
    public bool isFaceName;

    public void Read(byte[] bytes, BinayUtil binayUtil)
    {
        binayUtil.readUTFBytes(out firstName, 48);
        binayUtil.readUTFBytes(out secondName, 32);
        int faceName = 0;
        binayUtil.readUnsignedInt(out faceName);
        isFaceName = faceName > 0;
    }
}
