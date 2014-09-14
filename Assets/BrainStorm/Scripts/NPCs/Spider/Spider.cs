using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Spider/Spider")]
[RequireComponent(typeof(NPCPathFinder))]
public class Spider : NPC {

	public enum State {
		idle, stalking, attacking, dead
	}
	
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
				_pathfinder.destination = target.position;
				_pathfinder.moveSpeedModifier = 1f;
				_pathfinder.rotationSpeedModifier = 1f;
				_pathfinder.stopDistance = _search.minRange;
				break;
				
			case State.attacking:
				
				_pathfinder.moveSpeedModifier = 3f;
				_pathfinder.rotationSpeedModifier = 2f;
				_pathfinder.stopDistance = attackRange;
				break;
				
			case State.dead:
				Debug.Log ("Spider killed!");
				tag = "Untagged";
				audio.Stop();
				rigidbody.useGravity = true;
				rigidbody.freezeRotation = false;
				rigidbody.drag = 0.1f;
				rigidbody.angularDrag = 2f;
				collider.enabled = true;
				target = null;
				StartCoroutine( Death() );
				break;
			}
		}
	}
	
	private NPCPathFinder _pathfinder;
	private float _initPathHeightOffset;
	private GameObject _animator;
	private PrefabSpawner _artefactSpawner;
	
	protected override void Awake() {
		base.Awake();
		_pathfinder = GetComponent<NPCPathFinder>();
		_initPathHeightOffset = _pathfinder.pathHeightOffset;
		_animator = transform.FindChild("Animator").gameObject;
		_artefactSpawner = GetComponentInChildren<PrefabSpawner>();
	}
	
	IEnumerator Start() {
		state = State.idle;
		target = Player.Instance.transform;
		yield return new WaitForEndOfFrame();
		state = State.stalking;
	}
	
	// Use this for initialization
	protected override void OnEnable () {
		base.OnEnable();
		if (GameManager.Instance.levelTeardown) {
			if (state == State.dead) {
				_animator.SetActive(false);
			}
		}
	}
	
	protected override void OnDisable() {
		base.OnDisable();
		if (GameManager.Instance.levelTeardown) return;
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
		if (!hasTarget) {
			Debug.Log ("Spider lost target.");
			state = State.idle;
			return;
		}
		if (!targetIsOutOfRange) {
			state = State.attacking;
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, target.position) > 1f) {
			_pathfinder.destination = target.position;
		}
	}
	
	void AttackUpdate() {
		if (!hasTarget) {
			Debug.Log ("Spider lost target.");
			state = State.idle;
			return;
		}

		if (targetIsTooClose) {
			_pathfinder.moveSpeedModifier = 1f;
			// we're too close, back up
			if (target.position.y > transform.position.y) {
				_pathfinder.pathHeightOffset = 0f;
			}
			else {
				_pathfinder.pathHeightOffset = _initPathHeightOffset + 40f;
			}
		}
		else {
			_pathfinder.pathHeightOffset = _initPathHeightOffset;
		}

		if (targetIsInAttackRange) {
			BroadcastMessage("Attack", target.position);
		}
		else {
			if (attacking) BroadcastMessage("CancelAttack");
		}

		// if we're too far
		if (targetIsOutOfRange){
			state = State.stalking;
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, target.position) > 1f) {
			_pathfinder.destination = target.position;
		}
	}
	
	protected override void Damage(DamageInstance damage) {
		// only vulnerable while attacking
		if (!invulnerable) {
			if (damage.source.tag == "Player") {
				if (state != State.dead) state = State.dead;;
			}
		}
	}
	
	protected override void Killed(Transform victim) {
		Debug.Log ("Spider killed " + victim.name);
	}
	
	IEnumerator Death() {
		yield return new WaitForSeconds(2f);
		_animator.SetActive(false);
		rigidbody.isKinematic = true;
		GameManager.Instance.terrorComplete = true;
		_artefactSpawner.Spawn();
	}
}
