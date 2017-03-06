using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

using CodeStage.AdvancedFPSCounter;
using DG.Tweening;
using System.IO;
using PathologicalGames;

/// <summary>
/// nashnie
/// </summary>
public class FightManager : MonoBehaviour
{
    #region public instance properties
    public GlobalInfo UFE_Config;
    #endregion

    #region public event definitions
    public delegate void MeterHandler(float newFloat, CharacterInfo player);
    public static event MeterHandler OnLifePointsChange;

    public delegate void IntHandler(int newInt);
    public static event IntHandler OnRoundBegins;

    public delegate void StringHandler(string newString, CharacterInfo player);
    public static event StringHandler OnNewAlert;

    public delegate void HitHandler(HitBox strokeHitBox, MoveInfo move, CharacterInfo player);
    public static event HitHandler OnHit;
    public static event HitHandler OnBlock;
    public static event HitHandler OnParry;

    public delegate void MoveHandler(MoveInfo move, CharacterInfo player);
    public static event MoveHandler OnMove;
    public static event MoveHandler OnMoveEnd;

	public delegate void HeroDieHandler(Player player);
	public static event HeroDieHandler OnHeroDieHandler;

    public delegate void GameBeginHandler(CharacterInfo player1, CharacterInfo player2, StageOptions stage);
    public static event GameBeginHandler OnGameBegin;

    public delegate void GameEndsHandler(CharacterInfo winner, CharacterInfo loser);
    public static event GameEndsHandler OnGameEnds;
    public static event GameEndsHandler OnRoundEnds;

    public delegate void ModuleShowOrHideHandler(bool isShow, UIType type);
    public static event ModuleShowOrHideHandler OnModuleShowOrHideHandler;

    public delegate void HeroTurnHandler(CharacterInfo characterInfo);
    public static event HeroTurnHandler OnHeroTurnHandler;

    public delegate void NetHeroAddHandler(CharacterInfo characterInfo);
    public static event NetHeroAddHandler OnNetHeroAddHandler;

    public delegate void HeroAddHandler(GameObject go);
    public static event HeroAddHandler OnHeroAddHandler;
	public static event HeroAddHandler OnEnemyAddHandler;

    public delegate void TimerHandler(float time);
    public static event TimerHandler OnTimer;

    public delegate void TimeOverHandler();
    public static event TimeOverHandler OnTimeOver;

    public delegate void PlayerAttrChangeHandler(ulong id, int oldValue, int newValue);
    public static event PlayerAttrChangeHandler OnHeroSpeedChangeHandler;
    public static event PlayerAttrChangeHandler OnHeroLvChangeHandler;
    public static event PlayerAttrChangeHandler OnHeroLifeNumChange;
    public static event PlayerAttrChangeHandler OnCoinChange;

    public delegate void PlayerCareerChangeHandler(ulong id, int oldValue, int newValue, float duration);
    public static event PlayerCareerChangeHandler OnHeroCareerChangeHandler;

    public delegate void ProjectileHandler(ProjectileMoveScript projectile);
    public static event ProjectileHandler OnRemoveProjectileHandler;

    //TAG
    public static string ProjectileTag = "Projectile";
    public static string EnemyTag = "Enemy";
    public static string PlayerTag = "Player";

    #endregion

    #region public class properties


    public static GameMode gameMode = GameMode.VersusMode;
    public static GameNetMode gameNetMode = GameNetMode.offline;
    public static GlobalInfo config;

    public static bool debug = true;
	public static DefaultBattleScreen battleGUI { get; protected set; }
    public static GameObject gameEngine { get; protected set; }
    public static bool gameRunning { get; protected set; }
    #endregion

    #region private class properties
    private static float timer = 9999999;
    private static int intTimer;
    private static bool pauseTimer;
    private static bool newRoundCasted;
    public static CameraScript cameraScript;

    public static Player hero;

    public static List<Player> controlsScriptList;

    private static List<DelayedAction> delayedLocalActions = new List<DelayedAction>();
    private static List<DelayedAction> delayedSynchronizedActions = new List<DelayedAction>();

    private static bool closing = false;
    private static bool disconnecting = false;

    private static bool player1WonLastBattle;
    private const int defaultRandomNameIndexGap = 1000;

    public static List<GameObject> vortexList = new List<GameObject>();
    public static GameObject joystick;

    public static float m_sendDataTime; //0f;
    public static DateTime m_flRecHeartTime;// = 0f;
    public static float m_pingTime = 0;

