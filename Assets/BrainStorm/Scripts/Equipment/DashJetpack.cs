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

	private bool jetpacking = false;

	void OnEquip() {
		PlayerInventory.Instance.hasDashpack = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasDashpack = false;
	}
	
	void OnDashpackStart() {
		PlaySound(sounds.jetpackStart, false);
		jetpacking = true;
	}
	
	void OnDashpackStop() {
		PlaySound(sounds.jetpackStop, false);
		jetpacking = false;
	}
	
	void Update() {
		if (jetpacking && !audio.isPlaying) {
			PlaySound(sounds.jetpackLoop, true);
		}
	}
	
	void PlaySound(AudioClip clip, bool loop) {
		audio.clip = clip;
		audio.loop = loop;
		audio.volume = sounds.volume;
		audio.Play();
	}
}
