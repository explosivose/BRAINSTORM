using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Faction/Packed Bit")]
[RequireComponent(typeof(NPCFaction))]
public class NPCPackedBit : MonoBehaviour {

	public float timeBetweenAttacks;
	public int damage;
	
	private NPCFaction _faction;
	private DamageInstance _dmg = new DamageInstance();
	
	void Awake () {
		_faction = GetComponent<NPCFaction>();
		_dmg.damage = damage;
		_dmg.source = this.transform;
	}
	
	void Attack() {
		if (!_faction.attacking) StartCoroutine( AttackRoutine() );
	}
	
	
	IEnumerator AttackRoutine() {
		_faction.attacking = true;
		_faction.target.SendMessage("Damage", _dmg, SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(0.3f);
		_faction.attacking = false;
		yield return new WaitForSeconds(timeBetweenAttacks);
	}
	



}
