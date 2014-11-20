using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class AntigravitySphere : MonoBehaviour {

	public float strength;
	public float maxForce;

	private List<Transform> objects = new List<Transform>();

	void FixedUpdate() {
		foreach (Transform t in objects) {
			if (!t) {
				objects.Remove(t);
				continue;
			}
			Vector3 dir = (transform.position - t.position);
			float magnitude = strength/(dir.magnitude * dir.magnitude);
			magnitude = Mathf.Min(magnitude, maxForce);
			Vector3 force = dir.normalized * magnitude;
			
			t.rigidbody.AddForce(force);
		}
	}

	void OnTriggerEnter(Collider col) {
		if (col.rigidbody) {
			objects.Add(col.transform);
		}
	}
	
	void OnTriggerExit(Collider col) {
		objects.Remove(col.transform);
	}
}
