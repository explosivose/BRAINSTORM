using UnityEngine;
using System.Collections;

public class Multiplayer : Photon.MonoBehaviour {

	public static Multiplayer Instance;
	
	public GameObject playerPrefab;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this.gameObject);
		}
	}

	// joined photon lobby, time to join or start a game
	void OnJoinedLobby() {
		PhotonNetwork.JoinRandomRoom();
	}
	
	void OnPhotonRandomJoinFailed() {
		Debug.Log ("Can't join random room. Creating one instead.");
		PhotonNetwork.CreateRoom(null);
	}
	
	void OnJoinedRoom() {
		if (PhotonNetwork.isMasterClient) {
			GameManager.Instance.ChangeScene(Scene.Tag.GriefMP);
		}
		else {
			GameManager.Instance.ChangeRenderSettings(Scene.Tag.GriefMP);
		}
		
		GameObject player = PhotonNetwork.Instantiate(
			playerPrefab.name,
			Vector3.zero,
			Quaternion.identity,
			0
		);
		player.SendMessage("SetLocalPlayer", true);
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer player){ 
		if (player.isMasterClient) {
			PhotonNetwork.Disconnect();
		}
	}
}