    public static Dictionary<ulong, MsgPlayer> playerModelMap = new Dictionary<ulong, MsgPlayer>();
    private static Dictionary<ulong, string> playerShortMap = new Dictionary<ulong, string>();
    
    public static Dictionary<uint, UFEItem> gMapPosEntitys = new Dictionary<uint, UFEItem>();     //N个地图位置对应的道具

    public static Dictionary<string, Vector3> configPosMap = new Dictionary<string, Vector3>();

    public static readonly float DefaultMapBoundGap = 0f;

    //public static readonly float MAP_SCALE = 90 / 3200f;
    public static readonly float MAP_SCALE = 18 / 4000f;
    public static readonly int china = 156;
    public static readonly int grassCannotHideLv = 3;

    private static Vector3 serverTarget = Vector3.zero;

    private static bool isTraceFightLog = true;
    private static List<Npc> npcList = new List<Npc>();

    public static UFECamp winCamp;
    private static bool isTimeOver = false;
    private static readonly int maxShowCharacterNameLen = 8;
    private readonly static long maxCachePoolTime = 600;//战斗时间
    private readonly static int maxCachePoolCount = 3;//战斗场数
    private readonly static int maxCachePoolResCount = 500;
    private static long currentCachePoolTime = 0;
    private static long lastStartFightTime = 0;
    private static int currentCachePoolCount = 0;
    private static bool isJoystickLoading = false;
    private static bool isBattleInputButtonGroupLoading = false;
    private static List<BoxCollider> wallColliderList = new List<BoxCollider>();
    #endregion

    private static void Log(string msg)
    {
        if(isTraceFightLog)
        {
            Debug.Log(msg);
        }
    }

    public static void DelayLocalAction(Action action, float seconds = 0f)
    {
        if (Time.fixedDeltaTime > 0f)
        {
            FightManager.DelayLocalAction(action, Mathf.FloorToInt(seconds * config.fps));
        }
        else
        {
            FightManager.DelayLocalAction(action, 1);
        }
    }

    public static void DelayLocalAction(Action action, int steps)
    {
        FightManager.DelayLocalAction(new DelayedAction(action, steps));
    }

    public static void DelayLocalAction(DelayedAction delayedAction)
    {
        FightManager.delayedLocalActions.Add(delayedAction);
    }

    public static void DelaySynchronizedAction(Action action, float seconds = 0f)
    {
        if (Time.fixedDeltaTime > 0f)
        {
            FightManager.DelaySynchronizedAction(action, Mathf.FloorToInt(seconds * config.fps));
        }
        else
        {
            FightManager.DelaySynchronizedAction(action, 1);
        }
    }

    public static void RemoveDelaySynchronizedAction(Action action)
    {
        foreach (DelayedAction delayedAction in FightManager.delayedSynchronizedActions)
        {
            if (action == delayedAction.action)
            {
                FightManager.delayedSynchronizedActions.Remove(delayedAction);
                break;
            }
        }
    }

    public static void DelaySynchronizedAction(Action action, int steps)
    {
        FightManager.DelaySynchronizedAction(new DelayedAction(action, steps));
    }

    public static void DelaySynchronizedAction(DelayedAction delayedAction)
    {
        FightManager.delayedSynchronizedActions.Add(delayedAction);
    }


    public static bool FindDelaySynchronizedAction(Action action)
    {
        foreach (DelayedAction delayedAction in FightManager.delayedSynchronizedActions)
        {
            if (action == delayedAction.action)
            {
                return true;
            }
        }
        return false;
    }

    public static bool FindAndUpdateDelaySynchronizedAction(Action action, float seconds)
    {
        foreach (DelayedAction delayedAction in FightManager.delayedSynchronizedActions)
        {
            if (action == delayedAction.action)
            {
                delayedAction.steps = Mathf.FloorToInt(seconds * config.fps);
                return true;
            }
        }
        return false;
    }

    #region public class methods: GUI Related methods

    public static void HideScreen(UIType screen)
    {
        UIManager.Instance.Show(screen);
    }

