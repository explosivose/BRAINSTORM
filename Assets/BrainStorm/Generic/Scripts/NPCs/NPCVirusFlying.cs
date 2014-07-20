using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCFlying))]
public class NPCVirusFlying : MonoBehaviour {

	public CharacterStats stats = new CharacterStats();
	public CharacterAudio sounds = new CharacterAudio();
	public CharacterMaterials wardrobe = new CharacterMaterials();
	
	[System.Serializable]
	public class FlyingVirusStats {
		public Transform projectilePrefab;
		public float maxHeight = 100f;
	}
	public FlyingVirusStats virus = new FlyingVirusStats();
	
	private enum FlyingVirusState {
		Patrol, Camp, Pursue
	}
	private FlyingVirusState _state = FlyingVirusState.Patrol;
	
	private NPCFlying flyer;
	
	private float patrolTimer = 0f;
	
	private Transform _target; 
	private bool _attacking = false;
	private bool _hurt = false;
	private MeshRenderer _ren;
	
	// Use this for initialization
	void Start () {
		flyer = GetComponent<NPCFlying>();
		ObjectPool.CreatePool(virus.projectilePrefab);
		_ren = GetComponentInChildren<MeshRenderer>();
		
		wardrobe.normal = _ren.material;
	}
	
	// Update is called once per frame
	void Update () {
		switch(_state) {
		case FlyingVirusState.Patrol:
			PatrolUpdate();
			break;
		case FlyingVirusState.Camp:
			break;
		case FlyingVirusState.Pursue:
			PursueUpdate();
			break;
		default:
			break;
		}
	}
	
	void PatrolUpdate() {
		patrolTimer -= Time.deltaTime;
		if (patrolTimer < 0f) {
			Vector3 destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
			destination.y = Mathf.Min(destination.y, virus.maxHeight);
			patrolTimer = Vector3.Distance(transform.position, destination) / flyer.moveSpeed;
			flyer.destination = destination;
			flyer.stopDistance = flyer.defaultStopDistance;
		}
	}
	
	void PursueUpdate() {
		if (_target == null) {
			_state = FlyingVirusState.Patrol;
			return;
		}
		flyer.destination = _target.position;
		flyer.stopDistance = stats.attackRange;
		if (flyer.atDestination && !_attacking) {
			StartCoroutine( Attack() );
		}
	}
	
	IEnumerator Attack() {
		_attacking = true;
		_ren.material = wardrobe.attacking;
		StartCoroutine( FireProjectile() );
		yield return new WaitForSeconds(0.05f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_attacking = false;
	}
	
	IEnumerator FireProjectile() {
		float t = Random.value * 2 * Mathf.PI;
		Vector3 fireLocation = transform.position;
		fireLocation += transform.up * 1.5f * Mathf.Abs(Mathf.Sin(t));
		fireLocation += transform.right * 1.5f * Mathf.Cos(t);
		Quaternion fireRotation = Quaternion.LookRotation(fireLocation - transform.position);
		Transform i = virus.projectilePrefab.Spawn(fireLocation, fireRotation);
		i.BroadcastMessage("SetTarget", _target);
		yield return new WaitForSeconds(30f);
		if (i != null) i.Recycle();
	}
	
	void OnTriggerEnter(Collider col) {
		switch(col.tag) {
		case "Player":
			_state = FlyingVirusState.Pursue;
			_target = col.transform;
			break;
		}
	}
	
	void OnTriggerExit(Collider col) {
		switch(col.tag) {
		case "Player":
			_state = FlyingVirusState.Patrol;
			_target = null;
			break;
		}
	}
	
	public void Damage(Projectile.DamageInstance damage) {
		stats.health -= damage.damage;
		if (!_hurt) StartCoroutine(Hurt ());
	}
	
	IEnumerator Hurt() {
		_hurt = true;
		_ren.material = wardrobe.hurt;
		yield return new WaitForSeconds(0.1f);
		_ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		_hurt = false;
	}
	
}
