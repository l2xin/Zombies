using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Pathfinding;

/// <summary>
/// Player
/// </summary>
public class Player : MonoBehaviour
{
    private readonly float rotationSpeed = 50f;
	protected Transform myTransform;
    public bool isAIController = false;
    public GameObject character;
    public CharacterInfo myInfo;
    public UFEMapUnit ufeMapUnit = UFEMapUnit.player;
 
    public PossibleStates currentState;
    public SubStates currentSubState;

    public float stunTime;
    public bool isDead;

    public MoveInfo currentMove;
    public MoveInfo currentBlockMove;
    public MoveInfo storedMove;
    public float shakeDensity;
    public BasicMoveReference currentBasicMove;

    public UFECamp camp = UFECamp.Camp1;
    public bool isOffline = false;

    public List<ProjectileMoveScript> projectiles = new List<ProjectileMoveScript>();
    public MoveSetScript MoveSet { get { return this.myMoveSetScript; } }
    public PhysicsScript Physics { get { return this.myPhysicsScript; } }
    public AIGroupController AIGroupController { get { return this.myAIGroupController; } }

    public int playerNum;
    public ulong id;

    public float normalizedDistance;
    private bool isFast = false;
    private bool isStun = false;
    public bool isLookAtTarget = false;
    public string currentHitAnimation;
    public Hit currentHit;
    public bool applyRootMotion = false;

    private PhysicsScript myPhysicsScript;
    private MoveSetScript myMoveSetScript;
    public HitBoxesScript myHitBoxesScript;
    private AIGroupController myAIGroupController;

    private CameraScript cameraScript;

    private Quaternion targetRotation;

    private bool shakeCharacter;
    public bool shakeCamera;

    private float afkTimer;

    public float storedMoveTime;

    public HurtBox hurtBox;
    public CharacterInfo turnCharStorage = null;

    private GameObject shadow;

    private Vector3 offsetY = Vector3.zero;

    private bool isVisible = false;
    public VisibleChecker visibleChecker;
    private SkinnedMeshRenderer[] skinnedRendererList;
    private List<MeshRenderer> meshRendererList;

    private bool isContinueNormalAttack;
    public int aleadyEatHealthCount = 0;
    private bool isNetFast;

    private bool isNetPotionOfAngerBuff;
    private float netX = 0f;
    private float netZ = 0f;
    public Vector3 netPos = Vector3.zero;
    public float netRotAngleX;
    public float netRotAngleZ;
    private int netStatus;
    public string characterName;

    private Vector3 eulerAngles = Vector3.zero;
    private HurtBox[] hurtBoxList = null;
    private Vector2 particleEffectOffset = Vector2.zero;
    public Quaternion rotBeforeTurn = Quaternion.identity;
    public string gameObjectName = "";

    private bool netDead = false;
    private float fixedDeltaTimeOfFps = 0;

    private float updateHeroMovePositionTimeOffset = 0;

    private bool isInputFree = true;

    public Vector2 moveDirection = Vector2.zero;
    public Vector2 rotDirection = Vector2.zero;
    public Vector3 rotDirectionOfV3 = Vector3.zero;

    public bool isStopMove = false;
    public int skinId = 0;

    public bool isFireHeroDie = false;
    private Dictionary<string, MoveCastableTimeInfo> moveCdTimeInfoMap = new Dictionary<string, MoveCastableTimeInfo>();

    public CharacterBody Body;
    public bool isHero = false;
    private int currentUpdateHudIndex = 0;
    private readonly int maxUpdateHudIndexGap = 5;
    private Vector3 hitKnockBackTargetPos = Vector3.zero;

    private List<GameObject> particleEffectsList = new List<GameObject>();

    private Vector3 hitMoveDirection;
    private BloodBar bloodBar;

    public CharacterController characterController;
    private Seeker seeker;
    private FunnelModifier funnelModifier;
	private string ownerTag;

	private Dictionary<BodyPart, MeleeMoveScript> meleeAttackBodyPartMap;
	private BoxCollider playerCollider;
	private Transform cameraFollowTarget;
	private MeleeMoveScript meleeMoveScript;
    private List<GameObject> followNpcList;

    public bool IsFast
    {
        get
        {
            return isFast;
        }
        set
        {
            if (isFast != value)
            {
                isFast = value;
                if (isFast)
                {
                    Physics.PlayEffect(FightManager.config.roundOptions.fastEffect, 100f);
                }
                else
                {
                    Physics.RemoveEffect(FightManager.config.roundOptions.fastEffect);
                }
            }
        }
    }

    public bool IsStun
    {
        get
        {
            return isStun;
        }
        set
        {
            if (isStun != value)
            {
                isStun = value;
                if (isStun)
                {
                    Physics.PlayEffect(FightManager.config.roundOptions.decelerateSpeedEffect, 100f, 0, character.transform.localScale.x, false);
                }
                else
                {
                    Physics.RemoveEffect(FightManager.config.roundOptions.decelerateSpeedEffect);
                }
            }
        }
    }

    public bool IsContinueNormalAttack
    {
        get
        {
            return isContinueNormalAttack;
        }
        set
        {
            isContinueNormalAttack = value;
        }
    }

    void Awake()
    {
        gameObjectName = gameObject.name;
		myTransform = transform;
        myPhysicsScript = gameObject.AddComponent<PhysicsScript>();
		meleeAttackBodyPartMap = new Dictionary<BodyPart, MeleeMoveScript>();
        followNpcList = new List<GameObject>();
    }

