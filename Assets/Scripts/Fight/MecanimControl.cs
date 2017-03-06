using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 动画播放控制器
/// nashnie
/// </summary>
[System.Serializable]
public class AnimationData
{
	public AnimationClip clip;
	public string clipName;
    public string truelyPlayClipName;
    public float speed = 1;
	public float transitionDuration = -1;
	public WrapMode wrapMode;
	public bool applyRootMotion;
	[HideInInspector] public int timesPlayed = 0;
	[HideInInspector] public float secondsPlayed = 0;
	[HideInInspector] public float length = 1;
    [HideInInspector] public float originalSpeed = 1;
    [HideInInspector] public float normalizedSpeed = 1;
    [HideInInspector] public float normalizedTime = 1;
	[HideInInspector] public int stateHash;
	[HideInInspector] public string stateName;
    [HideInInspector] public UFEBasicMove ufeBasicMove = UFEBasicMove.none;

    [HideInInspector]
    public int bottomTimesPlayed = 0;
    [HideInInspector]
    public float bottomSecondsPlayed = 0;
    [HideInInspector]
    public float bottomOriginalSpeed = 1;
    [HideInInspector]
    public float bottomNormalizedSpeed = 1;
    [HideInInspector]
    public float bottomNormalizedTime = 1;
}

[RequireComponent (typeof (Animator))]
public class MecanimControl : MonoBehaviour
{
	public AnimationData defaultAnimation = new AnimationData();
    private List<UFEBasicMove> moveAnimationNameList = new List<UFEBasicMove>();

    public AnimationData[] animations = new AnimationData[0];
	public bool debugMode = false;
	public bool alwaysPlay = false;
	public bool overrideRootMotion = false;
    public bool overrideAnimatorUpdate = false;
	public float defaultTransitionDuration = 0.05f;
	public WrapMode defaultWrapMode = WrapMode.Loop;
    [HideInInspector]
    public bool isBodyMask = false;

    private Animator animator;

    private int topAvatar = -1;
    private int bottomAvatar = -1;

    private RuntimeAnimatorController controller;
    private bool isInitComplete = false;

	private AnimationData currentAnimationData;
    private AnimationData currentBottomAnimationData;
    private bool currentMirror;
    private AnimationData waitPlayAnimationData;

    public delegate void AnimEvent(AnimationData animationData);
	public static event AnimEvent OnAnimationBegin;
	public static event AnimEvent OnAnimationEnd;
	public static event AnimEvent OnAnimationLoop;
	
	void Awake ()
    {
		animator = gameObject.GetComponent<Animator>();
        animator.gameObject.SetActive(true);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (isBodyMask)
        {
            controller = animator.runtimeAnimatorController;
            bottomAvatar = -1;
            topAvatar = -1;
        }
        else
        {
            controller = animator.runtimeAnimatorController;
            topAvatar = 0;
        }
        foreach (AnimationData animData in animations)
        {
            if (animData.wrapMode == WrapMode.Default)
            {
                animData.wrapMode = defaultWrapMode;
            }
            animData.clip.wrapMode = animData.wrapMode;
        }
    }

    void Start()
    {
        if (defaultAnimation.clip == null && animations.Length > 0)
        {
            SetDefaultClip(animations[0].clip, "State1", animations[0].speed, animations[0].wrapMode, false);
        }

        AnimationData wantPlayAnimationData = waitPlayAnimationData != null ? waitPlayAnimationData : defaultAnimation;

		if (wantPlayAnimationData.clip != null)
        {
			currentAnimationData = wantPlayAnimationData;
            if (isBodyMask)
            {
                currentBottomAnimationData = wantPlayAnimationData;
            }
            currentAnimationData.stateName = "State1";

            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = controller;

            overrideController["Default"] = overrideController[currentAnimationData.truelyPlayClipName];
            overrideController["State1"] = overrideController[currentAnimationData.truelyPlayClipName];

            if (isBodyMask)
            {
                overrideController["Idle"] = overrideController[currentAnimationData.truelyPlayClipName];
                overrideController["Move"] = overrideController[currentAnimationData.truelyPlayClipName];
            }

            animator.runtimeAnimatorController = overrideController;
			animator.Play("State1", topAvatar, 0);
            if (isBodyMask)
            {
                animator.Play("Move", bottomAvatar, 0);
            }
            if (overrideRootMotion)
            {
                animator.applyRootMotion = currentAnimationData.applyRootMotion;
            }
			SetSpeed(currentAnimationData.speed);
		}
        isInitComplete = true;
    }
	