    public static void ShowScreen(UIType screen, System.Object data = null, bool isShowLoading = false)
    {
        UIManager.Instance.Show(screen, data, isShowLoading);
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public static void StartGame(float fadeTime)
    {
        FightManager._StartGame(0f);
        //GA.StartLevel("Fight");
    }

    public static void StartMainMenuScreen()
    {
        FightManager.StartMainMenuScreen(0f);
    }

    public static void StartGameOverScreen()
    {
        FightManager.StartGameOverScreen(0f);
    }

    public static void StartGameOverScreen(float fadeTime)
    {
        FightManager._StartGameOverScreen(0f);
    }

    private static void _StartGameOverScreen(float fadeTime)
    {
        FightManager.EndGame();
        UIManager.Instance.HideAll();
        FightManager.ShowScreen(UIType.BattleResultScreen, null, true);
    }

    public static void StartMainMenuScreen(float fadeTime)
    {
        FightManager._StartMainMenuScreen(fadeTime);
    }

    public static void StartNetworkGame(float fadeTime, int localPlayer)
    {
        AddNetworkEventListeners();
        FightManager.disconnecting = false;

        Application.runInBackground = true;

        FightManager.SetPlayer1(FightManager.config.p1CharStorage);
        FightManager.SetPlayer2(FightManager.config.p2CharStorage);
        FightManager.StartGame(0f);
    }

    public static void StartOfflineGame()
    {
		SceneAssetsManager.Instance.Setup();
		SceneAssetsManager.Instance.LoadMapDataFromCache();

        FightManager.config.strAccount = "offline12345";
        FightManager.config.selfCamp = UFECamp.Camp1;

        //添加主角
        MsgPlayer hero = new MsgPlayer();
        hero.Realposx = 0;
        hero.Realposz = 0;

        hero.name = LoginManager.Name;
        hero.id = LoginManager.playerId;
        hero.camp = (uint)UFECamp.Camp1;
#if UNITY_EDITOR
        Debug.Log("AddHero camp, " + hero.camp + " lifeNum, " + hero.LifeNum + " selfCamp " + FightManager.config.selfCamp);
#endif
        FightManager.playerModelMap.Remove(hero.id);
        FightManager.playerModelMap.Add(hero.id, hero);

        Application.runInBackground = true;

        FightManager.SetPlayer1(FightManager.config.p1CharStorage);
        FightManager.SetPlayer2(FightManager.config.p2CharStorage);
        FightManager.StartGame(0f);
    }

    #endregion

    public static void SetPlayer1(CharacterInfo player1)
    {
        config.player1Character = player1;
    }

    public static void SetPlayer2(CharacterInfo player2)
    {
        config.player2Character = player2;
    }

    #region public class methods: methods related to the behaviour of the characters during the battle

    public static Player GetControlsScript(PlayerCamp player, string name)
    {
        if (player == PlayerCamp.self)
        {
            return hero;
        }
        else if (player == PlayerCamp.opponent)
        {
            return FightManager.GetPlayerControlsScript(name);
        }
        return null;
    }

    public static Player GetHero()
    {
        return hero;
    }

    public static string GetPlayerName(ulong id)
    {
        if(playerModelMap.ContainsKey(id))
        {
            MsgPlayer player = playerModelMap[id];
            return player.name;
        }
        return "";
    }

    public static string GetPlayerShortName(ulong id)
    {
        if (playerShortMap.ContainsKey(id))
        {
            return playerShortMap[id];
        }
        else
        {
            string name = GetPlayerName(id);
            string characterName = FightManager.GetPlayerName(id);
            if (characterName.Length > maxShowCharacterNameLen)
            {
                characterName = characterName.Substring(0, maxShowCharacterNameLen);
                characterName += "...";
            }
            playerShortMap.Add(id, characterName);
            return characterName;
        }
    }

    public static Player GetPlayerControlsScriptById(ulong id, UFEMapUnit mapUnit = UFEMapUnit.player)
    {
        for (int i = 0; i < controlsScriptList.Count; i++)
        {
            Player controlsScript = controlsScriptList[i];
            if (controlsScript != null)
            {
                if (controlsScript.id == id ||
                   (controlsScript.myInfo != null && controlsScript.myInfo.id == id))
                {
                    if(controlsScript.ufeMapUnit == mapUnit)
                    {
                        return controlsScript;
                    }
                }
            }
        }
        return null;
    }

    public static Player GetPlayerControlsScriptByType(UFEMapUnit mapUnit)
    {
        for (int i = 0; i < controlsScriptList.Count; i++)
        {
            Player controlsScript = controlsScriptList[i];
            if (controlsScript != null)
            {
                if (controlsScript.myInfo != null)
                {
                    if (controlsScript.ufeMapUnit == mapUnit)
                    {
                        return controlsScript;
                    }
                }
            }
        }
        return null;
    }

    public static Player GetPlayerControlsScript(string name)
    {
        for (int i = 0; i < controlsScriptList.Count; i++)
        {
            Player controlsScript = controlsScriptList[i];
            if (controlsScript.gameObjectName == name)
            {
                return controlsScript;
            }
        }
        return null;
    }
    #endregion

    #region public class methods: methods that are used for raising events
    public static void SetLifePoints(float newValue, CharacterInfo player)
    {
        if (FightManager.OnLifePointsChange != null) FightManager.OnLifePointsChange(newValue, player);
    }

    public static void FireAddCoin(CharacterInfo player, int oldValue, int newValue)
    {
        if (FightManager.OnCoinChange != null) FightManager.OnCoinChange(player.id, oldValue, newValue);
    }

    public static void FireHeroSpeedChange(ulong id, int oldValue, int newValue)
    {
        if (FightManager.OnHeroSpeedChangeHandler != null) FightManager.OnHeroSpeedChangeHandler(id, oldValue, newValue);
    }

    public static void FireHeroLvChange(ulong id, int oldValue, int newValue)
    {
        if (FightManager.OnHeroLvChangeHandler != null) FightManager.OnHeroLvChangeHandler(id, oldValue, newValue);
    }

    public static void FireHeroLifeChange(ulong id, int oldValue, int newValue)
    {
        if (FightManager.OnHeroLifeNumChange != null) FightManager.OnHeroLifeNumChange(id, oldValue, newValue);
    }

    public static void FireHeroCareerChange(ulong id, int oldValue, int newValue, float duration)
    {
        if (FightManager.OnHeroCareerChangeHandler != null) FightManager.OnHeroCareerChangeHandler(id, oldValue, newValue, duration);
    }

    public static void FireRemoveProjectileHandler(ProjectileMoveScript projectileMoveScript)
    {
        if (FightManager.OnRemoveProjectileHandler != null) FightManager.OnRemoveProjectileHandler(projectileMoveScript);
    }

    public static void FireHeroTurn(CharacterInfo characterInfo)
    {
        if (FightManager.OnHeroTurnHandler != null) FightManager.OnHeroTurnHandler(characterInfo);
    }

    public static void FireAddNetHero(CharacterInfo characterInfo)
    {
        if (FightManager.OnNetHeroAddHandler != null)
        {
            FightManager.OnNetHeroAddHandler(characterInfo);
        }
    }

    public static void FireAddHero(GameObject go)
    {
        if (FightManager.OnHeroAddHandler != null)
        {
            FightManager.OnHeroAddHandler(go);
        }
    }

	public static void FireAddEnemy(GameObject go)
	{
		if (FightManager.OnEnemyAddHandler != null)
		{
			FightManager.OnEnemyAddHandler(go);
		}
	}

    public static void FireBlock(HitBox strokeHitBox, MoveInfo move, CharacterInfo player)
    {
        if (FightManager.OnBlock != null) FightManager.OnBlock(strokeHitBox, move, player);
    }

    public static void FireMove(MoveInfo move, CharacterInfo player, bool isEnd = false)
    {
        if(isEnd)
        {
            if (FightManager.OnMoveEnd != null) FightManager.OnMoveEnd(move, player);
        }
        else
        {
            if (FightManager.OnMove != null) FightManager.OnMove(move, player);
        }
    }

    public static void FireGameBegins()
    {
        gameRunning = true;
        if (FightManager.OnGameBegin != null)
        {
            FightManager.OnGameBegin(config.player1Character, config.player2Character, config.selectedStage);
        }
    }

    public static void FireGameEnds(CharacterInfo winner, CharacterInfo loser)
    {
        RemoveNetworkEventListeners();
        Time.timeScale = FightManager.config.gameSpeed;
        FightManager.gameRunning = false;
        FightManager.newRoundCasted = false;
        if (FightManager.OnGameEnds != null)
        {
            FightManager.OnGameEnds(winner, loser);
        }
    }

    public static void FireRoundBegins(int currentRound)
    {
        if (FightManager.OnRoundBegins != null) FightManager.OnRoundBegins(currentRound);
    }

	public static void FireHeroDie(Player player)
	{
		if (FightManager.OnHeroDieHandler != null) FightManager.OnHeroDieHandler(player);
	}

    public static void FireRoundEnds(CharacterInfo winner, CharacterInfo loser)
    {
        if (FightManager.OnRoundEnds != null) FightManager.OnRoundEnds(winner, loser);
    }

    public static void FireTimeOver()
    {
        if (FightManager.OnTimeOver != null) FightManager.OnTimeOver();
    }
    #endregion

    #region public class methods: UFE CORE methods

    public static Type SearchClass(string theClass)
    {
        Type type = null;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(theClass);
            if (type != null) { break; }
        }
        return type;
    }

