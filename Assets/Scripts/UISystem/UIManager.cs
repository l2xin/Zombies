using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class UIManager
{
    private Dictionary<UIType, BaseUI> m_dicUIObjs;
    private Dictionary<UIType, string> uiClassNameMap;

    private static UIManager _Instance;
    public static UIManager Instance
    {
        get
        {
            if( null == _Instance )
            {
                _Instance = new UIManager();
            }
            return _Instance;
        }
    }

    private UIManager() {}


    public static Canvas canvas { get; protected set; }

    public static Canvas TopCanvas { get; protected set; }

    public static CanvasGroup canvasGroup { get; protected set; }
    public static Camera UICamera { get; protected set; }

    public static RectTransform RectTransform { get; protected set; }

	public static Transform UIRoot { get; protected set; }

    public static EventSystem eventSystem { get; protected set; }

    public static GameObject bloodBarParent;
    public static GameObject followSphere;

    public void SetUp()
    {
        m_dicUIObjs = new Dictionary<UIType, BaseUI>();
        uiClassNameMap = new Dictionary<UIType, string>();
		uiClassNameMap.Add(UIType.BattleGUI, "DefaultBattleScreen");
        uiClassNameMap.Add(UIType.BattleResultScreen, "DefaultGameOverScreen");
        uiClassNameMap.Add(UIType.MainMenuScreen, "DefaultMainMenuScreen");
		uiClassNameMap.Add (UIType.DialogNotice,"DialogNotice");
		uiClassNameMap.Add (UIType.HUDPlayerController,"HUDPlayerController");

		UIRoot = GameObject.Find("Canvas/UI").transform;
		canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        UICamera = GameObject.Find("UICamera").GetComponent<Camera>();
        bloodBarParent = GameObject.Find("BloodBarParent");
        eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        RectTransform = canvas.GetComponent<RectTransform>();

        followSphere = GameObject.Find("Sphere");

        GameObject.DontDestroyOnLoad(UIRoot);
        GameObject.DontDestroyOnLoad(canvas);
        GameObject.DontDestroyOnLoad(UICamera);
        GameObject.DontDestroyOnLoad(bloodBarParent);
        GameObject.DontDestroyOnLoad(eventSystem);
        GameObject.DontDestroyOnLoad(followSphere);

        UIUtils.Setup(canvas);

		PopManager.Setup(GameObject.Find("Canvas").transform);

        if (Global.IsOpenFPSCounter)
        {
            UnityEngine.Object FPSCounterObject = Resources.Load("FPSCounter");
            GameObject fpsCounter = GameObject.Instantiate(FPSCounterObject) as GameObject;
            fpsCounter.name = "FPSCounter";
            GameObject.DontDestroyOnLoad(fpsCounter);
        }
    }

    /// <summary>
    /// 隐藏所有管理器面板
    /// </summary>
    public void HideAll()
    {
        List<UIType> needDestroyPanelList = new List<UIType>();
        foreach (KeyValuePair<UIType, BaseUI> _keyValue in m_dicUIObjs)
        {
            _keyValue.Value.OnHide();
            if (_keyValue.Value.isNeedCache == false)
            {
                needDestroyPanelList.Add(_keyValue.Key);
            }
        }
        while (needDestroyPanelList.Count > 0)
        {
            UIType type = needDestroyPanelList[0];
            needDestroyPanelList.RemoveAt(0);
            BaseUI panel = m_dicUIObjs[type];
            panel.Destroy();
            m_dicUIObjs.Remove(type);
        }
    }

   /// <summary>
   /// 释放资源
   /// </summary>
    public void DisposeAll()
    {
        foreach ( KeyValuePair<UIType, BaseUI> _keyValue in m_dicUIObjs )
        {
            _keyValue.Value.Destroy();
        }
        m_dicUIObjs.Clear();
    }

    public void Hide(UIType _type)
    {
        BaseUI _baseUI = null;
        if (m_dicUIObjs.ContainsKey(_type))
        {
            _baseUI = m_dicUIObjs[_type];
        }
        if (_baseUI != null)
        {
            _baseUI.OnHide();
            if (_baseUI.isNeedCache == false)
            {
                _baseUI.Destroy();
                m_dicUIObjs.Remove(_type);
            }
        }
    }

    public BaseUI GetBaseUI(UIType type)
    {
        BaseUI _baseUI = null;
        if (m_dicUIObjs.ContainsKey(type))
        {
            _baseUI = m_dicUIObjs[type];
        }
        return _baseUI;
    }

    public void Show(UIType type, System.Object data = null, bool isShowLoading = false)
    {
        BaseUI _baseUI = null;
        if (m_dicUIObjs.ContainsKey(type))
        {
            _baseUI = m_dicUIObjs[type];
            ShowModule(_baseUI, type, data);
        }
        else
        {
            //isShowLoading todo  
            MainEntry.Instance.StartLoad(type.ToString().ToLower(), AssetType.prefab, (GameObject uiPrefabObj, string nameTag) =>
            {
                if (m_dicUIObjs.ContainsKey(type))
                {
                    BaseUI baseUI = m_dicUIObjs[type];
                    GameObject.Destroy(baseUI.gameObject);
                    m_dicUIObjs.Remove(type);
                }
                string className = uiClassNameMap[type];
                uiPrefabObj.AddComponent(Type.GetType(className));
                _baseUI = uiPrefabObj.GetComponent<BaseUI>();
                m_dicUIObjs.Add(type, _baseUI);
                ShowModule(_baseUI, type, data);
            });
        }
    }

    public void ShowModule(BaseUI baseUI, UIType type, System.Object data = null)
    {
        baseUI.type = type;
        //_baseUI.SetSiblingIndex(m_dicUIObjs.Count);
        baseUI.transform.SetAsLastSibling();
        baseUI.OnShow(data);
    }

    public void ShowModule(GameObject uiPrefabObj, UIType type, System.Object data = null)
    {
        BaseUI _baseUI = null;
        if (m_dicUIObjs.ContainsKey(type))
        {
            _baseUI = m_dicUIObjs[type];
        }
        else
        {
            string className = uiClassNameMap[type];
            uiPrefabObj.AddComponent(Type.GetType(className));
            _baseUI = uiPrefabObj.GetComponent<BaseUI>();
            m_dicUIObjs.Add(type, _baseUI);
        }   
        ShowModule(_baseUI, type, data);
    }
}
