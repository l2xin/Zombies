using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// nashnie 动作初始化
/// </summary>
public class ButtonSequenceRecord
{
    public ButtonPress buttonPress;
    public float chargeTime;

    public ButtonSequenceRecord(ButtonPress buttonPress, float chargeTime)
    {
        this.buttonPress = buttonPress;
        this.chargeTime = chargeTime;
    }
}

public class MoveSetScript : MonoBehaviour
{
    public BasicMoves basicMoves;
    public MoveInfo[] attackMoves;
    public MoveInfo[] moves;
    [HideInInspector]
    public int totalAirMoves;
    [HideInInspector]
    public bool animationPaused;
    [HideInInspector]
    public float overrideNextBlendingValue = -1;
    [HideInInspector]
    public Dictionary<ButtonPress, float> chargeValues = new Dictionary<ButtonPress, float>();

#if !UFE_BASIC
    public MecanimControl MecanimControl { get { return this.mecanimControl; } }
#endif

    public LegacyControl LegacyControl { get { return this.legacyControl; } }

    private Player controlsScript;
    private HitBoxesScript hitBoxesScript;
    private List<ButtonSequenceRecord> lastButtonPresses = new List<ButtonSequenceRecord>();
    private List<BasicMoveInfo> basicMoveList = new List<BasicMoveInfo>();
    private float lastTimePress;
#if !UFE_BASIC
    private MecanimControl mecanimControl;
#endif

    private LegacyControl legacyControl;
    private bool precisionControl;
    private Array buttonPressArr;
    private BasicMoveInfo currentBasicMove;

    public void ChangeMoveStances(Player controlsScript)
    {
        this.controlsScript = controlsScript;
        buttonPressArr = Enum.GetValues(typeof(ButtonPress));
        hitBoxesScript = GetComponent<HitBoxesScript>();

        foreach (ButtonPress bp in buttonPressArr)
        {
            chargeValues.Add(bp, 0);
        }
        controlsScript.myInfo.currentCombatStance = CombatStances.Stance10;
        ChangeMoveStances(CombatStances.Stance1);
    }

    public void Respawn(Player controlsScript)
    {
        this.controlsScript = controlsScript;
    }

#if !UFE_BASIC
    void Start()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Mecanim)
        {
            mecanimControl.SetMirror(false);
        }
    }
