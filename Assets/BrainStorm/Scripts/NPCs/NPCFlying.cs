using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Flying")]
[RequireComponent(typeof(Rigidbody))]
public class NPCFlying : MonoBehaviour {

	public Vector3 destination {
		get { return _destination; }
		set { _destination = value; }
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
	public float defaultStopDistance; // don't move if destination is closer than this
	
	private float _moveSpeedMod = 1f;
	private float _rotSpeedMod = 1f;
	private Vector3 _destination = Vector3.zero;
	private bool _atDestination = false;
	private float _stopDistance = 0f;
	

	void Awake() {
		rigidbody.useGravity = false;
		rigidbody.freezeRotation = true;
		stopDistance = defaultStopDistance; 
	}
	
	void Update() {
		Quaternion rotation = Quaternion.LookRotation(destination - transform.position);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
		_atDestination = Vector3.Distance(transform.position, destination) < stopDistance;
	}
	
	void FixedUpdate () {
		Color lineColor = Color.green;
		if (!_atDestination) {
			float force = moveSpeed * rigidbody.mass * rigidbody.drag;
			rigidbody.AddForce(transform.forward * force);
			lineColor = Color.red;
		}
		if (drawDebug)
			Debug.DrawLine(transform.position, destination, lineColor);
	}

}
