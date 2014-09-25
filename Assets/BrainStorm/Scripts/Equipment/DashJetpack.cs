using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Dash Jetpack")]
public class DashJetpack : Photon.MonoBehaviour {

	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	public Material activeMaterial;
	public Transform thrusters;

	private Equipment equipment;
	private bool jetpacking = false;
	private MeshRenderer _ren;
	private Material _originalMaterial;
	
	void Awake() {
		equipment = GetComponent<Equipment>();
		_ren = thrusters.GetComponent<MeshRenderer>();
		_originalMaterial = _ren.material;
	}

	void OnEquip() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer) {
				equipment.owner.inventory.hasDashpack = true;
			}
		}
	}
	
	void OnDrop() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer) {
				equipment.owner.inventory.hasDashpack = false;
			}
		}
	}
	
	void OnDashpackStart() {
		photonView.RPC("OnDashpackStartRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnDashpackStartRPC() {
		equipment.AudioStart();
		jetpacking = true;
		_ren.material = activeMaterial;
	}
	
	void OnDashpackStop() {
		photonView.RPC("OnDashpackStopRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnDashpackStopRPC() {
		equipment.AudioStop();
		jetpacking = false;
		_ren.material = _originalMaterial;
	}
	
	void Update() {
		if (jetpacking && !audio.isPlaying) {
			equipment.AudioStart();
		}
		if (equipment.equipped)
			equipment.energy = equipment.owner.motor.dashpack.fuel01;
	}
}
