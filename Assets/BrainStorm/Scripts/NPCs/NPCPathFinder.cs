using UnityEngine;
using System.Collections;
using Pathfinding;

[AddComponentMenu("Character/PathFinder")]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Seeker))]
public class NPCPathFinder : MonoBehaviour {
	
	public Vector3 destination {
		get { return _destination; }
		set { 
			_destination = value; 
			_hasDestination = true;
			if (_pathUpdateTime + pathUpdateCooldown < Time.time) {
				_pathUpdateTime = Time.time;
				_seeker.StartPath(transform.position, _destination);
			}
		}
	}
	public bool atDestination {
		get { return _atDestination; }
	}
	public float stopDistance {
		get { return _stopDistance; }
		set { _stopDistance = value; }
	}
	public float moveSpeed {
		get { return maxMoveSpeed * _moveSpeedMod; }
	}
	public float moveSpeedModifier {
		get { return _moveSpeedMod; }
		set { _moveSpeedMod = value; }
	}
	public float rotationSpeed {
		get { return maxRotationSpeed * _rotSpeedMod; }
	}
	public float rotationSpeedModifier {
		get { return _rotSpeedMod; }
		set { _rotSpeedMod = value; }
	}
	
	public bool drawDebug = false;
	public float maxMoveSpeed;
	public float maxRotationSpeed;
	public float pathHeightOffset;	// for hovering NPCs
	public float defaultStopDistance; // don't move if destination is closer than this
	public float nextWaypointDistance = 2f;
	public float pathUpdateCooldown = 1f;
	
	private Seeker _seeker;
	private int _currentWaypoint;
	private Path _path;
	private float _pathUpdateTime = -999f;
	
	private float _moveSpeedMod = 1f;
	private float _rotSpeedMod = 1f;
	private Vector3 _destination = Vector3.zero;
	private bool _hasDestination = false;
	private bool _atDestination = false;
	private float _stopDistance = 0f;

	void Awake () {
		rigidbody.freezeRotation = true;
		stopDistance = defaultStopDistance; 
		_seeker = GetComponent<Seeker>();
	}
	
	void OnEnable() {
		_seeker.pathCallback += OnPathComplete;
		if (_hasDestination) {
			destination = _destination;
		}
	}
	
	void OnDisable() {
		// Abort calculation of path
		if (_seeker != null && !_seeker.IsDone()) _seeker.GetCurrentPath().Error();
		
		// Release current path
		//if (_path != null) _path.Release (this);
		//_path = null;
		
		//Make sure we receive callbacks when paths complete
		_seeker.pathCallback -= OnPathComplete;
	}
	
	// _seeker.StartPath() callback
	public void OnPathComplete(Path p) {
		if (!p.error) {
			_path = p;
			_currentWaypoint = 0;
		}
	}
	
	void FixedUpdate () {
		if (_path == null) {
			return; // we have no path to follow
		}
		if (_currentWaypoint >= _path.vectorPath.Count) {
			return; // we have reach the end of the path
		}
		
		_atDestination = Vector3.Distance(transform.position, destination) < stopDistance;
		
		Vector3 waypoint = _path.vectorPath[_currentWaypoint];
		//waypoint.y += pathHeightOffset + ((Random.value-0.5f) * pathHeightOffset);
		waypoint.y += pathHeightOffset;
		
		if (Vector3.Distance(transform.position, waypoint) < nextWaypointDistance) {
			_currentWaypoint++;
		}
		
		Color lineColor = Color.green;
		
		if (_atDestination) {
			Vector3 dir = (_destination - transform.position).normalized;
			Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
		}
		else {
			Vector3 dir = (waypoint - transform.position).normalized;
			Quaternion rotation = Quaternion.LookRotation(dir);
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
			
			float force = moveSpeed * rigidbody.mass * rigidbody.drag;
			rigidbody.AddForce(dir * force);
			lineColor = Color.red;
		}
		
		if (drawDebug)
			Debug.DrawLine(transform.position, destination, lineColor);
	}

}
