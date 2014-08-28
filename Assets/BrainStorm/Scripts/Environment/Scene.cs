using UnityEngine;
using System.Collections;

[System.Serializable]
public class Scene {
	public enum Tag {
		Lobby, Alone, Rage, Terror
	}
	
	public string name {
		get {
			return tag.ToString();
		}
	}
	public Tag tag;
	public Transform scenePrefab;
	
}
