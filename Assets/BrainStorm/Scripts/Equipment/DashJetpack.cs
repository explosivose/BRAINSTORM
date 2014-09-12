using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Dash Jetpack")]
public class DashJetpack : Equipment {

	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	private bool jetpacking = false;

	void OnEquip() {
		PlayerInventory.Instance.hasDashpack = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasDashpack = false;
	}
	
	void OnDashpackStart() {
		PlaySound(sounds.start, false);
		jetpacking = true;
	}
	
	void OnDashpackStop() {
		PlaySound(sounds.stop, false);
		jetpacking = false;
	}
	
	void Update() {
		if (jetpacking && !audio.isPlaying) {
			PlaySound(sounds.loop, true);
		}
	}
}
