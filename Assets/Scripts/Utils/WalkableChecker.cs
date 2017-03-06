using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// WalkableChecker
/// </summary>
public class WalkableChecker : MonoBehaviour
{
    public bool isOnTriggerEnter = false;
    public bool isInTriggerEnter = false;
    private bool isCenterTrigger = false;
    public List<bool> triggerEnterQueue;
    private readonly int maxtriggerEnterQueueLen = 2;

    public bool IsStopMoveImmediate
    {
        get
        {
            bool isStopMoveImmediate = triggerEnterQueue.Count >= maxtriggerEnterQueueLen
                && triggerEnterQueue[0] == false
                && triggerEnterQueue[1];
            return isStopMoveImmediate;
        }
    }

    public bool IsCenterTrigger
    {
        get
        {
            return isCenterTrigger;
        }
        set
        {
            isCenterTrigger = value;
            if(isCenterTrigger)
            {
                triggerEnterQueue = new List<bool>();
                TriggerExit();
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            isOnTriggerEnter = true;
            if (IsCenterTrigger)
            {
                triggerEnterQueue.Add(true);
                if (triggerEnterQueue.Count > maxtriggerEnterQueueLen)
                {
                    triggerEnterQueue.RemoveAt(0);
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            TriggerExit();
        }
    }

    private void TriggerExit()
    {
        isOnTriggerEnter = false;
        if (IsCenterTrigger)
        {
            triggerEnterQueue.Add(false);
            if (triggerEnterQueue.Count > maxtriggerEnterQueueLen)
            {
                triggerEnterQueue.RemoveAt(0);
            }
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Trigger"))
        {
            isOnTriggerEnter = true;
            if (IsCenterTrigger)
            {
                triggerEnterQueue.Add(true);
                if (triggerEnterQueue.Count > maxtriggerEnterQueueLen)
                {
                    triggerEnterQueue.RemoveAt(0);
                }
            }
        }
    }
}