    public static void SetTimer(float time)
    {
        timer = time;
        intTimer = (int)time;
        if (FightManager.OnTimer != null) OnTimer(timer);
    }

    public static float GetTimer()
    {
        return timer;
    }

    public static void PlayTimer()
    {
        pauseTimer = false;
    }

    public static void PauseTimer()
    {
        pauseTimer = true;
    }

    public static void EndGame()
    {
        PoolUtil.ClearMoveInfoMap();
        if (lastStartFightTime > 0)
        {
            currentCachePoolCount++;
            currentCachePoolTime += TimeManager.ServerTime - lastStartFightTime;
        }
        if (PoolUtil.GetCacheParticlesCount() >= maxCachePoolResCount ||
            (currentCachePoolCount >= maxCachePoolCount && currentCachePoolTime >= maxCachePoolTime))
        {
            RemoveCachePool();
            AddCachePool();
            currentCachePoolCount = 0;
            currentCachePoolTime = 0;
        }
        Time.timeScale = FightManager.config.gameSpeed;

        FightManager.OnRemoveProjectileHandler = null;
        FightManager.gameRunning = false;
        FightManager.newRoundCasted = false;
        FightManager.OnModuleShowOrHideHandler -= onModuleShowOrHideHandler;
        UIManager.Instance.Hide(UIType.BattleGUI);
		UIManager.Instance.Hide(UIType.HUDPlayerController);
        BattleInputController.Instance.Clear();
        if(controlsScriptList != null)
        {
            for (int i = 0; i < controlsScriptList.Count; i++)
            {
                Player controlsScript = controlsScriptList[i];
                controlsScript.DestroyThis();
            }
            controlsScriptList.Clear();
        }
       
        if (gameEngine != null)
        {
            GameObject.Destroy(gameEngine);
            gameEngine = null;
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.Offline();
        }

        vortexList.Clear();

		//TrackViewManager.Instance.Offline();
        //ItemManager.Instance.Destroy();
   
        SceneAssetsManager.Instance.Clear();
        gMapPosEntitys.Clear();
        configPosMap.Clear();
        playerModelMap.Clear();

        hero = null;

        //GA.FinishLevel("Fight");

        if(Global.isFirstLogin == false)
        {
            FightManager.GC();
        } 
    }

