using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Equipment/Rocket Skates")]
public class RocketSkates : MonoBehaviour {
	
	//  behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour
	
	private Equipment equipment;
	
	void Awake() {
		equipment = GetComponent<Equipment>();
	}
	
	void OnEquip() {
		Player.localPlayer.inventory.hasSprint = true;
	}
	
	void OnDrop() {
		Player.localPlayer.inventory.hasSprint = false;
	}
	
	void OnSprintStart() {
		equipment.AudioStart();
	}
	
	void OnSprintStop() {
		equipment.AudioStop();
	}
	
	void Update() {
		equipment.energy = Player.localPlayer.motor.sprint.stamina01;
	}
}
