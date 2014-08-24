using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Spider/Spider")]
[RequireComponent(typeof(NPCPathFinder))]
[RequireComponent(typeof(NPC))]
public class Spider : MonoBehaviour {

	public enum State {
		idle, stalking, attacking, dead
	}

	public CharacterStats stats = new CharacterStats();
	public float stalkDistance;
	
	public CharacterAudio sounds = new CharacterAudio();
	
	private bool _attacking;
	public bool attacking {
		get { return _attacking; }
		set {
			_attacking = value;
			if (_attacking) {
				_pathfinder.moveSpeedModifier = 0f;
			}
			else {
				_pathfinder.moveSpeedModifier = 1f;
			}
		}
	}
	
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
				
				_pathfinder.moveSpeedModifier = 3f;
				_pathfinder.rotationSpeedModifier = 2f;
				_pathfinder.stopDistance = stats.attackRange;
				break;
				
			case State.dead:
				Debug.Log ("Spider killed!");
				tag = "Untagged";
				audio.Stop();
				rigidbody.useGravity = true;
				rigidbody.freezeRotation = false;
				rigidbody.drag = 0.1f;
				collider.enabled = true;
				_target = null;
				break;
			}
		}
	}
	
	private NPCPathFinder _pathfinder;
	private GameObject _animator;
	private Transform _target;
	
	void Awake() {
		_pathfinder = GetComponent<NPCPathFinder>();
		_target = GameObject.FindGameObjectWithTag("Player").transform;
		_animator = transform.FindChild("Animator").gameObject;
	}
	
	IEnumerator Start() {
		state = State.idle;
		yield return new WaitForEndOfFrame();
		state = State.stalking;
	}
	
	// Use this for initialization
	void OnEnable () {
		if (GameManager.Instance.levelTeardown) {
			if (state == State.dead) {
				_animator.SetActive(false);
			}
		}
		tag = "NPC";
	}
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
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
		else {
			//if (attacking) BroadcastMessage("CancelAttack");
		}
		// if we're too far
		if (targetDistance > stalkDistance){
			state = State.stalking;
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			_pathfinder.destination = _target.position;
		}
	}
	
	void Damage(DamageInstance damage) {
		if (damage.source.tag == "Player") {
			if (state != State.dead) StartCoroutine( Death() );
		}
	}
	
	IEnumerator Death() {
		state = State.dead;
		yield return new WaitForSeconds(2f);
		_animator.SetActive(false);
		rigidbody.isKinematic = true;
		TerrorManager.Instance.StopTerror();
		
	}
}
