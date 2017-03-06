using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Gamelogic.Extensions;

public enum PlatformEnum
{
    guanwang = 1,
    qihu360,
    yingyongbao,
    baidu,
}

#if UFE_BASIC
public enum AnimationType{
	Legacy
}
#else
public enum AnimationType {
    Legacy,
    Mecanim
}
#endif
public enum PlayerCamp
{
    self = 1,
    opponent,
}

public enum ShotProjectileType
{
    none = 0,
    little,
    big,
    parryLittle,
    parryBig,
}

public enum GameMode
{ 
	TrainingRoom,//大乱斗
    VersusMode,//团队
}

public enum GameNetMode
{
    online,//online
    offline,//offline
}

public enum ExecutionBufferType{
	OnlyMoveLinks,
	AnyMove,
	NoBuffer
}

public enum UFEPlayer
{ 
	Hero, 
    Player2,
}

public enum UFEBasicMove
{
    none,
    idle,
    walkFont,
    runFont,
    walkBack,
    runBack,
    walkLeft,
    walkRight,
    RunLeft,
    RunRight,
    hit,
    die,
}

public enum AssetType
{
    none,
    prefab,
    png,
    asset,
    controller,
    mask,
    sound,
}

public enum MapTrap
{
    //流沙漩涡
    Vortex,
    //定身
    Stun,
    //伤害
    Hurt,
    //knockdown
    knockdown,
    //knockback
    knockback,
    //bomb
    Bomb,
}

public enum Sizes{
	None,
	Small,
	Medium,
	High
}

public enum AirRecoveryType {
    AllowMoves,
    CantMove,
    DontRecover
}

public enum HitEffectSpawnPoint {
    StrikingHurtBox,
    StrokeHitBox,
    InBetween
}

public class Debuff
{
    public DebuffType type;
    public ObservedValue<float> param;
    public bool isShowViewed = false;
    public DebuffView view;
}

[System.Serializable]
public class HitTypeOptions {
	public GameObject hitParticle;
	public float killTime;
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrokeHitBox;
	public float freezingTime;
	public float animationSpeed = .1f;
	public bool autoHitStop = true;
	public float hitStop = .1f;
	public bool shakeCharacterOnHit = true;
	public bool shakeCameraOnHit = true;
	public float shakeDensity = .8f;
	public bool editorToggle;
    public float moveSpeed = 1f;
    public float moveSpeedTime = 1f;
    public bool isShowDecelerateSpeedEffect = true;
}

[System.Serializable]
public class HitOptions {
	public bool resetAnimationOnHit = true;
	public bool useHitStunDeceleration = true;
	public HitTypeOptions weakHit;
	public HitTypeOptions mediumHit;
	public HitTypeOptions heavyHit;
	public HitTypeOptions crumpleHit;
	public HitTypeOptions customHit1;
	public HitTypeOptions customHit2;
	public HitTypeOptions customHit3;
}

[System.Serializable]
public class StageOptions: ICloneable
{
    public float groundFriction = 100;
	public float leftBoundary = 0;
	public float rightBoundary = 38;
    public float bottomBoundary = 0;
    public float topBoundary = 38;

    public object Clone()
    {
		return CloneObject.Clone(this);
	}
}


[System.Serializable]
public class InputReferences: ICloneable
{
	// Common Parameters
	public InputType inputType;
	public string inputButtonName;
	public ButtonPress engineRelatedButton;
	
	// Input Manager parameters
	public string joystickAxisName;
	
	// cInput parameters
	public string cInputPositiveKeyName;
	public string cInputPositiveDefaultKey;
	public string cInputPositiveAlternativeKey;
	
	public string cInputNegativeKeyName;
	public string cInputNegativeDefaultKey;
	public string cInputNegativeAlternativeKey;
	
	// Input Viewer
	public Texture2D inputViewerIcon1;
	public Texture2D inputViewerIcon2;
	public Texture2D activeIcon;
	
