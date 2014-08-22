using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Boid))]
[AddComponentMenu("Character/Virus/Zombie")]
public class NPCVirusZombie : MonoBehaviour {

	public enum State {
		Idle, Attacking, Dead
	}
	
	public CharacterStats stats = new CharacterStats();
	public CharacterMaterials wardrobe = new CharacterMaterials();
	public Boid.Profile idleProfile = new Boid.Profile();
	public Boid.Profile attackProfile = new Boid.Profile();
	public LayerMask targetSearchMask;
	public int damage;
	public float attackRate;
	public float targetSearchRange;
	public float timeBetweenTargetSearches;
	
	public State state {
		get { return _state; }
		set {
			_state = value;
			switch(_state) {
			case State.Idle:
			default:
				StartCoroutine(FindTarget());
				_boid.profile = idleProfile;
				// cheatsidoodle way to keep these NPCs from wandering off-scene
				_boid.SetTarget1(GameManager.Instance.transform);
				break;
			case State.Attacking:
				_boid.profile = attackProfile;
				_boid.SetTarget1(_attackTarget);
				break;
			case State.Dead:
				_boid.controlEnabled = false;
				_ren.material = wardrobe.dead;
				tag = "Untagged";
				rigidbody.useGravity = true;
				_attackTarget = null;
				StartCoroutine(Death ());
				break;
			}
		}
	}
	
	private int _health;
	private State _state;
	private Boid _boid;
	private MeshRenderer _ren;
	private bool _hurt = false;
	private Transform _attackTarget;
	private bool _attacking = false;
	private DamageInstance _damage = new DamageInstance();
	
	void Awake() {
		_boid = GetComponent<Boid>();
		_ren = GetComponentInChildren<MeshRenderer>();
		_damage.source = this.transform;
		_damage.damage = damage;
		_boid.defaultBehaviour = idleProfile;
	}
	
	void Start() {
		_boid.SetTarget1(GameManager.Instance.transform);
		state = State.Idle;
	}
	
	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		tag = "NPC";
		_health = stats.health;
		_attackTarget = null;
		_hurt = false;
		_attacking = false;
		
	}
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
		tag = "Untagged";
	}
	
	// Update is called once per frame
	void Update () {
		switch(_state) {
		case State.Idle:
		default:
			break;
		case State.Attacking:
			AttackUpdate();
			break;
		case State.Dead:
			break;
		}
	}
	
	void IdleUpdate(){
		
	}
	
	void AttackUpdate() {
		if (!_attackTarget) {
			state = State.Idle;
			return;
		}
		if (_attackTarget.tag == "Untagged") {
			state = State.Idle;
			return;
		}
		
		if (Vector3.Distance(transform.position, _attackTarget.position) < stats.attackRange) {
			if (!_attacking) StartCoroutine(AttackRoutine());
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;
		_attackTarget.SendMessage ("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(1f/attackRate);
		_attacking = false;
	}
	
	IEnumerator FindTarget() {
		while(state == State.Idle) {
			Collider[] colliders = Physics.OverlapSphere(transform.position, targetSearchRange, targetSearchMask);
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
			state = State.Idle;
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
		yield return new WaitForSeconds(2f);
		transform.Recycle();
	}
}
