using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Projectile))]
[RequireComponent(typeof(LineRenderer))]
public class ProjectileLaser : MonoBehaviour {

	public float 		lifetime;
	public bool 		moveLaserWithTransform = false;
	public Color 		startColor;
	public Color 		endColor;
	public float 		distanceBetweenPoints;
	public float 		laserNoise;
	public bool 		autofire; 			// fire laser in transform.forward
	public LayerMask 	autofireMask;		// raycast mask for autofire
	
	private Projectile 		_projectile;
	private Transform 		_target;
	private Vector3 		_hit;
	private float 			_startTime;
	private LineRenderer 	_line;
	private List<Vector3>  	_positions = new List<Vector3>();
	
	void Awake() {
		_projectile = GetComponent<Projectile>();
		_line = GetComponent<LineRenderer>();
	}
	
	void OnDisable() {
		if (GameManager.Instance.levelTeardown) return;
		_target = null;
	}
	
	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		if (autofire) {
			StartCoroutine( Autofire() );
		}
	}
	
	IEnumerator Autofire() {
		// wait for position to update before autofiring
		yield return new WaitForFixedUpdate();
		RaycastHit hit;
		Ray ray = new Ray(transform.position, transform.forward);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, autofireMask)) {
			SetTarget(hit.transform);
			HitPosition(hit.point);
		}
		else {
			HitPosition(transform.forward * 100);
		}
	}
	
	void SetTarget(Transform target) {
		_target = target;
		_target.SendMessage("Damage", _projectile.Damage, SendMessageOptions.DontRequireReceiver);
	}
	
	void HitPosition(Vector3 position) {
		_startTime = Time.time;
		_hit = position;
		SetLaserPoints();
	}
	
	void SetLaserPoints() {
		float distance = Vector3.Distance(transform.position, _hit);
		int laserVertices = Mathf.FloorToInt(distance/distanceBetweenPoints);
		if (laserVertices < 2) laserVertices = 2;
		_line.SetVertexCount(laserVertices);
		_line.SetColors(startColor, endColor);
		for(int i = 0; i<laserVertices; i++) {
			Vector3 point = Vector3.Lerp(transform.position, _hit, 
			                             (float)i/(float)laserVertices);
			point += Random.insideUnitSphere * laserNoise;
						_positions.Add(point);
			_line.SetPosition(i, point);
		}
		
		// clamp either end to start location and hit location
		_line.SetPosition(0, transform.position);
		_line.SetPosition(laserVertices-1, _hit);
	}
	
	
	// Update is called once per frame
	void Update () {
		if (GameManager.Instance.paused) return;
		float t = (Time.time - _startTime) / lifetime;
		Color s = Color.Lerp(startColor, Color.clear, t);
		Color e = Color.Lerp(endColor, Color.clear, t);
		_line.SetColors(s, e);
		
		if (moveLaserWithTransform) {
			SetLaserPoints();
		}
		
		if (t > 1) {
			_positions.Clear();
			transform.Recycle();
		} 
	}
}
