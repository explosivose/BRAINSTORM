using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Super Jump")]
public class SuperJump : MonoBehaviour {
	
	//  behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour
	
	[System.Serializable]
	public class AudioLibrary {
		public float volume = 0.25f;
		public AudioClip start;
		public AudioClip loop;
		public AudioClip stop;
	}
	
	public AudioLibrary sounds = new AudioLibrary();
	
	void OnEquip() {
		PlayerInventory.Instance.hasSuperJump = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasSuperJump = false;
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
