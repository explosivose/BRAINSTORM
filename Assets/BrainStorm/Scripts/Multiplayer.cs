using UnityEngine;
using System.Collections;

public class Multiplayer : Photon.MonoBehaviour {

	public static Multiplayer Instance;
	
	public bool showDebug;
	public PhotonLogLevel logLevel;
	public GameObject playerPrefab;
	
	private bool _master = false;
	private bool _waitingForLevel = false;
	
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
			
			message += " as " + PhotonNetwork.playerName;
			
			if (PhotonNetwork.inRoom) {

				if (PhotonNetwork.isMasterClient) {
					message += " (master)";
				}
				
				message += " (" + PhotonNetwork.playerList.Length.ToString() + ")";
				
				GUILayout.Label(message);
				
				// this is the time to and from the photon server (not the MasterClient)
				GUILayout.Label("Pringles: " + PhotonNetwork.networkingPeer.RoundTripTime);
				
				if (_waitingForLevel) {
					GUILayout.Label("Waiting for seed from master...");
				}
				else {
					GUILayout.Label("Seed: " + GameManager.Instance.masterSeed);
				}
				
			}
			else {
				GUILayout.Label(message);
			}
			if (Input.GetKey(KeyCode.Tab)) {
				foreach (PhotonPlayer player in PhotonNetwork.playerList)
				{
					GUILayout.Label(player.ToString() + " " + player.GetBounty().ToString() + " " + player.GetEarnings().ToString());
				}
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
			GameManager.Instance.ChangeToRandomScene();
		}
		else {
			_waitingForLevel = true;
			float waitStarted = Time.time;
			while (_waitingForLevel && !failed) {
				yield return new WaitForSeconds(0.1f);
				if (waitStarted + 10f < Time.time) {
					Debug.LogError("Timed out waiting for master level.");
					failed = true;
				}
			}
		}
		
		_waitingForLevel = false;
		
		if (failed)
			GameManager.Instance.Restart();
	}
	
	[RPC]
	void ChangeLevel(int sceneTag, int seed) {
		Scene.Tag tag = (Scene.Tag)sceneTag;
		Debug.Log("Change Level: " + tag.ToString() + " (" + seed + ")");
		GameManager.Instance.masterSeed = seed;
		GameManager.Instance.ChangeScene(tag);
		_waitingForLevel = false;
	}
	
	void OnSceneLoaded() {
		if (PhotonNetwork.isMasterClient) {
			int seed = GameManager.Instance.activeScene.seed;
			int tag = (int)GameManager.Instance.activeScene.tag;
			photonView.RPC ("ChangeLevel", PhotonTargets.Others, tag, seed);
		}
		GameObject player = PhotonNetwork.Instantiate(
			playerPrefab.name,
			Vector3.zero,
			Quaternion.identity,
			0
			);
	}
	
	void Restart() {
		_master = false;
		_waitingForLevel = false;
	}
	
	void OnPhotonPlayerConnected(PhotonPlayer player) {
		if (PhotonNetwork.isMasterClient) {
			int seed = GameManager.Instance.activeScene.seed;
			int tag = (int)GameManager.Instance.activeScene.tag;
			photonView.RPC("ChangeLevel", player, tag, seed);
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
