using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Character/NPC")]
public class NPC : MonoBehaviour {

	[System.Flags]
	public enum Type {
		//None		= 0x00
		Native 		= 0x01,		// healthy NPCs (friendly to player)
		Infected 	= 0x02,		// (Spider, etc)
		Virus		= 0x04,		// Virus' are friendly to one another, hostile to most other things
		Team1		= 0x08,		// Faction NPCs team 1 (pink)
		Team2		= 0x10		// Faction NPCs team 2 (purple)
				//	= 0x20
				//	= 0x40
				//	= 0x80
		//All		= 0xFF
	}
	
	public enum TargetSearchRange {
		minRange,
		maxRange,
	}
	
	public enum TargetProximity {
		Here,
		InRange,
		OutOfRange,
	}
	
	[System.Serializable]
	public class TargetSearchCriteria {
		public float 			minRange = 1f;
		public float 			maxRange = 15f;
		
		public List<string>			validTargetTags = new List<string>();
		public LayerMask			targetSearchMask;
		[EnumMask]
		public Type					valid_NPC_Targets;
		public TargetSearchRange	searchForTargetsIn = TargetSearchRange.maxRange;
		public float				timeBtwnTrgtSrchs = 2f;
		public float				timeBtwnSrchsRandomness = 1f;
	}
	
	// properties exposed to the unity inspector
	// -----------------------------------------
	public bool				drawDebug;
	public Type 			type;
	public int 				maxHealth = 100;
	public bool 			invulnerable = false;
	public int 				attackDamage = 5;
	public float			attackRate = 2f;
	public float 			attackRange = 10f;
	
	public TargetSearchCriteria targetSearch = new TargetSearchCriteria();
	
	
	// protected class members
	// -----------------------
	protected DamageInstance 		_damage = new DamageInstance();
	protected TargetSearchCriteria	_search;
	protected float					_searchRange = 100f;
	protected bool			_searchingForTarget = false;
	protected bool 			_searchRoutineActive = false;
	
	
	// private class members
	// ---------------------
	private Transform				_eyes;		// we raycast from here to check LOS
	
	
	// other properties in the c# get/set style
	// ----------------------------------------
	public Transform target { 
		get; 
		protected set; 
	}
	
	public int health {
		get;
		private set;
	}
	
	public bool isDead {
		get { return health <= 0; }
	}
	
	// if set true, start search routine
	protected bool searchForTargets {
		get { return _searchingForTarget; }
		set {
			_searchingForTarget = value;
			// if we weren't already searching, start search routine
			if (_searchingForTarget && !_searchRoutineActive)
				StartCoroutine( FindTarget() );
		}
	}
	
	// well, do we hasTarget?
	public bool hasTarget {
		get { return target != null; }
	}
	
	public TargetProximity targetProximity {
		get {
			if (!hasTarget) return TargetProximity.OutOfRange;
			float distance = Vector3.Distance(transform.position, target.position);
			if (distance < _search.minRange)
				return TargetProximity.Here;
			if (distance >= _search.minRange && distance < _search.maxRange)
				return TargetProximity.InRange;
			//else
			return TargetProximity.OutOfRange;
		}
	}
	
	// do we have Line Of Sight?
	public bool targetLOS {
		get {
			if (!hasTarget) return false;
			Vector3 seeFrom = _eyes ? _eyes.position : transform.position;
			bool LOS = !Physics.Linecast(seeFrom, target.position, ~_search.targetSearchMask);
			if (drawDebug &&  LOS) 	Debug.DrawLine(seeFrom, target.position, Color.cyan);
			if (drawDebug && !LOS)	Debug.DrawLine(seeFrom, target.position, Color.grey);
			return LOS;
		}
	}
	
	// does the target still match search criteria?
	public bool targetIsValid {
		get {
			bool targetStillValid = false;
			foreach(string s in _search.validTargetTags) 
				if (s == target.tag)
					targetStillValid = true;
			
			return targetStillValid;
		}
	}
	
