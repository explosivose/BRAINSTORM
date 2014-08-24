﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Carrot")]
[RequireComponent(typeof(Boid))]
public class NPCCarrot : MonoBehaviour {
	
	public float attackRange = 3f;
	public int attackDamage = 5;
	public int frenzyTippingPoint = 5;
	public int attackTippingPoint = 20;
	public float attackRollRate = 0.5f;
	public Boid.Profile boidAttackProfile = new Boid.Profile();
	public LayerMask targetSearchMask;
	[BitMask(typeof(NPC.Type))]
	public NPC.Type hostileTo;
	public float targetSearchRange = 50f;
	
	public enum State {
		Alone, Frenzied, Attack
	}
	/// <summary>
	/// Alone
	///		Carrot wanders around looking for a TV to stare at
	///		Once a TV is found, carrot stares at TV closely
	///		Looks at passerbys periodically
	///
	///	Frenzied
	///		Swarm flight with other carrots targetting Virus NPCs
	///
	/// Attack
	///		the larger the frenzy the more likely a boid is to attack
	///		
	/// </summary>
	private State _state;
	public State state {
		get { return _state; }
		set { 
			
			if (_state == State.Frenzied) 
				_carrotsInFrenzy--;
				
			_state = value;
			switch(value) {
			case State.Alone:
				_boid.SetTarget1(null);
				_boid.SetTarget2(_TVTarget);
				_boid.profile = boidAttackProfile;
				break;
			case State.Frenzied:
				_carrotsInFrenzy++;
				_attackTarget = null;
				_boid.controlEnabled = true;
				_boid.profile = _boid.defaultBehaviour;
				_boid.SetTarget1(_player);
				_boid.SetTarget2(null);
				StartCoroutine(AttackRollRoutine());
				break;
			case State.Attack:
				_boid.profile = boidAttackProfile;
				_boid.SetTarget2(_attackTarget);
				break;
			default:
				break;
			}
			
		}
	}
	
	public Transform attackTarget {
		get { return _attackTarget; }
	}

	public static float frenzyFactor {
		get { return (float)_carrotsInFrenzy/(float)_carrotCount; }
	}

	private static int _carrotCount;
	private static int _carrotsInFrenzy;
	private static int _updateIndex;
	private int _myIndex;

	private Boid _boid;
	private DamageInstance _damage;
	private float _attackRoll;
	private bool _attacking;
	private Transform _player;
	private Transform _attackTarget;
	private Transform _TVTarget;
	
	void Awake() {
		_boid = GetComponent<Boid>();
		_boid.controlEnabled = false;
		_damage = new DamageInstance();
		_damage.source = this.transform;
		_damage.damage = attackDamage;
		_myIndex = _carrotCount++;
	}
	
	void Start() {
		_player = GameObject.FindGameObjectWithTag("Player").transform;
		state = State.Alone;
	}
	
	void OnEnable() {
		_attacking = false;
		StartCoroutine( FindTarget() );
	}

	void Update () {
		if (_updateIndex == _myIndex) {
			switch(state) {
			case State.Alone:
				AloneUpdate();
				break;
			case State.Frenzied:
				FrenzyUpdate();
				break;
			case State.Attack:
				AttackUpdate();
				break;
			default:
				break;
			}
			if(++_updateIndex >= _carrotCount) {
				_updateIndex = 0;
			}
		}
	}
	
	void AloneUpdate() {
		
		if (_boid.neighbours.Count >= frenzyTippingPoint) {
			state = State.Frenzied;
			return;
		}
		
		if (_TVTarget == null) return;
		_boid.SetTarget2(_TVTarget);
		if (Vector3.Distance(transform.position, _TVTarget.position) < 5f) {
			_boid.controlEnabled = false;
		}
		else {
			_boid.controlEnabled = true;
		}
	}
	
	void FrenzyUpdate() {
		if (_boid.neighbours.Count < frenzyTippingPoint) {
			state = State.Alone;
			return;
		}
		
		float playerDistance = Vector3.Distance(_player.transform.position, transform.position);
		_boid.profile.target1Weight = playerDistance * 0.125f;
		
		if (_attackTarget == null) return;
		
		foreach(Transform b in _boid.neighbours) {
			NPCCarrot otherCarrot = b.GetComponent<NPCCarrot>();
			if (otherCarrot.state == State.Attack) {
				_attackTarget = otherCarrot.attackTarget;
				state = State.Attack;
				return;
			}
		}
		
		
		float scoreRequired = 1f - ((float)_boid.neighbours.Count / (float)attackTippingPoint);
		if (_attackRoll > scoreRequired) {
			state = State.Attack;
			return;
		}
	}
	
	/* rather than constantly rolling the dice...
	in future the carrots should roll a dice to decide what the want to do
	on the instant they encounter a new potential target
	using information like: how many enemies are in the vicinity
							how many other carrots are with me
	which means that maybe the boids need an avoid target behaviour
	for when carrots want to retreat from an area
	*/
	IEnumerator AttackRollRoutine() {
		while(state == State.Frenzied) {
			_attackRoll = Random.value;
			float scoreRequired = 1f - ((float)_boid.neighbours.Count / (float)attackTippingPoint);
			//Debug.Log ("Roll: " + _attackRoll + " Req: " + scoreRequired);
			yield return new WaitForSeconds(1f/attackRollRate);
		}
	}
	
	void AttackUpdate() {
		if (_attackTarget == null) {
			state = State.Frenzied;
			return;
		}
		if (_attackTarget.tag == "Untagged") {
			state = State.Frenzied;
			return;
		}
		_boid.SetTarget2(_attackTarget);
		// This should be an attack coroutine with an attack cooldown
		if (Vector3.Distance(transform.position, _attackTarget.position) < attackRange) {
			if (!_attacking) StartCoroutine(AttackRoutine());
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;

		_attackTarget.BroadcastMessage("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		_attackTarget.SendMessageUpwards("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		// This is BroadcastMessage() rather than SendMessage() 
		// because _attackTarget will be a child object of a virus zombie for example
		// this is because the virus zombie is a boid which needs an object
		// on boid layer and character layer.
		// I should think about how to organise boid character objects in the
		// hierarchy to avoid using BroadcastMessage()
		// Or perhaps think about writing a character parent class to call
		// functions on directly (which also carries common functions like
		// FindTarget(), Damage(), blah blah
		// There's a similar code that could be shared.
		yield return new WaitForSeconds(0.5f);
		_attacking = false;
	}
	
	void Killed(Transform victim) {

	}
	
	IEnumerator FindTarget() {
		while(true) {
			Collider[] colliders = Physics.OverlapSphere(transform.position, targetSearchRange, targetSearchMask);
			foreach(Collider c in colliders) {
				if ( c.isTrigger ) continue;
				if ( c.transform == this.transform) continue;
				if (_attackTarget != null) {
					if (_attackTarget.tag != "NPC") _attackTarget = null;
				}
				switch (c.tag) {
				case "TV":
					_TVTarget = CompareTargets(_TVTarget, c.transform);
					break;
				case "NPC":
					NPC npc = c.GetComponent<NPC>();
					if ((npc.type & hostileTo) == npc.type && npc.type > 0)
						_attackTarget = CompareTargets(_attackTarget, c.transform);
					break;
				default:
					break;
				}
			}
			yield return new WaitForSeconds(Random.Range(0.5f, 3f));
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
	
}
