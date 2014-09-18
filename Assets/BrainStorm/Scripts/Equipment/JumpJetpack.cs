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
		Player.localPlayer.inventory.hasJetpack = true;
	}
	
	void OnDrop() {
		Player.localPlayer.inventory.hasJetpack = false;
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
		equipment.energy = Player.localPlayer.motor.jetpack.fuel01;
	}	
}
