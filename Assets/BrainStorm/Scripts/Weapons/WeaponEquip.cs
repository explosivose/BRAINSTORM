using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WeaponEquip : MonoBehaviour {

	public bool equipped = false;
	public Vector3 eqippedPosition;
	public Vector3 defaultRotation;
	public Vector3 holsteredPosition;
	public Vector3 holsteredRotation;
	
	public void Equip() {
		equipped = true;
		transform.parent = Camera.main.transform;
		transform.localPosition = eqippedPosition;
		transform.localRotation = Quaternion.Euler(defaultRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
	}
	
	public void Drop() {
		equipped = false;
		transform.parent = null;
		transform.position = Camera.main.transform.position;
		transform.position += Camera.main.transform.forward;
		rigidbody.isKinematic = false;
		collider.enabled = true;
	}
	
	public void Holster() {
		equipped = false;
		transform.parent = Camera.main.transform;
		transform.localPosition = holsteredPosition;
		transform.localRotation = Quaternion.Euler(holsteredRotation);
		rigidbody.isKinematic = true;
		collider.enabled = false;
	}
}
