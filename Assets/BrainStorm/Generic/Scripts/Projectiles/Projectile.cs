using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public int damage;
	
	public Transform Source {
		get {
			return _source;
		}
	}
	public string SourceTag {
		get {
			return _source.tag;
		}
	}
	
	private Transform _source;
	
	void Start() {
		Initialise();
	}
	
	void OnEnable() {
		Initialise();
	}
	
	void Initialise() {
		_source = null;
	}
	
	void SetSource(Transform source) {
		_source = source;
	}
}
