using UnityEngine;
using System.Collections;

public class NPCFlyingVirus : MonoBehaviour {

	public CharacterStats 	stats = new CharacterStats();

	[System.Serializable]
	public class FlyingVirusStats {
		public float attackDistance = 10f;
		public float maxHeight = 100f;
	}
	
	public FlyingVirusStats virus = new FlyingVirusStats();
	
	[System.Serializable]
	public class FlyingVirusAudio {
		public AudioClip attack;
		public AudioClip hurt;
	}
	
	public FlyingVirusAudio audio = new FlyingVirusAudio();
	
	[System.Serializable]
	public class FlyingVirusMaterials {
		public Material attack;
		public Material hurt;
		[System.NonSerialized]
		public Material normal;
	}
	
	public FlyingVirusMaterials wardrobe = new FlyingVirusMaterials();
	
	private enum FlyingVirusState {
		Patrol, Camp, Pursue
	}
	private FlyingVirusState state = FlyingVirusState.Patrol;
	
	private Vector3 destination = Vector3.zero;
	private bool atDestination = false;
	private float patrolTimer = 0f;
	
	private Transform target; 
	private bool attacking = false;
	
	private MeshRenderer ren;
	
	// Use this for initialization
	void Start () {
		stats.moveSpeed = stats.maxMoveSpeed;
		stats.rotationSpeed = stats.maxRotationSpeed;
		ren = GetComponentInChildren<MeshRenderer>();
		wardrobe.normal = ren.material;
	}
	
	void FixedUpdate() {
		Vector3 force = transform.forward * stats.moveSpeed;
		if (atDestination) force = Vector3.zero;
		rigidbody.AddForce(force);
	}
	
	// Update is called once per frame
	void Update () {
		switch(state) {
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
			destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
			destination.y = Mathf.Min(destination.y, virus.maxHeight);
			patrolTimer = Vector3.Distance(transform.position, destination) / stats.moveSpeed;
		}
		FlyToDestination(1f);
	}
	
	void PursueUpdate() {
		if (target == null) {
			state = FlyingVirusState.Patrol;
			return;
		}
		destination = target.position;
		FlyToDestination(virus.attackDistance);
		if (atDestination && !attacking) {
			StartCoroutine( Attack() );
		}
	}
	
	void FlyToDestination(float minimumDistance) {
		Quaternion rotation = Quaternion.LookRotation(destination - transform.position);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, stats.rotationSpeed * Time.deltaTime);
		atDestination = (Vector3.Distance(transform.position, destination) < minimumDistance);
		Debug.DrawLine(transform.position, destination);
	}
	
	IEnumerator Attack() {
		attacking = true;
		ren.material = wardrobe.attack;
		yield return new WaitForSeconds(0.1f);
		ren.material = wardrobe.normal;
		yield return new WaitForSeconds(0.1f);
		attacking = false;
	}
	
	void OnTriggerEnter(Collider col) {
		switch(col.tag) {
		case "Player":
			state = FlyingVirusState.Pursue;
			target = col.transform;
			break;
		}
	}
	
	void OnTriggerExit(Collider col) {
		switch(col.tag) {
		case "Player":
			state = FlyingVirusState.Patrol;
			target = null;
			break;
		}
	}
	
}
