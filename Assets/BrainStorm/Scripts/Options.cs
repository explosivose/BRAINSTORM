using UnityEngine;
using System.Collections;

public class Options {

	public const string keyVolume = "audioVolume";
	public const string keyMouseSensivity = "mouseSensitivity";
	public const string keyPlayerName = "playerName";

	public static void Load() {
		AudioListener.volume = PlayerPrefs.GetFloat(keyVolume);
		MouseLook.sensitivity = PlayerPrefs.GetFloat(keyMouseSensivity);
		PhotonNetwork.playerName = PlayerPrefs.GetString(keyPlayerName);
	}
	
	public static void Save() {
		PlayerPrefs.SetFloat(keyVolume, AudioListener.volume);
		PlayerPrefs.SetFloat(keyMouseSensivity, MouseLook.sensitivity);
		PlayerPrefs.SetString(keyPlayerName, PhotonNetwork.playerName);
	}
}
