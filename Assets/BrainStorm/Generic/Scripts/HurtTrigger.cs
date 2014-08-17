using UnityEngine;
using System.Collections;

public class HurtTrigger : MonoBehaviour {

	public int damageOnEnter = 10;

	private DamageInstance _damage = new DamageInstance();

	void Start() {
		_damage.source = transform.parent;
		_damage.damage = damageOnEnter;
	}

	void OnTriggerEnter(Collider c) {
		if (!c.isTrigger)
			c.SendMessage("Damage", _damage, SendMessageOptions.DontRequireReceiver);
	}
}
