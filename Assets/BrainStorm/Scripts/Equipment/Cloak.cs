using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Cloak")]
public class Cloak : Photon.MonoBehaviour {

	public Transform cloakEffect;
	public CharacterMotorC.CharacterMotorSprint motorBehaviour;
	
	private Equipment equipment;
	private bool cloaked = false;
	private Transform effectInstance;
	
	
	void Awake() {
		equipment = GetComponent<Equipment>();
		cloakEffect.CreatePool();
	}
	
	void OnEquip() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer)
				equipment.owner.motor.sprint = motorBehaviour;
		}
	}
	
	void OnDrop() {
		if (equipment.owner) {
			if (equipment.owner.isLocalPlayer) {
				equipment.owner.motor.sprint = new CharacterMotorC.CharacterMotorSprint();
				equipment.owner.motor.sprint.enabled = false;
			}
		}
		if (effectInstance) {
			StartCoroutine(KillEffect(effectInstance));
		}
	}
	
	void OnSprintStop() {
		photonView.RPC ("OnCloakStopRPC", PhotonTargets.All);
		Player.localPlayer.photonView.RPC("OnCloakStopRPC", PhotonTargets.All);
	}
	
	[RPC]
	void OnCloakStartRPC() {
		cloaked = true;
		equipment.AudioStart();
		effectInstance = cloakEffect.Spawn(equipment.owner.transform.position);
		effectInstance.parent = equipment.owner.transform;
	}
	
	[RPC]
	void OnCloakStopRPC() {
		cloaked = false;
		equipment.AudioStop();
		StartCoroutine(KillEffect(effectInstance));
	}
	
	IEnumerator KillEffect(Transform effect) {
		effect.particleSystem.Stop();
		float wait = effect.particleSystem.startLifetime;
		wait += effect.particleSystem.duration;
		yield return new WaitForSeconds(wait);
		effect.Recycle();
	}
	
	void Update() {
		if (equipment.equipped) {
			equipment.energy = equipment.owner.motor.sprint.stamina01;
			
			bool sprint = equipment.owner.motor.inputSprint &&
							equipment.owner.motor.canSprint;
			
			if (equipment.owner.motor.sprint.sprinting && !cloaked) {
				photonView.RPC ("OnCloakStartRPC", PhotonTargets.All);
				Player.localPlayer.photonView.RPC("OnCloakStartRPC", PhotonTargets.All);
			}
			else if (!equipment.owner.motor.sprint.sprinting && cloaked) {
				photonView.RPC ("OnCloakStopRPC", PhotonTargets.All);
				Player.localPlayer.photonView.RPC("OnCloakStopRPC", PhotonTargets.All);
			}
		}
	}
}
