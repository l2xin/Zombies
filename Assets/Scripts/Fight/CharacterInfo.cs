using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions;

[System.Serializable]
public class MoveSetData: ICloneable
{
	public CombatStances combatStance = CombatStances.Stance1; 

	public BasicMoves basicMoves = new BasicMoves(); 
	public MoveInfo[] attackMoves = new MoveInfo[0]; 
	
	[HideInInspector] public bool enabledBasicMovesToggle = true;
	[HideInInspector] public bool basicMovesToggle;
	[HideInInspector] public bool attackMovesToggle;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class CharacterInfo: ScriptableObject
{
    public ulong id;
    public bool isDead = false;
    public bool isHero = false;
    public List<Debuff> debuffList = new List<Debuff>();
    public bool freezeDebuff = false;
    public int countryId;
    public string cityOrCountry = "";
    public string killerName = "";
    public bool isOffline = false;
    public string gameObjectName = "";
    public int mapUnit = 1;
    public UFECamp camp = UFECamp.Camp1;
	public int weaponIndex = -1;
	public WeaponType weaponType = WeaponType.None;

	public string characterName;

	public string characterDescription;

	public int lifePoints = 3;
    public int exLifePoints = 0;
    public int initGaugePoints;
    public float speed = 1.5f;
    public bool isRigidBody = false;

    public int maxGaugePoints;
    public int selfAddGaugePoints;
    public int selfAddGaugeTime;

    public int totalKillCount;
    public int maxComboKillCount;
    public bool isPotionOfAngerBuff = false;
    public GameObject characterPrefab; 
    public float offsetY = 2f;

    public bool isBodyMask = false;
    public RuntimeAnimatorController runtimeAnimatorController;
    public RuntimeAnimatorController runtimeOverrideAnimatorController;
    public float executionTiming = .3f;
    public float blendingTime = .1f; 

	public AnimationType animationType;
	public Avatar avatar; 
	public bool applyRootMotion; 

	public MoveSetData[] moves = new MoveSetData[0];
	
	public CombatStances currentCombatStance{get; set;}

	public float currentLifePoints { get; set; }

    private float currentGaugePointsOf;

    public float currentGaugePoints
    {
        get
        {
            return currentGaugePointsOf;
        }
        set
        {
            currentGaugePointsOf = value;
        }
    }

    public int playerNum { get; set; }

    public int currentLv = 1;

    public float turnLeftContinueTime = 0f;
    public float totalTurnContinueTime = 0f;
    public float currentTurnContinueTime = 0f;

    private Vector3 posBeforeTurn = Vector3.zero;
    private const int maxItemSlotCount = 3;

    public int netLv;
    public int netBlood;
    public int netCoins = 1;
    public bool isNetFast;
    public bool isNetPotionOfAngerBuff;

    public float netX = 0f;
    public float netZ = 0f;

    public ObservedValue<float> skillCd = new ObservedValue<float>(0f);
    public ObservedValue<int> skillId = new ObservedValue<int>(1001);
}