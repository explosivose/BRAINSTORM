using UnityEngine;
using System.Collections;

public class PlayerSpawnPoint : MonoBehaviour {

	
	void OnEnable() {
		Player.Instance.transform.position = transform.position;
		Player.Instance.transform.rotation = transform.rotation;
	}
	
}
