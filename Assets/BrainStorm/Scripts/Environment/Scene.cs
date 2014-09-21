using UnityEngine;
using System.Collections;

[System.Serializable]
public class Scene {
	public enum Tag {
		Lobby, 
		Grief, 
		Rage, 
		Terror,
		Joy,
		Calm,
		Safety,
		GriefMP
	}
	
	public string name {
		get {
			return tag.ToString();
		}
	}
	public Tag 			tag;
	public bool			multiplayer;
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
	
	public int seed {
		get; private set;
	}
	
	public void Load() {
		seed = Random.seed;
		LoadScene();
		isLoaded = true;
	}
	
	public void Load(int seedOverride) {
		Random.seed = seedOverride;
		seed = seedOverride;
		LoadScene();
		isLoaded = true;
	}
	
	private void LoadScene() {
		/*
		if (multiplayer) {
			_sceneInstance = PhotonNetwork.InstantiateSceneObject(
				"Scenes/" + scenePrefab.name,
				Vector3.zero,
				Quaternion.identity,
				0,
				null
				).transform;
		}
		else {*/
			ObjectPool.CreatePool(scenePrefab);
			_sceneInstance = scenePrefab.Spawn();
		//}
	}
	
	public void Unload() {
		if (_sceneInstance) _sceneInstance.Recycle();
		isLoaded = false;
	}
}
