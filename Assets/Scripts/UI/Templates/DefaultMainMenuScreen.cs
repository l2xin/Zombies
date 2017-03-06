using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// nash
/// </summary>
public class DefaultMainMenuScreen : BaseUI
{
	#region public instance fields

    [HideInInspector]
	public float delayBeforePlayingMusic = 0.1f;

    //private Image changeNameBtn;

    private Image startTrainingGameModeBtn;

    private InputField m_NameInput;

    private Text VersionText;
    private GameObject connectStatusEffect;
    private Text connectStatusEffectTips;

    public static float m_sendDataTime; //0f;
    public static DateTime m_flRecHeartTime;// = 0f;
    public static float m_pingTime = 0;

    #endregion

    #region public override methods

    #region -> 账号设置 <-
    //private Button mAccountSettingBtn;
	#endregion

    protected override void OnAwake() 
    {
		isNeedCache = true;
		m_NameInput = transform.FindChild("Login/InputField_Login").GetComponent<InputField>();
        startTrainingGameModeBtn = transform.FindChild("Login/trainingButton").GetComponent<Image>();

        connectStatusEffect = transform.Find("ConnectStatusEffect").gameObject;
        connectStatusEffectTips = connectStatusEffect.GetComponentInChildren<Text>();

        VersionText = transform.FindChild("Login/VersionText").GetComponent<Text>();
    }

    public override void OnShow(System.Object data = null)
    {
        base.OnShow();
        ProcessDebugState();
        EventTriggerListener.Get(startTrainingGameModeBtn.gameObject).onClick = onStartTrainingGame;
        if (Global.isFirstLogin)
        {
#if UNITY_EDITOR
            connectStatusEffect.SetActive(false);
#else
            SetTimeout.Start(HideConnectStatusEffect, 6f);
            connectStatusEffect.SetActive(true);
#endif
        }
        else
        {
            connectStatusEffect.SetActive(false);
            //TryGettPlayerInfo();
        }
        m_NameInput.onEndEdit.AddListener(ChangeUserName);

        this.gameObject.SetActive(true);
        AudioSourceManager.Instance.PlayMusic(1000);
    }

    private void onShowSignPanel(GameObject go)
    {
        UIManager.Instance.Show(UIType.SignPanel);
    }

    private void onShowSharePanel(GameObject go)
    {
        UIManager.Instance.Show(UIType.ShareGamePanel);
    }

    private void onShowShopPanel(GameObject go)
    {
        UIManager.Instance.Show(UIType.ShopPanel);
    }

    private void onStartTrainingGame(GameObject go)
    {
        if (ChangeUserNameAvailable())
        {
            FightManager.gameMode = GameMode.TrainingRoom;
            FightManager.gameNetMode = GameNetMode.offline;
            FightManager.StartOfflineGame();
            //GoToNetworkPlayScreen();
        }
    }

    private void OnHideConnectStatusEffect(BaseEvent obj)
    {
        HideConnectStatusEffect();
        //TryGettPlayerInfo();
    }

    private void HideConnectStatusEffect()
    {
        connectStatusEffect.SetActive(false);
    }

    private void onHeartBeat(BaseEvent obj)
    {
        m_pingTime = (Time.realtimeSinceStartup - m_sendDataTime);
    }


    private void OnCloseAlertEvent()
    {
        OnShow();
    }

    public void ProcessDebugState()
    {
        //VersionText.text = "v" + VersionManager.localVersion;

        string cacheName = LoginManager.Name;
        if (string.IsNullOrEmpty(cacheName) == false)
        {
            m_NameInput.text = cacheName;
        }
        else
        {
            OnChangeName(null);
        }
    }

    private void OnChangeName(GameObject go)
    {
        List<object> objectList = ConfigManager.Instance.GetRandomName();
        string name = (string)objectList[objectList.Count - 1];
        if (ConfigManager.Instance.DirtyWordConfig != null &&
            ConfigManager.Instance.DirtyWordConfig.CheckIsDirtyWord(name))
        {
            PopManager.ShowSimpleItem("请不要在昵称中输入敏感的词汇哦!", PopType.warning);
            return;
        }
        if(GeneralUtils.CheckAccountWithoutSpace(name) == false)
        {
            PopManager.ShowSimpleItem("请不要在昵称中输入空格哦!", PopType.warning);
            return;
        }
        if(m_NameInput != null)
        {
            m_NameInput.text = name;
            LoginManager.Name = m_NameInput.text;
        }
    }

    public override void OnHide()
    {
        base.OnHide();

        SetTimeout.Clear(HideConnectStatusEffect);
        m_NameInput.onEndEdit.RemoveListener(ChangeUserName);
    }

    #endregion


    private void ChangeUserName(string name)
    {
        if( string.IsNullOrEmpty(name.Trim()))
        {
            PopManager.ShowSimpleItem("输入名字不能为空!", PopType.warning);
            return;
        }   
        if (ConfigManager.Instance.DirtyWordConfig != null &&
            ConfigManager.Instance.DirtyWordConfig.CheckIsDirtyWord(name))
        {
            PopManager.ShowSimpleItem("请不要在昵称中输入敏感的词汇哦!", PopType.warning);
            return;
        }
        if (GeneralUtils.CheckAccountWithoutSpace(name) == false)
        {
            PopManager.ShowSimpleItem("请不要在昵称中输入空格哦!", PopType.warning);
            return;
        }
        m_NameInput.text = name;
        return;
    }

    private bool ChangeUserNameAvailable()
    {
        string name = m_NameInput.text;
        if (string.IsNullOrEmpty(name.Trim()))
        {
            PopManager.ShowSimpleItem("输入名字不能为空!", PopType.warning);
            return false;
        }
        if (ConfigManager.Instance.DirtyWordConfig != null &&
            ConfigManager.Instance.DirtyWordConfig.CheckIsDirtyWord(name))
        {
            PopManager.ShowSimpleItem("请不要在昵称中输入敏感的词汇哦!", PopType.warning);
            return false;
        }
        if (GeneralUtils.CheckAccountWithoutSpace(name) == false)
        {
            PopManager.ShowSimpleItem("请不要在昵称中输入空格哦!", PopType.warning);
            return false;
        }
        return true;
    }
}