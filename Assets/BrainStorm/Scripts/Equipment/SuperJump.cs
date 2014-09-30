using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Super Jump")]
public class SuperJump : MonoBehaviour {
	
	//  behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour
	
	void OnEquip() {
		Player.localPlayer.inventory.hasSuperJump = true;
	}
	
	void OnDrop() {
		Player.localPlayer.inventory.hasSuperJump = false;
	}
}
