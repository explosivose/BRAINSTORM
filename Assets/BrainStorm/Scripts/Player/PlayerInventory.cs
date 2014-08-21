using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterMotorC))]
public class PlayerInventory : MonoBehaviour {

	public static PlayerInventory Instance;

	public float playerReach = 4f;
	
	private CharacterMotorC _motor;
	private Transform _carryingObject;
	private Transform _equippedWeapon;
	private Transform _holsteredWeapon;
	private Transform _utility1;
	private Transform _utility2;

	// probably move this and an hasJetpack bool to PlayerInventory
	public float jetpack01 {
		get { return _motor.jetpack.fuel/_motor.jetpack.maxJetpackFuel; }
	}
	
	public float sprint01 {
		get { return _motor.sprint.stamina/_motor.sprint.sprintLength; }
	}

	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		_motor = GetComponent<CharacterMotorC>();
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		Transform cam = Camera.main.transform;
		if (Physics.Raycast(cam.position, cam.forward, out hit, playerReach)) {
			hit.transform.SendMessage("OnInspect", SendMessageOptions.DontRequireReceiver);
		}
		
		if (Input.GetButtonDown("Interact")) {
			if (_carryingObject) {
				Drop();
			}
			else {
				AttemptPickup();
			}
		}
		if (Input.GetButtonDown("ChangeWeapon")) {
			if (_equippedWeapon) {
				_equippedWeapon.SendMessage("Holster");
			}
			if (_holsteredWeapon) {
				_holsteredWeapon.SendMessage("Equip");
			}
			Transform temp = _holsteredWeapon;
			_holsteredWeapon = _equippedWeapon;
			_equippedWeapon = temp;
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
				if (_equippedWeapon != null)
					_equippedWeapon.SendMessage("Drop");
				hit.transform.SendMessage("Equip");
				_equippedWeapon = hit.transform;
				break;
			default:
				break;
			}
		}
	}
	

	
	void Carry(Transform obj) {
		_carryingObject = obj;
		_carryingObject.rigidbody.isKinematic = true;
		_carryingObject.parent = Camera.main.transform;
	}
	
	void Drop() {
		_carryingObject.parent = GameManager.Instance.activeScene;
		_carryingObject.rigidbody.isKinematic = false;
		_carryingObject = null;
	}

}
