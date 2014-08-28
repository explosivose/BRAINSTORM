using UnityEngine;
using System.Collections;

public class ChangeSceneTrigger : MonoBehaviour {

	public Scene.Tag changeTo = Scene.Tag.Lobby;
	
	void OnTriggerEnter(Collider col) {
		if (col.tag == "Player") {
			GameManager.Instance.ChangeScene(changeTo);
		}
	}
}