	// target distance less than attackRange
	public bool targetIsInAttackRange {
		get {
			if (!hasTarget) return false;
			float distance = Vector3.Distance(transform.position, target.position);
			return (distance < attackRange);
		}
	}
	
	// target distance less than minRange
	public bool targetIsTooClose {
		get {
			if (!hasTarget) return false;
			float distance = Vector3.Distance(transform.position, target.position);
			return (distance <= _search.minRange);
		}
	}
	
	// target distance between minRange and maxRange
	public bool targetIsInRange {
		get {
			if (!hasTarget) return false;
			float distance = Vector3.Distance(transform.position, target.position);
			return (distance > _search.minRange && distance <= _search.maxRange);
		}
	}
		
	// target distance greater than maxRange
	public bool targetIsOutOfRange {
		get {
			if (!hasTarget) return false;
			float distance = Vector3.Distance(transform.position, target.position);
			return (distance > _search.maxRange);
		}
	}
	

	// public methods
	// ----------------
	public void NullTarget() {
		target = null;
	}
	
	// private methods
	// ---------------
	
	protected virtual void Awake() {
		_search = targetSearch;
		_eyes = transform.Find("eyes");
		tag = "NPC";
		health = maxHealth;
		_damage.source = this.transform;
		_damage.damage = attackDamage;
	}
	
	protected virtual void OnEnable() {
		// this scene is being loaded
		if (GameManager.Instance.levelTeardown) {
			if (_searchingForTarget) 
				StartCoroutine( FindTarget() );
		}
		// NPC has been enabled by some other means
		else {
			tag = "NPC";
			health = maxHealth;
		}
	}
	
	protected virtual void OnDisable() {
		// this scene is being unloaded
		if (GameManager.Instance.levelTeardown) {

		}
		// NPC has been disabled by some other means
		else {
			tag = "Untagged";
			target = null;
		}
	}
	
	
	// NPC has received damage!
	protected virtual void Damage(DamageInstance damage) {
		if (invulnerable) return;
		if (health <= 0) return;
		if (damage.source == this.transform) return;
		
		health -= damage.damage;
		if (health <= 0) {
			health = 0;
			damage.source.BroadcastMessage("Killed", this.transform);
		}
	}
	
	// NPC has killed something!
	protected virtual void Killed(Transform victim) {
		
	}
	
	// search for valid target routine
	protected virtual IEnumerator FindTarget() {
		_searchRoutineActive = true;
		while(_searchingForTarget == true) {
				
			// set search range for new target search
			switch (_search.searchForTargetsIn) {
			case TargetSearchRange.minRange:
				_searchRange = _search.minRange;
				break;
			case TargetSearchRange.maxRange:
				_searchRange = _search.maxRange;
				break;
			}
			
			// perform search 
			Collider[] colliders = Physics.OverlapSphere(
				transform.position, _searchRange, _search.targetSearchMask
			);
			// and inspect teh loot
			foreach(Collider c in colliders) {
				if ( c.isTrigger ) continue;	// ignore trigger colliders
				if ( c.transform == this.transform) continue;	// don't target yourself
				
				foreach(string s in _search.validTargetTags) {
					// is tag valid?
					if (s == c.tag) {
						// is it an NPC?
						if (c.tag == "NPC") {
							// do i want to target this NPC?
							NPC npc = c.GetComponent<NPC>();
							if (!npc) Debug.LogError("Gameobject tagged NPC but " + 
										"does not contain an NPC componenent.", c);
							if ((npc.type & _search.valid_NPC_Targets) == npc.type && 
								npc.type > 0) {
								target = CompareTargets(target, c.transform);
							}
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
			
			float time = _search.timeBtwnTrgtSrchs;
			time += Random.Range(-_search.timeBtwnSrchsRandomness, 
			                     _search.timeBtwnSrchsRandomness);
			yield return new WaitForSeconds(time);
		}
		_searchRoutineActive = false;
	}
	
	protected Transform CompareTargets(Transform target1, Transform target2) {
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
}
