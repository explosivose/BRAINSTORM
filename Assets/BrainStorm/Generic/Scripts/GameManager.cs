using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;	
	
	public bool paused {
		get { return _paused; }
		set {
			_paused = value;
			if (_paused) {
				Screen.lockCursor = false;
				Time.timeScale = 0f;
				AudioListener.volume = 0f;
			}

			else {
				Screen.lockCursor = true;
				Time.timeScale = 1f;
				AudioListener.volume = 1f;
			}
			
		}
	}
	private bool _paused;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}
	
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		Screen.lockCursor = true;
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
	
	void OnGUI() {
		if (!paused) return;
		if (GUI.Button(new Rect(10, 10, 150, 100), "resume")) {
			paused = false;
		}
	}
}