#endif

    public void ChangeMoveStances(CombatStances newStance)
    {
        if (controlsScript.myInfo.currentCombatStance == newStance) return;
        foreach (MoveSetData moveSetData in controlsScript.myInfo.moves)
        {
            if (moveSetData.combatStance == newStance)
            {
                basicMoves = moveSetData.basicMoves;
                List<MoveInfo> attackMovesList = new List<MoveInfo>(moveSetData.attackMoves);
                for (int i = 0; i < attackMovesList.Count; i++)
                {
                    MoveInfo moveInfo = attackMovesList[i];
                    if (moveInfo == null)
                    {
                        attackMovesList.Remove(moveInfo);
                        i--;
                    }
                }
                attackMoves = attackMovesList.ToArray();
                moves = attackMoves;

                fillMoves();

                controlsScript.myInfo.currentCombatStance = newStance;

                System.Array.Sort(moves, delegate (MoveInfo move1, MoveInfo move2)
                {
                    return move1.previousMoves.Length.CompareTo(move2.previousMoves.Length);
                });

                System.Array.Reverse(moves);

                return;
            }
        }
    }

    private void fillMoves()
    {
        DestroyImmediate(gameObject.GetComponent("MecanimControl"));

        mecanimControl = gameObject.AddComponent<MecanimControl>();
        mecanimControl.isBodyMask = controlsScript.myInfo.isBodyMask;
        mecanimControl.defaultTransitionDuration = controlsScript.myInfo.blendingTime;
        mecanimControl.SetDefaultClip(basicMoves.idle.clip1, "default", basicMoves.idle.animationSpeed, WrapMode.Loop, false);

        mecanimControl.defaultWrapMode = WrapMode.Once;
        //mecanimControl.overrideAnimatorUpdate = true;

        foreach (MoveInfo move in moves)
        {
            if (move == null)
            {
                Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                continue;
            }
            if (move.animationClip != null)
            {
                attachAnimation(move.animationClip, move.name, move.animationSpeed, move.wrapMode);
            }
        }

        setBasicMoveAnimation(basicMoves.idle, "idle", BasicMoveReference.Idle, UFEBasicMove.idle);
        setBasicMoveAnimation(basicMoves.moveForward, "moveForward", BasicMoveReference.MoveForward, UFEBasicMove.walkFont);
        setBasicMoveAnimation(basicMoves.moveForwardOfFly, "moveForwardOfFly", BasicMoveReference.MoveForwardOfFly, UFEBasicMove.runFont);
        setBasicMoveAnimation(basicMoves.death, "death", BasicMoveReference.Death, UFEBasicMove.die);
        setBasicMoveAnimation(basicMoves.moveBack, "moveBack", BasicMoveReference.MoveBack, UFEBasicMove.walkBack);
        setBasicMoveAnimation(basicMoves.moveLeft, "moveLeft", BasicMoveReference.MoveLeft, UFEBasicMove.walkLeft);
        setBasicMoveAnimation(basicMoves.moveRight, "moveRight", BasicMoveReference.MoveRight, UFEBasicMove.walkRight);
        setBasicMoveAnimation(basicMoves.getHitLow, "hit", BasicMoveReference.HitKnockBack, UFEBasicMove.hit);
    }

    private void setBasicMoveAnimation(BasicMoveInfo basicMove, string animName, BasicMoveReference basicMoveReference, UFEBasicMove UFEBasicMove = UFEBasicMove.none)
    {
        if (basicMove.clip1 == null)
        {
            return;
        }
        basicMove.name = animName;
        basicMove.reference = basicMoveReference;

        basicMoveList.Add(basicMove);

        attachAnimation(basicMove.clip1, animName, basicMove.animationSpeed, basicMove.wrapMode, UFEBasicMove);
        WrapMode newWrapMode = animName == "idle" ? WrapMode.Once : basicMove.wrapMode;
        if (basicMove.clip2 != null) attachAnimation(basicMove.clip2, animName + "_2", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip3 != null) attachAnimation(basicMove.clip3, animName + "_3", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip4 != null) attachAnimation(basicMove.clip4, animName + "_4", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip5 != null) attachAnimation(basicMove.clip5, animName + "_5", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip6 != null) attachAnimation(basicMove.clip6, animName + "_6", basicMove.animationSpeed, newWrapMode);
    }

    public void attachAnimation(AnimationClip clip, string animName, float speed, WrapMode wrapMode, UFEBasicMove basicMove = UFEBasicMove.none)
    {
        attachAnimation(clip, animName, speed, wrapMode, clip.length, basicMove);
    }

    private void attachAnimation(AnimationClip clip, string animName, float speed, WrapMode wrapMode, float length, UFEBasicMove basicMove = UFEBasicMove.none)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.AddClip(clip, animName, speed, wrapMode, length);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.AddClip(clip, animName, speed, wrapMode, length, basicMove);
