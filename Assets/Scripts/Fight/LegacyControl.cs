using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LegacyAnimationData {
	public AnimationClip clip;
	public string clipName;
	public float speed = 1;
	public WrapMode wrapMode;
	[HideInInspector] public float length = 0;
	[HideInInspector] public float originalSpeed = 1;
	[HideInInspector] public float normalizedSpeed = 1;
	[HideInInspector] public AnimationState animState;
}

[RequireComponent(typeof(Animation))]
public class LegacyControl : MonoBehaviour {

    public LegacyAnimationData[] animations = new LegacyAnimationData[0];
    public bool debugMode = false;
    public bool overrideAnimatorUpdate = false;

    private Animation animator;

    private LegacyAnimationData currentAnimationData;
    private bool currentMirror;
    private float globalSpeed = 1;
    Vector3 lastPosition;

    void Awake() {
        animator = gameObject.GetComponent<Animation>();
        lastPosition = transform.position;
    }

    void Start() {
        if (animations[0] == null) Debug.LogWarning("No animation found!");
        currentAnimationData = animations[0];

        if (overrideAnimatorUpdate) {
            foreach (AnimationState animState in animator) {
                animState.speed = 0;
            }
        }
    }

    public void DoFixedUpdate()
    {
        if (animator == null || !animator.isPlaying || !overrideAnimatorUpdate) return;
        //Invalid parameter because it was infinity or NaN.
        float animStateTime = Time.fixedDeltaTime * globalSpeed;
        if (float.IsNaN(animStateTime) == false && float.IsInfinity(animStateTime) == false)
        {
            currentAnimationData.animState.time += animStateTime;
            animator.Sample();
        }
    }

    void OnGUI() {
        //Toggle debug mode to see the live data in action
        if (debugMode) {
            GUI.Box(new Rect(Screen.width - 340, 40, 340, 300), "Animation Data");
            GUI.BeginGroup(new Rect(Screen.width - 330, 60, 400, 300));
            {
                GUILayout.Label("Global Speed: " + globalSpeed);
                GUILayout.Label("Current Animation Data");
                GUILayout.Label("-Clip Name: " + currentAnimationData.clipName);
                GUILayout.Label("-Speed: " + currentAnimationData.speed);
                GUILayout.Label("-Normalized Speed: " + currentAnimationData.normalizedSpeed);
                GUILayout.Label("Animation State");
                GUILayout.Label("-Time: " + currentAnimationData.animState.time);
                GUILayout.Label("-Normalized Time: " + currentAnimationData.animState.normalizedTime);
                GUILayout.Label("-Lengh: " + currentAnimationData.animState.length);
                GUILayout.Label("-Speed: " + currentAnimationData.animState.speed);
            } GUI.EndGroup();
        }
    }



    // LEGACY CONTROL METHODS
    public void RemoveClip(string name) {
        List<LegacyAnimationData> animationDataList = new List<LegacyAnimationData>(animations);
        animationDataList.Remove(GetAnimationData(name));
        animations = animationDataList.ToArray();
    }

    public void RemoveClip(AnimationClip clip) {
        List<LegacyAnimationData> animationDataList = new List<LegacyAnimationData>(animations);
        animationDataList.Remove(GetAnimationData(clip));
        animations = animationDataList.ToArray();
    }

    public void AddClip(AnimationClip clip, string newName) {
        AddClip(clip, newName, 1, animator.wrapMode);
    }

    public void AddClip(AnimationClip clip, string newName, float speed, WrapMode wrapMode) {
        AddClip(clip, newName, speed, wrapMode, clip.length);
    }

    public void AddClip(AnimationClip clip, string newName, float speed, WrapMode wrapMode, float length)
    {
        if (GetAnimationData(newName) != null) Debug.LogWarning("An animation with the name '" + newName + "' already exists.");
        LegacyAnimationData animData = new LegacyAnimationData();
        animData.clip = (AnimationClip)Instantiate(clip);
        if (wrapMode == WrapMode.Default) wrapMode = animator.wrapMode;
        animData.clip.wrapMode = wrapMode;
        animData.clip.name = newName;
        animData.clipName = newName;
        animData.speed = speed;
        animData.originalSpeed = speed;
        animData.length = length;
        animData.wrapMode = wrapMode;

        List<LegacyAnimationData> animationDataList = new List<LegacyAnimationData>(animations);
        animationDataList.Add(animData);
        animations = animationDataList.ToArray();

        animator.AddClip(clip, newName);
        if (animator[newName] != null)
        {
            animator[newName].speed = speed;
            animator[newName].wrapMode = wrapMode;
        }

        foreach (AnimationState animState in animator)
        {
            if (animState.name == newName)
            {
                animData.animState = animState;
            }
        }
    }

