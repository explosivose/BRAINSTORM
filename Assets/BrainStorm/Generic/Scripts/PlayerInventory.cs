using UnityEngine;
using System.Collections;

public class PlayerInventory : MonoBehaviour {

	public float playerReach = 4f;
	
	private Transform carryingObject;
	private Transform equippedWeapon;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Interact")) {
			if (carryingObject != null) {
				Drop();
			}
			else {
				AttemptPickup();
			}
			
		}
	}
	
	void AttemptPickup() {
		// raycast from center of camera
		RaycastHit hit;
		Transform cam = Camera.main.transform;
		if (Physics.Raycast (cam.position, cam.forward, out hit, playerReach)) {
			Debug.DrawLine(cam.position, hit.point, Color.red, 1f);
			
			switch(hit.transform.tag) {
			case "TV":
				Debug.Log ("Carrying a TV.");
				Carry (hit.transform);
				break;
			case "Weapon":
				if (equippedWeapon != null)
					equippedWeapon.SendMessage("Drop");
				hit.transform.SendMessage("Equip");
				equippedWeapon = hit.transform;
				break;
			default:
				break;
			}
		}
	}
	
	void Carry(Transform obj) {
		carryingObject = obj;
		carryingObject.rigidbody.isKinematic = true;
		carryingObject.parent = Camera.main.transform;
	}
	
	void Drop() {
		carryingObject.parent = null;
		carryingObject.rigidbody.isKinematic = false;
		carryingObject = null;
	}
}
