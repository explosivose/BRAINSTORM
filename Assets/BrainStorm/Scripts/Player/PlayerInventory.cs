using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterMotorC))]
[AddComponentMenu("Player/Inventory")]
public class PlayerInventory : MonoBehaviour {

	public static PlayerInventory Instance;

	public float playerReach = 4f;
	public LayerMask raycastMask;
	
	private CharacterMotorC _motor;
	private float _inspectStartTime;	// the time we started inspected an equipable item
	private Transform _inspected;		// this is whatever equipable item the player is looking at
	private Equipment _inspectedEquip; // the equipment script of the inspected transform (null if not inspecting equipment)
	private Transform _carryingObject;
	private Transform _equippedWeapon;
	private Transform _holsteredWeapon;
	private Transform _utility1;		// utility equipment operated with Jump button
	private Transform _utility2;		// utility equipment operated with Sprint button
	

	// probably move this and an hasJetpack bool to PlayerInventory
	public float jetpack01 {
		get { return _motor.jetpack.fuel/_motor.jetpack.maxJetpackFuel; }
	}
	
	public float sprint01 {
		get { return _motor.sprint.stamina/_motor.sprint.sprintLength; }
	}

	public bool hasJetpack {
		get { return _motor.jetpack.enabled; }
		set { _motor.jetpack.enabled = value; }
	}
	
	public bool hasSprint {
		get { return _motor.sprint.enabled; }
		set { _motor.sprint.enabled = value; }
	}
	
	public bool hasSuperJump {
		get { return _motor.jumping.superJump; }
		set { _motor.jumping.superJump = value; }
	}

	private Transform utility1 {
		get { return _utility1; }
		set {
			_utility1 = value;
			GUIController.Instance.jetpackBar.visible = (_utility1 != null);
		}
	}
	
	private Transform utility2 {
		get { return _utility2; }
		set {
			_utility2 = value;
			GUIController.Instance.sprintBar.visible = (_utility2 != null);
		}
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
		
		InspectItem();
		
		// this first block swaps utility items when you press Jump or Sprint
		// if we've been inspecting equipment for some time
		if (_inspectedEquip && Time.time > _inspectStartTime + 0.2f) {
			// and that equipment is utility1, and we press Jump
			if (_inspectedEquip.type == Equipment.Type.utility1 &&
					Input.GetButtonDown("Jump")) {
				// swap with equipped utility1 if any
				if (_utility1) 
					_utility1.SendMessage("Drop");
				_inspected.SendMessage("Equip");
				utility1 = _inspected;
			}
			// or that equipment is utility2, and we press Sprint
			else if(_inspectedEquip.type == Equipment.Type.utility2 && 
					Input.GetButtonDown("Sprint")) {
				// swap with equipped utility2 if any
				if (_utility2)
					_utility2.SendMessage("Drop");
				_inspected.SendMessage("Equip");
				utility2 = _inspected;
			}
		}
		
		// This block handles what to do when you press the Interact button
		// Player presses the interact button (probably E on keyboard)
		if (Input.GetButtonDown("Interact")) {
			// Drop if we're carrying something;
			if (_carryingObject) {
				Drop();
			}
			// Else do something with _inspected item
			else if (_inspected){
				switch(_inspected.tag) {
				// If we're inspecting a TV, pick it up.
				case "TV":
					Debug.Log ("Carrying a TV.");
					Carry (_inspected.transform);
					break;
				// If we're inspecting an peice of equipment, determine the type
				case "Equipment":
					switch (_inspectedEquip.type) {
					// If it's a weapon, equip it immediately, 
					// and swap it with anything we have equipped now
					case Equipment.Type.weapon:
						if (_equippedWeapon)
							_equippedWeapon.SendMessage("Drop");
						_inspected.SendMessage("Equip");
						_equippedWeapon = _inspected;
						break;
					// If it's a utility1 (Jump) equip it
					// and swap it with any utility1 we already have
					case Equipment.Type.utility1:
							if (_utility1)
								_utility1.SendMessage("Drop");
							_inspected.SendMessage("Equip");
							utility1 = _inspected;
						break;
					// If it's a utility2 (Sprint) equip it
					// and swap it with any utility2 we already have
					case Equipment.Type.utility2:
							if (_utility2)
								_utility2.SendMessage("Drop");
							_inspected.SendMessage("Equip");
							utility2 = _inspected;
						break;
					}
					break;
				default:
					break;
				}
			}
		}
		
		// This block swaps between equipped and holstered weapons
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
	
	void InspectItem() {
		RaycastHit hit;
		Transform cam = Camera.main.transform;
		if (Physics.Raycast(cam.position, cam.forward, out hit, playerReach, raycastMask)) {
			hit.transform.SendMessage("OnInspect", SendMessageOptions.DontRequireReceiver);
			switch(hit.transform.tag) {
			case "TV":
				Debug.DrawLine(cam.position, hit.point, Color.green);
				_inspected = hit.transform;
				_inspectedEquip = null;
				break;
			case "Equipment":
				if (hit.transform != _inspected)
					_inspectStartTime = Time.time;
				Debug.DrawLine(cam.position, hit.point, Color.green);
				_inspected = hit.transform;
				_inspectedEquip = _inspected.GetComponent<Equipment>();
				break;
			default:
				Debug.DrawLine(cam.position, hit.point, Color.yellow);
				_inspected = null;
				_inspectedEquip = null;
				break;
			}
		}
		else {
			_inspected = null;
			_inspectedEquip = null;
			Debug.DrawRay(cam.position, cam.forward * playerReach, Color.red);
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
