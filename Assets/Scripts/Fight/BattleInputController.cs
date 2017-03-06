using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// nashnie
/// </summary>
public class BattleInputController : SingletonInstance<BattleInputController>
{
    public Dictionary<string, MoveInfo> moveInfoMap = new Dictionary<string, MoveInfo>();

	public bool isStartSetRightAxis = false;
    public float horizontalAxisRight = 0f;
    public float verticalAxisRight = 0f;
    public Vector2 axisRight = Vector2.zero;
    private bool isCastedMove = false;
    private bool isClearNormalAttack = false;

    public float horizontalLeft = 0;
    public float verticalLeft = 0;
    private float updateHeroMoveRotTimeOffset = 0f;

    private uint waitCastSkillId = 0;
    public bool isTouchJoystick = false;
    public bool Init = false;
    private MoveInfo currentMoveInfo;
    private Player attacker;
    public float lastNetHorizontalAxisRight;
    public float lastNetVerticalRight;

    private void onNormalAttack(GameObject go)
    {
        onNormalAttackAndRot();
    }

    private void onNormalAttackAndRot()
    {
        MainEntry.RunInNextFrame(NormalAttackAndRot);
    }

    private void NormalAttackAndRot()
    {
        if (attacker != null && attacker.isDead == false)
        {
            if (attacker.currentMove == null)
            {
                MoveInfo moveInfo = null;
                if (attacker.myInfo.skillId.Value > 0)
                {
                    string skillId = attacker.myInfo.skillId.Value.ToString();
                    moveInfo = attacker.MoveSet.GetMove(skillId);
                    if (moveInfo == null)
                    {
                        moveInfo = attacker.MoveSet.GetMove(ButtonPress.Button1);
                    }
                }
                else
                {
                    moveInfo = attacker.MoveSet.GetMove(ButtonPress.Button1);
                }
                if(moveInfo != null)
                {
                    isCastedMove = false;
                    isClearNormalAttack = false;
                    DelayRemoveButtonPress();

                    uint skillId = 0;
                    uint.TryParse(moveInfo.moveName, out skillId);
                    waitCastSkillId = skillId;
                    updateHeroMoveRotTimeOffset = 0.1f;
                    FightManager.DelaySynchronizedAction(WaitCastSkill, 0.1f);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log("Try NormalAttackAndRot fail...move is null");
#endif
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Try NormalAttackAndRot fail..." + FightManager.GetHero().currentMove.moveName);
#endif  
                isTouchJoystick = true;
            }
        }
    }

    private void WaitCastSkill()
    {
        FightManager.RemoveDelaySynchronizedAction(WaitCastSkill);
        if (waitCastSkillId > 0)
        {
            MoveInfo moveInfo = GetMoveInfo(waitCastSkillId.ToString());
            attacker.CastMove(moveInfo, true);
            attacker.IsContinueNormalAttack = true;
            updateHeroMoveRotTimeOffset = 0.15f;
            waitCastSkillId = 0;
        }
    }

    private void DelayRemoveButtonPress()
    {
        isCastedMove = true;
        ClearNormalAttack();
    }

    private void onClearNormalAttack()
    {
        isClearNormalAttack = true;
        isStartSetRightAxis = false;
        isTouchJoystick = false;

        FightManager.RemoveDelaySynchronizedAction(WaitCastSkill);
        if (waitCastSkillId > 0)
        {
            MoveInfo moveInfo = GetMoveInfo(waitCastSkillId.ToString());
            attacker.CastMove(moveInfo, true);
            waitCastSkillId = 0;
        }
        else
        {
            ClearNormalAttack();
        }
    }

    public MoveInfo GetMoveInfo(string skillId)
    {
        if(moveInfoMap.ContainsKey(skillId))
        {
            return moveInfoMap[skillId];
        }
        else
        {
            if (FightManager.GetHero().MoveSet != null)
            {
                MoveInfo moveInfo = attacker.MoveSet.GetMove(skillId);
                moveInfoMap[skillId] = moveInfo;
                return moveInfo;
            }
            return null;
        }
    }

    private void ClearNormalAttack()
    {
        if(isClearNormalAttack && isCastedMove)
        {
            attacker.IsContinueNormalAttack = false;
        }
    }

    public void Initialize()
    {
		HUDManager.Instance.PLAYERCONTROLLER.Right.ChangeMode(HRYJoyStick.Mode.Attack1);
		HUDManager.Instance.PLAYERCONTROLLER.Right.FirstMode(onNormalAttackAndRot, onClearNormalAttack, null, onMoveStart);
        FightManager.OnHeroAddHandler += FightManager_OnHeroAddHandler;
    }

    private void FightManager_OnHeroAddHandler(GameObject go)
    {
        attacker = FightManager.hero;
        attacker.myInfo.skillId.OnValueChange += SkillId_OnValueChange;
        attacker.myInfo.skillCd.OnValueChange += SkillCd_OnValueChange;
        FightManager.OnMove += FightManager_OnMove;
        FightManager.OnMoveEnd += FightManager_OnMoveEnd;
        SkillId_OnValueChange();
        SkillCd_OnValueChange();
    }

    private void FightManager_OnMoveEnd(MoveInfo move, CharacterInfo player)
    {
    }

    private void FightManager_OnMove(MoveInfo move, CharacterInfo player)
    {
    }

    private void SkillCd_OnValueChange()
    { 
    }

    private void SkillId_OnValueChange()
    {
    }

    private bool CheckMoveProjectileIsParabola(MoveInfo moveInfo)
    {
        if (moveInfo.projectiles.Length > 0)
        {
            for (int i = 0; i < moveInfo.projectiles.Length; i++)
            {
                Projectile projectile = moveInfo.projectiles[i];
                if (projectile.movementType == ProjectileMovementUtil.MovementType.parabola)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void onMoveStart()
    {
        isStartSetRightAxis = true;
    }

    public void DoUpdate()
    {
        if (attacker == null)
        {
            attacker = FightManager.hero;
        }
        if (FightManager.gameRunning && HUDManager.Instance.PLAYERCONTROLLER != null)
        {
			horizontalLeft = HUDManager.Instance.PLAYERCONTROLLER.Left.HORIZENTAL;
			verticalLeft = HUDManager.Instance.PLAYERCONTROLLER.Left.VERTICAL;

            if (!isStartSetRightAxis)
            {
                if (!HUDManager.Instance.PLAYERCONTROLLER.Left.Delay)
                {
                    horizontalAxisRight = horizontalLeft;
                    verticalAxisRight = verticalLeft;
                }
            }

            if (isStartSetRightAxis)
            {
                horizontalAxisRight = HUDManager.Instance.PLAYERCONTROLLER.Right.HORIZENTAL;
                verticalAxisRight = HUDManager.Instance.PLAYERCONTROLLER.Right.VERTICAL;
            }
        }
    }

    public void Clear()
    {
        currentMoveInfo = null;
        FightManager.RemoveDelaySynchronizedAction(DelayRemoveButtonPress);

        if(moveInfoMap != null)
        {
            moveInfoMap.Clear();
        }
        isTouchJoystick = false;

        if (HUDManager.Instance != null && 
            HUDManager.Instance.PLAYERCONTROLLER != null && 
            HUDManager.Instance.PLAYERCONTROLLER.Right != null)
        {
			HUDManager.Instance.PLAYERCONTROLLER.Left.ClearMain();
			HUDManager.Instance.PLAYERCONTROLLER.Right.ClearMain();
        }
    }

    public void NormalizeAxisRight()
    {
        if (FightManager.GetHero() != null && FightManager.GetHero().character != null)
        {
            GameObject go = FightManager.GetHero().character;
            float rotY = go.transform.eulerAngles.y * Mathf.Deg2Rad;
            axisRight.x = Mathf.Sin(rotY);
            axisRight.y = Mathf.Cos(rotY);
            axisRight.Normalize();
        }
        else
        {
            axisRight.x = horizontalAxisRight;
            axisRight.y = verticalAxisRight;
            axisRight.Normalize();
        }

        axisRight.x = (100 * axisRight.x);
        axisRight.y = (100 * axisRight.y);
    }
}
