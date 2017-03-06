using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// nash
/// </summary>
public class DefaultBattleScreen : BaseUI
{
	public static GameObject BossTip;

    [HideInInspector]
    public Text info;
    [HideInInspector]
    public Text timer;

    protected bool showInputs = true;
    protected bool hiding = false;

    [HideInInspector]
    public Transform progressBar;
    [HideInInspector]
    public Transform respawn;
    private Image progressBarImage;

    private Vector3 progressBarLocalScale = Vector3.one;
    private float progress = 0f;

    private Text alertText;
    private Text killTipText;
    private Text respawnCountDownTxt;
    private Transform backLoginBtn;
    private readonly int maxRespawnCountDown = 5;
    private int currentRespawnCountDown = 0;
    [HideInInspector]
    private Transform respawnBtn;
    [HideInInspector]
    public string death1 = "你已死亡";
    [HideInInspector]
    public string death2 = "你被%character%击败了";
    private Image bossTips;

    private GameObject vsModeTimeBg;
    private GameObject trainingModeTimeBg;
    private Text trainingModeTimeTxt;
    private Text trainingModeGoldTxt;
	private bool isRunning = false;

    void Awake()
    {
        isNeedCache = true;
        vsModeTimeBg = transform.Find("TimeBg").gameObject;
        trainingModeTimeBg = transform.Find("TimeBg2").gameObject;

        progressBar = transform.Find("ProgressBar/Bar");

        timer = transform.Find("TimeBg/Text_Timer").GetComponent<Text>();

        trainingModeTimeTxt = trainingModeTimeBg.transform.Find("time/CountDownTxt").GetComponent<Text>();
        trainingModeGoldTxt = trainingModeTimeBg.transform.Find("time/GoldTxt").GetComponent<Text>();
     
        alertText = gameObject.transform.Find("AlertText").GetComponent<Text>();

        respawn = transform.Find("RechallengeScreen");
        killTipText = respawn.Find("center image/center/enemy").GetComponent<Text>();
        backLoginBtn = respawn.transform.Find("center image/home page");
        respawnBtn = respawn.transform.Find("center image/rechallenge");
        respawn.gameObject.SetActive(false);

        respawnCountDownTxt = respawnBtn.GetComponentInChildren<Text>();
        isPlayOpenSound = false;
        isPlayHideSound = false;
        this.timer.text = "";
        this.trainingModeTimeTxt.text = "";
    }
		
    public float Progress
    {
        set
        {
            progress = value;
            if(progressBarImage == null)
            {
                progressBarImage = progressBar.GetComponent<Image>();
            }
            if (progress <= 0)
            {
                progressBar.gameObject.SetActive(false);
                progressBar.parent.gameObject.SetActive(false);
            }
            else
            {
                progressBarImage.fillAmount = progress;
                progressBar.gameObject.SetActive(true);
                progressBar.parent.gameObject.SetActive(true);
            }
        }
    }

    #region public override methods
    private void FixedUpdate()
    {
        if (this.isRunning)
        {
            if(FightManager.GetHero() != null && FightManager.GetHero().myInfo != null)
            {
                float maxTurnContinueTime = FightManager.GetHero().myInfo.totalTurnContinueTime;
                float currentTurnContinueTime = FightManager.GetHero().myInfo.currentTurnContinueTime;
                float progress = 0f;
                if (maxTurnContinueTime > 0)
                {
                    progress = currentTurnContinueTime / maxTurnContinueTime;
                }
                else
                {
                    progress = 0f;
                }
                Progress = progress;
            }
        }
    }
	
    public override void OnHide()
    {
		FightManager.OnGameBegin -= this.OnGameBegin;
		FightManager.OnGameEnds -= this.OnGameEnd;

		FightManager.OnTimer -= this.OnTimer;

		BossTip.SetActive (false);
        respawn.gameObject.SetActive(false);

        this.hiding = true;
        base.OnHide();

        EventTriggerListener.Get(backLoginBtn.gameObject).onClick -= OnBackLogin;
        EventTriggerListener.Get(respawnBtn.gameObject).onClick -= OnRespawn;
        currentRespawnCountDown = 0;
    }

