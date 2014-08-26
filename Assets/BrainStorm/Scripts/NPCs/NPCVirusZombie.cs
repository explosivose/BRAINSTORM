using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Virus/Zombie")]
public class NPCVirusZombie : NPC {

	public enum State {
		Idle, Stalking, Attacking, Dead
	}
	
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
				_target = null;
				_boid.profile = idleProfile;
				// cheatsidoodle way to keep these NPCs from wandering off-scene
				_boid.SetTarget1(GameManager.Instance.transform);
				searchForTargets = true;
				audio.pitch = _sf;
				break;
				
			case State.Stalking:
				searchForTargets = true;
				_boid.profile = stalkProfile;
				_boid.SetTarget1(_target);
				StartCoroutine(StalkRoutine());
				break;
				
			case State.Attacking:
				searchForTargets = false;
				_boid.profile = attackProfile;
				_boid.SetTarget1(_target);
				audio.pitch = _sf * 12f;
				break;
				
			case State.Dead:
				searchForTargets = false;
				_boid.controlEnabled = false;
				_ren.material = wardrobe.dead;
				tag = "Untagged";
				_boid.rigidbody.useGravity = true;
				_target = null;
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
		_boid = GetComponentInParent<Boid>();
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
			Debug.DrawLine(transform.position, _target.position, Color.grey);
			
		if (targetIsFar) {
			state = State.Stalking;
		}
	}
	
	void StalkUpdate() {
		if (!hasTarget) {
			state = State.Idle;
			return;
		}
		
		if(drawDebug)
			Debug.DrawLine(transform.position, _target.position, Color.yellow);
		
		TargetProximity proximity = targetProximity;
		
		if (proximity == TargetProximity.OutOfRange) {
			state = State.Idle;
			return;
		}
		
		if (proximity == TargetProximity.Near) {
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
	
	void AttackUpdate() {
		if (!hasTarget) {
			state = State.Idle;
			return;
		}
		
		if (drawDebug)
			Debug.DrawLine(transform.position, _target.position, Color.red);
		
		TargetProximity proximity = targetProximity;
		
		if (proximity == TargetProximity.Far || proximity == TargetProximity.OutOfRange) {
			state = State.Idle;
			return;
		}
		
		if (proximity == TargetProximity.Here) {
			if (!_attacking) StartCoroutine(AttackRoutine());
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;
		_target.BroadcastMessage ("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		_target.SendMessageUpwards("Damage", _damage, SendMessageOptions.DontRequireReceiver);
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
		if (victim == _target) {
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
		audio.pitch = _sf * 0.5f;
		yield return new WaitForSeconds(2f);
		audio.Stop();
		//transform.Recycle();
	}
}
