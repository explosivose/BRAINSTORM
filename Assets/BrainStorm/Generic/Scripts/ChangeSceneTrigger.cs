using UnityEngine;
using System.Collections;

public class ChangeSceneTrigger : MonoBehaviour {

	public enum Scene {
		Lobby, Alone, Rage, Terror
	}
	public Scene changeTo = Scene.Lobby;
	
	void OnTriggerEnter(Collider col) {
		if (col.tag == "Player") {
			switch (changeTo) {
			case Scene.Lobby:
				Application.LoadLevel("lobby");
				break;
			case Scene.Alone:
				Application.LoadLevel("alone");
				break;
			case Scene.Rage:
				Application.LoadLevel("rage");
				break;
			case Scene.Terror:
				Application.LoadLevel("terror");
				break;
			default:
				Debug.LogError("Broken Scene Trigger...");
				break;
			}
		}
	}
}
