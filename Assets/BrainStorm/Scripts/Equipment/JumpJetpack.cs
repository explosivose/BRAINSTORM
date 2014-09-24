using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Jump Jetpack")]
public class JumpJetpack : MonoBehaviour {
	
	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	private Equipment equipment;
	private bool jetpacking = false;
	
	void Awake() {
		equipment = GetComponent<Equipment>();
	}
	
	void OnEquip() {
		equipment.owner.inventory.hasJetpack = true;
	}
	
	void OnDrop() {
		equipment.owner.inventory.hasJetpack = false;
	}
	
	void OnJetpackStart() {
		equipment.AudioStart();
		jetpacking = true;
	}
	
	void OnJetpackStop() {
		equipment.AudioStop();
		jetpacking = false;
	}
	
	void Update() {
		if (jetpacking && !audio.isPlaying) {
			equipment.AudioLoop();
		}
		if (equipment.equipped)
			equipment.energy = equipment.owner.motor.jetpack.fuel01;
	}	
}
