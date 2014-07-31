using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCPathFinder))]
public class NPCFaction : MonoBehaviour {

	public enum Faction {
		Pink, Purple
	}
	public enum State {
		Idle, Advancing, Attacking, Dead, Calm
	}
	
	public CharacterStats stats = new CharacterStats();
	public float targetSearchRange;
	public float timeBetweenTargetSearches;
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials pinkWardrobe = new CharacterMaterials();
	public CharacterMaterials purpleWardrobe = new CharacterMaterials();
	
	private Vector3 _advancePosition;
	public Vector3 advancePosition {
		get { return _advancePosition;  }
		set { 
			_advancePosition = value; 
			state = State.Advancing;
		}
	}
	
	private Faction _team;
	public Faction team {
		get { return _team; }
		set { 
			_team = value;
			if (_team == NPCFaction.Faction.Pink) 
				_wardrobe = pinkWardrobe;
			else 
				_wardrobe = purpleWardrobe;
			
			_ren.material = _wardrobe.normal;
		}
	}
	
	private State _state;
	public State state {
		get { return _state; }
		set {
			_state = value;
			switch(_state) {
			case State.Advancing:
				AdvancingInit();
				break;
			case State.Attacking:
				AttackInit();
				break;
				
			case State.Dead:
				_ren.material = _wardrobe.dead;
				FactionManager.Instance.NPCDeath(team);
				tag = "Untagged";
				rigidbody.useGravity = true;
				_target = null;
				break;
			case State.Idle:
			case State.Calm:
			default:
				break;
			}
		}
	}
	
	private Transform _target;
	public Transform target {
		get { return _target; }
	}
	
	private bool _attacking; 
	public bool attacking {
		get { return _attacking; }
		set { 
			_attacking = value;
			if (_attacking) {
				_ren.material = _wardrobe.attacking;
			}
			else {
				_ren.material = _wardrobe.normal;
			}
		}
	}

	
	private int _health;
	private NPCPathFinder _pathfinder;
	private CharacterMaterials _wardrobe = new CharacterMaterials();
	private MeshRenderer _ren;
	
	
	private bool _hurt;
	
	void Awake() {
		_pathfinder = GetComponent<NPCPathFinder>();
		_ren = GetComponentInChildren<MeshRenderer>();
		_health = stats.health;
		state = State.Idle;
	}
	
	void OnEnable() {
		tag = "NPC";
		_health = stats.health;
		_target = null;
		_attacking = false;
		_hurt = false;
	}
	
	void OnDisable() {
		tag = "Untagged";
	}
	
	void Update () {
		switch(state) {
		case State.Advancing:
			break;
		case State.Attacking:
			AttackUpdate();
			break;
		case State.Idle:
		case State.Calm:
		case State.Dead:
		default:
			break;
		}
	}
	
	void AdvancingInit() {
		_pathfinder.destination = advancePosition;/// + Vector3.up * 10f;
		_pathfinder.stopDistance = 0f;
		_target = null;
		StartCoroutine( FindTarget() );
	}
	
	void AttackInit() {
		_pathfinder.destination = _target.position;
		_pathfinder.stopDistance = stats.attackRange;
	}
	
	void AttackUpdate() {
		if (_target == null) {
			Debug.Log ("target null");
			state = State.Advancing;
			return;
		}
		if (_target.tag == "Untagged") {
			Debug.Log ("target untagged");
			state = State.Advancing;
			return;
		}
		if (_pathfinder.atDestination && !_attacking) {
			SendMessage("Attack");
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			AttackInit();
		}
	}
	
	IEnumerator FindTarget() {
		while(state == State.Advancing) {
			
			Collider[] colliders = Physics.OverlapSphere(transform.position, targetSearchRange);
			foreach(Collider c in colliders) {
				if (c.isTrigger) continue;
				
				switch(c.tag) {
				case "NPC":
					NPCFaction f = c.GetComponent<NPCFaction>();
					if (f != null) {
						if (f.team != team) {
							// found opposite faction
							_target = CompareTargets(_target, c.transform);
						}
					}
					else {
						// found NPC that doesn't belong to faction
						_target = CompareTargets(_target, c.transform);
					}
					break;
				case "Player":
					_target = CompareTargets(_target, c.transform);
					break;
				}
				
			}
			
			if (_target != null) {
				state = State.Attacking;
			}
			yield return new WaitForSeconds(timeBetweenTargetSearches);
		}
	}
	
	Transform CompareTargets(Transform target1, Transform target2) {
		if (target1 == target2) return target1;
		if (target1 == null) return target2; // target2 won't be null because of previous line
		if (target2 == null) return target1;
		
		// choose closest target
		// at a later date could implement LOS priority
		float d1 = Vector3.Distance(transform.position, target1.position);
		float d2 = Vector3.Distance(transform.position, target2.position);
		if (d1 < d2) return target1;
		else return target2;
	}
	
	public void Damage(DamageInstance damage) {
		if (state == State.Dead) return;
		if (damage.source == this.transform) return;
		NPCFaction f = damage.source.GetComponent<NPCFaction>();
		if (f != null) {
			if (f.team == team) return; // no friendly fire
		}
		
		_health -= damage.damage;
		if (_health <= 0) {
			damage.source.SendMessage("Killed", this.transform);
			StartCoroutine(Death());
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
		}
	}
	
	public void Killed(Transform victim) {
		if (victim == _target) {
			state = State.Advancing;
		}
	}
	
	IEnumerator Hurt() {
		_hurt = true;
		_ren.material = _wardrobe.hurt;
		yield return new WaitForSeconds(0.1f);
		_ren.material = _wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_hurt = false;
	}
	
	IEnumerator Death() {
		
		_state = State.Dead;
		
		yield return new WaitForSeconds(2f);
		
		transform.Recycle();
		/*
		rigidbody.useGravity = true;
		_pathfinder.enabled = false;
		_faction.enabled = false;
		this.enabled = false;
		*/
	}
}
