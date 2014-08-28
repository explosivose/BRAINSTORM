using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public bool changeSceneOnAwake;
	public Scene[] scenes;
	
	public const string GameVersion = "BRAINSTORM v0.2-dev";
	
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
		get { return _activeScene; }
	}
	
	private bool _paused;
	private bool _levelTeardown;
	private Transform _activeScene = null;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		
		transform.position = Vector3.zero;
		foreach(Scene s in scenes) {
			ObjectPool.CreatePool(s.scenePrefab);
		}
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
	}
	
	public void ChangeScene(Scene.Tag scene) {
		StartCoroutine( ChangeSceneRoutine(scene) );
	}
	
	IEnumerator ChangeSceneRoutine( Scene.Tag scene ) {
		yield return new WaitForEndOfFrame();
		_levelTeardown = true;
		if (_activeScene)
			_activeScene.Recycle();
		foreach(Scene s in scenes) {
			if (s.tag == scene) {
				_activeScene = s.scenePrefab.Spawn();
				break;
			}
		}
		_levelTeardown = false;
	}
	
}
