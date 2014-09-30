using UnityEngine;
using System.Collections;

public class CTRL : MonoBehaviour {

	public static CTRL Instance;
	
	// implement our own deltaTime for Update() functions
	// because Time.deltaTime is useless while Time.timeScale = 0f
	// i.e. when the game is paused.
	public static float deltaTime {
		get; private set;
	}
	private static float lastCallTime;
	
	public static int level {get; set;}
	public static Vector3 zeroDirection = Vector3.forward;
	
	public Font font;
	
	public Transform startPrefab;
	public Transform pausePrefab;
	
	private Transform startInstance;
	private Transform pauseInstance;

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this.gameObject);
		}
	}

	void Update() {
		deltaTime = Time.realtimeSinceStartup - lastCallTime;
		lastCallTime = Time.realtimeSinceStartup;
	}

	public void ShowStartMenu() {
		HideStartMenu();
		zeroDirection = Camera.main.transform.forward;
		Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 5f;
		Quaternion rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
		startInstance = startPrefab.Spawn(position, rotation);
	}
	
	public void HideStartMenu() {
		if (startInstance)
			startInstance.Recycle();
	}

	public void ShowPauseMenu() {
		if (startInstance) return;
		HidePauseMenu();
		Transform mainCam = Camera.main.transform;
		zeroDirection = mainCam.forward;
		Vector3 position = mainCam.position + mainCam.forward * 7f;
		Quaternion rotation = Quaternion.LookRotation(position - mainCam.position);
		pauseInstance = pausePrefab.Spawn(position, rotation);
	}
	
	public void HidePauseMenu() {
		if (pauseInstance)
			pauseInstance.Recycle();
	}
}
