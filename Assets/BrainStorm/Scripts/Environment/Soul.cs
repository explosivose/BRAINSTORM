using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class Soul : MonoBehaviour {
	
	private TrailRenderer trail;

	void Start () {
		trail = GetComponent<TrailRenderer>();
	}
	
	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		StartCoroutine(init());
	}
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
		trail.enabled = false;
		rigidbody.velocity = Vector3.zero;
	}
	
	IEnumerator init() {
		yield return new WaitForEndOfFrame();
		trail.enabled = true;
	}
	
	void FixedUpdate () {
		Vector3 negativeGravity = -Physics.gravity;
		rigidbody.AddForce(negativeGravity);
	}
}