	public float heldDown{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class RoundOptions
{
	public int totalRounds = 3;
	public bool hasTimer = true;
	public float timer = 99;
	public float timerSpeed = 100;

	public float endGameDelay = 4;
	public float newRoundDelay = 1;

	public bool resetLifePoints = true;
	public bool resetPositions = true;
	public bool allowMovement = true;

    public GameObject lvUpEffect;
    public GameObject showWaitLoadingEffect;

    public GameObject coin;
    public GameObject coins;
    public GameObject boom;
    public GameObject knife;

	public GameObject callBossItem;
	public GameObject callbanditsItem;

    public GameObject potionOfSpeed;
    public GameObject potionOfHealth;
    public GameObject itemEffect;
    public GameObject getCoinEffect;
    public GameObject healthEffect;
    public GameObject decelerateSpeedEffect;
    public GameObject fastEffect;
    public GameObject shadow;
    public GameObject bloodBar;
    //--------------------------------------hry
    public GameObject arrow;
	public GameObject vortex;
	//--------------------------------------

    public Sprite knifeIcon;
    public Sprite boomIcon;
    public Sprite potionOfHealthIcon;
    //public Sprite potionOfAngerIcon;
}

public enum UFECamp
{
    Camp1,
    Camp2,
    CampOther,
    CampMonster,
    //不参与战斗，完全独立客观第三方
    None,
}

[System.Serializable]
public class GlobalInfo: ScriptableObject {

	#region public instance fields
	public float version = 0;
    public Color myColor = new Color(141f / 255f, 226f / 255f, 66f / 255f);
    public Color selfColor = new Color(112f / 255f, 198f / 255f, 219f / 255f);
    public Color opColor = new Color(255f / 255f, 89f / 255f, 109f / 255f);

    public ulong camp1ChampionID = 0;
    public ulong camp2ChampionID = 0;

    public UFECamp selfCamp = UFECamp.Camp1;
    public string strAccount;
    public bool isLastMatchLostConnection = false;
    public bool isBackToMainMenuByDeath = false;

    public CharacterInfo player1Character;
    public CharacterInfo player2Character;
    public List<CharacterInfo> player2CharacterList;

    public bool isRandomCharacter = true;

    public CharacterInfo p1CharStorage;
	public CharacterInfo p2CharStorage;

    public CharacterInfo bossCharStorage;

    public CharacterInfo[] skinCharacters = new CharacterInfo[0];
	public CharacterInfo[] levelMonsterCharacters = new CharacterInfo[0];

	public GameObject[] meleeWeapons = new GameObject[0];
	public GameObject[] bows = new GameObject[0];
	public GameObject[] shields = new GameObject[0];

    public StageOptions selectedStage;
	public bool p1CPUControl;
	public bool p2CPUControl;
	public string gameName;

	public int fps = 30;
	public float gameSpeed = 1;
	public int executionBufferTime = 10;
	public ExecutionBufferType executionBufferType;
    public int plinkingDelay = 1;

	public float gravity = .37f;
	public bool detect3D_Hits;
	public bool runInBackground;

	public RoundOptions roundOptions;

	public HitOptions hitOptions;
	
	public StageOptions[] stages = new StageOptions[0];

	public int currentRound{get; set;}
	public bool lockInputs{get; set;}
	public bool lockMovements{get; set;}
	#endregion
}

[System.Serializable]
public static class CloneObject{
	public static object objCopy;
	
	public static object Clone(object target){
		return ReflectionClone(target);
	}
	
	public static object Clone(object target, bool serialized){
		if (serialized) return SerializedClone(target);
		return ReflectionClone(target);
	}

	public static object SerializedClone(object target){
		if (target == null) return null;

		using (Stream objectStream = new MemoryStream()) {
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(objectStream, target);
			objectStream.Seek(0, SeekOrigin.Begin);
			return (object)formatter.Deserialize(objectStream);
		}
	}

	public static object[] ReflectionCloneArray(object[] target){
		object[] arrayObj = (object[]) Array.CreateInstance(target.GetType().GetElementType(), target.Length);

		for (int i = 0; i < target.Length; i ++){
			
			if (target[i] == null 
			    || target[i].GetType().IsEnum
			    || target[i].GetType().IsValueType
			    || target[i].GetType().IsGenericType
			    || target[i].GetType().Equals(typeof(String)) 
			    || target[i].GetType().IsSubclassOf(typeof(ScriptableObject))){
				
				// If its a simple element, use shallow copy
				arrayObj[i] = target[i];
			}else{
				// If its a complex interface, go deeper into recursion
				arrayObj[i] = ReflectionClone(target[i]);
			}
		}

		return arrayObj;
	}

	public static object ReflectionClone(object target){
		Type typeSource = target.GetType();

		// If its an array, identify and recurse each element
		if (typeSource.IsArray){
			return (object) ReflectionCloneArray((object[])target);
		}

		object newObj = Activator.CreateInstance(typeSource);
		FieldInfo[] fields = typeSource.GetFields();

		foreach (FieldInfo field in fields){
			object fieldValue = field.GetValue(target);

			if (fieldValue == null 
			    || field.FieldType.IsEnum
			    || field.FieldType.IsValueType
			    || field.FieldType.Equals(typeof(String)) 
			    || field.FieldType.GetInterface("ICloneable", true ) == null
			    || field.FieldType.GetInterface("ScriptableObject", true ) != null
			    || field.FieldType.IsSubclassOf(typeof(ScriptableObject))){
				// If its a simple element, use shallow copy
				field.SetValue(newObj, fieldValue);
			}else{
				// If its a complex interface, go deeper into recursion
				field.SetValue(newObj, ReflectionClone(fieldValue));

			}
		}
		return newObj;
	}
}