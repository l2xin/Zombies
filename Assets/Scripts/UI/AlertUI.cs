using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class AlertUI : MonoBehaviour 
{
    public enum emAlertUIType
    {
        None,
        CommonDialog,
        QuitGameDialog,
        MatchErrorDialog,
        ForceUpdateDialog,
    }

    private static GameObject currentLoadingEffect;
    public static GameObject ShowLoadingEffect(bool isShowMask)
    {
        HideLoadingEffect();
        GameObject go = PoolUtil.SpawnerGameObject(FightManager.config.roundOptions.showWaitLoadingEffect);
        if(go != null)
        {
            Transform mask = go.transform.Find("mask");
            if (mask != null)
            {
                mask.gameObject.SetActive(isShowMask);
            }
            go.transform.parent = UIManager.UIRoot.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }
        currentLoadingEffect = go;
        return go;
    }

    public static void HideLoadingEffect()
    {
        PoolUtil.Despawner(currentLoadingEffect);
        currentLoadingEffect = null;
    }

    public static void HideLoadingEffect(GameObject go)
    {
        if(currentLoadingEffect == go)
        {
            PoolUtil.Despawner(currentLoadingEffect);
            currentLoadingEffect = null;
        }
        else
        {
            PoolUtil.Despawner(go);
        }
    }


    public static AlertUI Show( emAlertUIType style = emAlertUIType.CommonDialog, 
                                string titleName = "",
                                string content = "", 
                                System.Action OnClosetCallBack = null,
                                System.Action OnLeftCallBack = null, System.Action OnRightCallBack = null, System.Action OnAddGroupCallBack = null,
                                string leftBtnName = "确定", string rightBtnName = "取消")
    {
        if ( m_DicTypeActivate.ContainsKey( style ) )
        {
            if ( m_DicTypeActivate[style] )
            {
                return null;
            }
            else
            {
                m_DicTypeActivate[style] = true;
                return ShowFunction(style, titleName, content, OnClosetCallBack, OnLeftCallBack, OnRightCallBack, OnAddGroupCallBack, leftBtnName, rightBtnName);
            }
        }
        else
        {
            m_DicTypeActivate.Add(style, true);
        }

        return ShowFunction(style, titleName, content, OnClosetCallBack, OnLeftCallBack, OnRightCallBack, OnAddGroupCallBack, leftBtnName, rightBtnName);
    }

    private static AlertUI ShowFunction(emAlertUIType style,
                                        string titleName,
                                        string content,
                                        System.Action OnClosetCallBack,
                                        System.Action OnLeftCallBack, System.Action OnRightCallBack, System.Action OnAddGroupCallBack,
                                        string leftBtnName, string rightBtnName)
    {
        GameObject _ui;
        if (_cacheList.Count > 0)
        {
            _ui = _cacheList.Dequeue();
        }
        else
        {
            _ui = Instantiate(_prefab);
            _ui.SetActive(true);
            _ui.AddComponent<AlertUI>();
        }
        RectTransform tranf = _ui.GetComponent<RectTransform>();
        tranf.localScale = new Vector2(0.3f, 0.3f);
        tranf.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        AlertUI _script = _ui.GetComponent<AlertUI>();
        _script.SetStyle(style);
        _script.SetTitleName(titleName);
        _script.SetContent(content);
        _script.SetCallBack(OnClosetCallBack, OnLeftCallBack, OnRightCallBack, OnAddGroupCallBack);
        _script.SetBtnName(style, leftBtnName, rightBtnName);
        tranf.SetParent(_root, false);
        _ui.SetActive(true);

        _root.FindChild("Mask").gameObject.SetActive(true);

        return _script;
    }

    public static void Setup()
    {
        _root = GameObject.FindGameObjectWithTag("TopCanvas").transform;
        _cacheList = new Queue<GameObject>();
        _prefab = Resources.Load<GameObject>("AlertUI");
        m_DicTypeActivate = new Dictionary<emAlertUIType, bool>();
    }

    void Awake()
    {
        _titleTxt = transform.FindChild("TitleTxt").GetComponent<Text>();

        _CloseBtn = transform.FindChild("CloseButton").GetComponent<Button>();
        _CloseBtn.onClick.AddListener(OnCloseBtnClicked);

        #region CommonDialog
        _CommonDialog = transform.FindChild("CommonDialog").gameObject;
        _contentTxt = _CommonDialog.transform.FindChild("ContentTxt").GetComponent<Text>();
        _leftButtonCommonDialog = _CommonDialog.transform.FindChild("BtnContent/leftBtn").GetComponent<Button>();
        _leftButtonCommonDialog.onClick.AddListener(OnLeftBtnClicked);
        _rightButtonCommonDialog = _CommonDialog.transform.FindChild("BtnContent/rightBtn").GetComponent<Button>();
        _rightButtonCommonDialog.onClick.AddListener(OnRightBtnClicked);
        #endregion

        #region QuitGameDialog
        _QuitGameDialog = transform.FindChild("QuitGameDialog").gameObject;
        _AddGroupBtn = _QuitGameDialog.transform.FindChild("AddGroupBtn").GetComponent<Button>();
        _AddGroupBtn.onClick.AddListener( OnAddGroupBtnChicked );
        _QuitGameBtn = _QuitGameDialog.transform.FindChild("BtnContent/QuitGameBtn").GetComponent<Button>();
        _QuitGameBtn.onClick.AddListener(OnLeftBtnClicked);
        _BackGameBtn = _QuitGameDialog.transform.FindChild("BtnContent/BackGameBtn").GetComponent<Button>();
        _BackGameBtn.onClick.AddListener(OnRightBtnClicked);
        #endregion
    }

    public void SetStyle(emAlertUIType style)
    {
        // 设置对话框按钮的隐藏和显示
        // TODO:
        // ...

        m_emCurAlertUITYpe = style;

        switch( style )
        {
            case emAlertUIType.CommonDialog:
                _CloseBtn.gameObject.SetActive( false );
                _QuitGameDialog.SetActive( false );
                _CommonDialog.SetActive( true );
                _leftButtonCommonDialog.gameObject.SetActive( true );
                _rightButtonCommonDialog.gameObject.SetActive( true );
                break;
            case emAlertUIType.QuitGameDialog:
                _CloseBtn.gameObject.SetActive( true );
                _QuitGameDialog.SetActive( true );
                _CommonDialog.SetActive( false );
                break;
            case emAlertUIType.MatchErrorDialog:
                _CloseBtn.gameObject.SetActive( true );
                _QuitGameDialog.SetActive( false );
                _CommonDialog.SetActive( true );
                _leftButtonCommonDialog.gameObject.SetActive( true );
                _rightButtonCommonDialog.gameObject.SetActive( false );
                break;
            case emAlertUIType.ForceUpdateDialog:
                _CloseBtn.gameObject.SetActive( false );
                _QuitGameDialog.SetActive( false );
                _CommonDialog.SetActive( true );
                _leftButtonCommonDialog.gameObject.SetActive( true );
                _rightButtonCommonDialog.gameObject.SetActive( false );
                break;
        }
    }

    public void SetTitleName( string titleName )
    {
        _titleTxt.text = titleName;
    }

    public void SetContent(string content)
    {
        _contentTxt.text = content;
    }

    public void SetCallBack(System.Action OnClosetCallBack, System.Action OnLeftCallBack, System.Action OnRightCallBack, System.Action OnAddGroupCallBack)
    {
        _onClosetCallBack = OnClosetCallBack;
        _onLeftCallBack = OnLeftCallBack;
        _onRightCallBack = OnRightCallBack;
        _onAddGroupCallBack = OnAddGroupCallBack;
    }

    public void SetBtnName( emAlertUIType style, string leftBtnName = "", string rightBtnName = "" )
    {
        switch (style)
        {
            case emAlertUIType.CommonDialog:
                _leftButtonCommonDialog.transform.FindChild("Text").GetComponent<Text>().text = leftBtnName;
                _rightButtonCommonDialog.transform.FindChild("Text").GetComponent<Text>().text = rightBtnName;
                break;
            case emAlertUIType.QuitGameDialog:
                _QuitGameBtn.transform.FindChild("Text").GetComponent<Text>().text = leftBtnName;
                _BackGameBtn.transform.FindChild("Text").GetComponent<Text>().text = rightBtnName;
                break;
            case emAlertUIType.ForceUpdateDialog:
            case emAlertUIType.MatchErrorDialog:
                _leftButtonCommonDialog.transform.FindChild("Text").GetComponent<Text>().text = leftBtnName;
                break;
        }
    }

    private void OnLeftBtnClicked()
    {
        if (_onLeftCallBack != null)
            _onLeftCallBack.Invoke();
        Hide();
    }

    private void OnRightBtnClicked()
    {
        if (_onRightCallBack != null)
            _onRightCallBack.Invoke();
        Hide();
    }

    private void OnAddGroupBtnChicked()
    {
        if (_onAddGroupCallBack != null)
        {
            _onAddGroupCallBack.Invoke();
        }
        Hide();
    }

    private void OnCloseBtnClicked()
    {
        if (_onClosetCallBack != null)
        {
            _onClosetCallBack.Invoke();
        }
        Hide();
    }

    public void Hide()
    {
        _onClosetCallBack = null;
        _onLeftCallBack = null;
        _onRightCallBack = null;
        _onAddGroupCallBack = null;

        gameObject.SetActive(false);
        _cacheList.Enqueue(gameObject);
        m_DicTypeActivate[m_emCurAlertUITYpe] = false;

        bool _bFind = false;

        foreach( KeyValuePair<emAlertUIType,bool> _keyValue in m_DicTypeActivate )
        {
            if (_keyValue.Value)
            {
                _bFind = true;
            }
        }

        if( !_bFind )
        {
            transform.parent.FindChild("Mask").gameObject.SetActive( false );
        }
    }

    private Text _titleTxt;
    
    
    private GameObject _CommonDialog;
    private Text _contentTxt;
    private Button _leftButtonCommonDialog;
    private Button _rightButtonCommonDialog;


    private GameObject _QuitGameDialog;
    private Button _AddGroupBtn;
    private Button _QuitGameBtn;
    private Button _BackGameBtn;
    private Button _CloseBtn;

    private System.Action _onLeftCallBack;
    private System.Action _onRightCallBack;
    private System.Action _onAddGroupCallBack;
    private System.Action _onClosetCallBack;

    private static Transform _root;
    private static Queue<GameObject> _cacheList;
    private static GameObject _prefab;

    private static emAlertUIType m_emCurAlertUITYpe = emAlertUIType.None;
    private static Dictionary<emAlertUIType, bool> m_DicTypeActivate;
}
