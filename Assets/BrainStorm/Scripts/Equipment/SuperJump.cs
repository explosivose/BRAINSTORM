using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Super Jump")]
public class SuperJump : MonoBehaviour {
	
	//  behaviour is currently in the CharacterMotorC script
	// this script is just sounds and enabling/disabling behaviour
	
	void OnEquip() {
		PlayerInventory.Instance.hasSuperJump = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasSuperJump = false;
	}
}
