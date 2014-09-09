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
	
	public Transform pausePrefab;
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

	public void ShowPauseMenu() {
		Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * 5f;
		Quaternion rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
		pauseInstance = pausePrefab.Spawn(position, rotation);
	}
	
	public void HidePauseMenu() {
		if (pauseInstance)
			pauseInstance.Recycle();
	}
}
