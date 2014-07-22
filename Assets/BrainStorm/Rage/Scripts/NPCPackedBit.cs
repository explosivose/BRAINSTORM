using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCPathFinder))]
[RequireComponent(typeof(NPCFaction))]
public class NPCPackedBit : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	
	private CharacterMaterials wardrobe = new CharacterMaterials();

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
	private int _health;
	private NPCPathFinder _pathfinder;
	private NPCFaction _faction;
	private MeshRenderer _ren;
	private Transform _target;
	private bool _attacking; 
	private bool _hurt;
	private float _calmTimer;
	
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
		_pathfinder = GetComponent<NPCPathFinder>();
		_faction = GetComponent<NPCFaction>();
		_ren = GetComponentInChildren<MeshRenderer>();
		_health = stats.health;
		state = State.Advancing;
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
		case State.Calm:
			CalmUpdate();
			break;
		case State.Idle:
		case State.Dead:
		default:
			break;
		}
	}
	
	void AdvancingInit() {
		_pathfinder.destination = _faction.advancePosition;
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
		// if target changes position, update destination
		if (Vector3.Distance(_pathfinder.destination, _target.position) > 1f) {
			AttackInit();
		}
	}
	
	IEnumerator Attack() {
		_attacking = true;
		_ren.material = wardrobe.attacking;
		yield return new WaitForSeconds(0.1f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_attacking = false;
	}
	
	void CalmUpdate() {
		_calmTimer -= Time.deltaTime;
		if (_calmTimer < 0f) {
			Vector3 destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
			_calmTimer = 20f;
			_pathfinder.destination = destination;
			_pathfinder.stopDistance = _pathfinder.defaultStopDistance;
			_pathfinder.moveSpeedModifier = 0.5f;
		}
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
		if (damage.source == this.transform) return; // dont shoot yourself
		NPCFaction f = damage.source.GetComponent<NPCFaction>();
		if (f != null) {
			if (f.team == _faction.team) return; // no friendly fire
		}
		
		_health -= damage.damage;
		if (_health <= 0) {
			damage.source.SendMessage("Killed", this.transform);
			StartCoroutine( Death() );
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
		}
	}
	
	public void Killed(Transform victim) {
		if (victim == _target) {
			_target = null;
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
		FactionManager.Instance.NPCDeath(_faction.team);
		tag = "Untagged";
		_state = State.Dead;
		_ren.material = wardrobe.dead;
		yield return new WaitForSeconds(2f);
		
		transform.Recycle();
		/*  
		maybe spawn a dead body prefab here instead
		all this state changing is a lot of work and you might not catch everything... i.e. triggers and stuff
		also see OnEnable, OnDisable
		_pathfinder.enabled = false;
		_faction.enabled = false;
		this.enabled = false;
		*/
	}
}
