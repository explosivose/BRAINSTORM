using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Player Spawn")]
[RequireComponent(typeof(BoxCollider))]
public class PlayerSpawn : MonoBehaviour {

	public static PlayerSpawn Multiplayer;

	public Transform spawnPoint;
	public bool spawnOnEnable = true;
	public bool spawnOnTrigger = true;
	public bool updateOnSceneChange = false;
	public bool multiplayer = false;
	
	public Vector3 randomSpawnPosition {
		get {
			Vector3 pos = PrefabSpawner.randomPositionIn(collider.bounds); 
			RaycastHit hit;
			if (Physics.Raycast(pos, Vector3.down, out hit, 1000f)) {
				pos.y = hit.point.y + 2f;
			}
			return pos;
		}
	}
	
	void Awake() {
		if (!spawnPoint) spawnPoint = this.transform;
		if (multiplayer) Multiplayer = this;
	}
	
	void OnEnable() {
		if (spawnOnEnable) {
			Player.localPlayer.transform.position = spawnPoint.position;
			Player.localPlayer.transform.rotation = spawnPoint.rotation;
		}
	}
	
	void OnDisable() {
		if (updateOnSceneChange) {
			// change spawn point to where the player is now
			// except moved backwards a bit and facing the opposite direction
			spawnPoint.position = Player.localPlayer.transform.position;
			spawnPoint.rotation = Player.localPlayer.transform.rotation;
			
			spawnPoint.position -= spawnPoint.forward;
			spawnPoint.rotation = Quaternion.LookRotation(-spawnPoint.forward);
		}
	}
	
	void OnTriggerEnter(Collider c) {
		if (c.tag == "Player" && spawnOnTrigger) {
			if (Multiplayer == null) {
				c.transform.position = spawnPoint.position;
			}
			else {
				c.transform.position = Multiplayer.randomSpawnPosition;
			}
		}
	}
}
