using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blob : MonoBehaviour {

	public Transform projectilePrefab;
	public float attackRate = 10f;
	public float carrotThreshold = 0.5f;
	public float deathThreshold = 0.1f;
	
	private Vector3 _initialScale;
	private float 	_scaleFactor;
	private List<Transform> _targets = new List<Transform>();
	private int 	_tindex = 0;
	private int		nearbyCarrots = 0;
	
	// Use this for initialization
	void Start () {
		_initialScale = transform.localScale;
		projectilePrefab.CreatePool();
		StartCoroutine( Attack() );
	}
	
	void OnEnable() {
		StartCoroutine( Attack() );
	}
	
	// Update is called once per frame
	void Update () {
		float scareFactor = (float)nearbyCarrots/(float)NPCCarrot.count;
		_scaleFactor = 1f - (scareFactor * 1f/carrotThreshold);
		_scaleFactor = Mathf.Clamp01(_scaleFactor);
		Vector3 scale = _initialScale * _scaleFactor;
		if (_scaleFactor < deathThreshold) {
			scale = Vector3.zero;
			GameManager.Instance.griefComplete = true;
		}
		transform.localScale = Vector3.Lerp(transform.localScale, scale, Time.deltaTime);

	}
	
	IEnumerator Attack() {
		yield return new WaitForSeconds(1f);
		while(true) {
			if (_targets.Count > 0) {
				_tindex++;
				if (_tindex >= _targets.Count) _tindex = 0; 
				Transform i = projectilePrefab.Spawn(transform.position);
				i.parent = GameManager.Instance.activeScene.instance;
				i.SendMessage("SetTarget", _targets[_tindex]);
				i.SendMessage("HitPosition", _targets[_tindex].position);
				i.SendMessage("SetDamageSource", this.transform);
			}
			yield return new WaitForSeconds(1f/attackRate);
		}
	}
	
	void OnTriggerEnter(Collider col) {
		if (!_targets.Contains(col.transform)) {
			_targets.Add(col.transform);
			NPCCarrot carrot = col.GetComponent<NPCCarrot>();
			if (carrot!=null) nearbyCarrots++;
		}
			
	}
	
	void OnTriggerExit(Collider col) {
		_targets.Remove(col.transform);
		NPCCarrot carrot = col.GetComponent<NPCCarrot>();
		if (carrot!=null) nearbyCarrots--;
	}
}
