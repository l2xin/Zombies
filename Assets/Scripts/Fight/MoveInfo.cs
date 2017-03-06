using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
[System.Serializable]

public class BasicMoveInfo : ICloneable
{
    public AnimationClip clip1;
    public AnimationClip clip2;
    public AnimationClip clip3;
    public AnimationClip clip4;
    public AnimationClip clip5;
    public AnimationClip clip6;
    public WrapMode wrapMode;
    public bool autoSpeed = true;
    public float animationSpeed = 1;
    public float restingClipInterval = 3;
    public bool overrideBlendingIn = false;
    public bool overrideBlendingOut = false;
    public float blendingIn = 0;
    public float blendingOut = 0;
    public bool invincible;
    public bool disableHeadLook;
    public bool applyRootMotion;

    public bool continuousSound;
    public ParticleInfo particleEffect = new ParticleInfo();
    public BasicMoveReference reference;

    [HideInInspector]
    public string name;
    [HideInInspector]
    public bool editorToggle;
    [HideInInspector]
    public bool soundEffectsToggle;

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}


[System.Serializable]
public class ParticleInfo : ICloneable
{
    public bool editorToggle;
    public GameObject prefab;
    public float duration = 1;
    public bool stick = false;
    public Vector3 offSet;
    public BodyPart bodyPart;

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}

[System.Serializable]
public class BasicMoves : ICloneable
{
    public BasicMoveInfo idle = new BasicMoveInfo();
    public BasicMoveInfo moveForward = new BasicMoveInfo();
    public BasicMoveInfo moveForwardOfFly = new BasicMoveInfo();
    public BasicMoveInfo death = new BasicMoveInfo();
    public BasicMoveInfo moveBack = new BasicMoveInfo();
    public BasicMoveInfo moveBackOfFly = new BasicMoveInfo();
    public BasicMoveInfo moveLeft = new BasicMoveInfo();
    public BasicMoveInfo moveRight = new BasicMoveInfo();
    public BasicMoveInfo getHitHigh = new BasicMoveInfo();
    public BasicMoveInfo getHitLow = new BasicMoveInfo();

    public bool moveEnabled = true;
    public bool jumpEnabled = true;
    public bool crouchEnabled = false;
    public bool blockEnabled = false;
    public bool parryEnabled = false;

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}

public enum ButtonPress
{
	Button1,
	Button2,
	Button3,
	Button4,
	Button5,
	Button6,
	Button7,
	Button8,
}

public enum BasicMoveReference {
	Idle,
	MoveForward,
    MoveForwardOfFly,
    MoveBack,
    MoveLeft,
    MoveRight,
	JumpStraight,
	JumpBack,
	JumpForward,
    Landing,
    HitStandingHigh,
    HitStandingLow,
    HitStandingCrouching,
    HitAirJuggle,
    HitKnockBack,
    HitStandingHighKnockdown,
    HitStandingMidKnockdown,
    HitSweep,
    HitCrumple,
    Death,
    None,
}

public enum ClipNum {
	Clip1,
	Clip2,
	Clip3,
	Clip4,
	Clip5,
	Clip6
}

public enum StandUpOptions {
	None,
	DefaultClip,
	HighKnockdownClip,
	LowKnockdownClip,
	SweepClip,
    AirJuggleClip,
    KnockBackClip,
    CrumpleClip,
    StandingWallBounceClip,
    AirWallBounceClip,
    GroundBounceClip
}

public enum InputType {
	HorizontalAxis,
	VerticalAxis,
	Button,
}

public enum PossibleStates {
	Stand,
	Crouch,
	StraightJump,
	ForwardJump,
	BackJump,
	Down
}

public enum SubStates {
	Resting,
	MovingForward,
	MovingBack,
    MovingLeft,
    MovingRight,
    Blocking,
	Stunned,
}

public enum CombatStances {
	Stance1,
	Stance2,
	Stance3,
	Stance4,
	Stance5,
	Stance6,
	Stance7,
	Stance8,
	Stance9,
	Stance10
}

public enum DamageType {
	Percentage,
	Points
}

