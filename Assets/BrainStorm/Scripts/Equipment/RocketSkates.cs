using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Rocket Skates")]
public class RocketSkates : MonoBehaviour {
	
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
