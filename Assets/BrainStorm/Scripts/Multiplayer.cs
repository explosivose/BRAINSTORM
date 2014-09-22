using UnityEngine;
using System.Collections;

public class Multiplayer : Photon.MonoBehaviour {

	public static Multiplayer Instance;
	
	public bool showDebug;
	public PhotonLogLevel logLevel;
	public GameObject playerPrefab;
	
	private bool _master = false;
	private bool _waitingForSeed = false;
	
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
				
				GUILayout.Label(message);
				
				GUILayout.Label("Pringles: " + PhotonNetwork.networkingPeer.RoundTripTime);
				
				if (_waitingForSeed) {
					GUILayout.Label("Waiting for seed from master...");
				}
				else {
					GUILayout.Label("Seed: " + GameManager.Instance.masterSeed);
				}
				
			}
			else {
				GUILayout.Label(message);
			}
			
			

			
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
	
	IEnumerator OnJoinedRoom() {
		
		bool failed = false;
		
		if (PhotonNetwork.isMasterClient) {
			_master = true;
		}
		else {
			_waitingForSeed = true;
			float waitStarted = Time.time;
			while (_waitingForSeed && !failed) {
				yield return new WaitForSeconds(0.1f);
				if (waitStarted + 10f < Time.time) {
					Debug.LogError("Timed out waiting for seed.");
					failed = true;
				}
			}
		}
		
		_waitingForSeed = false;
		
		if (failed)
			GameManager.Instance.Restart();
		else
			GameManager.Instance.ChangeScene(Scene.Tag.GriefMP);
	}
	
	[RPC]
	void SetMasterSeed(int seed) {
		Debug.Log("Seed received: " + seed);
		GameManager.Instance.masterSeed = seed;
		_waitingForSeed = false;
	}
	
	void OnSceneLoaded() {
		GameObject player = PhotonNetwork.Instantiate(
			playerPrefab.name,
			Vector3.zero,
			Quaternion.identity,
			0
			);
	}
	
	void Restart() {
		_master = false;
		_waitingForSeed = false;
	}
	
	void OnPhotonPlayerConnected(PhotonPlayer player) {
		if (PhotonNetwork.isMasterClient) {
			int seed = GameManager.Instance.masterSeed;
			photonView.RPC ("SetMasterSeed", player, seed);
		}
		
	}
	
	void OnPhotonPlayerDisconnected(PhotonPlayer player) { 
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
