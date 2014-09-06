using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Dash Jetpack")]
public class DashJetpack : MonoBehaviour {

	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	[System.Serializable]
	public class AudioLibrary {
		public float volume;
		public AudioClip jetpackStart;
		public AudioClip jetpackLoop;
		public AudioClip jetpackStop;
	}
	public AudioLibrary sounds = new AudioLibrary();

	void OnEquip() {
		PlayerInventory.Instance.hasDashpack = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasDashpack = false;
	}
	
	void OnDashpackStart() {
		PlaySound(sounds.jetpackStart);
	}
	
	void OnDashpackStop() {
		PlaySound(sounds.jetpackStop);
	}
	
	void PlaySound(AudioClip clip) {
		audio.clip = clip;
		audio.loop = false;
		audio.volume = sounds.volume;
		audio.Play();
	}
}