    void Start()
    {
		ownerTag = tag;
        SyncCommonAttribute();
		TurnPlayer(turnCharStorage);
        InitHud();

		if (isHero) {
            myAIGroupController = gameObject.AddComponent<AIGroupController>();
            cameraFollowTarget = new GameObject ().transform;
			cameraFollowTarget.parent = this.transform.parent;
			cameraFollowTarget.localPosition = this.transform.localPosition;
			cameraFollowTarget.name = "CameraFollowTarget";
			FightManager.cameraScript.SetTarget (cameraFollowTarget);
			FightManager.FireGameBegins ();
			FightManager.CastNewRound ();
		}

        FightManager.config.lockMovements = false;
        switch (ufeMapUnit)
        {
            case UFEMapUnit.none:
                break;
            case UFEMapUnit.player:
                gameObject.layer = (int)MapUnitLayer.player;
                break;
            case UFEMapUnit.monster:
                gameObject.layer = (int)MapUnitLayer.monster;
                break;
            default:
                break;
        }
        gameObject.tag = ownerTag;
        turnCharStorage = null;

        FightManager.OnHeroSpeedChangeHandler += onHeroSpeedChangeHandler;
        FightManager.OnHeroLifeNumChange += OnHeroLifeNumChange;
        FightManager.OnCoinChange += onAddCoin;
        FightManager.OnRemoveProjectileHandler += onRemoveProjectileHandler;

        rotDirection.x = 0;
        rotDirection.y = 1;

        rotDirectionOfV3.x = 0;
        rotDirectionOfV3.y = 0;
        rotDirectionOfV3.z = 1;

		if (isHero) {
			//SetTimeout.Start (TestChangeWeapon, 3f);
		}
    }

	private void TestChangeWeapon()
	{
		myPhysicsScript.UpdateWeapon (WeaponType.Range, 3);
	}

    private void onRemoveProjectileHandler(ProjectileMoveScript projectile)
    {
        if (projectile != null)
        {
            projectiles.Remove(projectile);
        }
    }

    private void onAddCoin(ulong id, int oldValue, int newValue)
    {
        if (isHero && this.id == id)
        {
            if (newValue > oldValue)
            {
                Physics.PlayEffect(FightManager.config.roundOptions.getCoinEffect, 1.3f, myInfo.offsetY);
            }
        }
    }

    private void OnHeroLifeNumChange(ulong id, int oldValue, int newValue)
    {
        if (this.id == id)
        {
            myInfo.currentLifePoints = newValue;
            if (visibleChecker.isVisible)
            {
                float scale = Mathf.Min(1.5f, Body.transform.localScale.x);
                Physics.PlayEffect(FightManager.config.roundOptions.healthEffect, 1f, 0f, scale);
            }
        }
    }

    private void onHeroSpeedChangeHandler(ulong id, int oldValue, int newValue)
    {
        if (this.id == id)
        {
            myInfo.speed = newValue * FightManager.MAP_SCALE;
        }
    }

    private void InitHud()
    {
        bloodBar = new BloodBar();
        bloodBar.id = id;
        bloodBar.myInfo = myInfo;
        bloodBar.visibleChecker = visibleChecker;
        bloodBar.hero = transform;
        bloodBar.offsetY = offsetY;
        bloodBar.InitHud();
    }

    private void SyncCommonAttribute()
    {
        MsgPlayer playerModel = FightManager.playerModelMap[id];
        netX = playerModel.Realposx;
        netZ = playerModel.Realposz;
        camp = (UFECamp)playerModel.camp;
    }

    private void RemovePotionOfAngerBuff()
    {
        FightManager.GetHero().myInfo.isPotionOfAngerBuff = false;
    }

    public void UpdateHUD()
    {
        if (bloodBar != null)
        {
            bloodBar.UpdateHUD();
        }
    }

    void OnDestroy()
    {
        FightManager.RemoveDelaySynchronizedAction(RemovePotionOfAngerBuff);

        FightManager.OnCoinChange -= onAddCoin;
        FightManager.OnRemoveProjectileHandler -= onRemoveProjectileHandler;
        FightManager.OnHeroSpeedChangeHandler -= onHeroSpeedChangeHandler;
        FightManager.OnHeroLifeNumChange -= OnHeroLifeNumChange;

        SetTimeout.Clear(FireHeroDie);
        SetTimeout.Clear(AutoRelife);

        isFireHeroDie = false;
        myPhysicsScript = null;
        myMoveSetScript = null;
    }

    public void TurnPlayer(CharacterInfo turnCharStorage)
    {
        if (BattleInputController.Instance != null)
        {
            BattleInputController.Instance.moveInfoMap.Clear();
        }

        DespawnerCharacter();

        if (isHero)
        {
            playerNum = (int)PlayerCamp.self;
            if (turnCharStorage != null)
            {
                myInfo = (CharacterInfo)GameObject.Instantiate(turnCharStorage);
            }
            else
            {
                InstantiatePlayerCharacter();
                myInfo.currentTurnContinueTime = 0f;
                myInfo.totalTurnContinueTime = 0f;
            }

            FightManager.config.player1Character = myInfo;
            FightManager.FireHeroTurn(myInfo);
        }
        else
        {
            playerNum = (int)PlayerCamp.opponent;
            if (turnCharStorage != null)
            {
                myInfo = (CharacterInfo)GameObject.Instantiate(turnCharStorage);
            }
            else
            {
                InstantiatePlayerCharacter();
            }
        }

        MsgPlayer msgPlayer = FightManager.playerModelMap[id];
        myInfo.currentLifePoints = myInfo.lifePoints;
        myInfo.currentLv = msgPlayer.Level;
        myInfo.characterName = msgPlayer.name;
        myInfo.gameObjectName = gameObjectName;
        myInfo.playerNum = playerNum;
        myInfo.camp = camp;
        myInfo.mapUnit = (int)ufeMapUnit;
        myInfo.id = id;
        myInfo.isHero = isHero;
        myInfo.isOffline = isOffline;

        character = PoolUtil.SpawnerGameObject(myInfo.characterPrefab);

        if (character.GetComponent<CharacterController>() != null)
        {
            BoxCollider characterBoxCollider = character.GetComponent<BoxCollider>();
            CharacterController characterCharacterController = character.GetComponent<CharacterController>();

            characterController = gameObject.AddComponent<CharacterController>();
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                boxCollider = gameObject.AddComponent<BoxCollider>();
                seeker = gameObject.AddComponent<Seeker>();
                funnelModifier = gameObject.AddComponent<FunnelModifier>();
            }
			float localScale = character.transform.localScale.x;
			boxCollider.center = characterBoxCollider.center * localScale;
			boxCollider.size = characterBoxCollider.size * localScale;
            boxCollider.isTrigger = false;
            boxCollider.material = characterBoxCollider.material;
			playerCollider = boxCollider;

			characterController.radius = characterCharacterController.radius * localScale;
			characterController.center = characterCharacterController.center * localScale;
			characterController.height = characterCharacterController.height * localScale;
            characterController.skinWidth = characterCharacterController.skinWidth;

            characterCharacterController.enabled = false;
            characterBoxCollider.enabled = false;
            //GameObject.Destroy(characterCharacterController);
            //GameObject.Destroy(characterBoxCollider);
        }

