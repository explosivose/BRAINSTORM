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
				StalkInit();
				break;
				
			case State.attacking:
				AttackInit();
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
			if (Time.time > 2f) state = State.stalking;
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
	
	void StalkInit() {
		_pathfinder.destination = _target.position;
		_pathfinder.moveSpeedModifier = 1f;
		_pathfinder.stopDistance = stalkDistance;
		_pathfinder.pathHeightOffset = walkHeight;
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
			StalkInit();
		}
	}
	
	void AttackInit() {
		_pathfinder.destination = _target.position;
		_pathfinder.moveSpeedModifier = 3f;
		_pathfinder.stopDistance = 0f;
		_pathfinder.pathHeightOffset = walkHeight/4f;
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
		if (_pathfinder.atDestination) {
			//ATTACK!
		}
		// if we're too far
		if (Vector3.Distance(transform.position, _target.position) > stalkDistance){
			state = State.stalking;
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			AttackInit();
		}
	}
}
