using UnityEngine;
using System.Collections;

public class NPCCarrot : MonoBehaviour {
	
	[System.Serializable]
	public class CarrotStats {
		public float TVWatchDistance;
	}
	public CharacterStats stats = new CharacterStats();
	public CarrotStats carrot = new CarrotStats();
	
	private enum CarrotState {
		Alone, Discovered, Anchored, Frenzied
	}
	/// <summary>
	/// Alone
	///		Carrot wanders around looking for a TV to stare at
	///		Once a TV is found, carrot stares at TV closely
	///		Looks at passerbys periodically</item>
	///
	///	Discovered
	///		If the TV is on the move then the carrot follows the TV
	///		The longer the carrot spends following the TV the less
	///		interested the carrot is in watching the TV</item>
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
	
	private bool atDestination = false;
	
	// Use this for initialization
	void Start () {
		stats.moveSpeed = stats.maxMoveSpeed;
		stats.rotationSpeed = stats.maxRotationSpeed;
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
			// slow and random wandering
			stats.moveSpeed = stats.maxMoveSpeed / 2f;
			FlyTo(Vector3.zero, 1f);
		}
		else {
			// watch TV
			stats.moveSpeed = stats.maxMoveSpeed;
			FlyTo(TV.position, carrot.TVWatchDistance);
		}
	}
	
	void DiscoveredUpdate() {
		
	}
	
	void FlyTo(Vector3 destination, float minimumDistance) {
		Quaternion rotation = Quaternion.LookRotation(destination - transform.position);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, stats.rotationSpeed * Time.deltaTime);
		atDestination = (Vector3.Distance(transform.position, destination) < minimumDistance);
	}
	
	
	void OnTriggerEnter(Collider col) {
		switch (col.tag) {
		case "TV":
			TV = col.transform;
			break;
		default:
			break;
		}
	}
}