        Body = character.GetComponent<CharacterBody>();

        character.transform.parent = transform;

        character.transform.localPosition = Vector3.zero;
        if (character.GetComponent<MoveSetScript>() == null)
        {
            character.AddComponent<MoveSetScript>();
            myMoveSetScript = character.GetComponent<MoveSetScript>();
            myMoveSetScript.ChangeMoveStances(this);
        }
        else
        {
            myMoveSetScript = character.GetComponent<MoveSetScript>();
            myMoveSetScript.Respawn(this);
        }

        if (turnCharStorage != null)
        {
            character.transform.rotation = rotBeforeTurn;
        }

        myHitBoxesScript = character.GetComponent<HitBoxesScript>();
        cameraScript = FightManager.gameEngine.GetComponent<CameraScript>();
        skinnedRendererList = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshRenderer[] meshRendererArr = character.GetComponentsInChildren<MeshRenderer>();
        meshRendererList = new List<MeshRenderer>(meshRendererArr);

        if (skinnedRendererList.Length > 0)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = skinnedRendererList[0];
            visibleChecker = skinnedMeshRenderer.gameObject.AddComponent<VisibleChecker>();
        }
        else if (meshRendererList.Count > 0)
        {
            MeshRenderer meshRenderer = meshRendererList[0];
            visibleChecker = meshRenderer.gameObject.AddComponent<VisibleChecker>();
        }

        if (turnCharStorage != null)
        {
            float rotY = rotBeforeTurn.eulerAngles.y * Mathf.Deg2Rad;
            myPhysicsScript.MoveDirectionOf = new Vector2(Mathf.Sin(rotY), Mathf.Cos(rotY));
        }

        HitBox hitBox = myHitBoxesScript.GetHitBox(BodyPart.root);
        hurtBox = new HurtBox();
        hurtBox.bodyPart = BodyPart.root;
        hurtBox.shape = hitBox.shape;
        hurtBox.radius = hitBox.radius;
        hurtBox.offSet = hitBox.offSet;

        UpdateHeadOffsetY();

        if (shadow == null)
        {
            shadow = PoolUtil.SpawnerGameObject(FightManager.config.roundOptions.shadow);
            shadow.transform.parent = transform;
            shadow.transform.localPosition = Vector3.up * 0.05f;
            shadow.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        shadow.transform.localScale = character.transform.localScale * .8f;
        if (Body != null)
        {
            Body.Shadow = shadow;
        }
        if (turnCharStorage == null)
        {
            transform.position = new Vector3(netX, 0.001f, netZ);
        }
    }

	public void UpdateWeaponMeleeMoveScript(BodyPart bodyPart)
	{
		if (myInfo.weaponType == WeaponType.Melee) {
			GameObject body = myHitBoxesScript.GetTransform(bodyPart).gameObject;
			meleeMoveScript = body.GetComponent<MeleeMoveScript> ();
			if (meleeMoveScript == null) {
				meleeMoveScript = body.AddComponent<MeleeMoveScript> ();
			}
			meleeMoveScript.myControlsScript = this;
			meleeMoveScript.ownerTag = ownerTag;
			meleeMoveScript.bodyPart = bodyPart;
			meleeAttackBodyPartMap.Remove (bodyPart);
			meleeAttackBodyPartMap.Add (bodyPart, meleeMoveScript);
			#if UNITY_EDITOR
			Debug.Log ("UpdateWeaponMeleeMoveScript " + meleeMoveScript.ownerTag);
			#endif
		}
	}

	public void UpdateWeaponMoveSkill()
	{
		#if UNITY_EDITOR
		Debug.Log ("UpdateWeaponMoveSkill " + myInfo.weaponType + myInfo.weaponIndex);
		#endif
		MoveInfo move = MoveSet.GetMove(myInfo.weaponType, myInfo.weaponIndex);
		if (move != null) {
			myInfo.skillId.Value = move.SkillId;
			#if UNITY_EDITOR
			Debug.Log ("UpdateWeaponMoveSkill " + myInfo.skillId.Value);
			#endif
		}
	}

    public void UpdateHeadOffsetY()
    {
        Vector3 head = myHitBoxesScript.GetPosition(BodyPart.head);
        myInfo.offsetY = head.y;
        offsetY.y = myInfo.offsetY;
    }

    private void InstantiatePlayerCharacter()
    {
        myInfo = (CharacterInfo)GameObject.Instantiate(FightManager.GetTargetCharacterInfo(camp, ufeMapUnit, isHero, skinId));
    }

    private void testCharacterRotation(float rotationSpeed)
    {
        testCharacterRotation(rotationSpeed, false);
    }

    public void testCharacterRotation(float rotationSpeed, bool forceMirror)
    {
        rotationOfBattleInputController(rotationSpeed);
    }

    private void rotationOfBattleInputController(float rotationSpeed)
    {
        if (character != null && isDead == false)
        {
            if (playerNum == (int)PlayerCamp.self)
            {
                float h = BattleInputController.Instance.horizontalAxisRight;
                float v = BattleInputController.Instance.verticalAxisRight;
                rotationOfTarget(h, v, rotationSpeed);
            }
            else
            {
                rotationOfTarget(netRotAngleX, netRotAngleZ, rotationSpeed);
            }
        }
    }

    private void rotationOfTarget(float h, float v, float rotationSpeed)
    {
        if (h != 0 || v != 0)
        {
            Vector3 axis = new Vector3(h, 0f, v);
            rotDirection.x = h;
            rotDirection.y = v;
            rotDirection.Normalize();
            rotDirectionOfV3.x = rotDirection.x;
            rotDirectionOfV3.z = rotDirection.y;
            Quaternion target = Quaternion.LookRotation(axis);
            //character.transform.rotation = Quaternion.Slerp(character.transform.rotation, target, Time.fixedDeltaTime * rotationSpeed);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.fixedDeltaTime * rotationSpeed);
        }
    }

    private void validateRotation()
    {
        if (Physics.Freeze == false)
        {
            testCharacterRotation(rotationSpeed);
        }
    }

    public void DoFixedUpdate()
    {
        if (myInfo == null)
        {
            return;
        }
        if (myPhysicsScript == null)
        {
            return;
        }

        if (myInfo.totalTurnContinueTime > 0)
        {
            myInfo.currentTurnContinueTime -= Time.fixedDeltaTime;
        }

        resolveMove();

        translateInputs();

        validateRotation();

        if (!myPhysicsScript.Freeze
            && !isDead
            && currentSubState != SubStates.Stunned
            && !myPhysicsScript.IsMoving()
            && currentMove == null
            && !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle))
        {
            myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
            currentState = PossibleStates.Stand;
            currentSubState = SubStates.Resting;
        }

        if (currentMove != null)
        {
            ReadMove(currentMove);
        }

        if (shakeDensity > 0)
        {
            shakeDensity -= Time.fixedDeltaTime;
            if (myHitBoxesScript.isHit)
            {
                if (shakeCharacter)
                {
                    shake();
                }
            }
        }
        else if (shakeDensity < 0)
        {
            shakeDensity = 0;
            shakeCamera = false;
            shakeCharacter = false;
        }

        if ((currentSubState == SubStates.Stunned) && stunTime > 0 && !myPhysicsScript.Freeze)
        {
            ApplyStun();
        }
        for (int i = 0; i < myInfo.debuffList.Count; i++)
        {
            Debuff debuff = myInfo.debuffList[i];
            if (debuff.isShowViewed == false)
            {
                DebuffView view = gameObject.AddComponent<DebuffView>();
                view.AddDebuff(debuff);
                debuff.view = view;
                debuff.isShowViewed = true;
            }
        }

        myPhysicsScript.ApplyForces(currentMove);

        CheckMoveCastableTime();

		if (isHero) {
			cameraFollowTarget.position = myTransform.position;
		}
    }

    private void resolveMove()
    {
        if (storedMoveTime > 0)
        {
            storedMoveTime -= Time.fixedDeltaTime;
        }
        if (storedMoveTime <= 0 && storedMove != null)
        {
            storedMoveTime = 0;
            if (FightManager.config.executionBufferType != ExecutionBufferType.NoBuffer)
            {
                storedMove = null;
            }
        }
        if (currentMove != null && storedMove == null)
        {
            if (myMoveSetScript != null)
            {
                storedMove = myMoveSetScript.GetNextMove(currentMove);
            }
        }

        if ((currentMove == null || currentMove.cancelable) && storedMove != null)
        {
            bool confirmQueue = false;
            bool ignoreConditions = false;
            if (currentMove != null && FightManager.config.executionBufferType == ExecutionBufferType.OnlyMoveLinks)
            {
                for (int j = 0; j < currentMove.frameLinks.Length; j++)
                {
                    FrameLink frameLink = currentMove.frameLinks[j];
                    confirmQueue = false;
                    ignoreConditions = false;
                    if (storedMove != null)
                    {
                        if (frameLink.cancelable)
                        {
                            confirmQueue = true;
                        }
                        else if (frameLink.counterCancelable)
                        {
                            confirmQueue = true;
                        }
                        if (frameLink.ignorePlayerConditions)
                        {
                            ignoreConditions = true;
                        }
                        if (confirmQueue)
                        {
                            for (int i = 0; i < frameLink.linkableMoves.Length; i++)
                            {
                                MoveInfo move = frameLink.linkableMoves[i];
                                if (storedMove.name == move.name)
                                {
                                    storedMove.overrideStartupFrame = frameLink.nextMoveStartupFrame - 1;
                                    if (confirmQueue &&
                                       (ignoreConditions || (myMoveSetScript != null && myMoveSetScript.ValidateMoveStances(storedMove.selfConditions, this))))
                                    {
                                        KillCurrentMove();
                                        this.SetMove(storedMove);

                                        storedMove = null;
                                        storedMoveTime = 0;
                                        break;
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        if (storedMove != null)
        {
            storedMoveTime = 0f;
        }
    }

    private void translateInputs()
    {
        if (isDead)
        {
            isInputFree = true;
            return;
        }
        if (currentSubState != SubStates.Stunned
            && isDead == false
            && Physics.Freeze == false
            && isHero
            && myPhysicsScript != null)
        {
            if ((BattleInputController.Instance.horizontalLeft != 0 ||
                BattleInputController.Instance.verticalLeft != 0) &&
                myInfo.speed > 0)
            {
                Vector3 axis = new Vector3(BattleInputController.Instance.horizontalLeft, 0f, BattleInputController.Instance.verticalLeft);
                Quaternion target = Quaternion.LookRotation(axis);
                Physics.SetWalkDirection();

                if (currentMove != null)
                {
                    if (IsFast)
                    {
                        myMoveSetScript.PlayMoveAnimation(UFEBasicMove.runFont);
                    }
                    else
                    {
                        myMoveSetScript.PlayMoveAnimation(UFEBasicMove.walkFont);
                    }
                }
                moveDirection.x = BattleInputController.Instance.horizontalLeft;
                moveDirection.y = BattleInputController.Instance.verticalLeft;
                myPhysicsScript.MoveByDirection(false, moveDirection.normalized);
            }
            else
            {
                isInputFree = true;
                myPhysicsScript.StopMove();
            }
        }
    }

    public void MoveByDirection(bool isStop, int mirror, Vector2 dirNormalize)
    {
        if (myPhysicsScript != null)
        {
            myPhysicsScript.MoveByDirection(isStop, dirNormalize);
        }
    }

    public void CastMove(MoveInfo move, bool overrideCurrentMove)
    {
        if (move.cdTime > 0 && LoginManager.playerId == id)
        {
            MoveCastableTimeInfo moveCastableTimeInfo;
            if (moveCdTimeInfoMap.ContainsKey(move.moveName))
            {
                moveCastableTimeInfo = moveCdTimeInfoMap[move.moveName];
            }
            else
            {
                moveCastableTimeInfo = new MoveCastableTimeInfo();
                moveCdTimeInfoMap[move.moveName] = moveCastableTimeInfo;
            }
            moveCastableTimeInfo.moveName = move.moveName;
            moveCastableTimeInfo.cdTime = move.cdTime;
            moveCastableTimeInfo.currentCdTime = move.cdTime;
        }
        CastMove(move, overrideCurrentMove, false, false);
        if (isHero)
        {
            FightManager.FireMove(move, myInfo);
        }
    }

    public void CastMove(MoveInfo move, bool overrideCurrentMove, bool forceGrounded)
    {
        CastMove(move, overrideCurrentMove, forceGrounded, false);
    }

    public void CastMove(MoveInfo move, bool overrideCurrentMove, bool forceGrounded, bool castWarning)
    {
        if (move != null)
        {
            if (castWarning && !myMoveSetScript.HasMove(move.moveName))
            {
                Debug.LogError("Move '" + move.name + "' could not be found under this character's move set.");
            }
            if (overrideCurrentMove)
            {
                KillCurrentMove();
                MoveInfo newMove = PoolUtil.SpawnerMoveInfo(move);
                newMove.name = move.name;
                this.SetMove(newMove);
                currentMove.currentFrame = 0;
                currentMove.currentTick = 0;
            }
            else
            {
                storedMove = PoolUtil.SpawnerMoveInfo(move);
            }
            if (forceGrounded)
            {
                myPhysicsScript.ForceGrounded();
            }
        }
    }

    public void SetMove(MoveInfo move)
    {
        currentMove = move;
        for (int i = 0; i < myHitBoxesScript.hitBoxes.Length; i++)
        {
            HitBox hitBox = myHitBoxesScript.hitBoxes[i];
            if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null)
            {
                bool visible = hitBox.defaultVisibility;
                if (move != null && move.bodyPartVisibilityChanges != null)
                {
                    for (int j = 0; j < move.bodyPartVisibilityChanges.Length; j++)
                    {
                        BodyPartVisibilityChange visibilityChange = move.bodyPartVisibilityChanges[j];
                        if (visibilityChange.castingFrame == 0 && visibilityChange.bodyPart == hitBox.bodyPart)
                        {
                            visible = visibilityChange.visible;
                            visibilityChange.casted = true;
                        }
                    }
                }
                hitBox.position.gameObject.SetActive(visible);
            }
        }
    }

    public void ReadMove(MoveInfo move)
    {
        MoveSet.RemoveAllBasicMoveParticle();

        currentBlockMove = null;

        if (move.currentTick == 0)
        {
            if (!myMoveSetScript.AnimationExists(move.name))
            {
                Debug.LogError("Animation for move '" + move.name + "' not found!");
            }

            if (visibleChecker.isVisible)
            {
                float normalizedTimeConv = myMoveSetScript.GetAnimationNormalizedTime(move.overrideStartupFrame, move);
                if (move.overrideBlendingIn)
                {
                    myMoveSetScript.PlayAnimation(move.name, move.blendingIn, normalizedTimeConv);
                }
                else
                {
                    myMoveSetScript.PlayAnimation(move.name, myInfo.blendingTime, normalizedTimeConv);
                }
            }

            move.currentTick = move.overrideStartupFrame;
            move.currentFrame = move.overrideStartupFrame;
            move.animationSpeedTemp = move.animationSpeed;

            myMoveSetScript.SetAnimationSpeed(move.name, move.animationSpeed);
            if (move.overrideBlendingOut)
            {
                myMoveSetScript.overrideNextBlendingValue = move.blendingOut;
            }
        }

        if (fixedDeltaTimeOfFps == 0)
        {
            fixedDeltaTimeOfFps = Time.fixedDeltaTime * FightManager.config.fps;
        }

        if (myMoveSetScript.animationPaused)
        {
            move.currentTick += fixedDeltaTimeOfFps * myMoveSetScript.GetAnimationSpeed();
        }
        else
        {
            move.currentTick += fixedDeltaTimeOfFps;
        }

        if (move.currentTick > move.currentFrame)
        {
            move.currentFrame++;
        }

        if (move.currentFrame <= move.startUpFrames)
        {
            move.currentFrameData = CurrentFrameData.StartupFrames;
        }
        else if (move.currentFrame > move.startUpFrames && move.currentFrame <= move.startUpFrames + move.activeFrames)
        {
            move.currentFrameData = CurrentFrameData.ActiveFrames;
        }
        else
        {
            move.currentFrameData = CurrentFrameData.RecoveryFrames;
        }

        if (!move.fixedSpeed)
        {
            for (int i = 0; i < move.animSpeedKeyFrame.Length; i++)
            {
                AnimSpeedKeyFrame speedKeyFrame = move.animSpeedKeyFrame[i];
                if (move.currentFrame >= speedKeyFrame.castingFrame
                    && !myPhysicsScript.Freeze)
                {
                    myMoveSetScript.SetAnimationSpeed(move.name, speedKeyFrame.speed * move.animationSpeed);
                }
            }
        }

        for (int i = 0; i < move.projectiles.Length; i++)
        {
            Projectile projectile = move.projectiles[i];
            if (!projectile.casted &&
                projectile.projectilePrefab != null &&
                move.currentFrame >= projectile.castingFrame)
            {
                projectile.casted = true;
                projectile.moveName = move.moveName;
                GameObject projectilePrefab = PoolUtil.SpawnerGameObject(projectile.projectilePrefab);
                ProjectileMoveScript projectileMoveScript = projectilePrefab.AddComponent<ProjectileMoveScript>();
                projectileMoveScript.myControlsScript = this;
                projectileMoveScript.ownerId = id;
                projectileMoveScript.Data = projectile;
                projectiles.Add(projectileMoveScript);
            }
        }
		for (int i = 0; i < move.hits.Length; i++) 
		{
			Hit hit = move.hits[i];
			HurtBox[] activeHurtBoxes = null;
			if (move.currentFrame >= hit.activeFramesBegin &&
			    move.currentFrame <= hit.activeFramesEnds) {
				if (hit.hurtBoxes.Length > 0) {
					activeHurtBoxes = hit.hurtBoxes;
					if (hit.disabled == false) {
						for (int j = 0; j < activeHurtBoxes.Length; j++) {
							HurtBox hurtBox = hit.hurtBoxes[j];
							BodyPart bodyPart = hurtBox.bodyPart;
							meleeAttackBodyPartMap.TryGetValue (bodyPart, out meleeMoveScript);
							if (meleeMoveScript == null) {
								UpdateWeaponMeleeMoveScript (bodyPart);
							}
							meleeMoveScript.Hit = hit;
							meleeMoveScript.DisableHit (false);
                            hit.meleeMoveScript = meleeMoveScript;

                        }
						hit.disabled = true;
					} 
				}
			} 
			else if(hit.meleeMoveScript != null)
			{
                hit.meleeMoveScript.DisableHit (true);
			}
			myHitBoxesScript.activeHurtBoxes = activeHurtBoxes;
		}

        for (int i = 0; i < move.particleEffects.Length; i++)
        {
            MoveParticleEffect particleEffect = move.particleEffects[i];
            if (!particleEffect.casted
                && move.currentFrame >= particleEffect.castingFrame)
            {
                particleEffect.casted = true;

                GameObject particleEffectObject = PoolUtil.SpawnerGameObject(particleEffect.particleEffect.prefab);
                particleEffectObject.transform.localScale = Vector3.one;
                particleEffectObject.gameObject.SetActive(true);
                particleEffectsList.Add(particleEffectObject);
                FightManager.DelaySynchronizedAction(delegate ()
                {
                    PoolUtil.Despawner(particleEffectObject, PoolUtil.particlesPoolName, true);
                    particleEffectsList.Remove(particleEffectObject);
                },
                particleEffect.particleEffect.duration);
                HitBox hitBox = myHitBoxesScript.GetHitBox(particleEffect.particleEffect.bodyPart);
                Transform bodyPart = hitBox.position;
                Vector3 newPosition = bodyPart.position;
                newPosition.x += hitBox.offSet.x;
                newPosition.y += hitBox.offSet.y;

                float x = 0f;
                float z = 0f;

                if (particleEffect.particleEffect.offSet.x != 0 || particleEffect.particleEffect.offSet.z != 0)
                {
                    float atan = Mathf.Atan2(particleEffect.particleEffect.offSet.x, particleEffect.particleEffect.offSet.z);
                    float rotY = character.transform.eulerAngles.y;
                    rotY = rotY * Mathf.Deg2Rad + atan;
                    particleEffectOffset.x = particleEffect.particleEffect.offSet.x;
                    particleEffectOffset.y = particleEffect.particleEffect.offSet.z;
                    float size = particleEffectOffset.magnitude;
                    x = size * Mathf.Sin(rotY);
                    z = size * Mathf.Cos(rotY);
                    newPosition.x += x;
                    newPosition.z += z;
                }

                newPosition.y += particleEffect.particleEffect.offSet.y;

                particleEffectObject.transform.position = newPosition;
                particleEffectObject.transform.rotation = character.transform.rotation;

                if (particleEffect.particleEffect.stick)
                {
                    particleEffectObject.transform.parent = bodyPart;
                }
                else
                {
                    particleEffectObject.transform.parent = FightManager.gameEngine.transform;
                }
            }
        }

        for (int i = 0; i < move.bodyPartVisibilityChanges.Length; i++)
        {
            BodyPartVisibilityChange visibilityChange = move.bodyPartVisibilityChanges[i];
            if (!visibilityChange.casted && move.currentFrame >= visibilityChange.castingFrame)
            {
                for (int j = 0; j < myHitBoxesScript.hitBoxes.Length; j++)
                {
                    HitBox hitBox = myHitBoxesScript.hitBoxes[j];
                    if (visibilityChange.bodyPart == hitBox.bodyPart)
                    {
                        hitBox.position.gameObject.SetActive(visibilityChange.visible);
                        visibilityChange.casted = true;
                    }
                }
            }
        }

        for (int i = 0; i < move.frameLinks.Length; i++)
        {
            FrameLink frameLink = move.frameLinks[i];
            if (move.currentFrame >= frameLink.activeFramesBegins &&
                move.currentFrame <= frameLink.activeFramesEnds)
            {
                if (frameLink.linkType == LinkType.NoConditions ||
                    (frameLink.linkType == LinkType.HitConfirm &&
                    (currentMove.hitConfirmOnStrike && frameLink.onStrike) ||
                    (currentMove.hitConfirmOnBlock && frameLink.onBlock) ||
                    (currentMove.hitConfirmOnParry && frameLink.onParry)))
                {
                    frameLink.cancelable = true;
                    move.cancelable = true;
                }
            }
            else
            {
                frameLink.cancelable = false;
            }
        }

        for (int i = 0; i < move.soundEffects.Length; i++)
        {
            SoundEffect soundEffect = move.soundEffects[i];
            if (soundEffect.casted == false &&
              move.currentFrame >= soundEffect.castingFrame)
            {
                AudioSourceManager.Instance.PlaySound(soundEffect.sounds);
                soundEffect.casted = true;
            }
        }

		for (int i = 0; i < move.callNpcs.Length; i++) {
			CallNpc callNpc = move.callNpcs[i];
			if (callNpc.casted == false && 
				move.currentFrame >= callNpc.castingFrame) {
				SceneAssetsManager.Instance.GeneratorFollowNpc (myTransform.position, myInfo.camp, callNpc.characterInfo);
                MainEntry.RunInNextFrame(AddLeaderFollowAI);
				callNpc.casted = true;
			}
		}
        for (int i = 0; i < move.appliedForces.Length; i++)
        {
            AppliedForce addedForce = move.appliedForces[i];
            if (addedForce.casted == false && move.currentFrame >= addedForce.castingFrame)
            {
                Physics.KnockBack(addedForce.force.x, rotDirectionOfV3 * -1f, addedForce.forceTweenTime);
                addedForce.casted = true;
            }
        }

        if (move.currentFrame >= move.totalFrames)
        {
            KillCurrentMove();
        }
    }

    private void AddLeaderFollowAI()
    {

    }

    public void KillCurrentMove()
    {
        if (currentMove == null)
        {
            return;
        }
        currentMove.currentFrame = 0;
        currentMove.currentTick = 0;

        testCharacterRotation(100);
        if (isHero)
        {
            FightManager.FireMove(currentMove, myInfo, true);
        }
        PoolUtil.DespawnerMoveInfo(currentMove);
        this.SetMove(null);
    }

    public void ApplyStun()
    {
        stunTime -= Time.fixedDeltaTime;
        if (stunTime <= 0)
        {
            ReleaseStun();
        }
    }

    private void ReleaseStun()
    {
        if (currentSubState != SubStates.Stunned)
        {
            return;
        }
        currentHit = null;
        currentSubState = SubStates.Resting;
        stunTime = 0;

        currentState = PossibleStates.Stand;
        translateInputs();
    }

    public void GetHit(Hit hit, float damage, Player opControlsScript)
    {
        BasicMoveInfo currentHitInfo = null;
        if (visibleChecker.isVisible)
        {
            HitTypeOptions hitEffects = null;
            if (hit != null)
            {
                currentHit = hit;
                myHitBoxesScript.isHit = true;
                if(myInfo.isRigidBody == false)
                {
                    if (hit.overrideHitAnimation)
                    {
                        BasicMoveInfo basicMoveOverride = myMoveSetScript.GetBasicAnimationInfo(hit.newHitAnimation);
                        if (basicMoveOverride != null)
                        {
                            currentHitInfo = basicMoveOverride;
                            currentHitAnimation = currentHitInfo.name;
                        }
                        else
                        {
                            currentHitInfo = myMoveSetScript.basicMoves.getHitLow;
                            currentHitAnimation = currentHitInfo.name;
                        }
                    }
                    else
                    {
                        currentHitInfo = myMoveSetScript.basicMoves.getHitLow;
                        currentHitAnimation = currentHitInfo.name;
                    }
                }
                else
                {
                    currentHitInfo = null;
                }
                hitEffects = hit.hitEffects;
                if (!hit.overrideHitEffects || hitEffects.hitParticle == null)
                {
                    if (hit.hitStrength == HitStrengh.Weak)
                    {
                        hitEffects = FightManager.config.hitOptions.weakHit;
                    }
                    else if (hit.hitStrength == HitStrengh.Medium)
                    {
                        hitEffects = FightManager.config.hitOptions.mediumHit;
                    }
                    else if (hit.hitStrength == HitStrengh.Heavy)
                    {
                        hitEffects = FightManager.config.hitOptions.heavyHit;
                    }
                    else if (hit.hitStrength == HitStrengh.Custom1)
                    {
                        hitEffects = FightManager.config.hitOptions.customHit1;
                    }
                    else if (hit.hitStrength == HitStrengh.Custom2)
                    {
                        hitEffects = FightManager.config.hitOptions.customHit2;
                    }
                    else if (hit.hitStrength == HitStrengh.Custom3)
                    {
                        hitEffects = FightManager.config.hitOptions.customHit3;
                    }
                }
            }
            else
            {
                hitEffects = FightManager.config.hitOptions.weakHit;
            }
            if (hitEffects != null)
            {
                if (hitEffects.hitParticle != null)
                {
                    HitEffectSpawnPoint spawnPoint = hitEffects.spawnPoint;
                    if (hit != null && hit.overrideEffectSpawnPoint)
                    {
                        spawnPoint = hit.spawnPoint;
                    }
                    if (hitEffects.killTime <= 0)
                    {
                        Debug.LogError("hitEffects killTime <= 0...");
                    }
                    GameObject hitEffect = PoolUtil.SpawnerGameObject(hitEffects.hitParticle, hitEffects.killTime);
                    hitEffect.transform.parent = FightManager.gameEngine.transform;
                    hitEffect.transform.position = transform.position;
                    hitEffect.transform.localScale = Vector3.one;
                }
                if (isHero)
                {
                    shakeCamera = hitEffects.shakeCameraOnHit;
                    shakeCharacter = hitEffects.shakeCharacterOnHit;
                    shakeDensity = hitEffects.shakeDensity;
                }
				else if (opControlsScript != null && opControlsScript.isHero)
                {
                    opControlsScript.shakeCamera = hitEffects.shakeCameraOnHit;
                    opControlsScript.shakeCharacter = hitEffects.shakeCharacterOnHit;
                    opControlsScript.shakeDensity = hitEffects.shakeDensity;
                }
            }
        }

        if (opControlsScript != null)
        {
            isDead = DamageMe(damage, true, opControlsScript.myInfo);
        }
        else
        {
            isDead = DamageMe(damage, true, null);
        }
        if (isDead)
        {
            BasicMoveInfo basicMoveOverride = myMoveSetScript.GetBasicAnimationInfo(BasicMoveReference.Death);
            if (basicMoveOverride != null)
            {
                currentHitInfo = basicMoveOverride;
                currentHitAnimation = currentHitInfo.name;
            }
            else
            {
                currentHitAnimation = null;
            }
        }

        if (currentMove == null || !currentMove.hitAnimationOverride)
        {
            if (isDead)
            {
                stunTime = 99999;
            }
            if (stunTime > 0)
            {
                currentSubState = SubStates.Stunned;
            }

            float hitAnimationSpeed = 0;
            if (currentHitAnimation != null && (isDead || hit != null))
            {
                hitAnimationSpeed = myMoveSetScript.GetAnimationLengh(currentHitAnimation);
                if (hit != null && hit.overrideHitAnimationBlend)
                {
                    myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit.newHitBlendingIn, FightManager.config.hitOptions.resetAnimationOnHit);
                }
                else
                {
                    myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, FightManager.config.hitOptions.resetAnimationOnHit);
                }

                if (currentHitInfo.autoSpeed && hitAnimationSpeed > 0)
                {
                    myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
                }
            }
        }
        if (hit != null && isDead == false)
        {
            if (hit.pushForce.x > 0 && Physics.Freeze == false)
            {
                Physics.KnockBack(hit.pushForce.x, hit.pushForceDirection, hit.pushForceTime);
            }
            if (hit.debuffType != DebuffType.none)
            {
                DebuffManager.Instance.AddDebuff(hit.debuffType, hit.debuffParam, myInfo);
            }
        }
    }

    public bool DamageMe(float damage, bool doesntKill, CharacterInfo opInfo)
    {
        return DamageMe(damage, opInfo);
    }

    private bool DamageMe(float damage, CharacterInfo opInfo)
    {
        myInfo.currentLifePoints -= damage;
        if (myInfo.currentLifePoints < 0)
        {
            myInfo.currentLifePoints = 0;
        }
        FightManager.SetLifePoints(myInfo.currentLifePoints, myInfo);

        if (myInfo.currentLifePoints == 0)
        {
			playerCollider.enabled = false;
            BattleInputController.Instance.isTouchJoystick = false;
            KillCurrentMove();
            storedMove = null;
            myPhysicsScript.Freeze = true;
            myPhysicsScript.StopMove();
            IsContinueNormalAttack = false;
            IsStun = false;
            IsFast = false;
            isStopMove = true;
            if (opInfo != null)
            {
                myInfo.killerName = opInfo.characterName;
            }
            else
            {
                myInfo.killerName = "";
            }
            isFireHeroDie = true;
            myInfo.isDead = true;
            SetTimeout.Start(FireHeroDie, 1f);
            SetTimeout.Start(AutoRelife, 8f);
            for (int i = 0; i < myInfo.debuffList.Count; i++)
            {
                Debuff debuff = myInfo.debuffList[i];
                GameObject.Destroy(debuff.view);
            }
            myInfo.debuffList.Clear();
            ClearBuff();
			if (isAIController) {
				myPhysicsScript.roleAIPath.canSearch = false;
				myPhysicsScript.roleAIPath.canMove = false;
				SceneAssetsManager.Instance.GeneratorSceneItems (myTransform.position);
			}
			FightManager.FireHeroDie (this);
            return true;
        }
        return false;
    }

    public void RemoveAutoRelife()
    {
        SetTimeout.Clear(AutoRelife);
    }

    private void AutoRelife()
    {
        /*MsgRelife msgRelife = new MsgRelife();
        msgRelife.Id = LoginManager.playerId;
        SocketManager.Send(MsgTypeCmd.ReLife, msgRelife);*/
    }

    public void Respawn(float x, float z)
    {
        ResetData(true);
        Body.LevelUp(Vector3.one);
        transform.position = new Vector3(x, 0.001f, z);
        myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
        isFireHeroDie = false;
    }

    private void FireHeroDie()
    {
        SceneAssetsManager.Instance.TryGeneratorNewWaveEnemys();
        if (isHero)
        {
            if (FightManager.battleGUI != null)
            {
				(FightManager.battleGUI as DefaultBattleScreen).ShowHeroRespawn();
            }
        }
        else
        {
            FightManager.RemoveNetPlayer(id);
        }
    }

    public void DestroyMeByOffline()
    {
        FightManager.controlsScriptList.Remove(this);
        DestroyThis();
        gameObject.SetActive(false);
        GameObject.Destroy(gameObject, 5f);
    }

    public void DestroyThis()
    {
        if (bloodBar != null)
        {
            bloodBar.DespawnerBloodBar();
        }
        DespawnerCharacter();
        DespawnerShader();
    }

    private void DespawnerCharacter()
    {
        for (int i = 0; i < particleEffectsList.Count; i++)
        {
            GameObject particleEffectObject = particleEffectsList[i];
            PoolUtil.Despawner(particleEffectObject, PoolUtil.particlesPoolName, true);
        }
        particleEffectsList.Clear();
        if (character != null)
        {
            rotBeforeTurn = character.transform.rotation;
            character.transform.SetParent(null);
            PoolUtil.Despawner(character);
            character = null;
        }
    }

    private void DespawnerShader()
    {
        if (shadow != null)
        {
            PoolUtil.Despawner(shadow);
            shadow = null;
        }
    }

    public void EndFight()
    {
        FightManager.config.lockMovements = true;
        FightManager.config.lockInputs = true;

        KillCurrentMove();

        foreach (ProjectileMoveScript projectileMoveScript in projectiles)
        {
            if (projectileMoveScript != null)
            {
                projectileMoveScript.DestroyGameObjectByFightEnd();
            }
        }
        projectiles.Clear();
        FightManager.FireRoundEnds(null, myInfo);
        FightManager.DelaySynchronizedAction(this.EndGame, FightManager.config.roundOptions.endGameDelay);
    }

    private void EndGame()
    {
        FightManager.FireGameEnds(null, myInfo);
    }

    public void ResetData(bool resetLife)
    {
        if (resetLife && myInfo != null)
        {
            if (FightManager.playerModelMap.ContainsKey(id))
            {
                myInfo.currentLifePoints = FightManager.playerModelMap[id].LifeNum;
            }
        }
        if (myPhysicsScript != null)
        {
            if (isDead)
            {
                myPhysicsScript.Freeze = false;
                isDead = false;
            }
        }
        currentState = PossibleStates.Stand;
        currentSubState = SubStates.Resting;
        stunTime = 0;
    }

    private void shake()
    {
        //float rnd = UnityEngine.Random.Range(-.1f * shakeDensity, .2f * shakeDensity);
        //character.transform.localPosition = new Vector3(rnd, 0, rnd);
    }

    public ProjectileMoveScript GetProjectile(int id, int skillId)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            ProjectileMoveScript projectileMoveScript = projectiles[i];
            if (projectileMoveScript.id == id &&
               projectileMoveScript.Data.moveName == skillId.ToString())
            {
                return projectileMoveScript;
            }
        }
        return null;
    }

    public MoveCastableTimeInfo GetMoveCastableTimeInfo(string moveName)
    {
        if (moveCdTimeInfoMap.ContainsKey(moveName))
        {
            return moveCdTimeInfoMap[moveName];
        }
        return null;
    }

    public void ClearBuff()
    {
        Physics.ClearBuff();
    }

    private void CheckMoveCastableTime()
    {
        if (LoginManager.playerId == id)
        {
            foreach (MoveCastableTimeInfo moveCastableTimeInfo in moveCdTimeInfoMap.Values)
            {
                if (moveCastableTimeInfo.currentCdTime > 0)
                {
                    moveCastableTimeInfo.currentCdTime -= Time.fixedDeltaTime;
                }
                if (moveCastableTimeInfo.currentCdTime < 0)
                {
                    moveCastableTimeInfo.currentCdTime = 0;
                }
            }
        }
    }
}
