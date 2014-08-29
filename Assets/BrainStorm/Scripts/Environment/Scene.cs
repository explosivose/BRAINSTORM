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
	public Tag 			tag;
	public Transform 	scenePrefab;
	public bool 		fog;
	public Color 		fogColor;
	public float 		fogDensity;
	public Color		ambientLight;
	public Material 	skybox;
	
	private Transform _sceneInstance;
	
	public bool isLoaded {
		get; private set;
	}
	
	public Transform instance {
		get { return _sceneInstance; }
	}
	
	public void Load() {
		ObjectPool.CreatePool(scenePrefab);
		_sceneInstance = scenePrefab.Spawn();
		isLoaded = true;
	}
	
	public void Unload() {
		if (_sceneInstance) _sceneInstance.Recycle();
		isLoaded = false;
	}
}
