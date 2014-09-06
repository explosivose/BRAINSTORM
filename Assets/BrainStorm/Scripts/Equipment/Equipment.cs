using UnityEngine;
using System.Collections;

// in future, perhaps, this could inherit from a PhysicsObject MonoBehaviour
// which plays audio for collisions 

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Player/Equipment/Equip")]
public class Equipment : MonoBehaviour {
	// utility1 is operated with the Jump button
	// utility2 is operated with the Sprint button
	public enum Type {
		weapon, utility1, utility2
	}
	public Type type;
	
	public enum EquipParent {
		player, camera
	}
	public EquipParent parent;

	public bool equipped {
		get { return _equipped; }
	}
	public Vector3 equippedPosition;
	public Vector3 defaultRotation;
	public Vector3 holsteredPosition;
	public Vector3 holsteredRotation;
	
	private bool _equipped;
	private Transform _parent;
	private GameObject _tooltip;
	
	void Start() {
		if (parent == EquipParent.camera) _parent = Camera.main.transform;
		if (parent == EquipParent.player) _parent = Player.Instance.transform;
		_tooltip = transform.Find("tooltip").gameObject;
		if (!_tooltip) Debug.LogWarning("Equipment is missing a tooltip.");
	}
	
	public void Equip() {
		_equipped = true;
		transform.parent = _parent;
		transform.localPosition = equippedPosition;
		transform.localRotation = Quaternion.Euler(defaultRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
		if (_tooltip) _tooltip.SetActive(false);
		SendMessage("OnEquip", SendMessageOptions.DontRequireReceiver);
	}
	
	public void Drop() {
		_equipped = false;
		transform.parent = GameManager.Instance.activeScene;
		transform.position = Camera.main.transform.position;
		transform.position += Camera.main.transform.forward;
		rigidbody.isKinematic = false;
		collider.enabled = true;
		if (_tooltip) _tooltip.SetActive(true);
		SendMessage("OnDrop", SendMessageOptions.DontRequireReceiver);
	}
	
	public void Holster() {
		_equipped = false;
		transform.parent = _parent;
		transform.localPosition = holsteredPosition;
		transform.localRotation = Quaternion.Euler(holsteredRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
		SendMessage("OnHolster", SendMessageOptions.DontRequireReceiver);
	}
}
