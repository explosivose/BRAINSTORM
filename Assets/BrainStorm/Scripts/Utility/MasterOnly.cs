using UnityEngine;
using System.Collections;

public class MasterOnly : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (!PhotonNetwork.isMasterClient && PhotonNetwork.inRoom) {
			Destroy(this.gameObject);
		}
	}
	

}