	public void DoFixedUpdate()
    {
        if (currentAnimationData == null || currentAnimationData.clip == null) return;

        currentAnimationData.secondsPlayed += (Time.fixedDeltaTime * GetSpeed());

        if (currentAnimationData.secondsPlayed > currentAnimationData.length)
        {
            currentAnimationData.secondsPlayed = currentAnimationData.length;
        }
        currentAnimationData.normalizedTime = currentAnimationData.secondsPlayed / currentAnimationData.length;

        if (currentAnimationData.secondsPlayed == currentAnimationData.length)
        {
            if (currentAnimationData.clip.wrapMode == WrapMode.Loop || currentAnimationData.clip.wrapMode == WrapMode.PingPong)
            {
                if (MecanimControl.OnAnimationLoop != null)
                {
                    MecanimControl.OnAnimationLoop(currentAnimationData);
                }
                currentAnimationData.timesPlayed++;

                if (currentAnimationData.clip.wrapMode == WrapMode.Loop)
                {
                    SetCurrentClipPosition(0);
                }

                if (currentAnimationData.clip.wrapMode == WrapMode.PingPong)
                {
                    SetSpeed(currentAnimationData.clipName, -currentAnimationData.speed);
                    SetCurrentClipPosition(0);
                }

            }
            else if (currentAnimationData.timesPlayed == 0)
            {
                if (MecanimControl.OnAnimationEnd != null)
                {
                    MecanimControl.OnAnimationEnd(currentAnimationData);
                }
                currentAnimationData.timesPlayed = 1;

                if (currentAnimationData.clip.wrapMode == WrapMode.Once && alwaysPlay && currentAnimationData.clipName != "death")
                {
					Play(defaultAnimation, currentMirror);
                }
                else if (!alwaysPlay)
                {
                    SetSpeed(0);
				}
			}
		}
        if (currentBottomAnimationData != null)
        {
            currentBottomAnimationData.bottomSecondsPlayed += (Time.fixedDeltaTime * GetSpeed());
            if (currentBottomAnimationData.bottomSecondsPlayed > currentBottomAnimationData.length)
            {
                currentBottomAnimationData.bottomSecondsPlayed = currentBottomAnimationData.length;
            }
            currentBottomAnimationData.bottomNormalizedTime = currentBottomAnimationData.bottomSecondsPlayed / currentBottomAnimationData.length;
            if (currentBottomAnimationData.bottomSecondsPlayed == currentBottomAnimationData.length)
            {
                currentBottomAnimationData.bottomTimesPlayed++;
                currentBottomAnimationData.bottomSecondsPlayed = 0;
                currentBottomAnimationData.bottomNormalizedTime = 0;
            }
        }
	}
	
	public void SetDefaultClip(AnimationClip clip, string name, float speed, WrapMode wrapMode, bool mirror)
    {
        defaultAnimation.truelyPlayClipName = clip.name;
        defaultAnimation.clip = clip;
        defaultAnimation.clip.wrapMode = wrapMode;
		defaultAnimation.clipName = name;
        defaultAnimation.speed = speed;
		defaultAnimation.originalSpeed = speed;
		defaultAnimation.transitionDuration = -1;
		defaultAnimation.wrapMode = wrapMode;
	}