    public static void CastNewRound()
    {
        FightManager.FireRoundBegins(config.currentRound);
        FightManager.config.lockInputs = false;
        FightManager.config.lockMovements = false;
        FightManager.PlayTimer();
        newRoundCasted = true;
    }

    #endregion

    #region protected instance methods: MonoBehaviour methods

    protected void Awake()
    {
        UFE_Config = ConfigManager.Instance.globalConfig;
        FightManager.config = ConfigManager.Instance.globalConfig;

        FightManager.config.selectedStage.leftBoundary = 0;
        FightManager.config.selectedStage.bottomBoundary = 0;
        FightManager.config.selectedStage.rightBoundary = 18f;
        FightManager.config.selectedStage.topBoundary = 18f;

        FightManager.config.isLastMatchLostConnection = false;

        Application.runInBackground = FightManager.config.runInBackground;

        controlsScriptList = new List<Player>();
        config.fps = 30;
        Time.timeScale = config.gameSpeed;
        Application.targetFrameRate = config.fps;

        GameObject wall = GameObject.Find("Land");
        BoxCollider[] boxColliderList = wall.GetComponentsInChildren<BoxCollider>();
        wallColliderList = new List<BoxCollider>(boxColliderList);

        AddCachePool();
    }

    private static void AddCachePool()
    {
        UnityEngine.Object particlesObject = Resources.Load("PoolOfParticles");
        GameObject poolOfParticles = GameObject.Instantiate(particlesObject) as GameObject;
        poolOfParticles.name = "PoolOfParticles";

        UnityEngine.Object GUIObject = Resources.Load("PoolOfGUI");
        GameObject PoolOfGUI = GameObject.Instantiate(GUIObject) as GameObject;
        PoolOfGUI.name = "PoolOfGUI";
    }

