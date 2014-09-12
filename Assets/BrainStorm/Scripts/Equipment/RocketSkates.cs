using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Rocket Skates")]
public class RocketSkates : Equipment {
	
	//  behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour
	
	void OnEquip() {
		PlayerInventory.Instance.hasSprint = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasSprint = false;
	}
	
	void OnSprintStart() {
		PlaySound(sounds.start);
	}
	
	void OnSprintStop() {
		PlaySound(sounds.stop);
	}
	
	void PlaySound(AudioClip clip) {
		audio.clip = clip;
		audio.loop = false;
		audio.volume = sounds.volume;
		audio.Play();
	}
}
