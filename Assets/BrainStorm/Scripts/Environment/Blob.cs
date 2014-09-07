using UnityEngine;
using System.Collections;

public class Blob : MonoBehaviour {

	public float carrotThreshold = 0.5f;
	
	private Vector3 _initialScale;
	private float _scaleFactor;
	
	// Use this for initialization
	void Start () {
		_initialScale = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		_scaleFactor = 1f - (NPCCarrot.frenzyFactor * 1f/carrotThreshold);
		_scaleFactor = Mathf.Clamp01(_scaleFactor);
		Vector3 scale = _initialScale * _scaleFactor;
		transform.localScale = Vector3.Lerp(transform.localScale, scale, Time.deltaTime);
		
	}
}
