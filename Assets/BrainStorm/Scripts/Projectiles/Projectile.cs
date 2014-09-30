using UnityEngine;
using System.Collections;

public class Projectile : Photon.MonoBehaviour {
	

	public int damage;
	
	public DamageInstance Damage {
		get {
			DamageInstance me = new DamageInstance();
			me.damage = damage;
			me.viewId = 0;
			return me;
		}
	}
	
	private Transform _source;
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
		_source = null;
	}
	
	void SetDamageSource(Transform source) {
		_source = source;
	}
}
