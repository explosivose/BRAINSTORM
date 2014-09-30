using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Virus/Walking")]
[RequireComponent(typeof(NPCPathFinder))]
public class NPCVirusWalking : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();

	private enum State {
		Patrol, Camp, Pursue, Dead
	}
	private State _state = State.Patrol;
	private NPCPathFinder _pathfinder;
	private Transform _target; 
	private float _patrolTimer;
	private bool _attacking = false;
	private bool _hurt = false;
	
	private MeshRenderer _ren;
	
	void Start () {
		_pathfinder = GetComponent<NPCPathFinder>();
		_ren = GetComponentInChildren<MeshRenderer>();
		wardrobe.normal = _ren.material;
	}
	
	void Update () {
		switch(_state) {
		case State.Patrol:
			PatrolUpdate();
			break;
		case State.Camp:
			break;
		case State.Pursue:
			PursueUpdate();
			break;
		case State.Dead:
		default:
			break;
		}
	}
	
	void PatrolUpdate() {
		_patrolTimer -= Time.deltaTime;
		if (_patrolTimer < 0f) {
			Vector3 destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
			_patrolTimer = 20f;
			_pathfinder.destination = destination;
			_pathfinder.stopDistance = _pathfinder.defaultStopDistance;
		}
	}
	
	void PursueUpdate() {
		if (_target == null) {
			_state = State.Patrol;
			return;
		}
		_pathfinder.destination = _target.position;
		_pathfinder.stopDistance = 0f;
		if (_pathfinder.atDestination && !_attacking) {
			StartCoroutine( Attack() );
		}
	}
	
	IEnumerator Attack() {
		_attacking = true;
		_ren.material = wardrobe.attacking;
		yield return new WaitForSeconds(0.05f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_attacking = false;
	}
	
	void OnTriggerEnter(Collider col) {
		
		if (_state == State.Dead) return;
		
		switch(col.tag) {
		case "Player":
			_state = State.Pursue;
			_target = col.transform;
			break;
		}
	}
	
	void OnTriggerExit(Collider col) {
		
		if (_state == State.Dead) return;
		
		switch(col.tag) {
		case "Player":
			_state = State.Patrol;
			_target = null;
			break;
		}
	}
	
	public void Damage(DamageInstance damage) {
		stats.health -= damage.damage;
		if (stats.health < 0) {
			Death();
		}
		else if (!_hurt) {
			StartCoroutine( Hurt() );
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
	
	void Death() {
		_state = State.Dead;
		_ren.material = wardrobe.dead;
		tag = "Untagged";
	}
}