    private static void RemoveCachePool()
    {
        GameObject poolOfParticles = GameObject.Find("PoolOfParticles");
        GameObject poolOfGUI = GameObject.Find("PoolOfGUI");
        if (poolOfParticles != null)
        {
            GameObject.DestroyImmediate(poolOfParticles, true);
        }
        if (poolOfGUI != null)
        {
            GameObject.DestroyImmediate(poolOfGUI, true);
        }
    }

    public static CharacterInfo GetTargetCharacterInfo(UFECamp camp, UFEMapUnit UFEMapUnit = UFEMapUnit.player, bool isHero = false, int containsSkinId = 0)
    {
        if (camp == UFECamp.Camp1)
        {
			return FightManager.config.p1CharStorage;
        }
        else
        {
			return FightManager.config.p2CharStorage;
        }
    }

	public static Player AddNetPlayer(ulong playerId, UFECamp camp, UFEMapUnit UFEMapUnit = UFEMapUnit.player, int skinId = 0, bool isAI = true, CharacterInfo characterInfo = null)
    {
        string name = playerId.ToString();
        GameObject go = new GameObject(name);
		if (camp == UFECamp.Camp1) {
			go.tag = PlayerTag;
		} else {
			go.tag = EnemyTag;	
		}
        go.transform.parent = FightManager.gameEngine.transform;

        Player controlsScript = go.AddComponent<Player>();
        FightManager.controlsScriptList.Add(controlsScript);
        controlsScript.playerNum = (int)PlayerCamp.opponent;
        controlsScript.camp = camp;
        controlsScript.id = playerId;
        controlsScript.ufeMapUnit = UFEMapUnit;
        controlsScript.skinId = skinId;
        controlsScript.isAIController = isAI;
		controlsScript.tag = go.tag;
		controlsScript.turnCharStorage = characterInfo;

        return controlsScript;
    }

    public static void RemoveNetPlayer(ulong playerId)
    {
        string name = playerId.ToString();
        Player controlsScript = FightManager.GetControlsScript(PlayerCamp.opponent, name);
        if (controlsScript != null)
        {
            controlsScript.DestroyMeByOffline();
        }
        playerModelMap.Remove(playerId);
    }

    protected void Update()
    {
        BattleInputController.Instance.DoUpdate();

        if (Time.timeScale <= 0f)
        {
            for (int i = FightManager.delayedLocalActions.Count - 1; i >= 0; --i)
            {
                DelayedAction action = FightManager.delayedLocalActions[i];
                --action.steps;
                if (action.steps <= 0)
                {
                    action.action();
                    FightManager.delayedLocalActions.RemoveAt(i);
                }
            }
        }
    }

    protected void FixedUpdate()
    {
        bool bothReady = true;
        if (bothReady)
        {
            if (CameraFade.instance.enabled)
            {
                CameraFade.instance.DoFixedUpdate();
            }
            if (gameRunning)
            {
                if (config.roundOptions.hasTimer && timer > 0)
                {
                    timer -= Time.fixedDeltaTime * (config.roundOptions.timerSpeed * .01f);
                    if (timer < intTimer)
                    {
                        intTimer--;
                        if (FightManager.OnTimer != null)
                        {
                            OnTimer(timer);
                        }
                    }
                }
                if (timer < 0)
                {
                    timer = 0;
                }
                if (intTimer < 0)
                {
                    intTimer = 0;
                }
            }

            for (int i = 0; i < controlsScriptList.Count; i++)
            {
                Player controlsScript = controlsScriptList[i];
                UpdateControlsScript(controlsScript);
            }

            for (int i = FightManager.delayedLocalActions.Count - 1; i >= 0; --i)
            {
                DelayedAction action = FightManager.delayedLocalActions[i];
                --action.steps;

                if (action.steps <= 0)
                {
                    action.action();
                    FightManager.delayedLocalActions.RemoveAt(i);
                }
            }

            for (int i = FightManager.delayedSynchronizedActions.Count - 1; i >= 0; --i)
            {
                DelayedAction action = FightManager.delayedSynchronizedActions[i];
                --action.steps;
                if (action.steps <= 0)
                {
                    action.action();
                    if (FightManager.delayedSynchronizedActions.Count > i)
                    {
                        FightManager.delayedSynchronizedActions.RemoveAt(i);
                    }
                    else
                    {
#if UNITY_EDITOR
                        Log("UFE.delayedSynchronizedActions error.remove out of range len, " + FightManager.delayedSynchronizedActions.Count + " i, " + i);
#endif
                    }
                }
            }
        }
    }

