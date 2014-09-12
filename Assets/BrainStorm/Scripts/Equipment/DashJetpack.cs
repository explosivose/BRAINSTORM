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
		PlayerInventory.Instance.hasDashpack = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasDashpack = false;
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
		equipment.energy = Player.Instance.motor.dashpack.fuel01;
	}
}
