using UnityEngine;
using System.Collections;

public class NPCCarrot : MonoBehaviour {
	
	[System.Serializable]
	public class CharacterStats {
		public float maxMoveSpeed;	// speed limit
		[System.NonSerialized]
		public float moveSpeed;		// speed in use
		public float rotationSpeed;	
		public float TVSearchRadius;
		public float TVSearchTime;
	}
	public CharacterStats stats = new CharacterStats();
	
	private enum CarrotState {
		Alone, Discovered, Anchored, Frenzied
	}
	/// <summary>
	/// Alone
	///		Carrot wanders around looking for a TV to stare at
	///		Once a TV is found, carrot stares at TV closely
	///		Looks at passerbys periodically
	///	Discovered
	///		If the TV is on the move then the carrot follows the TV
	///		The longer the carrot spends following the TV the less
	///		interested the carrot is in watching the TV
	///		 
	///	Anchored
	///		Carrot is brought to a carrot anchor where it will 
	///		fly happily around with other carrots until there
	///		are enough carrots there to make a frenzied swarm
	///
	///	Frenzied
	///		Swarm flight with other carrots targetting Virus NPCs
	
	/// </summary>
	private CarrotState state = CarrotState.Alone;
	
	/// <summary>
	/// The TV to watch
	/// </summary>
	private Transform TV = null;
	private bool TVSearching = false;
	
	private bool atDestination = false;
	
	// Use this for initialization
	void Start () {
		stats.moveSpeed = stats.maxMoveSpeed;
	}
	
	void FixedUpdate() {
		Vector3 force = transform.forward * stats.moveSpeed;
		if (atDestination) force = Vector3.zero;
		rigidbody.AddForce(force);
	}
	
	// Update is called once per frame
	void Update () {
		switch(state) {
		case CarrotState.Alone:
			AloneUpdate();
			break;
		case CarrotState.Discovered:
			break;
		case CarrotState.Anchored:
			break;
		case CarrotState.Frenzied:
			break;
		default:
			break;
		}
	}
	
	void AloneUpdate() {
		if (TV == null) {
			stats.moveSpeed = stats.maxMoveSpeed / 2f;
			if (!TVSearching) StartCoroutine( TVSearch() );
			FlyTo(Vector3.zero, 1f);
		}
		else {
			stats.moveSpeed = stats.maxMoveSpeed;
			FlyTo(TV.position, 1f);
		}
	}
	
	void DiscoveredUpdate() {
		
	}
	
	void FlyTo(Vector3 destination, float minimumDistance) {
		Quaternion rotation = Quaternion.LookRotation(destination - transform.position);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, stats.rotationSpeed * Time.deltaTime);
		atDestination = (Vector3.Distance(transform.position, destination) < minimumDistance);
	}
	
	IEnumerator TVSearch() {
		TVSearching = true;
		RaycastHit[] hits;
		Ray ray = new Ray(transform.position, transform.forward);
		hits = Physics.SphereCastAll(ray, stats.TVSearchRadius, 0f);
		foreach(RaycastHit hit in hits) {
			if (hit.transform.tag == "TV") {
				if (TV != null) {
					float TVDist = Vector3.Distance(TV.position, transform.position);
					float HitDist = Vector3.Distance(hit.point, transform.position);
					if (HitDist < TVDist) {
						TV = hit.transform;
					}
				}
				else {
					TV = hit.transform;
				}
			}
		}
		yield return new WaitForSeconds(stats.TVSearchTime);
		TVSearching = false;
	}
}
