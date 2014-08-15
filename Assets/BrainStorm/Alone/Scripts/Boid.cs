using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Boid : MonoBehaviour {

	public float moveSpeed;
	public float turnSpeed;

	public float boidSightDistance = 10f;
	public float boidListUpdateTime = 2f;
	
	public float seperationWeight = 1f;
	public float alignmentWeight = 1f;
	public float cohesionWeight = 1f;
	public float targetWeight = 1f;
	
	private List<Transform> _nearbyBoids = new List<Transform>();
	private Vector3 _separationDir;		// direction away from weighted (more nearby -> more weight) avg pos of nearbies
	private Vector3 _alignmentDir;		// avg velocity direction of nearbies
	private Vector3 _cohesionDir;		// direction toward avg pos of nearbies
	private Transform _target;
	
	void Start() {
		StartCoroutine( FindNearbyBoids() );
	}
	
	void SetTarget(Transform target ) {
		_target = target;
	}
	
	void FixedUpdate() {
		Debug.DrawRay(transform.position, _separationDir, Color.yellow);
		Debug.DrawRay(transform.position, _alignmentDir, Color.blue);
		Debug.DrawRay(transform.position, _cohesionDir, Color.magenta);
		
		Vector3 direction = _separationDir * seperationWeight +
							_alignmentDir * alignmentWeight +
							_cohesionDir * cohesionWeight;
		if (_target!=null)
			direction += (_target.position - transform.position).normalized * targetWeight;
		
		Debug.DrawRay(transform.position, direction, Color.white);
		
		Quaternion rotation = Quaternion.LookRotation(direction);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
		
		float force = rigidbody.drag * rigidbody.mass * moveSpeed;
		rigidbody.AddForce(transform.forward * force);
	}
	
	IEnumerator FindNearbyBoids() {
		while(true) {
			
			if (_nearbyBoids.Count > 2) {
				Calc();
			}
			else {
				_separationDir = transform.forward;
				_alignmentDir = transform.forward;
				_cohesionDir = transform.forward;
			}
			float wait = (boidListUpdateTime/2f) + (boidListUpdateTime*Random.value);
			yield return new WaitForSeconds(wait);
		}
	}
	
	void Calc() {
		Vector3 avgPosition = Vector3.zero;
		Vector3 avgVelocity = Vector3.zero;
		Vector3 weightedAvgPosition = Vector3.zero;
		foreach (Transform b in _nearbyBoids) {
			avgPosition += b.position;
			avgVelocity += b.rigidbody.velocity;
			weightedAvgPosition += b.position * Vector3.Distance(transform.position, b.position);
		}
		avgPosition /= _nearbyBoids.Count;
		avgVelocity /= _nearbyBoids.Count;
		weightedAvgPosition /= _nearbyBoids.Count;
		
		_separationDir = (transform.position - weightedAvgPosition).normalized;
		_alignmentDir = avgVelocity.normalized;
		_cohesionDir = (avgPosition - transform.position).normalized;
	}
	
	void OnTriggerEnter(Collider c) {
		if (c.tag == "Boid") {
			if (!_nearbyBoids.Contains(c.transform)) {
				_nearbyBoids.Add(c.transform);
				Debug.DrawLine(transform.position, c.transform.position, Color.green, 0.3f);
			}
		}
	}
	
	void OnTriggerExit(Collider c) {
		if (c.tag == "Boid") {
			_nearbyBoids.Remove(c.transform);
			Debug.DrawLine(transform.position, c.transform.position, Color.red, 0.3f);
		}
	}

}
