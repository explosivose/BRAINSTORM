using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Faction/Cubit")]
[RequireComponent(typeof(NPCFaction))]
public class NPCCubit : MonoBehaviour {

	public Transform rocketPrefab;
	public float timeBetweenRockets;
	
	private NPCFaction _faction;
	
	
	void Awake () {
		ObjectPool.CreatePool(rocketPrefab);

		_faction = GetComponent<NPCFaction>();

	}

	void Attack() {
		if (!_faction.attacking) StartCoroutine( AttackRoutine() );
	}

	
	IEnumerator AttackRoutine() {
		_faction.attacking = true;
		FireRocket();
		yield return new WaitForSeconds(0.3f);
		_faction.attacking = false;
		yield return new WaitForSeconds(timeBetweenRockets);
	}
	
	void FireRocket() {
		Vector3 fireLocation = transform.position + transform.forward * 3f;
		Vector3 fireDirection = _faction.target.position - transform.position;
		Quaternion fireRotation = Quaternion.LookRotation(fireDirection);
		Transform i = rocketPrefab.Spawn(fireLocation, fireRotation);
		i.parent = GameManager.Instance.activeScene.instance;
		i.SendMessage("SetDamageSource", this.transform);
	}

}
