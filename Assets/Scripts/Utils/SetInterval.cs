using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * anthor J
 * 
 */
public class SetInterval : MonoBehaviour
{
    private static Dictionary<Action, SetInterval> functionDict = new Dictionary<Action, SetInterval>();

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
		//---------------------------------------- hry 阻止反复Add
//		SetInterval interval = MainEntry.Instance.gameObject.GetComponent<SetInterval>();
//		if(interval == null){interval = MainEntry.Instance.gameObject.AddComponent<SetInterval>();}
		SetInterval interval = MainEntry.Instance.gameObject.AddComponent<SetInterval>();
		//----------------------------------------
        interval.Add(function, delay, isIgnoreTimeScale);

        functionDict.Add(function, interval);
    }

    public static void Clear(Action function)
    {
        if (function == null)
        {
            return;
        }

        if (functionDict.ContainsKey(function))
        {
            SetInterval interval = functionDict[function];
            functionDict.Remove(function);

            Destroy(interval);
            interval.fun = null;
        }
    }

    //--------------------//

    private Action fun;

    public void Add(Action function, float delay, bool isIgnoreTimeScale = true)
    {
        fun = function;
        float delayTime = isIgnoreTimeScale == false ? delay * Time.timeScale : delay;
        InvokeRepeating("Execute", delayTime, delayTime);
    }

    private void Execute()
    {
        if (fun != null)
        {
            fun.Invoke();
        }
    }
}