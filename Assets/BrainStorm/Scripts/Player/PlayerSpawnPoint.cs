using UnityEngine;
using System.Collections;

public class PlayerSpawnPoint : MonoBehaviour {

	public Transform spawnPoint;
	public bool spawnOnEnable = true;
	public bool spawnOnTrigger = true;
	
	void Awake() {
		if (!spawnPoint) spawnPoint = this.transform;
	}
	
	void OnEnable() {
		if (spawnOnEnable) {
			Player.Instance.transform.position = spawnPoint.position;
			Player.Instance.transform.rotation = spawnPoint.rotation;
		}
	}
	
	void OnTriggerEnter(Collider c) {
		if (c.tag == "Player" && spawnOnTrigger) {
			c.transform.position = spawnPoint.position;
		}
	}
}
