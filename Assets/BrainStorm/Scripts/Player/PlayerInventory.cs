﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterMotorC))]
[AddComponentMenu("Player/Inventory")]
// this component is only enabled on the local player
public class PlayerInventory : Photon.MonoBehaviour {

	public float playerReach = 4f;
	public LayerMask raycastMask;
	
	private CharacterMotorC _motor;
	private Transform _inspected;		// this is whatever equipable item the player is looking at
	private Equipment _inspectedEquip; // the equipment script of the inspected transform (null if not inspecting equipment)
	private PhotonView _inspectedView;
	private Transform _carryingObject;
	public Transform equippedWeapon;
	public Transform holsteredWeapon;
	private Transform _utility1;		// utility equipment operated with Jump button
	private Transform _utility2;		// utility equipment operated with Sprint button
	
			
	public float jumpbar {
		get {
			if (utility1)
				return utility1.GetComponent<Equipment>().energy;
			else 
				return 0f;
		}
	}
	
	public float sprintbar {
		get {
			if (utility2)
				return utility2.GetComponent<Equipment>().energy;
			else
				return 0f;
		}
	}
	
	public bool hasJetpack {
		get { return _motor.jetpack.enabled; }
		set { _motor.jetpack.enabled = value; }
	}
	
	public bool hasDashpack {
		get { return _motor.dashpack.enabled; }
		set { _motor.dashpack.enabled = value; }
	}
	
	public bool hasSuperJump {
		get { return _motor.jumping.superJump; }
		set { _motor.jumping.superJump = value; }
	}
	
	public bool hasSprint {
		get { return _motor.sprint.enabled; }
		set { _motor.sprint.enabled = value; }
	}
	
	public bool hasBlink {
		get; set;
	}

	public Transform utility1 {
		get { return _utility1; }
		set {
			_utility1 = value;
			Player.localPlayer.hud.jetpackBar.visible = (_utility1 != null);
		}
	}
	
	public Transform utility2 {
		get { return _utility2; }
		set {
			_utility2 = value;
			Player.localPlayer.hud.sprintBar.visible = (_utility2 != null);
		}
	}

	void Awake() {
		_motor = GetComponent<CharacterMotorC>();
	}
	
	void OnSpawn() {
		// destroy the weapon you were holding when you respawn
		equippedWeapon = null;
		holsteredWeapon = null;
		utility1 = null;
		utility2 = null;
	}
	
	void OnDeath() {
		// drop everything
		if(_carryingObject) Drop();
		if(equippedWeapon) 
			photonView.RPC(
				"DropRPC",
				PhotonTargets.AllBufferedViaServer,
				photonView.viewID,
				equippedWeapon.GetComponent<PhotonView>().viewID,
				Equipment.Type.weapon
				);
		if(holsteredWeapon) 
			photonView.RPC(
				"DropRPC",
				PhotonTargets.AllBufferedViaServer,
				photonView.viewID,
				holsteredWeapon.GetComponent<PhotonView>().viewID,
				Equipment.Type.weapon
				);
		if(utility1) 
			photonView.RPC(
				"DropRPC",
				PhotonTargets.AllBufferedViaServer,
				photonView.viewID,
				utility1.GetComponent<PhotonView>().viewID,
				Equipment.Type.utility1
				);
		if(utility2) 
			photonView.RPC(
				"DropRPC",
				PhotonTargets.AllBufferedViaServer,
				photonView.viewID,
				utility2.GetComponent<PhotonView>().viewID,
				Equipment.Type.utility2
				);
		

	}
	
	void OnBlinkStart() {
		MaterialOverride(Player.localPlayer.blinkMaterial);
	}
	
	void OnBlinkStop() {
		MaterialOverride(null);
	}
	
	void OnCloakStart() {
		MaterialOverride(Player.localPlayer.cloakMaterial);
	}
	
	void OnCloakStop() {
		MaterialOverride(null);
	}
	
