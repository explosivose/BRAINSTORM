using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
[AddComponentMenu("Character/Boid")]
[RequireComponent(typeof(Rigidbody))]
public class Boid : MonoBehaviour {

	public enum BoidLayer {
		Boid, Boid2
	}
	
	public BoidLayer layer = BoidLayer.Boid;

	[System.Serializable]
	public class Profile {
		public float moveSpeed;
		public float turnSpeed;
		
		public float separation = 1f;
		public float alignmentWeight = 1f;
		public float cohesionWeight = 1f;
		public float target1Weight = 1f;
		public float target2Weight = 1f;
	}
	
	public bool drawDebug;
	public bool controlEnabled = true;
	public Profile defaultBehaviour = new Profile();
	
	
	
	public List<Transform> neighbours {
		get { return _nearbyBoids; }
	}
	public static int boidCount {
		get { return _boidCount; }
	}
	public Profile profile {
		get { return _profile; }
		set { _profile = value; }
	}
	public Vector3 boidDir {
		get { return _boidDir; }
	}
	public Vector3 target1PositionOffset {
		get { return _target1Off; }
		set { _target1Off = value; }
	}
	public Vector3 target2PositionOffset {
		get { return _target2Off; }
		set { _target2Off = value; }
	}
	
	// instead of having 1 and 2 maybe have child classes
	// bad boid and good boid?
	private static int _boidCount = 0;
	private static int _boid2Count = 0;
	
	private static int _updateIndex = 0;
	private static int _update2Index = 0;
	private int _myIndex;
	

	private List<Transform> _nearbyBoids = new List<Transform>();
	private Profile _profile;
	private Vector3 _boidDir;
	private Vector3 _separationDir;		// direction away from weighted (more nearby -> more weight) avg pos of nearbies
	private Vector3 _alignmentDir;		// avg velocity direction of nearbies
	private Vector3 _cohesionDir;		// direction toward avg pos of nearbies
	private Vector3 _target1Dir;
	private Vector3 _target2Dir;
	private Transform _target1;
	private Vector3 _target1Off = Vector3.zero;
	private Transform _target2;
	private Vector3 _target2Off = Vector3.zero;
	
	public void SetTarget1(Transform target) {
		_target1 = target;
	}
	
	public void SetTarget2(Transform target) {
		_target2 = target;
	}
	
	void Start() {
		
		switch (layer) {
		case BoidLayer.Boid:
			gameObject.layer = LayerMask.NameToLayer("Boid");
			_myIndex = _boidCount++;
			break;
		case BoidLayer.Boid2:
			gameObject.layer = LayerMask.NameToLayer("Boid2");
			_myIndex = _boid2Count++;
			break;
		}
		
		profile = defaultBehaviour;
	}
	
	void Update() {
		if (drawDebug) {
			Debug.DrawRay(transform.position, _separationDir, Color.yellow);
			Debug.DrawRay(transform.position, _alignmentDir, Color.blue);
			Debug.DrawRay(transform.position, _cohesionDir, Color.magenta);
			Debug.DrawRay(transform.position, _boidDir, Color.white);
		}
		
		switch (layer) {
		case BoidLayer.Boid:
			if (_updateIndex == _myIndex) {
				if (controlEnabled) 
					Calc();
				if(++_updateIndex >= _boidCount) 
					_updateIndex = 0;
			}
			break;
			
		case BoidLayer.Boid2:
			if (_update2Index == _myIndex) {
				if (controlEnabled) 
					Calc();
				if(++_update2Index >= _boid2Count) 
					_update2Index = 0;
			}
			break;
		}
	}
	
	void FixedUpdate() {
		if (!controlEnabled) return;
		Quaternion rotation = Quaternion.LookRotation(_boidDir);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, profile.turnSpeed * Time.deltaTime);
		
		float force = rigidbody.drag * rigidbody.mass * profile.moveSpeed;
		rigidbody.AddForce(transform.forward * force);
	}
	
	void Calc() {
		if (_nearbyBoids.Count > 0) {
			Vector3 avgPosition = Vector3.zero;
			Vector3 avgVelocity = Vector3.zero;
			Vector3 weightedAvgRepulsionDir = Vector3.zero;
			foreach (Transform b in _nearbyBoids) {
				avgPosition += b.position;
				avgVelocity += b.rigidbody.velocity;
				weightedAvgRepulsionDir += (transform.position - b.position).normalized * 
					profile.separation/Vector3.Distance(transform.position, b.position);
			}
			avgPosition /= _nearbyBoids.Count;
			avgVelocity /= _nearbyBoids.Count;
			weightedAvgRepulsionDir /= _nearbyBoids.Count;
			
			//Debug.DrawLine(transform.position, avgPosition, Color.cyan);
			_separationDir = weightedAvgRepulsionDir;
			_alignmentDir = avgVelocity.normalized;
			_cohesionDir = (avgPosition - transform.position).normalized;
		}
		else {
			_separationDir = transform.forward;
			_alignmentDir = transform.forward;
			_cohesionDir = transform.forward;
		}
		
		_boidDir = _separationDir +
			_alignmentDir * profile.alignmentWeight +
				_cohesionDir * profile.cohesionWeight;
				
		if (_target1!=null)  {
			_target1Dir = (_target1.position + _target1Off - transform.position).normalized;
			_boidDir += _target1Dir * profile.target1Weight;
		}
		
		if (_target2!=null) {
			_target2Dir = (_target2.position + _target2Off - transform.position).normalized;
			_boidDir += _target2Dir * profile.target2Weight;
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
