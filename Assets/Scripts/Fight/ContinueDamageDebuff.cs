using UnityEngine;
using System.Collections;
/// <summary>
/// 持续伤害debuff
/// </summary>
public class DebuffView : MonoBehaviour
{
    private Player player;
    private Debuff debuff;

    void Awake()
    {
        player = gameObject.GetComponent<Player>();
    }

    public void AddDebuff(Debuff debuff)
    {
        this.debuff = debuff;
        this.debuff.param.OnValueChange += Param_OnValueChange;
        StartDebuff();
    }

    private void StartDebuff()
    {      
        switch (this.debuff.type)
        {
            case DebuffType.continueDamage:
                SetInterval.Start(ContinueDamage, 0.5f);
                break;
            case DebuffType.freeze:
                break;
            case DebuffType.slowDown:
                break;
            default:
                break;
        }
        SetTimeout.Start(EndDebuff, this.debuff.param.Value);
    }

    private void Param_OnValueChange()
    {
        StartDebuff();
    }

    public void EndDebuff()
    {
        SetInterval.Clear(ContinueDamage);
        SetTimeout.Clear(EndDebuff);
        this.debuff.param.OnValueChange -= Param_OnValueChange;
    }

    private void ContinueDamage()
    {
        player.GetHit(null, 1f, null);
    }

    void OnDestroy()
    {
        EndDebuff();
        player = null;
    }
}
