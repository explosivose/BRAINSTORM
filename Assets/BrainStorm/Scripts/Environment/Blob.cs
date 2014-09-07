using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blob : MonoBehaviour {

	public Transform projectilePrefab;
	public float attackRate = 10f;
	public float carrotThreshold = 0.5f;
	
	
	private Vector3 _initialScale;
	private float 	_scaleFactor;
	private List<Transform> _targets = new List<Transform>();
	private int 	_tindex = 0;
	private bool 	_attacking;
	
	// Use this for initialization
	void Start () {
		_initialScale = transform.localScale;
		projectilePrefab.CreatePool();
		StartCoroutine( Attack() );
	}
	
	void OnEnable() {
		StartCoroutine( Attack() );
	}
	
	void OnDisable() {
		_attacking = false;
	}
	
	// Update is called once per frame
	void Update () {
		_scaleFactor = 1f - (NPCCarrot.frenzyFactor * 1f/carrotThreshold);
		_scaleFactor = Mathf.Clamp01(_scaleFactor);
		Vector3 scale = _initialScale * _scaleFactor;
		transform.localScale = Vector3.Lerp(transform.localScale, scale, Time.deltaTime);
	}
	
	IEnumerator Attack() {
		yield return new WaitForSeconds(1f);
		_attacking = true;
		while(true) {
			if (_targets.Count > 0) {
				_tindex++;
				if (_tindex >= _targets.Count) _tindex = 0; 
				Transform i = projectilePrefab.Spawn(transform.position);
				i.parent = GameManager.Instance.activeScene;
				i.SendMessage("SetTarget", _targets[_tindex]);
				i.SendMessage("HitPosition", _targets[_tindex].position);
				i.SendMessage("SetDamageSource", this.transform);
			}
			yield return new WaitForSeconds(1f/attackRate);
		}
		_attacking = false;
	}
	
	void OnTriggerEnter(Collider col) {
		if (!_targets.Contains(col.transform))
			_targets.Add(col.transform);
	}
	
	void OnTriggerExit(Collider col) {
		_targets.Remove(col.transform);
	}
}
