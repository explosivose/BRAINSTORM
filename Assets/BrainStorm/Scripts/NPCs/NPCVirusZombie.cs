using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Virus/Zombie")]
public class NPCVirusZombie : NPC {

	public enum State {
		Idle, Stalking, Attacking, Dead
	}
	
	public Transform	virusPrefab;
	public CharacterMaterials wardrobe = new CharacterMaterials();
	public Boid.Profile idleProfile = new Boid.Profile();
	public Boid.Profile stalkProfile = new Boid.Profile();
	public Boid.Profile attackProfile = new Boid.Profile();
	
	public State state {
		get { return _state; }
		private set {
			_state = value;
			switch(_state) {
			case State.Idle:
			default:
				target = null;
				_boid.profile = idleProfile;
				// cheatsidoodle way to keep these NPCs from wandering off-scene
				_boid.SetTarget1(GameManager.Instance.transform);
				searchForTargets = true;
				audio.pitch = _sf;
				break;
				
			case State.Stalking:
				searchForTargets = true;
				_boid.profile = stalkProfile;
				_boid.SetTarget1(target);
				StartCoroutine(StalkRoutine());
				break;
				
			case State.Attacking:
				searchForTargets = false;
				_boid.profile = attackProfile;
				_boid.SetTarget1(target);
				audio.pitch = _sf * 12f;
				break;
				
			case State.Dead:
				searchForTargets = false;
				_boid.controlEnabled = false;
				_ren.material = wardrobe.dead;
				tag = "Untagged";
				rigidbody.useGravity = true;
				target = null;
				StartCoroutine(Death ());
				break;
			}
		}
	}
	
	private State _state;
	private Boid _boid;
	private MeshRenderer _ren;
	private bool _hurt = false;
	private bool _attacking = false;
	private float _sf;
	
	protected override void Awake() {
		base.Awake();
		ObjectPool.CreatePool(virusPrefab);
		_boid = GetComponentInChildren<Boid>();
		if (!_boid) Debug.LogError("BoidController missing");
		_ren = GetComponentInChildren<MeshRenderer>();
		_boid.defaultBehaviour = idleProfile;
		_sf = (Random.value/2f) + 0.5f;
		transform.localScale *= _sf;
		audio.pitch = _sf;
		audio.timeSamples = Random.Range(0, audio.clip.samples);
	}
	
	void Start() {
		_boid.SetTarget1(GameManager.Instance.transform);
		state = State.Idle;
	}
	
	protected override void OnEnable() {
		base.OnEnable();
		if (state == State.Dead) return;
		if (GameManager.Instance.levelTeardown) return;
		_hurt = false;
		_attacking = false;
		audio.Play();
		_boid.target1PositionOffset = Vector3.zero;
		
	}
	
	// Update is called once per frame
	void Update () {
		switch(state) {
		case State.Idle:
			IdleUpdate();
			break;
		case State.Stalking:
			StalkUpdate();
			break;
		case State.Attacking:
			AttackUpdate();
			break;
		}
	}
	
	void IdleUpdate() {
		if (!hasTarget) {
			return;
		}
		
		if(drawDebug)
			Debug.DrawLine(transform.position, target.position, Color.grey);
			
		if (targetLOS) {
			state = State.Stalking;
		}
	}
	
	
	// wobble closer to target until we're close enough
	void StalkUpdate() {
		if (!hasTarget) {
			state = State.Idle;
			return;
		}
		if (targetIsOutOfRange) {
			state = State.Idle;
			return;
		}
		if (!targetLOS) {
			state = State.Idle;
			return;
		}
		
		if(drawDebug)	Debug.DrawLine(transform.position, target.position, Color.yellow);

		if (targetIsInAttackRange) {
			state = State.Attacking;
		}
	}
	
	IEnumerator StalkRoutine() {
		float time = 1f;
		while(state == State.Stalking) {
			_boid.target1PositionOffset = transform.up * 10f;
			yield return new WaitForSeconds(time);
			_boid.target1PositionOffset = transform.right * 10f;
			yield return new WaitForSeconds(time);
			_boid.target1PositionOffset = -transform.up * 10f;
			yield return new WaitForSeconds(time);
			_boid.target1PositionOffset = -transform.right * 10f;
			yield return new WaitForSeconds(time);
		}
		_boid.target1PositionOffset = Vector3.zero;
	}
	
	// figure out whether can attack
	// called every frame while state == attacking
	void AttackUpdate() {
		
		if (drawDebug) 	Debug.DrawLine(transform.position, target.position, Color.red);
		
		// we're already attacking, return;
		if (_attacking) return;	
		
		// target lost, change state.
		if (!hasTarget) {
			state = State.Idle;
			return;
		}
		
		// target moved too far away, go back to stalking
		if (!targetIsInAttackRange) {
			state = State.Stalking;
			return;
		}
		
		// lost sight of target, give up lol
		if (!targetLOS)	{
			state = State.Idle;
			return;
		}
		
		// close enough to attack		
		if (targetIsHere) {
			// note we're already checking _attacking at the start of this method
			StartCoroutine(AttackRoutine());
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;
		target.BroadcastMessage ("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		target.SendMessageUpwards("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(1f/attackRate);
		_attacking = false;
	}
	
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
		virusPrefab.Spawn(victim.position, victim.rotation);
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
		audio.pitch = _sf * 0.5f;
		yield return new WaitForSeconds(2f);
		audio.Stop();
		//transform.Recycle();
		//spawn corpse and recycle
	}
}