	protected virtual void OnGameBegin(CharacterInfo player1, CharacterInfo player2, StageOptions stage)
	{
		this.isRunning = true;
	}

    public override void OnShow(System.Object data = null)
    {
		base.OnShow ();
		FightManager.OnGameBegin += this.OnGameBegin;
		FightManager.OnGameEnds += this.OnGameEnd;

		FightManager.OnTimer += this.OnTimer;
        this.hiding = false;
        if (FightManager.gameMode == GameMode.TrainingRoom)
        {
            trainingModeTimeBg.SetActive(true);
            vsModeTimeBg.SetActive(false);
        }
        else if(FightManager.gameMode == GameMode.VersusMode)
        {
            trainingModeTimeBg.SetActive(false);
            vsModeTimeBg.SetActive(true);
        }

        EventTriggerListener.Get(backLoginBtn.gameObject).onClick += OnBackLogin;
        EventTriggerListener.Get(respawnBtn.gameObject).onClick += OnRespawn;

        OnTimer(FightManager.GetTimer());
        this.isRunning = FightManager.gameRunning;
    }

    public void ShowHeroRespawn()
    {
        FightManager.GC();
        respawn.gameObject.SetActive(true);
        string killerName = FightManager.GetHero().myInfo.killerName;
        if (string.IsNullOrEmpty(killerName))
        {
            killTipText.text = death1;
        }
        else
        {
            string msg = death2;
            killerName = "<color=#F9DD00FF>" + killerName + "</color>";
            msg = msg.Replace("%character%", killerName);
            killTipText.text = msg;
        }
        currentRespawnCountDown = maxRespawnCountDown;
        respawnCountDownTxt.text = "复活(" + currentRespawnCountDown + "s)";
    }

    private void OnRespawn(GameObject go)
    {
        currentRespawnCountDown = 0;
        respawn.gameObject.SetActive(false);
        /*MsgRelife msgRelife = new MsgRelife();
        msgRelife.Id = LoginManager.playerId;
        SocketManager.Send(MsgTypeCmd.ReLife, msgRelife);*/
        FightManager.GetHero().RemoveAutoRelife();
    }

    private void OnBackLogin(GameObject go)
    {
        currentRespawnCountDown = 0;
        FightManager.config.isBackToMainMenuByDeath = true;
        FightManager.StartMainMenuScreen();
    }
    #endregion

    #region protected instance methods
    protected virtual string ProcessMessage(string msg, Player controlsScript)
    {
        return this.SetStringValues(msg, controlsScript);
    }

    protected virtual string SetStringValues(string msg, Player controlsScript)
    {
        CharacterInfo character = controlsScript != null ? controlsScript.myInfo : null;
        if (character != null)
        {
            msg = msg.Replace("%character%", character.characterName);
        }
        msg = msg.Replace("%round%", FightManager.config.currentRound.ToString());
        return msg;
    }
    #endregion

    #region protected override methods

    protected void OnGameEnd(CharacterInfo winner, CharacterInfo loser)
    {
		this.isRunning = false;
		FightManager.StartGameOverScreen();
        if (this.info != null)
        {
            this.info.text = string.Empty;
        }
        if (this.timer != null)
        {
            this.timer.text = string.Empty;
            this.trainingModeTimeTxt.text = string.Empty;
        }
    }


    protected void OnTimer(float time)
    {
        if (this.timer != null)
        {
            time = Mathf.Round(time);
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time - min * 60);

            string strMin = "";
            string strSec = "";
            if (min < 10)
            {
                strMin = "0" + min;
            }
            else
            {
                strMin = min.ToString();
            }
            if (sec < 10)
            {
                strSec = "0" + sec;
            }
            else
            {
                strSec = sec.ToString();
            }
            if(FightManager.gameMode == GameMode.TrainingRoom)
            {
                this.trainingModeTimeTxt.text = strMin + ":" + strSec;
            }
            else
            {
                this.timer.text = strMin + ":" + strSec;
            }
        }
        if (currentRespawnCountDown > 0)
        {
            currentRespawnCountDown -= 1;
            respawnCountDownTxt.text = "立即复活(" + currentRespawnCountDown + "s)";
            if(currentRespawnCountDown <= 0)
            {
                OnRespawn(null);
            }
        }
    }

    #endregion

}