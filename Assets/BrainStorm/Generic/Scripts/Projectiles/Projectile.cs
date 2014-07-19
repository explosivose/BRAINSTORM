using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public int damage;
	
	public Transform Source {
		get {
			return sauce;
		}
	}
	public string SourceTag {
		get {
		 return sauce.tag;
		}
	}
	
	private Transform sauce;
	
	void Start() {
		Initialise();
	}
	
	void OnEnable() {
		Initialise();
	}
	
	void Initialise() {
		sauce = null;
	}
	
	void SetSource(Transform source) {
		sauce = source;
	}
}