    private static void UpdateControlsScript(Player controlsScript)
    {
        if (controlsScript != null)
        {
            if (controlsScript.MoveSet != null && controlsScript.MoveSet.MecanimControl != null)
            {
                controlsScript.MoveSet.MecanimControl.DoFixedUpdate();
            }
            if (controlsScript.MoveSet != null && controlsScript.MoveSet.LegacyControl != null)
            {
                controlsScript.MoveSet.LegacyControl.DoFixedUpdate();
            }
            controlsScript.DoFixedUpdate();
        }
    }

    protected void OnApplicationQuit()
    {
        FightManager.closing = true;
#if !UNITY_WEBGL
        FightManager.EnsureNetworkDisconnection();
#endif
    }
    #endregion

    #region protected instance methods: Network Events
    public static void EnsureNetworkDisconnection()
    {
#if !UNITY_WEBGL
        FightManager.disconnecting = true;
        FightManager.RemoveNetworkEventListeners();
#endif
#if UNITY_EDITOR
        Log("EnsureNetworkDisconnection SocketManager Close.");
#endif
    }

    protected static void AddNetworkEventListeners()
    {
    }

    private static void OnUpdateBasicSkillParam(BaseEvent evt)
    {
    }

    protected static void RemoveNetworkEventListeners()
    {
    }

    private static void _StartMainMenuScreen(float fadeTime)
    {
#if !UNITY_WEBGL
        FightManager.EnsureNetworkDisconnection();
#endif
        FightManager.EndGame();
        UIManager.Instance.HideAll();
        FightManager.ShowScreen(UIType.MainMenuScreen);
    }

    private static void _StartGame(float fadeTime)
    {
        lastStartFightTime = TimeManager.ServerTime;

        //TrackViewManager.Instance.Online();
        playerShortMap.Clear();
        UIManager.Instance.HideAll();

        FightManager.OnModuleShowOrHideHandler += onModuleShowOrHideHandler;

        UIManager.Instance.ShowModule(ConfigManager.Instance.joystick, UIType.HUDPlayerController);
        UIManager.Instance.Show(UIType.BattleGUI);
 
		gameEngine = new GameObject("Game");
		FightManager.cameraScript = gameEngine.AddComponent<CameraScript>();

		FightManager.config.currentRound = 1;
		FightManager.config.lockInputs = false;

		FightManager.PauseTimer();
		controlsScriptList.Clear();

        AddMainHero();
        SceneAssetsManager.Instance.GeneratorNewWaveEnemys();

        FightManager.config.isLastMatchLostConnection = false;
        FightManager.config.isBackToMainMenuByDeath = false;
        isTimeOver = false;
    }

    private static void AddMainHero()
    {
        GameObject hero = new GameObject(SceneAssetsManager.roleStartId.ToString());
        hero.transform.parent = gameEngine.transform;
        FightManager.hero = hero.AddComponent<Player>();
        controlsScriptList.Add(FightManager.hero);
        FightManager.hero.camp = UFECamp.Camp1;
        //FightManager.hero.id = LoginManager.playerId;
        FightManager.hero.isHero = true;
        FightManager.hero.ufeMapUnit = UFEMapUnit.player;
		FightManager.hero.tag = FightManager.PlayerTag;
    }

    private static void onModuleShowOrHideHandler(bool isShow, UIType type)
    {
        if(type == UIType.BattleGUI)
        {
			FightManager.battleGUI = UIManager.Instance.GetBaseUI(UIType.BattleGUI) as DefaultBattleScreen;
        }
    }

    public static void UpdateHUD()
    {
        for (int i = 0; i < FightManager.controlsScriptList.Count; i++)
        {
            Player controlsScript = FightManager.controlsScriptList[i];
            controlsScript.UpdateHUD();
        }
    }

    public static void FireModule(bool isShow, UIType type)
    {
        if(FightManager.OnModuleShowOrHideHandler != null)
        {
            FightManager.OnModuleShowOrHideHandler(isShow, type);
        }
    }

    public static void GC()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    #endregion
}


