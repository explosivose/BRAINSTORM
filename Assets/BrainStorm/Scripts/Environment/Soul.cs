using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class Soul : MonoBehaviour {
	
	public float spireAttraction = 1f;

	private Transform spireTop;
	private TrailRenderer trail;

	void Start () {
		spireTop = Spire.Instance.top;
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
		float distance = Vector3.Distance(spireTop.position, transform.position);
		if (distance < 10f) transform.Recycle();
		Vector3 direction = (spireTop.position - transform.position).normalized;
		float magnitude = (rigidbody.drag * rigidbody.mass * spireAttraction)
						/ (distance * distance);
		Vector3 force = direction * magnitude;
		Vector3 negativeGravity = -Physics.gravity;
		rigidbody.AddForce(force + negativeGravity);
	}
}
