using UnityEngine;
using System.Collections;

public class CTRL : MonoBehaviour {

	public static CTRL Instance;

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

	void Start() {
		ObjectPool.CreatePool(pausePrefab);
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
