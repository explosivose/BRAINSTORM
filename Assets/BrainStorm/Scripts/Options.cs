using UnityEngine;
using System.Collections;

public class Options {

	public const string keyVolume = "audioVolume";
	public const string keyMouseSensivity = "mouseSensitivity";
	public const string keyPlayerName = "playerName";

	public static void Load() {
		AudioListener.volume = PlayerPrefs.GetFloat(keyVolume, 0.5f);
		MouseLook.sensitivity = PlayerPrefs.GetFloat(keyMouseSensivity, 5f);
		PhotonNetwork.playerName = PlayerPrefs.GetString(keyPlayerName, "Enter Your Name");
	}
	
	public static void Save() {
		PlayerPrefs.SetFloat(keyVolume, AudioListener.volume);
		PlayerPrefs.SetFloat(keyMouseSensivity, MouseLook.sensitivity);
		PlayerPrefs.SetString(keyPlayerName, PhotonNetwork.playerName);
	}
	
	public static void Reset() {
		PlayerPrefs.DeleteAll();
		Load();
	}
}
