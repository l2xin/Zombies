using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 浮动的弹框提示
/// </summary>
public class PopManager
{
    private static GameObject curObj;
    private static Transform _parent;
    private static List<PopData> popDataList;
    private static bool isShowPop = false;

    public static void Setup(Transform parent)
    {
        _parent = parent;
        popDataList = new List<PopData>();
    }

    public static void ShowSimpleItem(string str, PopType type = PopType.normal, float time = 1.5f)
    {
        PopData popData = new PopData();
        popData.str = str;
        popData.type = type;
        popData.time = time;
        if (isShowPop == false)
        {
            ShowSimpleItem(popData);
        }
        else
        {
            popDataList.Add(popData);
        }
    }

    public static void ShowSimpleItem(PopData popData)
    {
        isShowPop = true;
        string str = popData.str;
        PopType type = popData.type;
        float time = popData.time;
        UnityEngine.Object res = Resources.Load("UIPrefab/popitem");
        GameObject obj = (GameObject)GameObject.Instantiate(res);
        UIUtils.AddChild(_parent, obj.transform);
        UIUtils.CenterToScreen(obj.transform);

        Color color = Color.white;
        switch (type)
        {
            case PopType.normal:
                color = Color.white;
                break;
            case PopType.warning:
                color = new Color(212 / 255f, 0f, 92 / 255f, 1f);
                break;
        }

        Text txt = obj.transform.FindChild("Text").GetComponent<Text>();
        txt.text = str;
        txt.color = color;
        curObj = obj;

        RectTransform rectTr = obj.GetComponent<RectTransform>();
        Vector3 ov = rectTr.localPosition;

        rectTr.localPosition = new Vector3(ov.x, -50, ov.z);
        obj.transform.DOLocalMoveY(50, 1.0f);

        SetTimeout.Start(() =>
        {
            GameObject.Destroy(obj);
        }, time);
        SetTimeout.Start(ShowNextPop, 0.5f);
    }

    private static void ShowNextPop()
    {
        isShowPop = false;
        if (popDataList.Count > 0)
        {
            PopData popData = popDataList[0];
            popDataList.RemoveAt(0);
            ShowSimpleItem(popData);
        }
    }
}

public struct PopData
{
    public string str;
    public PopType type;
    public float time;
}

public enum PopType
{
    normal = 0,
    warning = 1,
}