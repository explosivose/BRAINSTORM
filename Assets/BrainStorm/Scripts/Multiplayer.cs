using UnityEngine;
using System.Collections;

public class Multiplayer : Photon.MonoBehaviour {

	public static Multiplayer Instance;
	
	public bool showDebug;
	public PhotonLogLevel logLevel;
	public GameObject playerPrefab;
	
	private bool _master = false;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this.gameObject);
		}
	}

	void OnGUI() {
		if (showDebug) {
			
			string message = PhotonNetwork.connectionStateDetailed.ToString();
			
			
			if (PhotonNetwork.inRoom) {
				if (PhotonNetwork.isMasterClient) {
					message += " as MasterClient";
				}
				else {
					message += " as Client";
				}
			}
			
			GUILayout.Label(message);
			
		}
	}

	void OnConnectedToMaster() {
		PhotonNetwork.logLevel = logLevel;
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
			_master = true;
		}
		
		GameManager.Instance.ChangeScene(Scene.Tag.GriefMP);
	}
	
	void OnSceneLoaded() {
		GameObject player = PhotonNetwork.Instantiate(
			playerPrefab.name,
			Vector3.zero,
			Quaternion.identity,
			0
			);
		player.SendMessage("SetLocalPlayer", true);
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer player){ 
		// if a player left and now I'm the MaserClient, that
		// means the master has left the game. I should leave too,
		// because MasterClient was in charge of NPCs and things
		// Later on, instead of leaving, I could take the reigns and
		// re-enable my local NPC behaviours?
		if (PhotonNetwork.isMasterClient && !_master) {
			GameManager.Instance.Restart();
		}
	}
}
