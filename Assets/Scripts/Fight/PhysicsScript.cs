using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;

/// <summary>
/// 控制移动等常规行为
/// nashnie
/// </summary>
public class PhysicsScript : MonoBehaviour
{
    public LayerMask groundLayerMask;
    public int groundLayer = 8;

    private bool freeze;

    private float moveDirection = 0;
    private Vector2 moveDirectionOfVector2 = Vector2.one;
    private Vector3 moveDirectionOfVector3 = Vector3.one;

    private float horizontalForce = 0;

    private Player myControlsScript;
    private MoveSetScript myMoveSetScript;

    private bool isWalkBack = false;
    private bool isWalkLeft = false;
    private bool isWalkRight = false;

    private const float angleOf90 = 135;
    private const float angleOf45 = 45;

    private List<GameObject> effectList;
    private CharacterController characterController;
    private Vector3 hitKnockBackTargetPos = Vector3.zero;
	public AIPathAgent roleAIPath;

	private Transform enemyTarget;
	private Player enemyTargetPlayer;

	private Transform followTarget;
	private Player followTargetPlayer;

	private Transform target;

    private Player hero;
    private MoveInfo moveInfo;

    void Awake()
    {
        effectList = new List<GameObject>();
        myControlsScript = gameObject.GetComponent<Player>();
        groundLayerMask = 1 << groundLayer;
    }

    void Start()
    {
        if (myControlsScript.isAIController &&
            myControlsScript.ufeMapUnit == UFEMapUnit.monster)
        {
            hero = FightManager.hero;
            moveInfo = myControlsScript.MoveSet.GetMove(ButtonPress.Button1);
			if (myControlsScript.camp == UFECamp.Camp1) {
				FightManager.OnHeroDieHandler += onAITargetDie;
				FightManager.OnEnemyAddHandler += onEnemyAdd;
				followTargetPlayer = FightManager.hero;
				followTarget = FightManager.hero.transform;
				enemyTargetPlayer = SceneAssetsManager.Instance.GetNearestEnemy ();
				if (enemyTargetPlayer != null) {
					enemyTarget = enemyTargetPlayer.transform;
				}
			} else {
				enemyTargetPlayer = FightManager.hero;
				enemyTarget = FightManager.hero.transform;
			}
        }
        if (myControlsScript.isAIController)
        {
            SetAIFollowDistance();
            SetAIPathTarget();
            FightManager.FireAddEnemy(gameObject);
        }
        else
        {
            SetAIPathTarget();
            FightManager.FireAddHero(gameObject);
        }
    }

	private void onAITargetDie(Player target)
	{
		if (this.enemyTargetPlayer == target) {
			Player targetPlayer = SceneAssetsManager.Instance.GetNearestEnemy ();
			if (targetPlayer != null) {
				this.enemyTargetPlayer = targetPlayer;
				this.enemyTarget = targetPlayer.transform;
				SetAIFollowDistance();
				SetAIPathTarget();
			}
		}
	}

	private void onEnemyAdd(GameObject target)
	{
		if (this.enemyTargetPlayer == null) {
			Player targetPlayer = SceneAssetsManager.Instance.GetNearestEnemy ();
			if (targetPlayer != null) {
				this.enemyTargetPlayer = targetPlayer;
				this.enemyTarget = targetPlayer.transform;
				SetAIFollowDistance();
				SetAIPathTarget();
			}
		}
	}

    public void SetAIPathTarget()
    {
        if (roleAIPath == null)
        {
            roleAIPath = gameObject.AddComponent<AIPathAgent>();
        }

		/*if (enemyTargetPlayer != null) 
		{
			roleAIPath.target = enemyTarget;
		}
		else 
		{
			roleAIPath.target = followTarget;
		}
		target = roleAIPath.target;*/
    }

    public void SetAIFollowDistance()
    {
        if (roleAIPath == null)
        {
			gameObject.AddComponent<SimpleSmoothModifier>();
            roleAIPath = gameObject.AddComponent<AIPathAgent>();
            roleAIPath.enabled = false;

            /*roleAIPath.pickNextWaypointDist = 0.8f;
			roleAIPath.speed = 5;
			roleAIPath.turningSpeed = 2f;
            float proximityRangeBegins = myControlsScript.MoveSet.GetMoveProximityRangeBegins(moveInfo);
            float proximityRangeEnds = myControlsScript.MoveSet.GetMoveProximityRangeEnds(moveInfo);
            roleAIPath.endReachedDistance = UnityEngine.Random.Range(proximityRangeBegins, proximityRangeEnds);
            roleAIPath.repathRate = UnityEngine.Random.Range(0.5f, 1f);*/
        }
    }