#endif
        }
    }

    public BasicMoveInfo GetBasicAnimationInfo(BasicMoveReference reference)
    {
        for (int i = 0; i < basicMoveList.Count; i++)
        {
            BasicMoveInfo basicMove = basicMoveList[i];
            if (basicMove.reference == reference)
            {
                return basicMove;
            }
        }
        return null;
    }

    public string GetAnimationString(BasicMoveInfo basicMove, int clipNum)
    {
        if (clipNum == 1) return basicMove.name;
        if (clipNum == 2 && basicMove.clip2 != null) return basicMove.name + "_2";
        if (clipNum == 3 && basicMove.clip3 != null) return basicMove.name + "_3";
        if (clipNum == 4 && basicMove.clip4 != null) return basicMove.name + "_4";
        if (clipNum == 5 && basicMove.clip5 != null) return basicMove.name + "_5";
        if (clipNum == 6 && basicMove.clip6 != null) return basicMove.name + "_6";
        return basicMove.name;
    }


    public bool IsBasicMovePlaying(BasicMoveInfo basicMove)
    {
        if (basicMove.clip1 != null && IsAnimationPlaying(basicMove.name)) return true;
        if (basicMove.clip2 != null && IsAnimationPlaying(basicMove.name + "_2")) return true;
        if (basicMove.clip3 != null && IsAnimationPlaying(basicMove.name + "_3")) return true;
        if (basicMove.clip4 != null && IsAnimationPlaying(basicMove.name + "_4")) return true;
        if (basicMove.clip5 != null && IsAnimationPlaying(basicMove.name + "_5")) return true;
        if (basicMove.clip6 != null && IsAnimationPlaying(basicMove.name + "_6")) return true;
        return false;
    }

    public bool IsAnimationPlaying(string animationName)
    {
        return IsAnimationPlaying(animationName, 0);
    }

    public bool IsAnimationPlaying(string animationName, float weight)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.IsPlaying(animationName);
        }
        else
        {
#if !UFE_BASIC
            return mecanimControl.IsPlaying(animationName, weight);
#else
            return 0;
#endif
        }
    }

    public float GetAnimationLengh(string animationName)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            LegacyAnimationData legacyAnimationData = legacyControl.GetAnimationData(animationName);
            return legacyControl.GetAnimationData(animationName).length;
        }
        else
        {
#if !UFE_BASIC
            return mecanimControl.GetAnimationData(animationName).length;
#else
            return 0;
#endif
        }
    }

    public bool AnimationExists(string animationName)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return (legacyControl.GetAnimationData(animationName) != null);
        }
        else
        {
#if !UFE_BASIC
            return (mecanimControl.GetAnimationData(animationName) != null);
#else
            return false;
#endif
        }
    }

    public void PlayAnimation(string animationName, float blendingTime)
    {
        PlayAnimation(animationName, blendingTime, 0);
    }

    public void PlayAnimation(string animationName, float blendingTime, float normalizedTime)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.Play(animationName, blendingTime, normalizedTime);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.Play(animationName, blendingTime, normalizedTime, false);
#endif
        }
    }

    public void PlayMoveAnimation(UFEBasicMove ufeBasicMove)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Mecanim)
        {
#if !UFE_BASIC
            mecanimControl.PlayMoveAnimation(ufeBasicMove);
#endif
        }
    }

    public void StopAnimation(string animationName)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.Stop(animationName);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.Stop();
#endif
        }
    }

    public void SetAnimationSpeed(float speed)
    {
        if (speed < 1) animationPaused = true;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetSpeed(speed);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.SetSpeed(speed);
#endif
        }
    }

    public void SetAnimationSpeed(string animationName, float speed)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetSpeed(animationName, speed);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.SetSpeed(animationName, speed);
#endif
        }
    }

    public void SetAnimationNormalizedSpeed(string animationName, float normalizedSpeed)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetNormalizedSpeed(animationName, normalizedSpeed);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.SetNormalizedSpeed(animationName, normalizedSpeed);
