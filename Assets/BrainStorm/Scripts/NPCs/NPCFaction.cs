using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Faction/Faction")]
[RequireComponent(typeof(NPCPathFinder))]
public class NPCFaction : NPC {

	public enum Faction {
		Pink, Purple
	}
	public enum State {
		Idle, Advancing, Attacking, Dead, Calm
	}
	
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials pinkWardrobe = new CharacterMaterials();
	public CharacterMaterials purpleWardrobe = new CharacterMaterials();
	
	private Vector3 _advancePosition;
	public Vector3 advancePosition {
		get { return _advancePosition;  }
		set { _advancePosition = value; }
	}
	
	private Faction _team;
	public Faction team {
		get { return _team; }
		set { 
			_team = value;
			if (_team == NPCFaction.Faction.Pink) 
				_wardrobe = pinkWardrobe;
			else 
				_wardrobe = purpleWardrobe;
			
			_ren.material = _wardrobe.normal;
		}
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
				_pathfinder.stopDistance = targetSearch.nearRange;
				searchForTargets = false;
				break;
				
			case State.Dead:
				searchForTargets = false;
				tag = "Untagged";
				_pathfinder.moveSpeedModifier = 0f;
				_ren.material = _wardrobe.dead;
				FactionManager.Instance.NPCDeath(team);
				rigidbody.useGravity = true;
				target = null;
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
	
	void AttackUpdate() {
		if (!hasTarget) {
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
				if (f.team == team) return; // no friendly fire
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
	
	// Faction NPCs need their own findtarget routine to identify the opposite faction
	protected override IEnumerator FindTarget() {
		_searchRoutineActive = true;
		while(_searchingForTarget == true) {
			
			if(target) {
				// null target if it has changed to invalid tag
				// this happens if, for example, the target is killed
				bool targetStillValid = false;
				foreach(string s in _search.validTargetTags) 
					if (s == target.tag)
						targetStillValid = true;
				
				if (!targetStillValid) target = null;
			}
			
			// set search range for new target search
			switch (_search.searchForTargetsIn) {
			case TargetSearchRange.NearRange:
				_searchRange = _search.nearRange;
				break;
			case TargetSearchRange.FarRange:
				_searchRange = _search.farRange;
				break;
			case TargetSearchRange.FarthestRange:
				_searchRange = _search.farthestRange;
				break;	
			}
			
			// perform search and inspect teh loot
			Collider[] colliders = Physics.OverlapSphere(transform.position, _searchRange, _search.targetSearchMask);
			foreach(Collider c in colliders) {
				if ( c.isTrigger ) continue;
				if ( c.transform == this.transform) continue;
				
				foreach(string s in _search.validTargetTags) {
					// is tag valid?
					if (s == c.tag) {
						// is it an NPC?
						if (c.tag == "NPC") {
							// is this a faction NPC?
							NPCFaction f = c.GetComponent<NPCFaction>();
							if (f) {
								// if you're not on my side
								if (f.team != team) 
									target = CompareTargets(target, c.transform);
							}
							else {
								// do i want to target this NPC?
								NPC npc = c.GetComponent<NPC>();
								if ((npc.type & _search.valid_NPC_Targets) == npc.type && 
								    npc.type > 0) {
									target = CompareTargets(target, c.transform);
								}
							}
						}
						// is it the player and can I target the player?
						else if (c.tag == "Player" && _search.targetPlayer) {
							target = CompareTargets(target, c.transform);
						}
						// for everything else, we just go for it
						else {
							target = CompareTargets(target, c.transform);
						}
					}
				}
			}
			
			if (target && drawDebug)
				Debug.DrawLine(transform.position, target.position, Color.red);
			
			if (hasTarget)
				state = State.Attacking;
			
			float time = _search.timeBetweenTargetSearches;
			time += Random.Range(-_search.timeBtwnSrchsRandomness, 
			                     _search.timeBtwnSrchsRandomness);
			yield return new WaitForSeconds(time);
		}
		_searchRoutineActive = false;
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
