using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NPCFlying))]
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
	private NPCFlying flyer;
	private float wanderTimer = 0f;
	
	void Awake() {
		flyer = GetComponent<NPCFlying>();
		flyer.stopDistance = carrot.TVWatchDistance;
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
			flyer.moveSpeedModifier = 0.3f;
			wanderTimer -= Time.deltaTime;
			if (wanderTimer < 0f) {
				Vector3 destination = transform.position + Random.onUnitSphere * (Random.value * 50f);
				wanderTimer = Vector3.Distance(transform.position, destination) / flyer.moveSpeed;
				flyer.destination = destination;
				flyer.stopDistance = flyer.defaultStopDistance;
			}
		}
		else {
			// watch TV
			flyer.destination = TV.position;
			flyer.stopDistance = carrot.TVWatchDistance;
			flyer.moveSpeedModifier = 1f;
		}
	}
	
	void DiscoveredUpdate() {
		
	}
	
	void AnchoredUpdate() {
		
	}
	
	void FrenzyUpdate() {
		
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
