using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
[RequireComponent(typeof(Rigidbody))]
public class Boid : MonoBehaviour {

	public bool drawDebug;
	public float moveSpeed;
	public float turnSpeed;

	public float seperation = 1f;
	public float alignmentWeight = 1f;
	public float cohesionWeight = 1f;
	public float targetWeight = 1f;
	
	private static int _boidCount = 0;
	private static int _updateIndex = 0;
	private int _myIndex;
	
	private List<Transform> _nearbyBoids = new List<Transform>();
	private Vector3 _boidDir;
	private Vector3 _separationDir;		// direction away from weighted (more nearby -> more weight) avg pos of nearbies
	private Vector3 _alignmentDir;		// avg velocity direction of nearbies
	private Vector3 _cohesionDir;		// direction toward avg pos of nearbies
	private Vector3 _targetDir;
	private Transform _target;
	
	void Start() {
		_myIndex = _boidCount++;
	}
	
	void SetTarget(Transform target ) {
		_target = target;
	}
	
	void Update() {
		if (drawDebug) {
			Debug.DrawRay(transform.position, _separationDir, Color.yellow);
			Debug.DrawRay(transform.position, _alignmentDir, Color.blue);
			Debug.DrawRay(transform.position, _cohesionDir, Color.magenta);
			Debug.DrawRay(transform.position, _boidDir, Color.white);
		}
		if (_updateIndex == _myIndex) {
			if (_nearbyBoids.Count > 2) {
				Calc();
			}
			else {
				_separationDir = transform.forward;
				_alignmentDir = transform.forward;
				_cohesionDir = transform.forward;
				if (_target!=null)
					_targetDir = (_target.position - transform.position).normalized;
			}
			if(++_updateIndex >= _boidCount) {
				_updateIndex = 0;
			}
			
		}
	}
	
	void FixedUpdate() {
		Quaternion rotation = Quaternion.LookRotation(_boidDir);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime);
		
		float force = rigidbody.drag * rigidbody.mass * moveSpeed;
		rigidbody.AddForce(transform.forward * force);
	}
	
	void Calc() {
		Vector3 avgPosition = Vector3.zero;
		Vector3 avgVelocity = Vector3.zero;
		Vector3 weightedAvgRepulsionDir = Vector3.zero;
		foreach (Transform b in _nearbyBoids) {
			avgPosition += b.position;
			avgVelocity += b.rigidbody.velocity;
			weightedAvgRepulsionDir += (transform.position - b.position).normalized * 
				seperation/Vector3.Distance(transform.position, b.position);
		}
		avgPosition /= _nearbyBoids.Count;
		avgVelocity /= _nearbyBoids.Count;
		weightedAvgRepulsionDir /= _nearbyBoids.Count;
		
		//Debug.DrawLine(transform.position, avgPosition, Color.cyan);
		_separationDir = weightedAvgRepulsionDir;
		_alignmentDir = avgVelocity.normalized;
		_cohesionDir = (avgPosition - transform.position).normalized;
			
		_boidDir = _separationDir +
				_alignmentDir * alignmentWeight +
				_cohesionDir * cohesionWeight;
				
		if (_target!=null)  {
			_targetDir = (_target.position - transform.position).normalized;
			_boidDir += _targetDir * targetWeight;
		}
		
	}
	
	void OnTriggerEnter(Collider c) {
		if (c.isTrigger) return;
		if (!_nearbyBoids.Contains(c.transform)) {
			_nearbyBoids.Add(c.transform);
			
			if (drawDebug)
				Debug.DrawLine(transform.position, c.transform.position, Color.green);
		}
	}
	
	void OnTriggerExit(Collider c) {
		if (c.isTrigger) return;
		
		_nearbyBoids.Remove(c.transform);
		
		if (drawDebug)
			Debug.DrawLine(transform.position, c.transform.position, Color.red);
	}

}
