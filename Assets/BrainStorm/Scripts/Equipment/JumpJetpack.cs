using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Jump Jetpack")]
public class JumpJetpack : Equipment {
	
	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	private bool jetpacking = false;
	
	void OnEquip() {
		PlayerInventory.Instance.hasJetpack = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasJetpack = false;
	}
	
	void OnJetpackStart() {
		PlaySound(sounds.start, false);
		jetpacking = true;
	}
	
	void OnJetpackStop() {
		PlaySound(sounds.stop, false);
		jetpacking = false;
	}
	
	void Update() {
		if (jetpacking && !audio.isPlaying) {
			PlaySound(sounds.loop, true);
		}
	}	
}
