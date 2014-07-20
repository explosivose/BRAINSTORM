using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Projectile))]
public class ProjectileSeeking : MonoBehaviour {

	public float acceleration;
	public float turnSpeed;

	private Transform _target;
	
	void SetTarget(Transform target) {
		_target = target;
	}
	
	void Start() {
		StartCoroutine( Initialize() );
	}
	
	void OnEnable() {
		StartCoroutine( Initialize() );
	}
	
	void OnDisable() {
		GetComponent<TrailRenderer>().enabled = false;
	}
	
	IEnumerator Initialize() {
		yield return new WaitForSeconds(0.1f);
		GetComponent<TrailRenderer>().enabled = true;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (_target != null && _target != this.transform) {
			Quaternion rotation = Quaternion.LookRotation(_target.position - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
		}
		rigidbody.AddForce(transform.forward * acceleration);
	}
}
