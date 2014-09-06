using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;	
	
	// if you press R then the current render settings are copied
	// to the current scene in scenes[] to be saved manually
	// by the game designer
	public bool copyRenderSettings = false;
	public bool changeSceneOnLoad;
	public Scene[] scenes;
	
	
		
	public bool paused {
		get { return _paused; }
		set {
			_paused = value;
			if (_paused) {
				_camRotationBeforePause = Camera.main.transform.localRotation;
				Time.timeScale = 0f;
				AudioListener.volume = 0f;
				Screen.lockCursor = false;
				CTRL.Instance.ShowPauseMenu();
			}

			else {
				Camera.main.transform.localRotation = _camRotationBeforePause;
				Time.timeScale = 1f;
				AudioListener.volume = 1f;
				Screen.lockCursor = true;
				CTRL.Instance.HidePauseMenu();
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
	private Quaternion _camRotationBeforePause;
	private GUIText _header;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this.gameObject);
		}
		_header = transform.Find("Header").guiText;
		_header.text = Strings.gameVersion;
		transform.position = Vector3.zero;
	}
	
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		StartGame();
	}
	
	void OnLevelWasLoaded() {
		StartGame();
	}
	
	void StartGame() {
		Screen.lockCursor = true;
		if (changeSceneOnLoad) {
			ChangeScene(Scene.Tag.Lobby);
		}
		paused = false;
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