#endif
        }
    }

    public float GetAnimationSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetSpeed();
        }
        else
        {
#if !UFE_BASIC
            return mecanimControl.GetSpeed();
#else
            return 0;
#endif
        }
    }

    public float GetAnimationSpeed(string animationName)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetSpeed(animationName);
        }
        else
        {
#if !UFE_BASIC
            return mecanimControl.GetSpeed(animationName);
#else
            return 0;
#endif
        }
    }

    private void updateCurrentMoveFrames(float speed)
    {
        if (speed > 0 && controlsScript.currentMove != null && controlsScript.currentMove.animationSpeedTemp != speed)
        {
            controlsScript.currentMove.totalFrames = (int)Mathf.Abs(Mathf.Floor(
                (controlsScript.currentMove.fps * controlsScript.currentMove.animationClip.length) / speed));
            controlsScript.currentMove.animationSpeedTemp = speed;
        }
    }

    public void RestoreAnimationSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.RestoreSpeed();
            if (controlsScript.currentMove != null && legacyControl.IsPlaying(controlsScript.currentMove.name))
            {
                controlsScript.currentMove.currentFrame = (int)Mathf.Round(legacyControl.GetCurrentClipPosition() * (float)controlsScript.currentMove.totalFrames);
                controlsScript.currentMove.currentTick = controlsScript.currentMove.currentFrame;
            }
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.RestoreSpeed();
            if (controlsScript.currentMove != null && mecanimControl.IsPlaying(controlsScript.currentMove.name))
            {
                controlsScript.currentMove.currentFrame = (int)Mathf.Round(mecanimControl.GetCurrentClipPosition() * (float)controlsScript.currentMove.totalFrames);
                controlsScript.currentMove.currentTick = controlsScript.currentMove.currentFrame;
            }
