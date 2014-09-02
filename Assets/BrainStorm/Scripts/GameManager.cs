using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// if you press R then the current render settings are copied
	// to the current scene in scenes[] to be saved manually
	// by the game designer
	public bool copyRenderSettings = false;
	public bool changeSceneOnAwake;
	public Scene[] scenes;
	
	public const string GameVersion = "BRAINSTORM v0.3-a";
	
	public static GameManager Instance;	
		
	public bool paused {
		get { return _paused; }
		set {
			_paused = value;
			if (_paused) {
				Time.timeScale = 0f;
				AudioListener.volume = 0f;
				Screen.lockCursor = false;
				GameMenu.Instance.showMenu = true;
			}

			else {
				Time.timeScale = 1f;
				AudioListener.volume = 1f;
				Screen.lockCursor = true;
				GameMenu.Instance.showMenu = false;
			}
		}
	}
	
	
	public bool levelTeardown {
		get { return _levelTeardown; }
	}
	
	public Transform activeScene {
		get {
			if (_activeScene != null) 
				return _activeScene.instance;
			else 
				return null;
		}
	}
	
	private bool _paused;
	private bool _levelTeardown;
	private Scene _activeScene;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		
		transform.position = Vector3.zero;
	}
	
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		Screen.lockCursor = true;
		if (changeSceneOnAwake) {
			ChangeScene(Scene.Tag.Lobby);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!Screen.lockCursor && !paused) {
			paused = true;
		}
		if (Input.GetKeyUp(KeyCode.Escape) && !paused) {
			paused = true;
		}
		if (Application.isEditor & copyRenderSettings) {
			CopyRenderSettings();
		}
	}
	
	void CopyRenderSettings() {
		if (Input.GetKeyUp(KeyCode.R)) {
			foreach(Scene s in scenes) {
				if (s.isLoaded) {
					s.ambientLight = RenderSettings.ambientLight;
					s.fog = RenderSettings.fog;
					s.fogColor = RenderSettings.fogColor;
					s.fogDensity = RenderSettings.fogDensity;
					s.skybox = RenderSettings.skybox;
					Debug.Log ("Render Settings for " + s.name + " copied.");
					break;
				}
			}
		}
	}
	
	public void ChangeScene(Scene.Tag scene) {
		StartCoroutine( ChangeSceneRoutine(scene) );
	}
	
	IEnumerator ChangeSceneRoutine( Scene.Tag scene ) {
		yield return new WaitForEndOfFrame();
		_levelTeardown = true;
		// unload active scene
		if (_activeScene != null)
			_activeScene.Unload();
		// spawn next scene
		foreach(Scene s in scenes) {
			if (s.tag == scene) {
				_activeScene = s;
				_activeScene.Load();
				RenderSettings.ambientLight = _activeScene.ambientLight;
				RenderSettings.fog = _activeScene.fog;
				RenderSettings.fogColor = _activeScene.fogColor;
				RenderSettings.fogDensity = _activeScene.fogDensity;
				RenderSettings.skybox = _activeScene.skybox;
				break;
			}
		}
		_levelTeardown = false;
	}
	
}