    public void AddClip(AnimationClip clip, string newName, float speed, WrapMode wrapMode)
    {
        AddClip(clip, newName, speed, wrapMode, clip.length);
    }

    public void AddClip(AnimationClip clip, string newName, float speed, WrapMode wrapMode, float length, UFEBasicMove basicMove = UFEBasicMove.none)
    {
        if (GetAnimationData(newName) != null)
        {
            Debug.LogWarning("An animation with the name '" + newName + "' already exists.");
        }
		AnimationData animData = new AnimationData();
        if (wrapMode == WrapMode.Default)
        {
            wrapMode = defaultWrapMode;
        }
        animData.clip = clip;
        animData.clip.wrapMode = wrapMode;
		animData.clipName = newName;
        animData.truelyPlayClipName = clip.name;
        animData.speed = speed;
        animData.originalSpeed = speed;
        animData.length = length;
		animData.wrapMode = wrapMode;
        animData.ufeBasicMove = basicMove;

        List<AnimationData> animationDataList = new List<AnimationData>(animations);
		animationDataList.Add(animData);
		animations = animationDataList.ToArray();

        switch (basicMove)
        {   
            case UFEBasicMove.idle:
            case UFEBasicMove.walkFont:
            case UFEBasicMove.runFont:
            case UFEBasicMove.walkBack:
            case UFEBasicMove.runBack:
            case UFEBasicMove.walkLeft:
            case UFEBasicMove.walkRight:
            case UFEBasicMove.RunLeft:
            case UFEBasicMove.RunRight:
            case UFEBasicMove.die:
                moveAnimationNameList.Add(basicMove);
                break;
            case UFEBasicMove.none:
            default:
                break;
        }
    }

