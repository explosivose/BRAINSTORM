using UnityEngine;
using System.Collections;


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
		Quaternion fireRotation = Quaternion.LookRotation(transform.forward);
		Transform i = rocketPrefab.Spawn(fireLocation, fireRotation);
		i.SendMessage("SetDamageSource", this.transform);
	}

}
