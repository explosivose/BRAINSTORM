using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
	
	public class DamageInstance {
		public int damage;
		public Transform source;
	}
	
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
	
	void Start() {
		Initialise();
	}
	
	void OnEnable() {
		Initialise();
	}
	
	void Initialise() {
		_source = null;
	}
	
	void SetDamageSource(Transform source) {
		_source = source;
	}
}
