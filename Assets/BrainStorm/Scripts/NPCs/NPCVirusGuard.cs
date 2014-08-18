using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCPathFinder))]
public class NPCVirusGuard : MonoBehaviour {

	public enum State {
		Idle, Defending, Attacking, Dead
	}
	
	public CharacterStats stats = new CharacterStats();
	public float defendRange;
	public float targetSearchRange;
	public float timeBetweenTargetSearches;
	public Transform projectilePrefab;
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();
	
	private State _state;
	public State state {
		get { return _state; }
		set {
			_state = value;
			switch(_state) {
			case State.Defending:
				Vector3 destination = _defendTarget.position;
				destination += Random.insideUnitSphere * defendRange;
				_pathfinder.destination = destination;
				_pathfinder.stopDistance = _pathfinder.pathHeightOffset + 2f;
				StartCoroutine( FindTarget() );
				break;
				
			case State.Attacking:
				_pathfinder.destination = _attackTarget.position;
				_pathfinder.stopDistance = stats.attackRange;
				break;
				
			case State.Dead:
				StartCoroutine( Death() );
				break;
				
			case State.Idle:
			default:
				break;
				
			}
		}
	}
	
	private Transform _attackTarget;
	private Transform _defendTarget;
	private int _health;
	private NPCPathFinder _pathfinder;
	private MeshRenderer _ren;
	private bool _hurt;
	private bool _attacking;

	void Awake() {
		_pathfinder = GetComponent<NPCPathFinder>();
		_ren = GetComponentInChildren<MeshRenderer>();
		_health = stats.health;
		_state = State.Idle;
	}

	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		tag = "NPC";
		_health = stats.health;
		_attackTarget = null;
		_defendTarget = null;
		_attacking = false;
		_hurt = false;
	}
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
		tag = "Untagged";
	}

	void Defend(Transform target) {
		_defendTarget = target;
		state = State.Defending;
	}

	void Update () {
		switch(state) {
		case State.Defending:
			DefendUpdate();
			break;
			
		case State.Attacking:
			AttackUpdate();
			break;
			
		case State.Dead:
			break;
			
		case State.Idle:
		default:
			break;
			
		}
	}
	
	void DefendUpdate() {
		if (_pathfinder.atDestination) {
			state = State.Defending;
			return;
		}
	}
	
	void AttackUpdate() {
		if (_attackTarget == null) {
			state = State.Defending;
			return;
		}
		if (_attackTarget.tag == "Untagged") {
			state = State.Defending;
			return;
		}
		if (Vector3.Distance(transform.position, _defendTarget.position) > defendRange) {
			state = State.Defending;
		}
		if (_pathfinder.atDestination && !_attacking) {
			SendMessage("Attack");
		}
		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, _attackTarget.position) > 1f) {
			state = State.Attacking; // reinitialise (update pathfinder)
		}
	}
	
	IEnumerator FindTarget() {
		while(state == State.Defending) {
			
			Collider[] colliders = Physics.OverlapSphere(transform.position, targetSearchRange);
			foreach(Collider c in colliders) {
				if ( c.isTrigger ) continue;
				if ( c.transform == this.transform) continue;
				
				switch (c.tag) {
				//case "NPC":
				case "Player":
					_attackTarget = CompareTargets(_attackTarget, c.transform);
					break;
				default:
					break;
				}
			}
			
			if (_attackTarget != null) {
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
	
	IEnumerator Attack() {
		_attacking = true;
		_ren.material = wardrobe.attacking;
		FireProjectile();
		yield return new WaitForSeconds(0.05f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_attacking = false;
	}
	
	void FireProjectile() {
		float t = Random.value * 2 * Mathf.PI;
		Vector3 fireLocation = transform.position;
		fireLocation += transform.up * 1.5f * Mathf.Abs(Mathf.Sin(t));
		fireLocation += transform.right * 1.5f * Mathf.Cos(t);
		Quaternion fireRotation = Quaternion.LookRotation(fireLocation - transform.position);
		Transform i = projectilePrefab.Spawn(fireLocation, fireRotation);
		i.parent = GameManager.Instance.activeScene;
		i.SendMessage("SetTarget", _attackTarget);
		i.SendMessage("SetDamageSource", this.transform);
	}
	
	void Damage(DamageInstance damage) {
		if (state == State.Dead) return;
		if (damage.source == this.transform) return;
		
		_health -= damage.damage;
		if (_health <= 0) {
			damage.source.SendMessage("Killed", this.transform);
			state = State.Dead;
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
		}
	}
	
	void Killed(Transform victim) {
		if (victim == _attackTarget) {
			state = State.Defending;
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
		_ren.material = wardrobe.dead;
		tag = "Untagged";
		rigidbody.useGravity = true;
		_attackTarget = null;
		yield return new WaitForSeconds(2f);
		transform.Recycle();
	}
}
