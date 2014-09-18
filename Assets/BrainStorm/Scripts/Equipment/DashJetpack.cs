using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Dash Jetpack")]
public class DashJetpack : MonoBehaviour {

	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	private Equipment equipment;
	private bool jetpacking = false;
	
	void Awake() {
		equipment = GetComponent<Equipment>();
	}

	void OnEquip() {
		Player.localPlayer.inventory.hasDashpack = true;
	}
	
	void OnDrop() {
		Player.localPlayer.inventory.hasDashpack = false;
	}
	
	void OnDashpackStart() {
		equipment.AudioStart();
		jetpacking = true;
	}
	
	void OnDashpackStop() {
		equipment.AudioStop();
		jetpacking = false;
	}
	
	void Update() {
		if (jetpacking && !audio.isPlaying) {
			equipment.AudioStart();
		}
		equipment.energy = Player.localPlayer.motor.dashpack.fuel01;
	}
}
