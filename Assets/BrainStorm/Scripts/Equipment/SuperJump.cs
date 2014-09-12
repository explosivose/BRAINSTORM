using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Super Jump")]
public class SuperJump : Equipment {
	
	//  behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour
	
	void OnEquip() {
		PlayerInventory.Instance.hasSuperJump = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasSuperJump = false;
	}
	
	void PlaySound(AudioClip clip) {
		audio.clip = clip;
		audio.loop = false;
		audio.volume = sounds.volume;
		audio.Play();
	}
}
