using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCWalking))]
public class NPCVirusWalking : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();

	private enum WalkingVirusState {
		Patrol, Camp, Pursue
	}
	private WalkingVirusState _state = WalkingVirusState.Patrol;
	private NPCWalking _walker;
	private Transform _target; 
	private float _patrolTimer;
	private bool _attacking = false;
	private MeshRenderer _ren;
	
	// Use this for initialization
	void Start () {
		_walker = GetComponent<NPCWalking>();
		_ren = GetComponentInChildren<MeshRenderer>();
		wardrobe.normal = _ren.material;
	}
	
	// Update is called once per frame
	void Update () {
		switch(_state) {
		case WalkingVirusState.Patrol:
			PatrolUpdate();
			break;
		case WalkingVirusState.Camp:
			break;
		case WalkingVirusState.Pursue:
			PursueUpdate();
			break;
		default:
			break;
		}
	}
	
	void PatrolUpdate() {
		_patrolTimer -= Time.deltaTime;
		if (_patrolTimer < 0f) {
			Vector3 destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
			_patrolTimer = 20f;
			_walker.destination = destination;
			_walker.stopDistance = _walker.defaultStopDistance;
		}
	}
	
	void PursueUpdate() {
		if (_target == null) {
			_state = WalkingVirusState.Patrol;
			return;
		}
		_walker.destination = _target.position;
		_walker.stopDistance = 0f;
		if (_walker.atDestination && !_attacking) {
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
		switch(col.tag) {
		case "Player":
			_state = WalkingVirusState.Pursue;
			_target = col.transform;
			break;
		}
	}
	
	void OnTriggerExit(Collider col) {
		switch(col.tag) {
		case "Player":
			_state = WalkingVirusState.Patrol;
			_target = null;
			break;
		}
	}
}