    public void StopMove()
    {
        moveDirection = 0;
        MoveDirectionOf = Vector2.zero;
        moveDirectionOfVector3 = Vector3.zero;

        MyControlsScript.currentSubState = SubStates.MovingForward;
        horizontalForce = 0;
        SetWalkDirection();
    }

    public void MoveByDirection(bool isStop, Vector2 direction)
    {
        moveDirection = isStop ? 0 : 1;
        MoveDirectionOf = direction;
        moveDirectionOfVector3.x = direction.x;
        moveDirectionOfVector3.z = direction.y;

        if (MyControlsScript != null && MyControlsScript.myInfo != null)
        {
            MyControlsScript.currentSubState = SubStates.MovingForward;
            float moveForwardSpeed = isStop ? 0 : MyControlsScript.myInfo.speed;
            horizontalForce = moveForwardSpeed;
            SetWalkDirection();
        }
    }

    public Vector2 MoveDirectionOf
    {
        get
        {
            return moveDirectionOfVector2;
        }
        set
        {
            moveDirectionOfVector2 = value;
        }
    }

    public Player MyControlsScript
    {
        get
        {
            return myControlsScript;
        }
    }

    public bool Freeze
    {
        get
        {
            return freeze && MyControlsScript.myInfo.freezeDebuff;
        }
        set
        {
            freeze = value;
        }
    }

    public bool IsMoving()
    {
        return moveDirection > 0;
    }

    public void ForceGrounded()
    {
        horizontalForce = 0;
        if (transform.position.y != 0)
        {
            transform.Translate(new Vector3(0, -transform.position.y, 0));
        }
        MyControlsScript.currentState = PossibleStates.Stand;
    }

    public void ApplyForces(MoveInfo move)
    {
        if (Freeze == false && myControlsScript.myInfo.freezeDebuff == false)
        {
            if (horizontalForce != 0)
            {
                float speed = DebuffManager.Instance.CalculateSpeed(MyControlsScript.myInfo);
                Vector3 motion = speed * moveDirectionOfVector3 * Time.fixedDeltaTime;
                motion.y = -transform.position.y;
                myControlsScript.characterController.Move(motion);
            }

			if (move == null &&
	            MyControlsScript.currentSubState != SubStates.Stunned &&
	            MyControlsScript.currentState == PossibleStates.Stand) 
			{
				PlayMoveAnimation ();
			} 
            if (horizontalForce == 0)
            {
                moveDirection = 0;
            }

            for (int i = 0; i < effectList.Count; i++)
            {
                GameObject commonEffect = effectList[i];
                if (commonEffect != null && commonEffect.activeSelf == false)
                {
                    effectList.Remove(commonEffect);
                    break;
                }
            }

            if (myControlsScript.isAIController)
            {
                if (roleAIPath.TargetReached)
                {
					if (roleAIPath.target != null) {
						myControlsScript.netRotAngleX = roleAIPath.target.position.x - transform.position.x;
						myControlsScript.netRotAngleZ = roleAIPath.target.position.z - transform.position.z;
					}
                    //attack
					if (myControlsScript.currentMove == null && 
						object.ReferenceEquals(enemyTarget, roleAIPath.target))
                    {
                        SetNextMove();
                        myControlsScript.CastMove(moveInfo, true);
                        myControlsScript.IsContinueNormalAttack = true;
                    }
					moveDirection = 0;
                }
				else if(roleAIPath.target == null)
				{
					myControlsScript.IsContinueNormalAttack = false;
					moveDirection = 0;
					myControlsScript.netRotAngleX = 0;
					myControlsScript.netRotAngleZ = 0;
				}
                else
                {
                    myControlsScript.IsContinueNormalAttack = false;
					moveDirection = 1;
					myControlsScript.netRotAngleX = 0;
					myControlsScript.netRotAngleZ = 0;
                }
            }
        }
        if (myControlsScript != null && roleAIPath != null)
        {
            myControlsScript.normalizedDistance = roleAIPath.targetDistance;
        }
    }

    private void SetNextMove()
    {
        int length = myControlsScript.MoveSet.attackMoves.Length;
        if (length > 1)
        {
            int index = UnityEngine.Random.Range(0, length - 1);
            moveInfo = myControlsScript.MoveSet.attackMoves[index];
            if (roleAIPath != null)
            {
                float proximityRangeBegins = myControlsScript.MoveSet.GetMoveProximityRangeBegins(moveInfo);
                float proximityRangeEnds = myControlsScript.MoveSet.GetMoveProximityRangeEnds(moveInfo);
                //roleAIPath.endReachedDistance = UnityEngine.Random.Range(proximityRangeBegins, proximityRangeEnds);
            }
        }
    }

