using UnityEngine;
using System.Collections;

public class LightningEffect : MonoBehaviour {

	void OnEnable() {
		transform.LookAt(Player.localPlayer.transform);
	}
}
