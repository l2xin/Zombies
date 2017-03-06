using UnityEngine;
using System.Collections;

public class SceneItem : MonoBehaviour 
{
	private string ownerTag = "";
	public WeaponType weaponType = WeaponType.None;
	//0 - 99999 武器技能
	//-1 - -99999 其他技能，比如召唤类等技能
	public int weaponIndex = -1;
	public float angleSpeed = 30;
	public float deleyDestroyTime = 3f; 

	void Awake()
	{
		ownerTag = FightManager.EnemyTag;
		SelfRotate selfRotate = gameObject.AddComponent<SelfRotate> ();
		selfRotate.AngleSpeed.y = angleSpeed;
	}

    void Start()
    {
        SetTimeout.Start(AutoDestroyItem, deleyDestroyTime);
    }

	private void AutoDestroyItem()
	{
		SetTimeout.Clear (AutoDestroyItem);
		GameObject.Destroy (this);
		PoolUtil.Despawner(gameObject, PoolUtil.particlesPoolName, true);
		weaponType = WeaponType.None;
		weaponIndex = -1;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(ownerTag) == false && 
			(other.CompareTag(FightManager.EnemyTag) || other.CompareTag(FightManager.PlayerTag)))
		{
			Player enemy = other.gameObject.GetComponent<Player>();
			enemy.Physics.UpdateWeapon (weaponType, weaponIndex);
			AutoDestroyItem ();
		}
	}
}
