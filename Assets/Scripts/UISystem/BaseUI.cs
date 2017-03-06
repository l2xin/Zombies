using UnityEngine;
using System;
using System.Collections;

public class BaseUI : MonoBehaviour
{
    [HideInInspector]
    public UIType type = UIType.UI_NONE;
    protected object data;

    protected bool isPlayOpenSound = true;
    protected bool isPlayHideSound = true;
    public bool isNeedCache = false;

    void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    void Update()
    {
        OnUpdate();
    }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }

    public virtual void OnShow(object data = null)
    {
        this.data = data;
        this.transform.SetParent(UIManager.canvas != null ? UIManager.canvas.transform : null, false);
        gameObject.SetActive(true);
        FightManager.FireModule(true, type);
    }

    public virtual void OnHide()
    {
        gameObject.SetActive(false);
        FightManager.FireModule(false, type);
    }

    public virtual void Destroy()
    {
        GameObject.Destroy(gameObject);
    }

    public void SetSiblingIndex(int sibIndex)
    {
        transform.SetSiblingIndex(sibIndex);
    }
}
