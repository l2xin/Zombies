using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Melee move script.
/// </summary>
public class MeleeMoveScript : MonoBehaviour 
{
	public Player myControlsScript;
	public string ownerTag;
	public BodyPart bodyPart;

	private BoxCollider weaponCollider;
	private Hit hit;
	private bool isHitSpace = false;

	void Awake()
	{
		weaponCollider = gameObject.GetComponent<BoxCollider> ();
		if (weaponCollider != null) {
			weaponCollider.enabled = false;
			weaponCollider.isTrigger = false;
		}
		isHitSpace = true;
	}

	public Hit Hit {
		get {
			return hit;
		}
		set {
			hit = value;
		}
	}

	public void DisableHit(bool isHitSpace)
	{
		if (this.isHitSpace != isHitSpace) {
			this.isHitSpace = isHitSpace;
			if (weaponCollider != null) {
				if (isHitSpace) {
					weaponCollider.enabled = false;
					weaponCollider.isTrigger = false;
				} else {
					weaponCollider.enabled = true;
					weaponCollider.isTrigger = true;
                }
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (ValidateHit(hit) == false)
		{
			return;
		}
		if (other.CompareTag(ownerTag) == false && 
			(other.CompareTag(FightManager.EnemyTag) || other.CompareTag(FightManager.PlayerTag)))
		{
			Player enemy = other.gameObject.GetComponent<Player>();
            if(enemy.isDead == false)
            {
                uint hpDec = (uint)hit.damageOnHit;
                enemy.GetHit(hit, hpDec, myControlsScript);
            }
		}
	}

	private bool ValidateHit(Hit hit)
	{
		return true;
	}
}
