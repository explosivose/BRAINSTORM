using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Spawn Point")]
public class PlayerSpawnPoint : MonoBehaviour {

	public Transform spawnPoint;
	public bool spawnOnEnable = true;
	public bool spawnOnTrigger = true;
	public bool updateOnSceneChange = false;
	
	void Awake() {
		if (!spawnPoint) spawnPoint = this.transform;
	}
	
	void OnEnable() {
		if (spawnOnEnable) {
			Player.LocalPlayer.transform.position = spawnPoint.position;
			Player.LocalPlayer.transform.rotation = spawnPoint.rotation;
		}
	}
	
	void OnDisable() {
		if (updateOnSceneChange) {
			// change spawn point to where the player is now
			// except moved backwards a bit and facing the opposite direction
			spawnPoint.position = Player.LocalPlayer.transform.position;
			spawnPoint.rotation = Player.LocalPlayer.transform.rotation;
			
			spawnPoint.position -= spawnPoint.forward;
			spawnPoint.rotation = Quaternion.LookRotation(-spawnPoint.forward);
		}
	}
	
	void OnTriggerEnter(Collider c) {
		if (c.tag == "Player" && spawnOnTrigger) {
			c.transform.position = spawnPoint.position;
		}
	}
}
