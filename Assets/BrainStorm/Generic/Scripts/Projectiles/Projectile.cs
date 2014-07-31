using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	

	public int damage;
	
	public DamageInstance Damage {
		get {
			DamageInstance me = new DamageInstance();
			me.damage = damage;
			me.source = _source;
			return me;
		}
	}
	
	private Transform _source;
	
	void OnDisable() {
		_source = null;
	}
	
	void SetDamageSource(Transform source) {
		_source = source;
	}
}
