using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Virus/Zombie")]
public class NPCVirusZombie : NPC {

	public enum State {
		Idle, Stalking, Attacking, Dead
	}
	
	public float attackReach = 2f;
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
				searchForTargets = photonView.isMine; // search for targets routine runs on owner
				audio.pitch = _sf;
				break;
				
			case State.Stalking:
				searchForTargets = photonView.isMine;
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
				_boid.enabled = false;
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
	
	// variables used when !photonView.isMine
	private Vector3 latestCorrectPos;
	private Vector3 onUpdatePos;
	private float fraction;
	
	protected override void Awake() {
		base.Awake();
		
		if (photonView.isMine) {
			Transform boid = transform.Find("BoidControl");
			if (!boid) Debug.LogError("BoidControl missing");
			boid.gameObject.SetActive(true);
			_boid = boid.GetComponent<Boid>();
			if (!_boid) Debug.LogError("BoidControl missing");
			_boid.defaultBehaviour = idleProfile;
			ObjectPool.CreatePool(virusPrefab);
			_sf = (Random.value/2f) + 0.5f;
			transform.localScale *= _sf;
		}

		_ren = GetComponentInChildren<MeshRenderer>();
		audio.pitch = _sf;
		audio.timeSamples = Random.Range(0, audio.clip.samples);
	}
	
	void Start() {
		
		if (photonView.isMine) {
			state = State.Idle;
		}
		else {
			latestCorrectPos = transform.position;
			onUpdatePos = transform.position;
		}
		
	}
	
	protected override void OnEnable() {
		base.OnEnable();
		if (state == State.Dead) return;
		if (GameManager.Instance.levelTeardown) return;
		if (photonView.isMine) {
			
			_boid.enabled = true;
			_boid.target1PositionOffset = Vector3.zero;
		}
		_attacking = false;
		_hurt = false;
		audio.Play();
	}
	
	[RPC]
	void ChangeState(int change) {
		state = (State)change;
	}
	
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 pos = transform.localPosition;
			Quaternion rot = transform.localRotation;
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		}
		else
		{
			// Receive latest state information
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			
			latestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
			onUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
			fraction = 0;                           // reset the fraction we alreay moved. see Update()
			
			transform.localRotation = rot;          // this sample doesn't smooth rotation
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		if (!photonView.isMine) {
			fraction = fraction + Time.deltaTime * 9;
			transform.localPosition = Vector3.Lerp(onUpdatePos, latestCorrectPos, fraction);    // set our pos between A and B
			return;
		}
		
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
			target = null;
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
			_boid.target1PositionOffset = transform.up * targetDistance;
			yield return new WaitForSeconds(time);
			_boid.target1PositionOffset = transform.right * targetDistance;
			yield return new WaitForSeconds(time);
			_boid.target1PositionOffset = -transform.up * targetDistance;
			yield return new WaitForSeconds(time);
			_boid.target1PositionOffset = -transform.right * targetDistance;
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
		if (!hasTarget || !targetIsTagValid) {
			target = null;
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
		if (targetDistance < attackReach) {
			// note we're already checking _attacking at the start of this method
			StartCoroutine(AttackRoutine());
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;
		target.BroadcastMessage ("Damage", attackDamage, SendMessageOptions.DontRequireReceiver);
		target.SendMessageUpwards("Damage", attackDamage, SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(1f/attackRate);
		_attacking = false;
	}
	
	[RPC]
	protected override void Damage(int damage) {
		
		if (state == State.Dead) return;
		
		if (photonView.isMine) {
			base.Damage(damage);
			if (isDead) {
				photonView.RPC("ChangeState", PhotonTargets.AllBufferedViaServer, (int)State.Dead);
			}
			else {
				photonView.RPC("Hurt", PhotonTargets.All);
			}
		}
		// I don't own this. Tell the owner I damaged their Virus
		else {
			photonView.RPC("Damage", photonView.owner, damage);
		}

	}
	
	protected override void Killed(Transform victim) {
		Transform v = virusPrefab.Spawn(victim.position, victim.rotation);
		v.parent = GameManager.Instance.activeScene.instance;
		if (victim == target) {
			state = State.Idle;
		}
	}
	
	[RPC]
	void Hurt() {
		if (!_hurt) StartCoroutine( HurtEffect() );
	}
	
	IEnumerator HurtEffect() {
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
