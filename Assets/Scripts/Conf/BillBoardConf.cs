using UnityEngine;
using System.Collections;

/// <summary>
/// 公告版配置
/// </summary>
public class BillBoardConf
{
	public int numId;
	public int boardType;
	public int srcId;
	public string titleName;
	public string buttonName;
	public string words;

	public void Read(byte[] bytes, BinayUtil binayUtil)
	{
		binayUtil.readUnsignedInt(out numId);
		binayUtil.readUnsignedInt(out boardType);
		binayUtil.readUnsignedInt(out srcId);

		//10字
		binayUtil.readUTFBytes(out titleName,30);
		//5字
		binayUtil.readUTFBytes(out buttonName,15);
		//800字
		binayUtil.readUTFBytes(out words,2400);
	}
}
