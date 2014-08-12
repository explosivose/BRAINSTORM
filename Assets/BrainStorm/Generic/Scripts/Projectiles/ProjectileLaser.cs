using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
[RequireComponent(typeof(LineRenderer))]
public class ProjectileLaser : MonoBehaviour {

	public float lifetime;
	public Color startColor;
	public Color endColor;
	public float distanceBetweenPoints;
	public float laserNoise;
	
	private Projectile _projectile;
	private Transform _target;
	private Vector3 _hit;
	private float _startTime;
	private LineRenderer _line;

	
	void Awake() {
		_projectile = GetComponent<Projectile>();
		_line = GetComponent<LineRenderer>();
	}
	
	void OnDisable() {
		_target = null;
	}
	
	void SetTarget(Transform target) {
		_target = target;
		_target.SendMessage("Damage", _projectile.Damage, SendMessageOptions.DontRequireReceiver);
	}
	
	void HitPosition(Vector3 position) {
		_startTime = Time.time;
		_hit = position;
		float distance = Vector3.Distance(transform.position, _hit);
		int laserVertices = Mathf.FloorToInt(distance/distanceBetweenPoints);
		if (laserVertices < 2) laserVertices = 2;
		_line.SetVertexCount(laserVertices);
		_line.SetColors(startColor, endColor);
		
		for(int i = 0; i<laserVertices; i++) {
			Vector3 point = Vector3.Lerp(transform.position, _hit, distance*((float)i/(float)laserVertices));
			if (!_line.useWorldSpace) {
				point = transform.parent.InverseTransformPoint(point);
			}
			point += Random.insideUnitSphere * laserNoise;
			_line.SetPosition(i, point);
		}
		
		// clamp either end to start location and hit location
		if (_line.useWorldSpace) {
			_line.SetPosition(0, transform.position);
			_line.SetPosition(laserVertices-1, _hit);
		}
		else {
			_line.SetPosition(0, transform.localPosition);
			_line.SetPosition(laserVertices-1, transform.parent.InverseTransformPoint(_hit));
		}
	}
	
	// Update is called once per frame
	void Update () {
		float t = (Time.time - _startTime) / lifetime;
		Color s = Color.Lerp(startColor, Color.clear, t);
		Color e = Color.Lerp(endColor, Color.clear, t);
		_line.SetColors(s, e);
		if (t > 1) transform.Recycle();
	}
}
