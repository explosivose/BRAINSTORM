using UnityEngine;
using System.Collections;

[System.Serializable]
public class Scene {
	public enum Tag {
		Lobby, Armory, Alone, Rage, Terror
	}
	
	public string name {
		get {
			return tag.ToString();
		}
	}
	public Tag tag;
	public Transform scenePrefab;
	
}

public class ChangeSceneTrigger : MonoBehaviour {

	public Scene.Tag changeTo = Scene.Tag.Lobby;
	
	void OnTriggerEnter(Collider col) {
		if (col.tag == "Player") {
			GameManager.Instance.ChangeScene(changeTo);
		}
	}
}
