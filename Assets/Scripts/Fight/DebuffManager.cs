using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// debuff
/// </summary>
public class DebuffManager : SingletonInstance<DebuffManager>
{
    public void AddDebuff(DebuffType debuffType, float param, CharacterInfo info)
    {
        List<Debuff> debuffList = info.debuffList;
        Debuff debuff = null;
        for (int i = 0; i < debuffList.Count; i++)
        {
            Debuff aleardyDebuff = debuffList[i];
            if (aleardyDebuff.type == debuffType)
            {
                debuff = aleardyDebuff;
                break;
            }
        }
        if (debuff == null)
        {
            debuff = new Debuff();
            debuff.type = debuffType;
            debuff.param = new Gamelogic.Extensions.ObservedValue<float>(param);
            debuffList.Add(debuff);
        }
        else
        {
            debuff.param.Value = param;
        }
        if (debuffType == DebuffType.freeze)
        {
            info.freezeDebuff = true;
        }
    }

    public void RemoveDebuff(DebuffType debuffType, CharacterInfo info)
    {
        List<Debuff> debuffList = info.debuffList;
        for (int i = 0; i < debuffList.Count; i++)
        {
            Debuff debuff = debuffList[i];
            if(debuff.type == debuffType)
            {
                debuffList.Remove(debuff);
                if (debuffType == DebuffType.freeze)
                {
                    info.freezeDebuff = false;
                }
                break;
            }
        }
    }

    public float CalculateSpeed(CharacterInfo info)
    {
        for (int i = 0; i < info.debuffList.Count; i++)
        {
            Debuff debuff = info.debuffList[i];
            if (debuff.type == DebuffType.slowDown)
            {
                return info.speed * debuff.param.Value;
            }
        }
        return info.speed;
    }
}
