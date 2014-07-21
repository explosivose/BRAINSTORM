using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCPathFinder))]
[RequireComponent(typeof(NPCFaction))]
public class NPCCubit : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();
	
	public Transform rocketPrefab;
	public float timeBetweenRockets;
	
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
			case State.Idle:
			case State.Calm:
			case State.Dead:
			default:
				break;
			}
		}
	}
	
	public enum State {
		Idle, Advancing, Attacking, Dead, Calm
	}
	private State _state;
	private NPCPathFinder _pathfinder;
	private NPCFaction _faction;
	private MeshRenderer _ren;
	private Transform _target;
	private bool _attacking; 
	private bool _hurt;
	
	public void Advance() {
		state = State.Advancing;
	}
	
	void Awake () {
		ObjectPool.CreatePool(rocketPrefab);
		_pathfinder = GetComponent<NPCPathFinder>();
		_faction = GetComponent<NPCFaction>();
		_ren = GetComponentInChildren<MeshRenderer>();
		wardrobe.normal = _ren.material;
		state = State.Idle;
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
	}

	void AttackInit() {
		_pathfinder.destination = _target.position;
		_pathfinder.stopDistance = stats.attackRange;
	}
	
	void AttackUpdate() {
		if (_target == null) {
			state = State.Advancing;
			return;
		}
		if (_target.tag == "Untagged") {
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
				_target = null;
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
		
		stats.health -= damage.damage;
		if (stats.health < 0) {
			Death();
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
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
	
	void Death() {
		_state = State.Dead;
		_ren.material = wardrobe.dead;
		rigidbody.useGravity = true;
		tag = "Untagged";
		FactionManager.Instance.NPCDeath(_faction.team);
		_pathfinder.enabled = false;
		_faction.enabled = false;
		this.enabled = false;
	}
}
