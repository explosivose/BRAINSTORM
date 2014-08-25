using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Carrot")]
public class NPCCarrot : NPC {
	
	public TargetSearchCriteria frenzyTargetSearch;
	public int frenzyTippingPoint = 5;
	public int attackTippingPoint = 20;
	public float attackRollRate = 0.5f;
	public Boid.Profile boidAttackProfile = new Boid.Profile();
	
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
				type = Type.Infected;
				_boid.profile = boidAttackProfile;
				_search = targetSearch;
				searchForTargets = true;
				break;
			case State.Frenzied:
				_carrotsInFrenzy++;
				type = Type.Native;
				_boid.controlEnabled = true;
				_boid.profile = _boid.defaultBehaviour;
				_boid.SetTarget1(_player);
				_boid.SetTarget2(null);
				StartCoroutine(AttackRollRoutine());
				_search = frenzyTargetSearch;
				searchForTargets = true;
				break;
			case State.Attack:
				_boid.profile = boidAttackProfile;
				_boid.SetTarget2(_target);
				break;
			default:
				break;
			}
			
		}
	}
	
	public static float frenzyFactor {
		get { return (float)_carrotsInFrenzy/(float)_carrotCount; }
	}

	private static int _carrotCount;
	private static int _carrotsInFrenzy;
	private static int _updateIndex;
	private int _myIndex;

	private Boid _boid;
	private float _attackRoll;
	private bool _attacking;
	private Transform _player;
	
	protected override void Awake() {
		base.Awake();
		_boid = GetComponentInParent<Boid>();
		if (!_boid) Debug.LogError("NPCCarrot parent must have Boid component");
		_boid.controlEnabled = false;
		_myIndex = _carrotCount++;
	}
	
	void Start() {
		_player = Player.Instance.transform;
	}
	
	protected override void OnEnable() {
		base.OnEnable();
		_attacking = false;
		state = State.Alone;
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
		
		if (!hasTarget) return;
		_boid.SetTarget2(_target);
		
		if (targetIsHere) {
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
		
		if (!hasTarget) return;
		
		foreach(Transform b in _boid.neighbours) {
			NPCCarrot otherCarrot = b.GetComponentInChildren<NPCCarrot>();
			if (otherCarrot.state == State.Attack) {
				_target = otherCarrot._target;
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
		if (!hasTarget) {
			state = State.Frenzied;
			return;
		}
		_boid.SetTarget2(_target);
		// This should be an attack coroutine with an attack cooldown
		if (targetIsHere) {
			if (!_attacking) StartCoroutine(AttackRoutine());
		}
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;

		_target.BroadcastMessage("Damage", _damage, SendMessageOptions.DontRequireReceiver);
		_target.SendMessageUpwards("Damage", _damage, SendMessageOptions.DontRequireReceiver);
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
		yield return new WaitForSeconds(attackRate);
		_attacking = false;
	}
}