#endif
        }
        animationPaused = false;
    }

    public void PlayBasicMove(BasicMoveInfo basicMove)
    {
        PlayBasicMove(basicMove, basicMove.name);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, bool replay)
    {
        PlayBasicMove(basicMove, basicMove.name, replay);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName)
    {
        PlayBasicMove(basicMove, clipName, true);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, bool replay)
    {
        if (overrideNextBlendingValue > -1)
        {
            PlayBasicMove(basicMove, clipName, overrideNextBlendingValue);
            overrideNextBlendingValue = -1;
        }
        else if (basicMove.overrideBlendingIn)
        {
            PlayBasicMove(basicMove, clipName, basicMove.blendingIn, replay, basicMove.invincible);
        }
        else
        {
            PlayBasicMove(basicMove, clipName, controlsScript.myInfo.blendingTime, replay, basicMove.invincible);
        }

        if (basicMove.overrideBlendingOut) overrideNextBlendingValue = basicMove.blendingOut;
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, float blendingTime)
    {
        PlayBasicMove(basicMove, clipName, blendingTime, true, basicMove.invincible);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, float blendingTime, bool replay)
    {
        PlayBasicMove(basicMove, clipName, blendingTime, replay, basicMove.invincible);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, float blendingTime, bool replay, bool hideHitBoxes)
    {
        if (IsAnimationPlaying(clipName) && !replay) return;
        PlayAnimation(clipName, blendingTime);

        controlsScript.applyRootMotion = basicMove.applyRootMotion;

        _playBasicMove(basicMove);
        hitBoxesScript.HideHitBoxes(hideHitBoxes);
    }

    public void RemoveAllBasicMoveParticle()
    {
        if (basicMoveParticleEffectMap != null)
        {
            foreach (string moveName in basicMoveParticleEffectMap.Keys)
            {
                GameObject particleEffect = basicMoveParticleEffectMap[moveName];
                GameObject.Destroy(particleEffect);
            }
            basicMoveParticleEffectMap.Clear();
        }
    }

    public void ScaleBasicMoveEffect(float scale)
    {
        if (basicMoveParticleEffectMap != null && currentBasicMove != null)
        {
            foreach (string moveName in basicMoveParticleEffectMap.Keys)
            {
                GameObject particleEffect = basicMoveParticleEffectMap[moveName];
                GeneralUtils.ScaleParticleSystem(currentBasicMove.particleEffect.prefab, particleEffect, scale);
            }
        }
    }

    private Dictionary<string, GameObject> basicMoveParticleEffectMap;
    private void _playBasicMove(BasicMoveInfo basicMove)
    {
        controlsScript.currentBasicMove = basicMove.reference;
        currentBasicMove = basicMove;
        GameObject cachePTemp = null;
        if (basicMoveParticleEffectMap != null)
        {
            if (basicMove.particleEffect.prefab != null && basicMoveParticleEffectMap.ContainsKey(basicMove.name))
            {
                cachePTemp = basicMoveParticleEffectMap[basicMove.name];
                basicMoveParticleEffectMap.Remove(basicMove.name);
            }
            foreach (string moveName in basicMoveParticleEffectMap.Keys)
            {
                if (moveName != basicMove.name)
                {
                    GameObject particleEffect = basicMoveParticleEffectMap[moveName];
                    GameObject.Destroy(particleEffect);
                }
            }
            basicMoveParticleEffectMap.Clear();
        }

        if (basicMove.particleEffect.prefab != null)
        {
            GameObject pTemp = null;
            if (cachePTemp != null)
            {
                pTemp = cachePTemp;
            }
            else
            {
                pTemp = (GameObject)Instantiate(basicMove.particleEffect.prefab);
            }
            Transform newPosition = hitBoxesScript.GetTransform(basicMove.particleEffect.bodyPart);
            if (basicMove.particleEffect.stick)
            {
                pTemp.transform.parent = newPosition;
            }
            pTemp.transform.localPosition = Vector3.zero;
            pTemp.transform.localRotation = Quaternion.identity;

            if (basicMove.particleEffect.duration < 0)
            {
                if (basicMoveParticleEffectMap == null)
                {
                    basicMoveParticleEffectMap = new Dictionary<string, GameObject>();
                }
                if (basicMoveParticleEffectMap.ContainsKey(basicMove.name) == false)
                {
                    basicMoveParticleEffectMap.Add(basicMove.name, pTemp);
                }
            }
            else
            {
                Destroy(pTemp, basicMove.particleEffect.duration);
            }
        }
    }

    public void SetAnimationPosition(string animationName, float normalizedTime)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetCurrentClipPosition(normalizedTime);
        }
        else
        {
#if !UFE_BASIC
            mecanimControl.SetCurrentClipPosition(normalizedTime);
#endif
        }
    }

    public Vector2 GetDeltaPosition()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Mecanim)
        {
#if !UFE_BASIC
            return mecanimControl.GetDeltaPosition();
#endif
        }
        else
        {
            return legacyControl.GetDeltaPosition();
        }
    }

    public float GetCurrentClipPosition()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipPosition();
        }
        else
        {
#if !UFE_BASIC
            return mecanimControl.GetCurrentClipPosition();
#else
            return 0;
#endif
        }
    }

    public float GetAnimationTime(int animFrame, MoveInfo move)
    {
        if (move == null) return 0;
        if (move.animationSpeed < 0)
        {
            return (((float)animFrame / (float)FightManager.config.fps) * move.animationSpeed) + move.animationClip.length;
        }
        else
        {
            return ((float)animFrame / (float)FightManager.config.fps) * move.animationSpeed;
        }
    }

    public float GetAnimationNormalizedTime(int animFrame, MoveInfo move)
    {
        if (move == null) return 0;
        if (move.animationSpeed < 0)
        {
            return ((float)animFrame / (float)move.totalFrames) + 1;
        }
        else
        {
            return (float)animFrame / (float)move.totalFrames;
        }
    }

    public MoveInfo InstantiateMove(MoveInfo move)
    {
        if (move == null)
        {
            return null;
        }
        MoveInfo newMove = PoolUtil.SpawnerMoveInfo(move);
        newMove.name = move.name;
        return newMove;
    }

    public MoveInfo GetNextMove(MoveInfo currentMove)
    {
        if (currentMove.frameLinks.Length == 0) return null;
        for (int i = 0; i < currentMove.frameLinks.Length; i++)
        {
            FrameLink frameLink = currentMove.frameLinks[i];
            if (frameLink.linkableMoves.Length == 0) continue;
            if (frameLink.cancelable || frameLink.counterCancelable)
            {
                for (int j = 0; j < frameLink.linkableMoves.Length; j++)
                {
                    MoveInfo move = frameLink.linkableMoves[j];
                    if (move == null) continue;
                    if (move.buttonExecution.Length == 0 || frameLink.ignoreInputs)
                    {
                        return InstantiateMove(move);
                    }
                }
            }
        }
        return null;
    }

    public void ClearLastButtonSequence()
    {
        lastButtonPresses.Clear();
        lastTimePress = 0;
        for (int i = 0; i < buttonPressArr.Length; i++)
        {
            ButtonPress bp = (ButtonPress)buttonPressArr.GetValue(i);
            chargeValues[bp] = 0;
        }
    }

    public MoveInfo GetMove(ButtonPress buttonPress)
    {
        for (int i = 0; i < attackMoves.Length; i++)
        {
            MoveInfo moveInfo = attackMoves[i];
            for (int j = 0; j < moveInfo.buttonExecution.Length; j++)
            {
                ButtonPress button = moveInfo.buttonExecution[j];
                if (button == buttonPress)
                {
                    return moveInfo;
                }
            }
        }
        return null;
    }

	public MoveInfo GetMove(WeaponType weaponType, int weaponIndex)
	{
		for (int i = 0; i < attackMoves.Length; i++)
		{
			MoveInfo moveInfo = attackMoves[i];
			PlayerConditions selfConditions = moveInfo.selfConditions;
			for (int j = 0; j < selfConditions.possibleMoveStates.Length; j++) {
				PossibleMoveStates possibleMoveStates = selfConditions.possibleMoveStates[j];
				if (possibleMoveStates.weaponType == weaponType && possibleMoveStates.weaponIndex == weaponIndex) {
					return moveInfo;			
				}
			}
		}
		return null;
	}

    public float GetMoveProximityRangeBegins(MoveInfo moveInfo)
    {
        PlayerConditions selfConditions = moveInfo.selfConditions;
        for (int j = 0; j < selfConditions.possibleMoveStates.Length; j++)
        {
            PossibleMoveStates possibleMoveStates = selfConditions.possibleMoveStates[j];
            return (float)possibleMoveStates.proximityRangeBegins;
        }
        return 0f;
    }

    public float GetMoveProximityRangeEnds(MoveInfo moveInfo)
    {
        PlayerConditions selfConditions = moveInfo.selfConditions;
        for (int j = 0; j < selfConditions.possibleMoveStates.Length; j++)
        {
            PossibleMoveStates possibleMoveStates = selfConditions.possibleMoveStates[j];
            return (float)possibleMoveStates.proximityRangeEnds;
        }
        return 10f;
    }

    public MoveInfo GetMove(string moveName)
    {
        for (int i = 0; i < attackMoves.Length; i++)
        {
            MoveInfo moveInfo = attackMoves[i];
            if (moveInfo.moveName == moveName)
            {
                return moveInfo;
            }
        }
        return null;
    }

    private bool searchMove(string moveName, FrameLink[] frameLinks)
    {
        return searchMove(moveName, frameLinks, false);
    }

    private bool searchMove(string moveName, FrameLink[] frameLinks, int currentFrame)
    {
        for (int i = 0; i < frameLinks.Length; i++)
        {
            FrameLink frameLink = frameLinks[i];
            if ((currentFrame >= frameLink.activeFramesBegins && currentFrame <= frameLink.activeFramesEnds)
                || (currentFrame >= (frameLink.activeFramesBegins - FightManager.config.executionBufferTime)
                && currentFrame <= frameLink.activeFramesEnds) && frameLink.allowBuffer)
            {
                for (int j = 0; j < frameLink.linkableMoves.Length; j++)
                {
                    MoveInfo move = frameLink.linkableMoves[j];
                    if (move != null && moveName == move.moveName)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool searchMove(string moveName, FrameLink[] frameLinks, bool ignoreConditions)
    {
        for (int i = 0; i < frameLinks.Length; i++)
        {
            FrameLink frameLink = frameLinks[i];
            if (frameLink.cancelable)
            {
                if (ignoreConditions && !frameLink.ignorePlayerConditions)
                {
                    continue;
                }
                for (int j = 0; j < frameLink.linkableMoves.Length; j++)
                {
                    MoveInfo move = frameLink.linkableMoves[j];
                    if (move == null) continue;
                    if (moveName == move.moveName) return true;
                }
            }
        }
        return false;
    }

    private bool searchMove(string moveName, MoveInfo[] moves)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            MoveInfo move = moves[i];
            if (move == null) continue;
            if (moveName == move.moveName) return true;
        }
        return false;
    }

    public bool HasMove(string moveName)
    {
        for (int i = 0; i < this.moves.Length; i++)
        {
            MoveInfo move = this.moves[i];
            if (move == null) continue;
            if (moveName == move.moveName) return true;
        }
        return false;
    }


    public bool ValidateMoveExecution(MoveInfo move)
    {
        if (!ValidateMoveStances(move.selfConditions, controlsScript, true)) return false;
        if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return false;
        //if (!hasEnoughGauge(move.gaugeUsage)) return false;
        if (move.previousMoves.Length > 0 && controlsScript.currentMove == null) return false;
        if (move.previousMoves.Length > 0 && !searchMove(controlsScript.currentMove.moveName, move.previousMoves)) return false;

        if (controlsScript.currentMove != null && controlsScript.currentMove.frameLinks.Length == 0) return false;
        if (controlsScript.currentMove != null && !searchMove(move.moveName, controlsScript.currentMove.frameLinks)) return false;
        return true;
    }


    public bool ValidateMoveStances(PlayerConditions conditions, Player cScript)
    {
        return ValidateMoveStances(conditions, cScript, false);
    }

    public bool ValidateMoveStances(PlayerConditions conditions, Player cScript, bool bypassCrouchStance)
    {
        bool stateCheck = conditions.possibleMoveStates.Length > 0 ? false : true;
        for (int i = 0; i < conditions.possibleMoveStates.Length; i++)
        {
            PossibleMoveStates possibleMoveState = conditions.possibleMoveStates[i];
            if(cScript.isAIController)
            {
                if (cScript.normalizedDistance < (float)possibleMoveState.proximityRangeBegins / 100) continue;
                if (cScript.normalizedDistance > (float)possibleMoveState.proximityRangeEnds / 100) continue;
            }

            //if (possibleMoveState.isNeedBoom && cScript.IsContainsBoom == false) continue;
            //if (possibleMoveState.isNeedKnife && cScript.IsContainsKnife == false) continue;
            if (possibleMoveState.isContinueNormalAttack && (cScript.IsContinueNormalAttack == false || cScript.isDead)) continue;

			//if (possibleMoveState.weaponType != WeaponType.None && cScript.myInfo.weaponIndex != possibleMoveState.weaponIndex) continue;

            //if (!possibleMoveState.isFast && cScript.IsFast) continue;
            //if (!possibleMoveState.isNotFast && cScript.IsFast == false) continue;

            stateCheck = true;
        }
        return stateCheck;
    }

    public bool ValidadeBasicMove(PlayerConditions conditions, Player cScript)
    {
        if (conditions.basicMoveLimitation.Length == 0) return true;
        if (System.Array.IndexOf(conditions.basicMoveLimitation, cScript.currentBasicMove) != -1) return true;
        return false;
    }

    private T[] ArrayIntersect<T>(T[] a1, T[] a2)
    {
        if (a1 == null || a2 == null) return null;

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        List<T> intersection = new List<T>();
        int nextStartingPoint = 0;
        for (int i = 0; i < a1.Length; i++)
        { // button sequence
            bool added = false;
            for (int k = nextStartingPoint; k < a2.Length; k++)
            { // button presses
                if (comparer.Equals(a1[i], a2[k]))
                {
                    intersection.Add(a2[k]);
                    nextStartingPoint = k;
                    added = true;
                    break;
                }
            }
            if (!added) return null;
        }

        return intersection.ToArray();
    }

    private bool ArraysEqual<T>(T[] a1, T[] a2)
    {
        if (ReferenceEquals(a1, a2)) return true;
        if (a1 == null || a2 == null) return false;
        if (a1.Length != a2.Length) return false;
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < a1.Length; i++)
        {
            if (!comparer.Equals(a1[i], a2[i])) return false;
        }
        return true;
    }
}
