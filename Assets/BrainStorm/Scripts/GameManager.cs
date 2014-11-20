using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ScreenFade))]
public class GameManager : MonoBehaviour {

	public static GameManager Instance;	
		
	public bool 		grabCursor = false;
	public bool 		playerPauseEnabled = true;

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
				CTRL.Instance.ShowPauseMenu();
				Player.localPlayer.motor.canControl = false;
				_camRotationBeforePause = Camera.main.transform.localRotation;
				if (!PhotonNetwork.inRoom)
					Time.timeScale = 1f;

			}
			else {
				_paused = false;
				Screen.lockCursor = true && grabCursor;
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
	
	public int masterSeed {
		get; set;
	}
	
	public float timeSinceSceneChange {
		get {
			return Time.time - _sceneChangeTime;
		}
	}
	
	public GameObject defaultCamera {
		get; private set;
	}
	
	public bool roundOver {
		get; private set;
	}

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this.gameObject);
		}
		_header = transform.Find("Header").guiText;
		defaultCamera = transform.Find("Default Camera").gameObject;
		_header.text = Strings.gameTitle + " " + Strings.gameVersion;
		_fade = GetComponent<ScreenFade>();
		transform.position = Vector3.zero;
		Options.Load();
	}
	
	
	void Start () {
		DontDestroyOnLoad(this);
		_activeScene = new Scene();
		StartGame();
	}
	
	// this is called when the game is restarted
	// Application.LoadLevel(Application.loadedLevel);
	void OnLevelWasLoaded() {
		StartGame();
	}
	
	void OnApplicationFocus(bool focus) {

	}
	
	void StartGame() {
		PhotonNetwork.Disconnect();
		PhotonNetwork.offlineMode = true;
		if (Application.loadedLevelName == "0splash") {
			defaultCamera.SetActive(true);
			defaultCamera.camera.backgroundColor = Color.white;
			CTRL.Instance.ShowSplash();
		}
		else if (Application.loadedLevelName == "1menu") {
			defaultCamera.SetActive(true);
			defaultCamera.camera.backgroundColor = Color.black;
			CTRL.Instance.ShowStartMenu();
			Multiplayer.Instance.enabled = true;
		}
		else if (Application.loadedLevelName == "multiplayer") {
			PhotonNetwork.ConnectUsingSettings(Strings.gameVersion);
		}
	}
	
	public void Restart() {
		Application.LoadLevel(1);
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
		if (PhotonNetwork.inRoom) {
			if (Input.GetKeyDown(KeyCode.Tab)) {
				CTRL.Instance.ShowScoreboard();
			}
			if (Input.GetKeyUp(KeyCode.Tab)) {
				CTRL.Instance.HideScoreboard();
			}
		}
	}
	
	public void ChangeScene(Scene.Tag scene) {
		if (timeSinceSceneChange < 1f) return;
		paused = false;
		_sceneChangeTime = Time.time;
		StartCoroutine( ChangeSceneRoutine(scene) );
	}
	
	public void ChangeToRandomScene() {
		float roll = Random.value;
		Scene.Tag scene = Scene.Tag.GriefCity;
		if (roll < 0.5f) {
			scene = Scene.Tag.RageDesert;
		}
		ChangeScene(scene);
	}
	
	private IEnumerator ChangeSceneRoutine( Scene.Tag scene ) {
		_fade.StartFade(Color.black, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForEndOfFrame();
		_levelTeardown = true;
		
		PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player.ID);
		
		// unload active scene
		if (_activeScene.isLoaded) {
			Debug.Log ("Unloading scene " + _activeScene.tag.ToString());
			_activeScene.Unload();
		}
		
		// override the seed with the master seed if we're not the master
		bool overrideSeed = 
			(PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient);
		
		// Load scene settings
		foreach(Scene s in scenes) {
			if (s.tag == scene) {
				_activeScene = s;
				Debug.Log ("Loading scene " + _activeScene.tag.ToString() +
					" with seed override " + overrideSeed);
				
				if (overrideSeed) 
					_activeScene.Load(masterSeed);
				else 
					_activeScene.Load();
					
				RenderSettings.ambientLight = _activeScene.ambientLight;
				RenderSettings.fog = _activeScene.fog;
				RenderSettings.fogColor = _activeScene.fogColor;
				RenderSettings.fogDensity = _activeScene.fogDensity;
				RenderSettings.skybox = _activeScene.skybox;
				
				yield return new WaitForSeconds(0.5f);
				
				_activeScene.StartSpawners();
				break;
			}
		}
		
		// if we're the master, store the scene seed
		if (PhotonNetwork.inRoom  && PhotonNetwork.isMasterClient) {
			masterSeed = activeScene.seed;
			BountyExtensions.SetCashPool(1000);
			Debug.Log ("Master seed set: " + masterSeed);
		}
		
		
		_levelTeardown = false;
		defaultCamera.SetActive(false);
		yield return new WaitForSeconds(0.2f);
		SendMessage("OnSceneLoaded");
		_fade.StartFade(Color.clear, 0.5f);
		yield return new WaitForSeconds(0.5f);
	}
	
	[RPC]
	void RoundOverRPC() {
		roundOver = true;
		Player.localPlayer.motor.canControl = false;
		MouseLook.freeze = true;
		CTRL.Instance.ShowScoreboard();
		if (PhotonNetwork.isMasterClient) {
			StartCoroutine(EndOfRound());
		}
	}
	
	IEnumerator EndOfRound() {
		yield return new WaitForSeconds(15f);
		ChangeToRandomScene();
	}
	
	public void Quit() {
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
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
