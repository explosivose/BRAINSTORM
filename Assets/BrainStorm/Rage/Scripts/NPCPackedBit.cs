using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCWalking))]
[RequireComponent(typeof(NPCFaction))]
public class NPCPackedBit : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();

	public enum State {
		Advancing, Attacking, Dead, Calm
	}
	private State _state = State.Advancing;
	private NPCWalking _walker;
	private NPCFaction _faction;
	private MeshRenderer _ren;
	private Transform _target;
	private bool _attacking; 
	private float _calmTimer;
	
	
	
	void Start () {
		_walker = GetComponent<NPCWalking>();
		_faction = GetComponent<NPCFaction>();
		_ren = GetComponentInChildren<MeshRenderer>();
		wardrobe.normal = _ren.material;
	}
	

	void Update () {
		switch(_state) {
		case State.Advancing:
			AdvancingUpdate();
		 	break;
		case State.Attacking:
			AttackUpdate();
			break;
		case State.Calm:
			CalmUpdate();
			break;
		case State.Dead:
		default:
			break;
		}
	}
	
	void AdvancingUpdate() {
		_walker.destination = _faction.advancePosition;
		_walker.stopDistance = 0f;
	}
	
	void AttackUpdate() {
		_walker.destination = _target.position;
		_walker.stopDistance = stats.attackRange;
		if (_walker.atDestination && !_attacking) {
			StartCoroutine( Attack() );
		}
	}
	
	IEnumerator Attack() {
		_attacking = true;
		_ren.material = wardrobe.attacking;
		yield return new WaitForSeconds(0.1f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_attacking = false;
	}
	
	void CalmUpdate() {
		_calmTimer -= Time.deltaTime;
		if (_calmTimer < 0f) {
			Vector3 destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
			_calmTimer = 20f;
			_walker.destination = destination;
			_walker.stopDistance = _walker.defaultStopDistance;
			_walker.moveSpeedModifier = 0.5f;
		}
	}
	
	void OnTriggerEnter(Collider col) {
		if (_state != State.Advancing) return;
		
		switch(col.tag) {
		case "NPC":
			NPCFaction f = col.GetComponent<NPCFaction>();
			if (f != null) {
				if (f.team != _faction.team) {
					// found opposite faction
					_state = State.Attacking;
					_target = col.transform;
				}
			}
			else {
				// found NPC that doesn't belong to faction
				_state = State.Attacking;
				_target = col.transform;
			}
			break;
		case "Player":
			_state = State.Attacking;
			_target = col.transform;
			break;
		}
	}
	
	void OnTriggerExit(Collider col) {
		if (_state == State.Calm) return;
		
		if ( col.gameObject == _target.gameObject ) {
			_state = State.Advancing;
			_target = null;
		}
	}
}
