using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCPathFinder))]
[RequireComponent(typeof(NPCFaction))]
public class NPCCubit : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	
	private CharacterMaterials wardrobe = new CharacterMaterials();
	
	public Transform rocketPrefab;
	public float timeBetweenRockets;
	public float targetSearchRange;
	public float timeBetweenTargetSearches;
	
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
				_ren.material = wardrobe.dead;
				FactionManager.Instance.NPCDeath(_faction.team);
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
	
	public enum State {
		Idle, Advancing, Attacking, Dead, Calm
	}
	private State _state;
	private int _health;
	private NPCPathFinder _pathfinder;
	private NPCFaction _faction;
	private MeshRenderer _ren;
	private Transform _target;
	private bool _attacking; 
	private bool _hurt;
	
	public void Advance() {
		state = State.Advancing;
	}
	
	public void ChangeFaction() {
		if (_faction.team == NPCFaction.Faction.Pink) 
			wardrobe = _faction.pinkWardrobe;
		else 
			wardrobe = _faction.purpleWardrobe;
		
		_ren.material = wardrobe.normal;
	}
	
	void Awake () {
		ObjectPool.CreatePool(rocketPrefab);
		_pathfinder = GetComponent<NPCPathFinder>();
		_faction = GetComponent<NPCFaction>();
		_ren = GetComponentInChildren<MeshRenderer>();
		_health = stats.health;
		state = State.Idle;
	}
	
	void OnEnable() {
		tag = "NPC";
		_health = stats.health;
		rigidbody.useGravity = false;
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
		_pathfinder.destination = _faction.advancePosition + Vector3.up * 10f;
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
			StartCoroutine( Attack() );
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			AttackInit();
		}
	}
	
	IEnumerator Attack() {
		_attacking = true;
		_ren.material = wardrobe.attacking;
		FireRocket();
		yield return new WaitForSeconds(0.3f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(timeBetweenRockets);
		_attacking = false;
	}
	
	void FireRocket() {
		Vector3 fireLocation = transform.position + transform.forward * 3f;
		Quaternion fireRotation = Quaternion.LookRotation(transform.forward);
		Transform i = rocketPrefab.Spawn(fireLocation, fireRotation);
		i.SendMessage("SetDamageSource", this.transform);
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
						if (f.team != _faction.team) {
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
	
	void OnTriggerEnter(Collider col) {
		if (state == State.Calm) return;
		if (state == State.Dead) return;
		if (col.isTrigger) return;
		
		switch(col.tag) {
		case "NPC":
			NPCFaction f = col.GetComponent<NPCFaction>();
			if (f != null) {
				if (f.team != _faction.team) {
					// found opposite faction
					_target = col.transform;
					state = State.Attacking;
				}
			}
			else {
				// found NPC that doesn't belong to faction
				_target = col.transform;
				state = State.Attacking;
			}
			break;
		case "Player":
			_target = col.transform;
			state = State.Attacking;
			break;
		}
	}
	
	void OnTriggerExit(Collider col) {
		if (state == State.Calm) return;
		if (state == State.Dead) return;
		if (col.isTrigger) return;
		
		if ( _target != null ) {
			if ( col.gameObject == _target.gameObject ) {
				state = State.Advancing;
			}
		}
	}
	
	public void Damage(Projectile.DamageInstance damage) {
		if (state == State.Dead) return;
		if (damage.source == this.transform) return;
		NPCFaction f = damage.source.GetComponent<NPCFaction>();
		if (f != null) {
			if (f.team == _faction.team) return; // no friendly fire
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
		_ren.material = wardrobe.hurt;
		yield return new WaitForSeconds(0.1f);
		_ren.material = wardrobe.normal;
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
