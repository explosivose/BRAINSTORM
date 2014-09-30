using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Jump Jetpack")]
public class JumpJetpack : Photon.MonoBehaviour {
	
	// jetpack behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour

	public Material activeMaterial;

	private Equipment equipment;
	private bool jetpacking = false;
	private MeshRenderer _ren;
	private Material _originalMaterial;
	private Vector3 _originalPos;
	
	
	void Awake() {
		equipment = GetComponent<Equipment>();
		_originalPos = equipment.equippedPosition;
		_ren = GetComponentInChildren<MeshRenderer>();
		_originalMaterial = _ren.material;
	}
	
	void OnEquip() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer) {
				equipment.owner.inventory.hasJetpack = true;
				equipment.owner.motor.jumping.extraJumpEnabled = false;
			}
		}
			
	}
	
	void OnDrop() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer) {
				equipment.owner.inventory.hasJetpack = false;
				equipment.owner.motor.jumping.extraJumpEnabled = true;
			}
		}
	}
	
	void OnJetpackStart() {
		photonView.RPC ("OnJetpackStartRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnJetpackStartRPC() {
		jetpacking = true;
		equipment.AudioStart();
		_ren.material = activeMaterial;
	}
	
	void OnJetpackStop() {
		photonView.RPC ("OnJetpackStopRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnJetpackStopRPC() {
		jetpacking = false;
		equipment.AudioStop();
		_ren.material = _originalMaterial;
	}
	
	void Update() {
		if (equipment.equipped) {
			equipment.energy = equipment.owner.motor.jetpack.fuel01;
			if (jetpacking) {
				transform.localPosition = Vector3.Lerp(
					transform.localPosition,
					_originalPos + Vector3.up * 0.75f + Vector3.up * 0.125f * Random.value,
					Time.deltaTime * 4f);
			}
			else {
				transform.localPosition = Vector3.Lerp(
					transform.localPosition,
					_originalPos,
					Time.deltaTime * 4f);
			}
		}
			
	}	
}