    public LegacyAnimationData GetAnimationData(string clipName) {
        foreach (LegacyAnimationData animData in animations) {
            if (animData.clipName == clipName) {
                return animData;
            }
        }
        return null;
    }

    public LegacyAnimationData GetAnimationData(AnimationClip clip) {
        foreach (LegacyAnimationData animData in animations) {
            if (animData.clip == clip) {
                return animData;
            }
        }
        return null;
    }

    public bool IsPlaying(string clipName)
    {
        if (currentAnimationData == GetAnimationData(clipName)
            && currentAnimationData != null
            && currentAnimationData.wrapMode == WrapMode.ClampForever)
        {
            return true;
        }
        return (animator.IsPlaying(clipName));
    }

    public bool IsPlaying(LegacyAnimationData animData) {
        return (currentAnimationData == animData);
    }

    public void Play(string animationName, float blendingTime, float normalizedTime) {
        Play(GetAnimationData(animationName), blendingTime, normalizedTime);
    }

    public void Play(LegacyAnimationData animData, float blendingTime, float normalizedTime)
    {
        if (animData == null) return;

        if (currentAnimationData != null)
        {
            currentAnimationData.speed = currentAnimationData.originalSpeed;
            currentAnimationData.normalizedSpeed = 1;
        }

        currentAnimationData = animData;

        if (blendingTime == 0) {
            animator.Play(currentAnimationData.clipName);
        } else {
            animator.CrossFade(currentAnimationData.clipName, blendingTime);
        }

        currentAnimationData.animState.normalizedTime = normalizedTime;
        SetSpeed(currentAnimationData.speed);
        animator.Sample();
    }

    public void SetCurrentClipPosition(float normalizedTime) {
        SetCurrentClipPosition(normalizedTime, false);
    }

    public void SetCurrentClipPosition(float normalizedTime, bool pause) {
        normalizedTime = Mathf.Clamp01(normalizedTime);
        currentAnimationData.animState.normalizedTime = normalizedTime;
        if (pause) Pause();
    }

    public float GetCurrentClipPosition() {
        return currentAnimationData.animState.normalizedTime;
    }

    public Vector2 GetDeltaPosition() {
        Vector3 deltaPosition = transform.position - lastPosition;
        lastPosition = transform.position;
        return deltaPosition;
    }

    public void Stop() {
        animator.Stop();
    }

    public void Stop(string animName) {
        animator.Stop(animName);
    }

    public void Pause() {
        globalSpeed = 0;
    }

    public void SetSpeed(AnimationClip clip, float speed) {
        SetSpeed(GetAnimationData(clip), speed);
    }

    public void SetSpeed(string clipName, float speed) {
        SetSpeed(GetAnimationData(clipName), speed);
    }

    public void SetSpeed(LegacyAnimationData animData, float speed) {
        if (animData != null) {
            animData.speed = speed;
            animData.normalizedSpeed = speed / animData.originalSpeed;
            if (IsPlaying(animData)) SetSpeed(speed);
        }
    }

    public void SetSpeed(float speed) {
        globalSpeed = speed;

        if (!overrideAnimatorUpdate) {
			foreach(AnimationState animState in animator) {
                animState.speed = speed;
            }
        }
    }

    public void SetNormalizedSpeed(AnimationClip clip, float normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clip), normalizedSpeed);
    }

    public void SetNormalizedSpeed(string clipName, float normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clipName), normalizedSpeed);
    }

    public void SetNormalizedSpeed(LegacyAnimationData animData, float normalizedSpeed) {
        animData.normalizedSpeed = normalizedSpeed;
        animData.speed = animData.originalSpeed * animData.normalizedSpeed;
        if (IsPlaying(animData)) SetSpeed(animData.speed);
    }

    public float GetSpeed(AnimationClip clip) {
        return GetSpeed(GetAnimationData(clip));
    }

    public float GetSpeed(string clipName) {
        return GetSpeed(GetAnimationData(clipName));
    }

    public float GetSpeed(LegacyAnimationData animData) {
        return animData.speed;
    }

    public float GetSpeed() {
        return globalSpeed;
    }

    public float GetNormalizedSpeed(AnimationClip clip) {
        return GetNormalizedSpeed(GetAnimationData(clip));
    }

    public float GetNormalizedSpeed(string clipName) {
        return GetNormalizedSpeed(GetAnimationData(clipName));
    }

    public float GetNormalizedSpeed(LegacyAnimationData animData) {
        return animData.normalizedSpeed;
    }

    public void RestoreSpeed() {
        SetSpeed(currentAnimationData.speed);

        if (!overrideAnimatorUpdate) {
            foreach (AnimationState animState in animator) {
                animState.speed = GetAnimationData(animState.name).speed;
            }
        }
    }
}