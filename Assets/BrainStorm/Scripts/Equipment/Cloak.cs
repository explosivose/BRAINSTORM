using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Cloak")]
public class Cloak : Photon.MonoBehaviour {

	public CharacterMotorC.CharacterMotorSprint motorBehaviour;
	
	private Equipment equipment;
	
	void Awake() {
		equipment = GetComponent<Equipment>();
	}
	
	void OnEquip() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer)
				equipment.owner.motor.sprint = motorBehaviour;
		}
	}
	
	void OnDrop() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer)
				equipment.owner.motor.sprint.enabled = false;
		}
	}
	
	void OnSprintStart() {
		photonView.RPC ("OnCloakStartRPC", PhotonTargets.All);
		Player.localPlayer.photonView.RPC("OnCloakStartRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnCloakStartRPC() {
		equipment.AudioStart();
	}
	
	void OnSprintStop() {
		photonView.RPC ("OnCloakStopRPC", PhotonTargets.All);
		Player.localPlayer.photonView.RPC("OnCloakStopRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnCloakStopRPC() {
		equipment.AudioStop();
	}
	
	void Update() {
		if (equipment.equipped) {
			equipment.energy = equipment.owner.motor.sprint.stamina01;
		}
	}
}
