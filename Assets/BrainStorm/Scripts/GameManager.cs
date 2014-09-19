using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ScreenFade))]
public class GameManager : MonoBehaviour {

	public static GameManager Instance;	
	
	[System.Flags]
	public enum WinStates {
		None 	= 0x00,
		Grief 	= 0x01,
		Rage	= 0x02,
		Terror 	= 0x04,
		All		= 0xFF
	}
	
	public bool 		grabCursor = false;
	public bool 		playerPauseEnabled = true;

	[EnumMask]
	public WinStates 	winState = WinStates.None;
	public Scene[] 		scenes;
	public Scene 		copyScene;
	// if you press R then the current render settings are copied
	// to the current scene in scenes[] to be saved manually
	// by the game designer
	public bool 		copyRenderSettings = false;
	
	private bool 		_paused;
	private bool 		_levelTeardown;
	private float 		_sceneChangeTime = -999f;
	private Scene 		_activeScene;
	private Quaternion 	_camRotationBeforePause;
	private GUIText 	_header;
	private ScreenFade 	_fade;
	
	public bool paused {
		get { return _paused; }
		set {
			// don't try to pause if there is no player or camera
			if (!Player.localPlayer) {
				Debug.LogWarning("Won't pause/unpause: player instance not found.");
				return;
			}
			if (!Camera.main) {
				Debug.LogWarning("Won't pause/unpause: player found, but no camera found!");
				return;
			}
			
			
			if (value && playerPauseEnabled) {
				_paused = true;
				Screen.lockCursor = false;
				AudioListener.volume = 0f;
				CTRL.Instance.ShowPauseMenu();
				Player.localPlayer.motor.canControl = false;
				_camRotationBeforePause = Camera.main.transform.localRotation;
				if (!PhotonNetwork.inRoom)
					Time.timeScale = 1f;

			}
			else {
				_paused = false;
				Screen.lockCursor = true && grabCursor;
				AudioListener.volume = 1f;
				CTRL.Instance.HidePauseMenu();
				Player.localPlayer.motor.canControl = true;
				Camera.main.transform.localRotation = 
						playerPauseEnabled ? 
							_camRotationBeforePause : 
							Camera.main.transform.localRotation;
				if (!PhotonNetwork.inRoom)
					Time.timeScale = 1f;

			}
		}
	}
	
	public bool levelTeardown {
		get { return _levelTeardown; }
	}
	
	public Scene activeScene {
		get {
			return _activeScene;
		}
	}
	
	public float timeSinceSceneChange {
		get {
			return Time.time - _sceneChangeTime;
		}
	}
	
	public bool griefComplete {
		get {
			return (
				(winState & WinStates.Grief) == WinStates.Grief &&
				winState != WinStates.None
			);
		}
		set {
			if (value) 
				winState |= WinStates.Grief;
			else 
				winState &= ~WinStates.Grief;
		}
	}
	
	public bool rageComplete {
		get {
			return (
				(winState & WinStates.Rage) == WinStates.Rage &&
				winState != WinStates.None
			);
		}
		set {
			if (value) 
				winState |= WinStates.Rage;
			else 
				winState &= ~WinStates.Rage;
		}
	}
	
	public bool terrorComplete {
		get {
			return  (
				(winState & WinStates.Terror) == WinStates.Terror &&
				winState != WinStates.None
			);
		}
		set {
			if (value)
				winState |= WinStates.Terror;
			else
				winState &= ~WinStates.Terror;
		}
	}

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this.gameObject);
		}
		_header = transform.Find("Header").guiText;
		_header.text = Strings.gameTitle + " " + Strings.gameVersion;
		_fade = GetComponent<ScreenFade>();
		transform.position = Vector3.zero;
	}
	
	
	void Start () {
		DontDestroyOnLoad(this);
		StartGame();
	}
	
	// this is called when the game is restarted
	// Application.LoadLevel(Application.loadedLevel);
	void OnLevelWasLoaded() {
		StartGame();
	}
	
	void StartGame() {
		if (Application.loadedLevel == 0) {
			CTRL.Instance.ShowStartMenu();
		}
		else if (Application.loadedLevelName == "brainstorm") {
			ChangeScene(Scene.Tag.Lobby);
		}
		else if (Application.loadedLevelName == "multiplayer") {
			PhotonNetwork.ConnectUsingSettings(Strings.gameVersion);
		}
		//paused = false;
	}
	
	public void Restart() {
		PhotonNetwork.Disconnect();
		Application.LoadLevel(0);
	}
	
	void Update () {
		if (!Player.localPlayer) return;
		if (!Screen.lockCursor && !paused) {
			paused = true;
		}
		if (Input.GetKeyUp(KeyCode.Escape)) {
			paused = !paused;
		}
		if (Application.isEditor & copyRenderSettings) {
			CopyRenderSettings();
		}
	}
	
	
	public void SceneComplete() {
		switch (_activeScene.tag) {
		case Scene.Tag.Grief:
			ChangeScene(Scene.Tag.Joy);
			break;
		case Scene.Tag.Rage:
			ChangeScene(Scene.Tag.Calm);
			break;
		case Scene.Tag.Terror:
			ChangeScene(Scene.Tag.Safety);
			break;
		default:
			Debug.LogError("SceneComplete() called inappropriately in " +
				_activeScene.tag.ToString() + " scene.");
			break;
		}
	}
	
	public void ChangeScene(Scene.Tag scene) {
		if (timeSinceSceneChange < 1f) return;
		paused = false;
		_sceneChangeTime = Time.time;
		StartCoroutine( ChangeSceneRoutine(scene) );
	}
	
	private IEnumerator ChangeSceneRoutine( Scene.Tag scene ) {
		_fade.StartFade(Color.black, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForEndOfFrame();
		_levelTeardown = true;
		// unload active scene
		if (_activeScene != null) {
			Debug.Log ("Unloading scene " + _activeScene.tag.ToString());
			_activeScene.Unload();
		}
			
		// load prefab if we're not in a multiplayer game, or if we're the master in a multiplayer game
		bool loadPrefab = !PhotonNetwork.inRoom ||
			(PhotonNetwork.inRoom && PhotonNetwork.isMasterClient);
			
		// Load scene settings
		foreach(Scene s in scenes) {
			if (s.tag == scene) {
				_activeScene = s;
				if (loadPrefab) {
					Debug.Log ("Loading scene " + _activeScene.tag.ToString());
					_activeScene.Load();
				}
				RenderSettings.ambientLight = _activeScene.ambientLight;
				RenderSettings.fog = _activeScene.fog;
				RenderSettings.fogColor = _activeScene.fogColor;
				RenderSettings.fogDensity = _activeScene.fogDensity;
				RenderSettings.skybox = _activeScene.skybox;
				break;
			}
		}
		_levelTeardown = false;
		SendMessage("OnSceneLoaded");
		yield return new WaitForEndOfFrame();
		_fade.StartFade(Color.clear, 0.5f);
		yield return new WaitForSeconds(0.5f);
	}
	
	
	void CopyRenderSettings() {
		if (Input.GetKeyUp(KeyCode.R)) {
			copyScene = new Scene();
			copyScene.ambientLight = RenderSettings.ambientLight;
			copyScene.fog = RenderSettings.fog;
			copyScene.fogColor = RenderSettings.fogColor;
			copyScene.fogDensity = RenderSettings.fogDensity;
			copyScene.skybox = RenderSettings.skybox;
		}
	}
}