    private void PlayMoveAnimation()
    {
        myMoveSetScript = MyControlsScript.MoveSet;
        if (MyControlsScript.visibleChecker.isVisible)
        {
			if (moveDirection > 0 || 
				(myControlsScript.isAIController && moveDirection > 0))
			{
				if (MyControlsScript.IsFast)
				{
					if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveForwardOfFly.name))
					{
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveForwardOfFly);
					}
				}
				else
				{
					if (isWalkBack)
					{
						if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveBack.name))
						{
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveBack);
						}
					}
					else if (isWalkRight)
					{
						if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveRight.name))
						{
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveRight);
						}
					}
					else if (isWalkLeft)
					{
						if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveLeft.name))
						{
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveLeft);
						}
					}
					else
					{
						if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveForward.name))
						{
							myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveForward);
						}
					}
				}
			}
			else
			{
				if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.idle.name))
				{
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
				}
			}
        }
    }

    public void PlayEffect(int id, float delay, float offset = 0, float scale = 1.0f, bool isScaleParticleSystem = true)
    {
        MainEntry.Instance.StartLoad(id.ToString(), AssetType.prefab, (GameObject commonEffect, string tag) =>
        {
            if (MyControlsScript != null)
            {
                commonEffect.transform.parent = MyControlsScript.transform;
                commonEffect.transform.localScale = Vector3.one;
                commonEffect.name = id.ToString();
                if (offset > 0)
                {
                    commonEffect.transform.localPosition = Vector3.zero + new Vector3(0, offset, 0);
                }
                else
                {
                    commonEffect.transform.localPosition = Vector3.zero;
                }
                FightManager.DelaySynchronizedAction(() =>
                {
                    PoolUtil.Despawner(commonEffect);
                }, delay);
            }
            else
            {
                GameObject.Destroy(commonEffect);
            }
        });
    }

    public void PlayEffect(GameObject effect, float delay, float offset = 0, float scale = 1.0f, bool isScaleParticleSystem = true, bool isForcePlay = false)
    {
        if (MyControlsScript.visibleChecker.isVisible || delay >= 5f || isForcePlay)
        {
            PlayEffect(MyControlsScript.gameObject, effect, delay, offset, scale, isScaleParticleSystem);
        }
    }

    public void RemoveEffect(GameObject effect)
    {
        if (effectList != null)
        {
            for (int i = 0; i < effectList.Count; i++)
            {
                GameObject commonEffect = effectList[i];
                if (commonEffect != null && commonEffect.name == effect.name)
                {
                    effectList.Remove(commonEffect);
                    PoolUtil.Despawner(commonEffect);
                    break;
                }
            }
        }
    }

    public void PlayEffect(GameObject parent, GameObject effect, float delay, float offset = 0, float scale = 1.0f, bool isScaleParticleSystem = true)
    {
        if (effect != null)
        {
            GameObject commonEffect = PoolUtil.SpawnerGameObject(effect, delay);
            commonEffect.transform.parent = parent.transform;
            commonEffect.transform.localScale = Vector3.one;
            commonEffect.name = effect.name;
            if (isScaleParticleSystem)
            {
                GeneralUtils.ScaleParticleSystem(effect, commonEffect, scale);
            }
            else
            {
                commonEffect.transform.localScale = new Vector3(scale, scale, scale);
            }
            if (offset > 0)
            {
                commonEffect.transform.localPosition = Vector3.zero + new Vector3(0, offset, 0);
            }
            else
            {
                commonEffect.transform.localPosition = Vector3.zero;
            }
            effectList.Add(commonEffect);
        }
    }


    public void SetWalkDirection()
    {
        if (MyControlsScript.visibleChecker.isVisible)
        {
            float angle = Vector2.Dot(MoveDirectionOf, MyControlsScript.rotDirection);
            angle = Mathf.Acos(angle) * Mathf.Rad2Deg;
            isWalkBack = angle >= angleOf90;
            isWalkLeft = angle > angleOf45 && angle < angleOf90 && MoveDirectionOf.x >= MyControlsScript.rotDirection.x;
            isWalkRight = angle > angleOf45 && angle < angleOf90 && MoveDirectionOf.x < MyControlsScript.rotDirection.y;
        }
    }

    public void KnockBack(float force, Vector3 direction, float tweenTime)
    {
        float knockBackContinueTime = tweenTime > 0 ? tweenTime : 0.2f;
        RaycastHit hit;
        if (UnityEngine.Physics.Raycast(transform.position, direction, out hit, force, groundLayerMask))
        {
            hitKnockBackTargetPos = hit.point;
        }
        else
        {
            Vector3 forceVec = force * direction;
            //TODO 击飞 addedForce.force.y
            hitKnockBackTargetPos.x = transform.position.x + forceVec.x;
            hitKnockBackTargetPos.y = transform.position.y;
            hitKnockBackTargetPos.z = transform.position.z + forceVec.z;
        }

        //TODO play HitKnockBack
        /*if (myControlsScript.isDead == false)
        {
            BasicMoveInfo basicMoveOverride = myMoveSetScript.GetBasicAnimationInfo(BasicMoveReference. );
            myMoveSetScript.PlayBasicMove(basicMoveOverride, basicMoveOverride.name, 0.1f, true);
            float animSpeed = basicMoveOverride.clip1.length * 0.5f / (knockBackContinueTime / 2);
            HitPause(animSpeed);
            FightManager.DelaySynchronizedAction(PausePlayAnimation, knockBackContinueTime / 2);
        }*/
		
        Hashtable table = iTween.Hash("position", hitKnockBackTargetPos, "time", knockBackContinueTime, "easetype", iTween.EaseType.easeInSine);
        iTween.MoveTo(gameObject, table);
        FightManager.DelaySynchronizedAction(onKnockBackComplete, knockBackContinueTime);

        myControlsScript.characterController.detectCollisions = false;
        SetTimeout.Start(ClearKnockBack, knockBackContinueTime);
        Freeze = true;
    }

    private void ClearKnockBack()
    {
        iTween.Stop(gameObject);
        SetTimeout.Clear(ClearKnockBack);
        FightManager.RemoveDelaySynchronizedAction(onKnockBackComplete);
        HitUnpause();
        myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
        transform.position = hitKnockBackTargetPos;
        myControlsScript.characterController.detectCollisions = true;
        Freeze = false;
    }

    private void onKnockBackComplete()
    {
        PausePlayAnimation(true);
    }

    void HitPause(float animSpeed)
    {
        if (myControlsScript.shakeCamera)
        {
            Camera.main.transform.position += Vector3.forward / 2;
        }
        myControlsScript.Physics.Freeze = true;
        PausePlayAnimation(true, animSpeed);
    }

    void HitUnpause()
    {
        myControlsScript.Physics.Freeze = false;
        PausePlayAnimation(false);
    }

    private void PausePlayAnimation()
    {
        PausePlayAnimation(true, 0f);
    }

    private void PausePlayAnimation(bool pause)
    {
        PausePlayAnimation(pause, 0);
    }

    private void PausePlayAnimation(bool pause, float animSpeed)
    {
        if (animSpeed < 0)
        {
            animSpeed = 0;
        }
        if (myMoveSetScript != null)
        {
            if (pause)
            {
                myMoveSetScript.SetAnimationSpeed(animSpeed);
            }
            else
            {
                myMoveSetScript.RestoreAnimationSpeed();
            }
        }
    }

    public void ClearBuff()
    {
        FightManager.RemoveDelaySynchronizedAction(PausePlayAnimation);
        FightManager.RemoveDelaySynchronizedAction(onKnockBackComplete);
        HitUnpause();
    }

    void OnDestroy()
    {
		FightManager.OnHeroDieHandler -= onAITargetDie;
		FightManager.OnEnemyAddHandler -= onEnemyAdd;
        FightManager.RemoveDelaySynchronizedAction(onKnockBackComplete);
        SetTimeout.Clear(ClearKnockBack);
    }

	public void UpdateWeapon(WeaponType weaponType, int weaponIndex = -1, BodyPart body = BodyPart.rightHand)
	{
		#if UNITY_EDITOR
		Debug.Log ("UpdateWeapon weaponType " + weaponType  + " weaponIndex " + weaponIndex);
		#endif
		if (weaponIndex >= 0) {
			GameObject weaponPrefab = GetWeaponGameObject (weaponType, weaponIndex);
			GameObject weapon = GameObject.Instantiate(weaponPrefab);
			if (weapon != null) {
				Transform oldWeapon = myControlsScript.myHitBoxesScript.GetTransform (body);
				Transform weaponParent= oldWeapon.parent;
				weapon.transform.parent = weaponParent;
				weapon.transform.localPosition = oldWeapon.localPosition;
				weapon.transform.localScale = oldWeapon.localScale;
				weapon.transform.localRotation = oldWeapon.localRotation;
				GameObject.DestroyObject(oldWeapon.gameObject);
				myControlsScript.myHitBoxesScript.UpdateHitBoxes (body, weapon.transform);
			} else {
				Debug.LogError ("can not find weapon weaponType " + weaponType + " weaponIndex " + weaponIndex);
			}
		}
		myControlsScript.myInfo.weaponType = weaponType;
		myControlsScript.myInfo.weaponIndex = weaponIndex;
		myControlsScript.UpdateWeaponMeleeMoveScript (body);
		myControlsScript.UpdateWeaponMoveSkill ();
	}

	private GameObject GetWeaponGameObject(WeaponType weaponType, int weaponIndex = -1)
	{
		switch (weaponType) {
		case WeaponType.Melee:
			return FightManager.config.meleeWeapons[weaponIndex];
		case WeaponType.Range:
			return FightManager.config.bows[weaponIndex];
		default:
			return null;
		}
	}
}
