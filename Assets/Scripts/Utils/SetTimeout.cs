using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * anthor J
 * 
 */
public class SetTimeout : MonoBehaviour
{
    private static Dictionary<Action, SetTimeout> functionDict = new Dictionary<Action, SetTimeout>();

    public static void Start(Action function, float delay, bool isIgnoreTimeScale = true)
    {
        Clear(function);

        if (function == null)
        {
            return;
        }
        if (MainEntry.Instance == null)
        {
            return;
        }

        SetTimeout timeout = MainEntry.Instance.gameObject.AddComponent<SetTimeout>();
        timeout.Add(function, delay, isIgnoreTimeScale);
        functionDict.Add(function, timeout);
    }

    public static void Clear(Action function)
    {
        if (function == null)
        {
            return;
        }
        if (functionDict.ContainsKey(function))
        {
            SetTimeout timeout = functionDict[function];
            functionDict.Remove(function);

            timeout.fun = null;
            //DestroyImmediate(timeout);
            Destroy(timeout);
        }
    }

    //--------------------//
    private Action fun;

    public void Add(Action function, float delay, bool isIgnoreTimeScale = true)
    {
        fun = function;
        float delayTime = isIgnoreTimeScale == false ? delay * Time.timeScale : delay;
        Invoke("Execute", delayTime);
    }

    private void Execute()
    {
        if (fun != null)
        {
            fun.Invoke();
        }
        Clear(fun);
    }
}