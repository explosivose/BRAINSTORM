using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCPathFinder))]
public class Spider : MonoBehaviour {

	public enum State {
		idle, stalking, attacking, dead
	}

	public CharacterStats stats = new CharacterStats();
	public float stalkDistance;
	
	public CharacterAudio sounds = new CharacterAudio();
	
	private State _state;
	public State state {
		get { return _state; }
		set {
			_state = value;
			switch(_state) {
			default:
			case State.idle:
				break;
				
			case State.stalking:
				_pathfinder.destination = _target.position;
				_pathfinder.moveSpeedModifier = 1f;
				_pathfinder.rotationSpeedModifier = 1f;
				_pathfinder.stopDistance = stalkDistance;
				break;
				
			case State.attacking:
				_pathfinder.destination = _target.position;
				_pathfinder.moveSpeedModifier = 3f;
				_pathfinder.rotationSpeedModifier = 2f;
				_pathfinder.stopDistance = stats.attackRange;
				break;
				
			case State.dead:
				tag = "Untagged";
				rigidbody.useGravity = true;
				_target = null;
				break;
			}
		}
	}
	
	private int _health;
	private float walkHeight;
	private NPCPathFinder _pathfinder;
	private Transform _target;
	
	void Awake() {
		_pathfinder = GetComponent<NPCPathFinder>();
		_target = GameObject.FindGameObjectWithTag("Player").transform;
		walkHeight = _pathfinder.pathHeightOffset;
	}
	
	IEnumerator Start() {
		yield return new WaitForSeconds(2f);
		state = State.stalking;
	}
	
	// Use this for initialization
	void OnEnable () {
		tag = "NPC";
		_health = stats.health;
		state = State.idle;
	}
	
	void OnDisable() {
		tag = "Untagged";
	}
	
	// Update is called once per frame
	void Update () {
		switch(_state) {
		default:
		case State.idle:
			break;
			
		case State.stalking:
			StalkUpdate();
			break;
			
		case State.attacking:
			AttackUpdate();
			break;
			
		case State.dead:
			break;
		}
	}
	
	void StalkUpdate() {
		if (_target == null) {
			Debug.Log ("target null");
			state = State.idle;
			return;
		}
		if (_target.tag == "Untagged") {
			Debug.Log ("target untagged");
			state = State.idle;
			return;
		}
		if (_pathfinder.atDestination) {
			state = State.attacking;
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			state = State.stalking;
		}
	}
	
	void AttackUpdate() {
		if (_target == null) {
			Debug.Log ("target null");
			state = State.idle;
			return;
		}
		if (_target.tag == "Untagged") {
			Debug.Log ("target untagged");
			state = State.idle;
			return;
		}

		float targetDistance = Vector3.Distance(transform.position, _target.position);

		if (_pathfinder.atDestination) {
			BroadcastMessage("Attack", _target.position);
		}
		// if we're too far
		if (targetDistance > stalkDistance){
			state = State.stalking;
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			state = State.attacking;
		}
	}
}
