using UnityEngine;
using System.Collections;
/// <summary>
/// 弹道
/// </summary>
public class ProjectileMovementUtil
{
    public static float frequency = 1;
    public static float amplitude = 100 * FightManager.MAP_SCALE;
    public static float foldLineLen = 150.0f * FightManager.MAP_SCALE;    // 折线每次走150.0拐弯
    public static float circleRadious = 500.0f * FightManager.MAP_SCALE;  // 旋转半径

    public enum ThroughType
    {
        collider,
        through,
        bounce,
    }

    public enum MovementType
    {
        line = 1,   // 直线
        circle,     // 圆形旋转
        boomerang,  // 来回
        sinLine,    // 正弦
        foldLine,   // 折线
        parabola,   // 抛物线
    }
}