public enum AttackType {
	Neutral,
	NormalAttack,
	ForwardLauncher,
	BackLauncher,
	Dive,
	AntiAir,
	Projectile,
    Flash,
}

public enum GaugeUsage {
	Any,
	None,
	Quarter,
	Half,
	ThreeQuarters,
	All
}

public enum ProjectileType {
	Shot,
	Beam
}

public enum HitType {
	Mid,
	Low,
	Overhead,
	Launcher,
	HighKnockdown,
	MidKnockdown,
	KnockBack,
	Sweep
}

public enum HitStrengh {
	Weak,
	Medium,
	Heavy,
	Crumple,
	Custom1,
	Custom2,
	Custom3
}

public enum HitStunType {
	FrameAdvantage,
	Frames,
	Seconds
}

public enum LinkType {
	HitConfirm,
	CounterMove,
	NoConditions
}

public enum CounterMoveType {
	MoveFilter,
	SpecificMove
}

public enum CinematicType {
	CameraEditor,
	AnimationFile,
	Prefab
}

public enum CharacterDistance {
	Any,
	VeryClose,
	Close,
	Mid,
	Far,
	VeryFar,
	Other
}

public enum FrameSpeed {
	Any,
	VerySlow,
	Slow,
	Normal,
	Fast,
	VeryFast
}

public enum JumpArc {
	Any,
	TakeOff,
	Jumping,
	Top,
	Falling,
	Landing,
	Other
}

public enum CurrentFrameData {
	Any,
	StartupFrames,
	ActiveFrames,
	RecoveryFrames
}


[System.Serializable]
public class SoundEffect : ICloneable
{
    public int castingFrame;
    public AudioClip sound;
    public AudioClip[] sounds = new AudioClip[0];

    [HideInInspector]
    public bool soundEffectsToggle;

    public bool casted { get; set; }

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}

[System.Serializable]
public class BodyPartVisibilityChange: ICloneable {
	public int castingFrame;
	public BodyPart bodyPart;
	public bool visible;
	
	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

public enum ProjectileMoveableType
{
    normal,//通用弹道，可以移动
    laser,//激光射线弹道
    boxWithoutMove, //box碰撞,不能移动弹道
    circleWithoutMove,//circle碰撞,不能移动弹道
}

[System.Serializable]
public class Projectile: ICloneable
{
	public int castingFrame = 1;
	public GameObject projectilePrefab;
	public GameObject impactPrefab;
    public Vector3 startPos = Vector3.zero;
    public BodyPart bodyPart;

	public int speed = 20;
    public float range;
    public Vector2 rangeOfV2;
    public float directionAngle;
    public float gravity = 9.81f;
    public float duration;
	public float impactDuration = 1;
    public float selfRotationSpeed = 0;

    public Sizes spaceBetweenHits;

    public ProjectileMoveableType moveableType = ProjectileMoveableType.normal;

    public ProjectileMovementUtil.ThroughType throughEnemyType = ProjectileMovementUtil.ThroughType.collider;
    public ProjectileMovementUtil.ThroughType throughWallType = ProjectileMovementUtil.ThroughType.collider;
    public ProjectileMovementUtil.MovementType movementType = ProjectileMovementUtil.MovementType.line;

    public bool unblockable;

	public HitBox hitBox;
	public HurtBox hurtBox;

    public string moveName;

	public bool overrideHitEffects;
    public bool overrideHitAnimation;
    public BasicMoveReference newHitAnimation = BasicMoveReference.HitKnockBack;

	public HitTypeOptions hitEffects;

	public HitStrengh hitStrength;

    public Vector2 pushForce;
    public float pushForceTime;

    public DebuffType debuffType;
    public float debuffParam;

    public uint skillId;
    public uint id;
    public Vector3 directionVec = Vector3.zero;
    public bool isCastFromBodyPart = false;

