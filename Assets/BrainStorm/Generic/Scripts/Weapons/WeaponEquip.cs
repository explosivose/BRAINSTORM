using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WeaponEquip : MonoBehaviour {

	public bool equipped = false;
	public Vector3 localPosition;
	public Vector3 localRotation;
	
	public void Equip() {
		equipped = true;
		transform.parent = Camera.main.transform;
		transform.localPosition = localPosition;
		transform.localRotation = Quaternion.Euler(localRotation);
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
}
