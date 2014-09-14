using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Faction/Faction")]
[RequireComponent(typeof(NPCPathFinder))]
public class NPCFaction : NPC {

	public enum State {
		Idle, Advancing, Attacking, Dead, Calm
	}
	
	public Transform soulPrefab;
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials pinkWardrobe = new CharacterMaterials();
	public CharacterMaterials purpleWardrobe = new CharacterMaterials();
	
	private Vector3 _advancePosition;
	public Vector3 advancePosition {
		get { return _advancePosition;  }
		set { _advancePosition = value; }
	}

	
	private State _state;
	public State state {
		get { return _state; }
		set {
			_state = value;
			switch(_state) {
			case State.Advancing:
				_pathfinder.destination = advancePosition;
				_pathfinder.stopDistance = 10f;
				target = null;
				searchForTargets = true;
				break;
			case State.Attacking:
				_pathfinder.destination = target.position;
				_pathfinder.stopDistance = attackRange;
				searchForTargets = false;
				break;
				
			case State.Dead:
				searchForTargets = false;
				tag = "Untagged";
				_pathfinder.moveSpeedModifier = 0f;
				_ren.material = _wardrobe.dead;
				FactionManager.Instance.NPCDeath(type);
				rigidbody.useGravity = true;
				target = null;
				Transform soul = soulPrefab.Spawn(transform.position);
				soul.parent = GameManager.Instance.activeScene;
				StartCoroutine(Death ());
				break;
				
			case State.Calm:
				_ren.material = _wardrobe.lingerie;
				_pathfinder.destination = advancePosition;
				_pathfinder.stopDistance = 10f;
				target = null;
				type = Type.Native;
				break;
			case State.Idle:
			default:
				break;
			}
		}
	}
	
	private bool _attacking; 
	public bool attacking {
		get { return _attacking; }
		set { 
			_attacking = value;
			if (_attacking) {
				_ren.material = _wardrobe.attacking;
			}
			else if (state == State.Dead){
				_ren.material = _wardrobe.dead;
			}
			else {
				_ren.material = _wardrobe.normal;
			}
		}
	}

	private NPCPathFinder _pathfinder;
	private CharacterMaterials _wardrobe = new CharacterMaterials();
	private MeshRenderer _ren;
	private bool _hurt;
	
	protected override void Awake() {
		base.Awake();
		_pathfinder = GetComponent<NPCPathFinder>();
		_ren = GetComponentInChildren<MeshRenderer>();
		state = State.Idle;
		ObjectPool.CreatePool(soulPrefab);
		FactionInit();
	}
	
	protected override void OnEnable() {
		base.OnEnable();
		// this scene is being loaded
		if (GameManager.Instance.levelTeardown) {
			_ren.material = _wardrobe.normal;
			state = state;
			if (state == State.Dead) {
				transform.Recycle();
			}
		}
		// NPC has been enabled by other means
		else {
			_attacking = false;
			_hurt = false;
			_pathfinder.moveSpeedModifier = 1f;
		}
	}
	
	public void FactionInit() {
		if (type == Type.Team1) {
			_wardrobe = pinkWardrobe;
			_search.valid_NPC_Targets |= NPC.Type.Team2; // target team 2
			_search.valid_NPC_Targets &= ~NPC.Type.Team1; // don't target team 1
		}
		else if (type == Type.Team2) {
			_wardrobe = purpleWardrobe;
			_search.valid_NPC_Targets |= NPC.Type.Team1; // target team 1
			_search.valid_NPC_Targets &= ~NPC.Type.Team2; // don't target team 2
		}
		else {
			Debug.LogError("Faction NPC type incorrect (expected " +
				"Team1 (pink) or Team2 (purple) NPC.Type");
		}
		_ren.material = _wardrobe.normal;
	}
	
	void Update () {
		switch(state) {
		case State.Advancing:
			AdvanceUpdate();
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
	
	void AdvanceUpdate() {
		if (hasTarget) {
			state = State.Attacking;
			return;
		}
	}
	
	
	void AttackUpdate() {
		if (!hasTarget || !targetLOS) {
			Debug.Log ("target lost");
			state = State.Advancing;
			return;
		}
		if (!_attacking) {
			if (targetIsInAttackRange && targetLOS) {
				SendMessage("Attack");
			}
		}

		// if target changes location, update destination
		if (Vector3.Distance(_pathfinder.destination, target.position) > 1f) {
			_pathfinder.destination = target.position;
		}
	}
	
	
	void CalmUpdate() {
		if (_pathfinder.atDestination) {
			StartCoroutine(Death());
		}
	}
	
	protected override void Damage(DamageInstance damage) {
		if (state == State.Dead) return;
		if (damage.source != null) {
			NPCFaction f = damage.source.GetComponent<NPCFaction>();
			if (f != null) {
				if (f.type == type) return; // no friendly fire
			}
		}

		base.Damage(damage);
	
		if (isDead) {
			damage.source.SendMessage("Killed", this.transform);
			state = State.Dead;
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
		}
	}
	
	protected override void Killed(Transform victim) {
		if (victim == target) {
			state = State.Advancing;
		}
	}
	
	
	IEnumerator Hurt() {
		_hurt = true;
		_ren.material = _wardrobe.hurt;
		yield return new WaitForSeconds(0.1f);
		_ren.material = _wardrobe.normal;
		if (state == State.Calm) _ren.material = _wardrobe.lingerie;
		yield return new WaitForSeconds(0.1f);
		_hurt = false;
	}
	
	IEnumerator Death() {
		
		yield return new WaitForSeconds(60f);
		
		// spawn a corpse?
		
		transform.Recycle();
	}
}
