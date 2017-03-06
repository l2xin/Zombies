using UnityEngine;
using System.Collections;
/// <summary>
/// 常用战斗枚举
/// </summary>
public enum UFEStatus
{
    camp,
    attackAct,
    knife,
    boom,
    death,
    speed,
    atkPoints,
}

public enum BuffType
{
    speed = 1,
    attack,
    life,
    hitKnockBack
}

public enum UFEMapUnit
{
    none = 0,
    player = 1,
    monster = 2,
    boom = 3,
    tuteng = 4,
    boss = 5,
}

public enum MapUnitLayer
{
    player = 9,
    monster = 15,
}

public enum DebuffType
{
    none,
    continueDamage,
    freeze,
    slowDown,
}

public enum UFECareer
{
    Warrior = 1000,
    Mage = 2000,
    Shaman = 3000,
    Archer = 4000,
    Shield = 5000,
}

public enum SyncHpType
{
    normal,
    lvUp,
    dead,
    addBlood,
}

public enum SkinEnum
{
    gaoda= 1000,
    bear=1001,
}

