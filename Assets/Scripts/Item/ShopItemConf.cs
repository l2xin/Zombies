using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShopItemConf
{
    public int id;
    public int index;
    public int priceId;
    public int priceNum;
    public string itemName = "";
    public bool isCanSell = true;
    public int offset = 0;
    public List<int> skinIdList = new List<int>();
    public List<string> skinNameList = new List<string>();
    public Dictionary<int, int> skinIdMap = new Dictionary<int, int>();
}
