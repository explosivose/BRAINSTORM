using UnityEngine;
using System.Collections;

[System.Serializable]
public class Scene {
	public enum Tag {
		GriefCity,
		RageDesert
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
		get {
			 if (!_sceneInstance) 
			 	return GameManager.Instance.transform;
			 
			 return _sceneInstance;
		}
	}
	
	// all children of entities are local objects to be destroyed on scene change
	public Transform entities {
		get; private set;		
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
		ObjectPool.CreatePool(scenePrefab);
		_sceneInstance = scenePrefab.Spawn();
		entities = _sceneInstance.Find ("Entities");
		if (!entities) Debug.LogError("Entities object not found in scene.");
		TerrainGenerator.Instance.Generate();
		Random.seed = seed;
		Transform t = _sceneInstance.Find ("Building Spawner");
		if (t) {
			Debug.Log ("Creating buildings...");
			t.GetComponent<PrefabSpawner>().Spawn();
		}
	}
	
	public void StartSpawners() {
		Transform t = _sceneInstance.Find("Spawn Location Spawner");
		if (t) {
			Debug.Log ("Spawning spawers...");
			t.GetComponent<PrefabSpawner>().Spawn();
		}	
	}
	
	public void Unload() {
		if (entities) {
			for(int i = 0; i < entities.childCount; i++) {
				entities.GetChild(i).Recycle();
			}
		}
		if (_sceneInstance) _sceneInstance.Recycle();
		isLoaded = false;
	}
}
