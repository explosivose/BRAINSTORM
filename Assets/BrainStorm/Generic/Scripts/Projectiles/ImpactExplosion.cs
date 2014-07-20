﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class ImpactExplosion : MonoBehaviour {

	public Transform impactPrefab;
	public float impactRadius;
	public float explosionForceAtCenter; 
	
	private bool _impact = false;
	private Projectile _projectile;

	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(impactPrefab);
		_projectile = GetComponent<Projectile>();
	}
	
	void OnEnable() {
		_impact = false;
	}
	
	IEnumerator OnCollisionEnter(Collision col) {
		if (!_impact) {
			_impact = true;
			// spawn explosion effect
			Transform i = impactPrefab.Spawn(transform.position, transform.rotation);
			i.particleSystem.time = 0f;
			i.particleSystem.Play();
			
			// deal damage and physics forces to nearby objects
			Collider[] cols = Physics.OverlapSphere(transform.position, impactRadius);
			foreach(Collider c in cols) {
				if (c.rigidbody != null) {
					c.rigidbody.AddExplosionForce(explosionForceAtCenter, transform.position, impactRadius, 1f, ForceMode.Impulse);
				}
				c.SendMessage("Damage", _projectile.Damage, SendMessageOptions.DontRequireReceiver);
			}
			
			transform.Recycle();
			yield return new WaitForSeconds(i.particleSystem.duration);
			i.Recycle();
		}
	}
}
