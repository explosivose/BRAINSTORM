using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Projectile))]
public class ProjectileSeeking : MonoBehaviour {

	public float acceleration;
	public float turnSpeed;
	public float lifeTime;
	
	private bool _targetSet = false;
	private Transform _target;
	private Vector3 _destination;
	private TrailRenderer trail;
	
	void SetTarget(Transform target) {
		_targetSet = true;
		_target = target;
	}
	
	void HitPosition(Vector3 position) {
		_destination = position;
	}
	
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
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
		_targetSet = false;
		_target = null;
	}
	
	IEnumerator Initialize() {
		yield return new WaitForSeconds(0.1f);
		trail = GetComponent<TrailRenderer>();
		if (trail != null) trail.enabled = true;
		yield return new WaitForSeconds(lifeTime);
		transform.Recycle();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (_target != null && _target != this.transform) {
			_destination = _target.position;
		}
		if (_targetSet) {
			Quaternion rotation = Quaternion.LookRotation(_destination - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
		}
		rigidbody.AddForce(transform.forward * acceleration);
	}
}
