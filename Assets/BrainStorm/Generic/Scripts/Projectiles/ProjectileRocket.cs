using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Projectile))]
public class ProjectileRocket : MonoBehaviour {

	public float acceleration;
	
	private TrailRenderer trail;

	void Start() {
		StartCoroutine( Initialize() );
	}
	
	void OnEnable() {
		StartCoroutine( Initialize() );
	}
	
	void OnDisable() {
		if (trail != null) trail.enabled = false;
	}
	
	IEnumerator Initialize() {
		yield return new WaitForSeconds(0.1f);
		trail = GetComponent<TrailRenderer>();
		if (trail != null) trail.enabled = true;
	}
	
	void FixedUpdate() {
		rigidbody.AddForce(transform.forward * acceleration);
	}
}
