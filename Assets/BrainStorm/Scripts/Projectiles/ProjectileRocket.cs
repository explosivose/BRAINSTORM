using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Projectile))]
public class ProjectileRocket : MonoBehaviour {

	public float acceleration;
	public float lifeTime;
	
	private TrailRenderer trail;

	void Start() {
		StartCoroutine( Initialize() );
	}
	
	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		StartCoroutine( Initialize() );
	}
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
		if (trail != null) trail.enabled = false;
	}
	
	IEnumerator Initialize() {
		rigidbody.isKinematic = false;
		yield return new WaitForSeconds(0.1f);
		trail = GetComponent<TrailRenderer>();
		if (trail != null) trail.enabled = true;
		yield return new WaitForSeconds(lifeTime);
		transform.Recycle();
	}
	
	void FixedUpdate() {
		rigidbody.AddForce(transform.forward * acceleration);
	}
}
