using UnityEngine;
using System;
using System.Text;

public class BinayUtil 
{
    private int position = 4;
    private byte[] bytes;

    public BinayUtil(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public void readUnsignedInt(out int outInt)
    {
        outInt = 0;
        outInt = System.BitConverter.ToInt32(this.bytes, this.position);
        position += 4;
    }

    public void readFloat(out float outFloat)
    {
        outFloat = 0f;
        outFloat = System.BitConverter.ToSingle(this.bytes, this.position);
        position += 4;
    }

    public void readUTFBytes(out string outStr, int strLen)
    {
        outStr = "";
        int removeCnt = 0;

        for (int i = position + strLen - 1; i >= position; i--)
        {
            if (bytes[i] == 0)
            {
                removeCnt++;
            }
            else
            {
                break;
            }
        }

        outStr = Encoding.UTF8.GetString(bytes, position, strLen - removeCnt);
        position += strLen;
    }
}