    [HideInInspector] public bool damageOptionsToggle;
	[HideInInspector] public bool hitStunOptionsToggle;
	[HideInInspector] public bool preview;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class AppliedForce: ICloneable
{
	public int castingFrame;
	public bool resetPreviousVertical;
	public bool resetPreviousHorizontal;
	public Vector2 force;
    public float forceTweenTime;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class Hit: ICloneable
{
	public int activeFramesBegin;
	public int activeFramesEnds;

    public bool techable = true;
	public Sizes spaceBetweenHits;
    public PlayerConditions opponentConditions = new PlayerConditions();

	public bool overrideHitEffects;
	public HitTypeOptions hitEffects;
	public bool overrideEffectSpawnPoint;
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrokeHitBox;
    public bool overrideHitAnimationBlend;
    public float newHitBlendingIn;

    public bool overrideHitAnimation;

    public BasicMoveReference newHitAnimation = BasicMoveReference.HitKnockBack;

    public HitStrengh hitStrength;
    public Vector2 pushForce;
    public Vector3 pushForceDirection;
    public float pushForceTime;

    public DebuffType debuffType;
    public float debuffParam;

    public bool cornerPush = true;

    public bool groundBounce = true;
    public bool overrideForcesOnGroundBounce = false;
    public bool resetGroundBounceHorizontalPush;
    public bool resetGroundBounceVerticalPush;
    public Vector2 groundBouncePushForce;

	public bool damageScaling;
	public DamageType damageType;
	public float damageOnHit;
	public float damageOnBlock;
	public bool doesntKill;

    public MeleeMoveScript meleeMoveScript;


    public HurtBox[] hurtBoxes = new HurtBox[0];

	[HideInInspector]	public bool opponentForceToggle;
	[HideInInspector]	public bool selfForceToggle;

	[HideInInspector]	public bool damageOptionsToggle;
	[HideInInspector]	public bool forceOptionsToggle;
	[HideInInspector]	public bool overrideEventsToggle;
	[HideInInspector]	public bool hitConditionsToggle;
	[HideInInspector]	public bool hurtBoxesToggle;

	
	public bool disabled{get; set;}

	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class FrameLink: ICloneable
{
	public LinkType linkType = LinkType.NoConditions;
	public bool allowBuffer = true;
    public bool onStrike = true;
	public bool onBlock = true;
	public bool onParry = true;
	public int activeFramesBegins;
	public int activeFramesEnds;
	public CounterMoveType counterMoveType;
	public MoveInfo counterMoveFilter;
	public bool disableHitImpact = true;
	public bool anyHitStrength = true;
	public HitStrengh hitStrength;
	public bool anyStrokeHitBox = true;
	public HitBoxType hitBoxType;
	public bool anyHitType = true;
	public HitType hitType;
	public bool ignoreInputs;
	public bool ignorePlayerConditions;
	public int nextMoveStartupFrame = 1;

	public MoveInfo[] linkableMoves = new MoveInfo[0];

    public bool cancelable { get; set; }
    public bool counterCancelable { get; set; }
	[HideInInspector]	public bool linkableMovesToggle;
	[HideInInspector]	public bool hitConfirmToggle;
	[HideInInspector]	public bool counterMoveToggle;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class MoveParticleEffect: ICloneable {
	public int castingFrame;
	public ParticleInfo particleEffect;

	public bool casted{get; set;}

	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class CallNpc: ICloneable {
	public int castingFrame;
	public CharacterInfo characterInfo;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class StanceChange: ICloneable {
	public int castingFrame;
	public CombatStances newStance;
	
	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class CharacterSpecificMoves {
	public MoveInfo move;
	public string characterName;
}

[System.Serializable]
public class PossibleMoveStates: ICloneable {
	public PossibleStates possibleState;
	public JumpArc jumpArc;
	public int jumpArcBegins = 0;
	public int jumpArcEnds = 100;
	
	public CharacterDistance opponentDistance;
	public int proximityRangeBegins = 0;
	public int proximityRangeEnds = 100;

	public bool movingForward = true;
	public bool movingBack = true;

	public bool standBy = true;
	public bool blocking;
	public bool stunned;
    //public bool isInAir = true;
    //public bool isNotInAir = true;
    //public bool isNeedBoom = false;
    //public bool isNeedKnife = false;
    public bool isContinueNormalAttack = false;
    //public bool isFast = true; 
    //public bool isNotFast = true; 
	public int weaponIndex = -1;
	public WeaponType weaponType = WeaponType.None;

    public object Clone() {
		return CloneObject.Clone(this);
	}
}

public enum WeaponType
{
	Melee,
	Range,
	None,
}

[System.Serializable]
public class PlayerConditions {
	public BasicMoveReference[] basicMoveLimitation = new BasicMoveReference[0];
	public PossibleMoveStates[] possibleMoveStates = new PossibleMoveStates[0];

	[HideInInspector]	public bool basicMovesToggle = false;
	[HideInInspector]	public bool statesToggle = false;
}

[System.Serializable]
public class AnimSpeedKeyFrame: ICloneable {
    public int castingFrame = 0;
    public float speed = 1;
    
    public object Clone() {
        return CloneObject.Clone(this);
    }
}

[System.Serializable]
public class MoveCastableTimeInfo
{
    public string moveName;
    public float currentCdTime = 0f;
    public float currentCastableCount = 0f;
    public float currentResetCastableGapTime = 0f;

    public float cdTime = 0f;
    public float castableCount = 0f;
    public float resetCastableGapTime = 0f;

    public bool isNewCast = false;
}

[System.Serializable]
public class MoveInfo: ScriptableObject
{
    public GameObject characterPrefab;
	public string moveName;
    public Sprite icon;
    public float cdTime = 0f;
    public float castableCount = 0f;
    public float resetCastableGapTime = 0f;
   
    public string description;

	public int fps = 30;

	public int frameWindowRotation;

    public PossibleStates[] possibleStates = new PossibleStates[0];
	public PossibleMoveStates[] possibleMoveStates = new PossibleMoveStates[0];

	public AnimationClip animationClip;

	public WrapMode wrapMode;

    public bool fixedSpeed = true;
    public float animationSpeed = 1;
    public AnimSpeedKeyFrame[] animSpeedKeyFrame = new AnimSpeedKeyFrame[0];
	public int totalFrames = 15;

	public int startUpFrames = 0;
	public int activeFrames = 1;
	public int recoveryFrames = 2;
	public bool applyRootMotion = false;
	public bool forceGrounded = false;
	public BodyPart rootMotionNode = BodyPart.none;
	public bool overrideBlendingIn = true;
	public bool overrideBlendingOut = false;
	public float blendingIn = 0;
	public float blendingOut = 0;

	public bool onReleaseExecution;
	public bool onPressExecution = true;
	public ButtonPress[] buttonSequence = new ButtonPress[0];
	public ButtonPress[] buttonExecution = new ButtonPress[0];
    public SoundEffect[] soundEffects = new SoundEffect[0];
    public AppliedForce[] appliedForces = new AppliedForce[0];
	public CallNpc[] callNpcs = new CallNpc[0];

    public MoveInfo[] previousMoves = new MoveInfo[0];
	public FrameLink[] frameLinks = new FrameLink[0];
	
	public MoveParticleEffect[] particleEffects = new MoveParticleEffect[0];

	public BodyPartVisibilityChange[] bodyPartVisibilityChanges = new BodyPartVisibilityChange[0];

	public StanceChange[] stanceChanges = new StanceChange[0];
	
	public Hit[] hits = new Hit[0];
	
	public Projectile[] projectiles = new Projectile[0];
    public PlayerConditions opponentConditions = new PlayerConditions();
	public PlayerConditions selfConditions = new PlayerConditions();

	[HideInInspector]
    public bool speedKeyFrameToggle = false;

    public bool cancelable { get; set; }
	public bool kill{get; set;}
	public int currentFrame {get; set;}
	public int overrideStartupFrame{get; set;}
	public float animationSpeedTemp{get; set;}
	public float currentTick{get; set;}
	public bool hitConfirmOnBlock{get; set;}
	public bool hitConfirmOnParry{get; set;}
	public bool hitConfirmOnStrike{get; set;}
	public bool hitAnimationOverride{get; set;}
	public CurrentFrameData currentFrameData{get; set;}

    private int skillId = 0;
    public int SkillId
    {
        get
        {
            if (skillId <= 0)
            {
                int.TryParse(moveName, out skillId);
            }
            return skillId;
        }
    }
}