	public AnimationData GetAnimationData(string clipName)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            AnimationData animData = animations[i];
            if (animData.clipName == clipName)
            {
                return animData;
            }
        }
        if (clipName == defaultAnimation.clipName)
        {
            return defaultAnimation;
        }
		return null;
	}

	public AnimationData GetAnimationData(AnimationClip clip)
    {
        for (int i = 0; i < animations.Length; i++)
        {
            AnimationData animData = animations[i];
            if (animData.clip == clip)
            {
                return animData;
            }
        }
        if (clip == defaultAnimation.clip)
        {
            return defaultAnimation;
        }
		return null;
	}
	
	public void CrossFade(string clipName, float blendingTime){
		CrossFade(clipName, blendingTime, 0, currentMirror);
	}

	public void CrossFade(string clipName, float blendingTime, float normalizedTime, bool mirror){
		_playAnimation(GetAnimationData(clipName), blendingTime, normalizedTime, mirror);
	}
	
	public void CrossFade(AnimationData animationData, float blendingTime, float normalizedTime, bool mirror){
		_playAnimation(animationData, blendingTime, normalizedTime, mirror);
	}

	public void Play(string clipName, float blendingTime, float normalizedTime, bool mirror){
		_playAnimation(GetAnimationData(clipName), blendingTime, normalizedTime, mirror);
	}

    public void PlayMoveAnimation(UFEBasicMove ufeBasicMove)
    {
        if (isBodyMask)
        {
            AnimationData basicAnimation = null;
            for (int i = 0; i < animations.Length; i++)
            {
                AnimationData animation = animations[i];
                if (animation.ufeBasicMove == ufeBasicMove)
                {
                    basicAnimation = animation;
                    break;
                }
            }
            if (basicAnimation != currentBottomAnimationData && basicAnimation != null)
            {
                currentBottomAnimationData = basicAnimation;
            }
        }
    }

    public void Play(AnimationClip clip, float blendingTime, float normalizedTime, bool mirror){
		_playAnimation(GetAnimationData(clip), blendingTime, normalizedTime, mirror);
	}

	public void Play(string clipName, bool mirror){
		_playAnimation(GetAnimationData(clipName), 0, 0, mirror);
	}

	public void Play(string clipName){
		_playAnimation(GetAnimationData(clipName), 0, 0, currentMirror);
	}
	
	public void Play(AnimationClip clip, bool mirror){
		_playAnimation(GetAnimationData(clip), 0, 0, mirror);
	}

	public void Play(AnimationClip clip){
		_playAnimation(GetAnimationData(clip), 0, 0, currentMirror);
	}

	public void Play(AnimationData animationData, bool mirror){
		_playAnimation(animationData, animationData.transitionDuration, 0, mirror);
	}

	public void Play(AnimationData animationData){
		_playAnimation(animationData, animationData.transitionDuration, 0, currentMirror);
	}
	
	public void Play(AnimationData animationData, float blendingTime, float normalizedTime, bool mirror){
		_playAnimation(animationData, blendingTime, normalizedTime, mirror);
	}

	public void Play(){
        SetSpeed(currentAnimationData.speed);
	}


    /// <summary>
    /// todo check clip count
    /// </summary>
    /// <param name="targetAnimationData"></param>
    /// <param name="blendingTime"></param>
    /// <param name="normalizedTime"></param>
    /// <param name="mirror"></param>
	private void _playAnimation(AnimationData targetAnimationData, float blendingTime, float normalizedTime, bool mirror)
    {
        if (targetAnimationData == null)
        {
            return;
        }
        if(isInitComplete == false)
        {
            waitPlayAnimationData = targetAnimationData;
            return;
        }
        currentMirror = mirror;

		float newAnimatorSpeed = Mathf.Abs(targetAnimationData.originalSpeed);

        float currentNormalizedTime = GetCurrentClipPosition();

        string targetState = "State1";
        if (!mirror)
        {
            if (targetAnimationData.originalSpeed >= 0)
            {
                targetState = "State1";
			}
            else
            {
                targetState = "State2";
			}
		}
        else
        {
            if (targetAnimationData.originalSpeed >= 0)
            {
                targetState = "State3";
			}
            else
            {
                targetState = "State4";
			}
		}

        AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

        if (currentAnimationData != null && currentAnimationData.clip != null)
        {
            overrideController["Default"] = overrideController[currentAnimationData.truelyPlayClipName];
        }
        overrideController[targetState] = overrideController[targetAnimationData.truelyPlayClipName];

        if (isBodyMask)
        {
            if (moveAnimationNameList.IndexOf(targetAnimationData.ufeBasicMove) >= 0)
            {
                if (currentBottomAnimationData != targetAnimationData)
                {
                    if (currentBottomAnimationData != null)
                    {
                        currentBottomAnimationData.bottomNormalizedTime = 0;
                    }
                    currentBottomAnimationData = targetAnimationData;
                    currentBottomAnimationData.bottomNormalizedTime = 0;
                }
            }
            if (currentBottomAnimationData != null)
            {
                overrideController["Idle"] = overrideController[currentBottomAnimationData.truelyPlayClipName];
                overrideController["Move"] = overrideController[currentBottomAnimationData.truelyPlayClipName];
            }
        }

        if (blendingTime == -1) blendingTime = currentAnimationData.transitionDuration;
		if (blendingTime == -1) blendingTime = defaultTransitionDuration;

        if (blendingTime <= 0 || currentAnimationData == null)
        {
			animator.runtimeAnimatorController = overrideController;
			animator.Play(targetState, topAvatar, normalizedTime);
        }
        else
        {
			animator.runtimeAnimatorController = overrideController;
            currentAnimationData.stateName = "Default";
            SetCurrentClipPosition(currentNormalizedTime);

            animator.Play("Default", topAvatar, normalizedTime);
            animator.CrossFade(targetState, blendingTime/newAnimatorSpeed, topAvatar, normalizedTime);
        }

        if (isBodyMask)
        {
            animator.Play("Move", bottomAvatar, currentBottomAnimationData.bottomNormalizedTime);
        }

        animator.Update(0);

        targetAnimationData.timesPlayed = 0;
        targetAnimationData.secondsPlayed = (normalizedTime * targetAnimationData.length) / newAnimatorSpeed;
        targetAnimationData.normalizedTime = normalizedTime;
        targetAnimationData.speed = targetAnimationData.originalSpeed;

        if (overrideRootMotion)
        {
            animator.applyRootMotion = targetAnimationData.applyRootMotion;
        }
        SetSpeed(targetAnimationData.originalSpeed);
        if (currentAnimationData != null)
        {
            currentAnimationData.speed = currentAnimationData.originalSpeed;
            currentAnimationData.normalizedSpeed = 1;
            currentAnimationData.timesPlayed = 0;
        }

		currentAnimationData = targetAnimationData;
		currentAnimationData.stateName = targetState;

        if (MecanimControl.OnAnimationBegin != null)
        {
            MecanimControl.OnAnimationBegin(currentAnimationData);
        }
		/*#if UNITY_EDITOR
		UnityEngine.Debug.Log("_playAnimation clipName " + currentAnimationData.clipName);
		#endif*/
	}
	
	public bool IsPlaying(string clipName){
		return IsPlaying(GetAnimationData(clipName));
	}
	
	public bool IsPlaying(string clipName, float weight){
		return IsPlaying(GetAnimationData(clipName), weight);
	}
	
	public bool IsPlaying(AnimationClip clip){
		return IsPlaying(GetAnimationData(clip));
	}
	
	public bool IsPlaying(AnimationClip clip, float weight){
		return IsPlaying(GetAnimationData(clip), weight);
	}
	
	public bool IsPlaying(AnimationData animData){
        return (currentAnimationData == animData);
	}
	
	public bool IsPlaying(AnimationData animData, float weight)
    {
		if (animData == null) return false;
		if (currentAnimationData == null) return false;
		if (currentAnimationData == animData && animData.wrapMode == WrapMode.Once && animData.timesPlayed > 0) return false;
		if (currentAnimationData == animData && animData.wrapMode == WrapMode.ClampForever) return true;
		if (currentAnimationData == animData) return true;

        AnimatorClipInfo[] animationInfoArray;
        if(isBodyMask)
        {
            animationInfoArray = animator.GetCurrentAnimatorClipInfo(1);
        }
        else
        {
            animationInfoArray = animator.GetCurrentAnimatorClipInfo(0);
        }
        for (int i = 0; i < animationInfoArray.Length; i++)
        {
            AnimatorClipInfo animationInfo = animationInfoArray[i];
            if (animData.clip == animationInfo.clip && animationInfo.weight >= weight)
            {
                return true;
            }
        }
		return false;
	}
	
	public string GetCurrentClipName(){
		return currentAnimationData.clipName;
	}
	
	public AnimationData GetCurrentAnimationData(){
		return currentAnimationData;
	}
	
	public int GetCurrentClipPlayCount(){
		return currentAnimationData.timesPlayed;
	}
	
	public float GetCurrentClipTime(){
		return currentAnimationData.secondsPlayed;
	}

	public float GetCurrentClipLength(){
		return currentAnimationData.length;
	}

    public Vector2 GetDeltaPosition() {
        return animator.deltaPosition;
    }

	public void SetCurrentClipPosition(float normalizedTime){
		SetCurrentClipPosition(normalizedTime, false);
	}

    public void SetCurrentClipPosition(float normalizedTime, bool pause)
    {
        normalizedTime = Mathf.Clamp01(normalizedTime);
        currentAnimationData.secondsPlayed = normalizedTime * currentAnimationData.length;
        currentAnimationData.normalizedTime = normalizedTime;
        animator.Play(currentAnimationData.stateName, topAvatar, normalizedTime);
        if(isBodyMask)
        {
            if (currentAnimationData == currentBottomAnimationData)
            {
                animator.Play("Move", bottomAvatar, normalizedTime);
            }
            else
            {
                animator.Play("Move", bottomAvatar, currentBottomAnimationData.bottomNormalizedTime);
            }
        }
        animator.Update(0);
        if (pause)
        {
            Pause();
        }
    }

    public float GetCurrentClipPosition()
    {
        if (currentAnimationData == null)
        {
            return 0;
        }
		return currentAnimationData.secondsPlayed/currentAnimationData.length;
	}
	
	public void Stop()
    {
		Play(defaultAnimation.clip, defaultTransitionDuration, 0, currentMirror);
	}

    public void Pause()
    {
        SetSpeed(0);
	}

    public void SetSpeed(AnimationClip clip, float speed) {
        SetSpeed(GetAnimationData(clip), speed);
    }

    public void SetSpeed(string clipName, float speed)
    {
        SetSpeed(GetAnimationData(clipName), speed);
    }

    public void SetSpeed(AnimationData animData, float speed)
    {
        if (animData != null)
        {
            animData.normalizedSpeed = speed / animData.originalSpeed;
            animData.speed = speed;
            if (IsPlaying(animData))
            {
                SetSpeed(speed);
            }
        }
    }

    public void SetSpeed(float speed)
    {
        animator.speed = Mathf.Abs(speed);
    }

    public void SetNormalizedSpeed(AnimationClip clip, float normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clip), normalizedSpeed);
    }

    public void SetNormalizedSpeed(string clipName, float normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clipName), normalizedSpeed);
    }

    public void SetNormalizedSpeed(AnimationData animData, float normalizedSpeed) {
        animData.normalizedSpeed = normalizedSpeed;
        animData.speed = animData.originalSpeed * animData.normalizedSpeed;
        if (IsPlaying(animData)) SetSpeed(animData.speed);
    }
	
	public void RestoreSpeed()
    {
        if (currentAnimationData != null)
        {
            SetSpeed(currentAnimationData.speed);
        }
		else
        {
            SetSpeed(1f);
        }
	}
	
	public void Rewind(){
		SetSpeed(-currentAnimationData.speed);
	}

	public void SetWrapMode(WrapMode wrapMode){
		defaultWrapMode = wrapMode;
	}
	
	public void SetWrapMode(AnimationData animationData, WrapMode wrapMode){
		animationData.wrapMode = wrapMode;
		animationData.clip.wrapMode = wrapMode;
	}

	public void SetWrapMode(AnimationClip clip, WrapMode wrapMode){
		AnimationData animData = GetAnimationData(clip);
		animData.wrapMode = wrapMode;
		animData.clip.wrapMode = wrapMode;
	}

	public void SetWrapMode(string clipName, WrapMode wrapMode){
		AnimationData animData = GetAnimationData(clipName);
		animData.wrapMode = wrapMode;
		animData.clip.wrapMode = wrapMode;
	}

    public float GetSpeed(AnimationClip clip) {
        return GetSpeed(GetAnimationData(clip));
	}

    public float GetSpeed(string clipName) {
        return GetSpeed(GetAnimationData(clipName));
	}

    public float GetSpeed(AnimationData animData) {
        return animData.speed;
    }

    public float GetSpeed() {
        //return animator.GetFloat("Speed");
		return animator.speed;
	}

    public float GetNormalizedSpeed(AnimationClip clip) {
        return GetNormalizedSpeed(GetAnimationData(clip));
    }

    public float GetNormalizedSpeed(string clipName) {
        return GetNormalizedSpeed(GetAnimationData(clipName));
    }

    public float GetNormalizedSpeed(AnimationData animData) {
        return animData.normalizedSpeed;
    }

	public void SetMirror(bool toggle){
		SetMirror(toggle, 0, false);
	}

	public void SetMirror(bool toggle, float blendingTime, bool forceMirror)
    {
		if (currentMirror == toggle && !forceMirror) return;
		
		if (blendingTime == 0) blendingTime = defaultTransitionDuration;
		_playAnimation(currentAnimationData, blendingTime, GetCurrentClipPosition(), toggle);
	}
}