	void MaterialOverride(Material material) {
		bool enable = material != null;
		
		
		// set equipment materials
		if (equippedWeapon) {
			equippedWeapon.GetComponent<Equipment>().overrideMaterial = material;
			equippedWeapon.GetComponent<Equipment>().materialOverride = enable;
		}
		
		if (holsteredWeapon) {
			holsteredWeapon.GetComponent<Equipment>().overrideMaterial = material;
			holsteredWeapon.GetComponent<Equipment>().materialOverride = enable;
		}
		
		if (utility1) {
			utility1.GetComponent<Equipment>().overrideMaterial = material;
			utility1.GetComponent<Equipment>().materialOverride = enable;
		}
		
		if (utility2) {
			utility2.GetComponent<Equipment>().overrideMaterial = material;
			utility2.GetComponent<Equipment>().materialOverride = enable;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!photonView.isMine) return;
		
		InspectItem();
		
		bool canInteract = false;
		if (_inspected) {
			canInteract = Vector3.Distance(_inspected.position, transform.position) < playerReach;
		}
		
		// This block handles what to do when you press the Interact button
		// Player presses the interact button (probably E on keyboard)
		if (Input.GetButtonDown("Interact") && canInteract) {
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
					InteractWithEquipment();
					break;
				default:
					break;
				}
			}
		}
		
		// This block swaps between equipped and holstered weapons
		if (Input.GetButtonDown("ChangeWeapon")) {
			photonView.RPC (
				"HolsterRPC", 
				PhotonTargets.AllBufferedViaServer, 
				photonView.viewID
				);
		}
	}

	
	// this is called on the local player to detect what they're looking at
	void InspectItem() {
		RaycastHit hit;
		Transform cam = Camera.main.transform;
		if (Physics.Raycast(cam.position, cam.forward, out hit, 100f, raycastMask)) {
			if (_inspected != hit.transform) {
				if (_inspected)
					_inspected.BroadcastMessage("OnInspectStop", SendMessageOptions.DontRequireReceiver);
				_inspected = hit.transform;
				_inspected.BroadcastMessage("OnInspectStart", SendMessageOptions.DontRequireReceiver);
			}
			switch(hit.transform.tag) {
			case "Equipment":
				Debug.DrawLine(cam.position, hit.point, Color.green);
				_inspected = hit.transform;
				_inspectedEquip = _inspected.GetComponent<Equipment>();
				_inspectedView = _inspected.GetComponent<PhotonView>();
				break;
			default:
				Debug.DrawLine(cam.position, hit.point, Color.yellow);
				_inspectedEquip = null;
				break;
			}
		}
		else {
			if (_inspected) {
				_inspected.BroadcastMessage("OnInspectStop", SendMessageOptions.DontRequireReceiver);
			}
			_inspected = null;
			_inspectedView = null;
			_inspectedEquip = null;
		}
	}

	// this is called on the local player when they attempt
	// to interact with an equipment object
	void InteractWithEquipment() {
		
		// 1. if we already have an equipment of this type then drop it
		// 2. equip this inspected object
		
		// DROP if we already have an equipment of the same type
		switch (_inspectedEquip.type) {
		case Equipment.Type.weapon:
			if (equippedWeapon) {
				photonView.RPC(
					"DropRPC",
					PhotonTargets.AllBufferedViaServer,
					photonView.viewID,
					equippedWeapon.GetComponent<PhotonView>().viewID,
					Equipment.Type.weapon
					);
			}
			break;
		case Equipment.Type.utility1:
			if (utility1) {
				photonView.RPC(
					"DropRPC",
					PhotonTargets.AllBufferedViaServer,
					photonView.viewID,
					utility1.GetComponent<PhotonView>().viewID,
					Equipment.Type.utility1
					);
			}
			break;
		case Equipment.Type.utility2:
			if (utility2) {
				photonView.RPC(
					"DropRPC",
					PhotonTargets.AllBufferedViaServer,
					photonView.viewID,
					utility2.GetComponent<PhotonView>().viewID,
					Equipment.Type.utility2
					);
			}
			break;
		}
		
		// then EQUIP the inspected equipment
		photonView.RPC (
			"EquipRPC", 
			PhotonTargets.AllBufferedViaServer,
			photonView.viewID,
			_inspectedView.viewID,
			_inspectedEquip.type
		);
	}

	
	[RPC]
	void EquipRPC(int playerID, int equipmentID, int equipmentType) {
		PhotonView equipmentPV = PhotonView.Find(equipmentID);
		Equipment equipment = equipmentPV.GetComponent<Equipment>();
		// attach equipment to the player
		equipment.Equip(playerID);
		// cache equipment transform for changing materials and stuff later on
		Equipment.Type type = (Equipment.Type)equipmentType;
		switch (type) {
		case Equipment.Type.weapon:
			equippedWeapon = equipment.transform;
			break;
		case Equipment.Type.utility1:
			utility1 = equipment.transform;
			break;
		case Equipment.Type.utility2:
			utility2 = equipment.transform;
			break;
		default:
			Debug.LogError("Invalid equipment type enumeration received!");
			break;
		}
	}
	
	[RPC]
	void DropRPC(int playerID, int equipmentID, int equipmentType) {
		PhotonView equipmentPV = PhotonView.Find (equipmentID);
		Equipment equipment = equipmentPV.GetComponent<Equipment>();
		// return this equipment to the world (master client control via rigidbody)
		equipment.Drop();
		// uncache the transform so we don't go changing its material anymore
		Equipment.Type type = (Equipment.Type)equipmentType;
		switch (type) {
		case Equipment.Type.weapon:
			equippedWeapon = null;
			break;
		case Equipment.Type.utility1:
			utility1 = null;
			break;
		case Equipment.Type.utility2:
			utility2 = null;
			break;
		default:
			Debug.LogError("Invalid equipment type enumeration received!");
			break;
		}
	}
	
	[RPC]
	void HolsterRPC(int playerID) {
		Equipment equipment;
		if (equippedWeapon) {
			equipment = equippedWeapon.GetComponent<Equipment>();
			equipment.Holster();
		}
		if (holsteredWeapon) {
			equipment = holsteredWeapon.GetComponent<Equipment>(); 
			equipment.Equip(playerID);
		}
		Transform temp = holsteredWeapon;
		holsteredWeapon = equippedWeapon;
		equippedWeapon = temp;
	}
	
	void Carry(Transform obj) {
		_carryingObject = obj;
		_carryingObject.rigidbody.isKinematic = true;
		_carryingObject.parent = Camera.main.transform;
	}
	
	void Drop() {
		_carryingObject.parent = GameManager.Instance.activeScene.instance;
		_carryingObject.rigidbody.isKinematic = false;
		_carryingObject = null;
	}


}
