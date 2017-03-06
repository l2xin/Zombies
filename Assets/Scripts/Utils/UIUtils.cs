using UnityEngine;
using System.Collections.Generic;

internal class UIUtils
{
    public static void Setup(Canvas canvas)
    {
        RectTransform trans = canvas.GetComponent<RectTransform>();
        SCREEN_WIDTH = (int)trans.rect.width;
        SCREEN_HEIGHT = (int)trans.rect.height;
    }

    internal static void ResetRectTransform(RectTransform rectTransform)
    {
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    internal static GameObject AddChild(Transform parent, GameObject prefab, Vector3 pos)
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.gameObject.layer;
        }
        return go;
    }

    internal static void GenerateObjCells(ref List<GameObject> listObjs, int iDataNums, ref GameObject itemModel, ref Transform itemParent)
    {
        if (listObjs.Count < iDataNums)
        {
            int _iNeedGenerateNums = iDataNums - listObjs.Count;
            for (int _index = 0; _index < _iNeedGenerateNums; ++_index)
            {
                GameObject _obj = UIUtils.AddChild(itemParent, itemModel, Vector3.zero);
                listObjs.Add(_obj);
            }
        }

        for (int _index = 0; _index < listObjs.Count; ++_index)
        {
            listObjs[_index].SetActive(false);
            listObjs[_index].transform.SetParent(itemParent);
        }
    }


    public static int SCREEN_WIDTH;
    public static int SCREEN_HEIGHT;

    /// <summary>
    /// 设置一个UI屏幕中心对议题
    /// </summary>
    /// <param name="trans"></param>
    internal static void CenterToScreen(Transform trans)
    {
        Rect rect = trans.GetComponent<RectTransform>().rect;
        float x = (SCREEN_WIDTH - rect.width) / 55f;
        float y = (SCREEN_HEIGHT - rect.height) / 55f;
        trans.localPosition = new Vector3(x, y, 0);
    }

    internal static void AddChild(Transform parent, Transform child)
    {
        if (child != null && parent != null)
        {
            child.SetParent(parent.transform, false);
            //child.parent = parent.transform;
            //child.localPosition = Vector3.zero;
            //child.localRotation = Quaternion.identity;
            //child.localScale = Vector3.one;
            child.gameObject.layer = parent.gameObject.layer;
        }
    }

    internal static T GetComponent<T>(Transform transform, string name = "") where T : Component
    {
        Transform trans = transform.Find(name);

        if (trans)
        {
            GameObject obj = trans.gameObject;
            return obj.GetComponent<T>();
        }
        return null;
    }
}