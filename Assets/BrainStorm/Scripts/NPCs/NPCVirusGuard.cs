using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Virus/Guard")]
public class NPCVirusGuard : NPC {

	public enum State {
		Idle, Defending, Attacking, Dead
	}
	
	public Transform	virusPrefab;
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();
	public Boid.Profile defendProfile = new Boid.Profile();
	public Boid.Profile attackProfile = new Boid.Profile();
	
	private State _state;
	public State state {
		get { return _state; }
		set {
			_state = value;
			switch(_state) {
			case State.Defending:
				target = null;
				_boid.profile = defendProfile;
				// cheatsidoodle way to keep these NPCs from wandering off-scene
				_boid.SetTarget2(_defendTarget);
				searchForTargets = true;
				break;
				
			case State.Attacking:
				searchForTargets = false;
				_boid.profile = attackProfile;
				_boid.SetTarget1(target);
				break;
				
			case State.Dead:
				searchForTargets = false;
				_boid.enabled = false;
				_ren.material = wardrobe.dead;
				tag = "Untagged";
				rigidbody.useGravity = true;
				target = null;
				StartCoroutine(Death ());
				break;
				
			case State.Idle:
			default:
				break;
				
			}
		}
	}
	
	private Transform _defendTarget;
	private Boid _boid;
	private MeshRenderer _ren;
	private bool _hurt;
	private bool _attacking;
	
	protected override void Awake() {
		base.Awake();
		ObjectPool.CreatePool(virusPrefab);
		_boid = GetComponentInChildren<Boid>();
		if (!_boid) Debug.LogError("BoidController missing");
		_ren = GetComponentInChildren<MeshRenderer>();
		_boid.defaultBehaviour = defendProfile;
	}

	protected override void OnEnable() {
		base.OnEnable();
		if (state == State.Dead) return;
		if (GameManager.Instance.levelTeardown) return;
		_hurt = false;
		_attacking = false;
		_boid.enabled = true;
		_boid.target1PositionOffset = Vector3.zero;
		
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

	}
	
	void AttackUpdate() {
		if (!hasTarget) {
			state = State.Defending;
			return;
		}
		if (Vector3.Distance(transform.position, _defendTarget.position) > targetSearch.farthestRange) {
			state = State.Defending;
		}
		if (targetIsInAttackRange && targetLOS && !_attacking) {
			SendMessage("AttackRoutine");
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;
		target.BroadcastMessage ("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		target.SendMessageUpwards("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(1f/attackRate);
		_attacking = false;
	}
	
	/* I want this function to be on one of the faction NPCs later...
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
	*/
	
	protected override void Damage(DamageInstance damage) {
		if (state == State.Dead) return;
		base.Damage(damage);
		if (isDead) {
			state = State.Dead;
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
		}
	}
	
	protected override void Killed(Transform victim) {
		Transform v = virusPrefab.Spawn(victim.position, victim.rotation);
		v.parent = GameManager.Instance.activeScene;
		if (victim == target) {
			state = State.Idle;
		}
	}
	
	IEnumerator Hurt() {
		_hurt = true;
		_ren.material = wardrobe.hurt;
		yield return new WaitForSeconds(0.1f);
		// if we didn't die during that last wait
		if (!isDead) 
			_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_hurt = false;
	}
	
	IEnumerator Death() {
		yield return new WaitForSeconds(2f);
		//transform.Recycle();
		//spawn corpse and recycle
	}
}
