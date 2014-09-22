using UnityEngine;
using System.Collections;

public class ScreenShake : Singleton<ScreenShake> 
{
	
	private Transform _cam;
	private Vector3 _originalPosition = Vector3.zero;
	
	private float _magnitude;
	private float _decayRate;
	
	void OnLevelWasLoaded()
	{
		init();
	}
	
	void Awake()
	{
		init();
	}
	
	void init()
	{
		if (!Camera.main) {
			Debug.LogError("ScreenShake could not find main camera.");
			return;
		}
		_cam = Camera.main.transform;
		_originalPosition = _cam.localPosition;
	}
	
	public void Shake(float magnitude, float duration)
	{
		_magnitude = magnitude;
		_decayRate = magnitude/duration; 
	}
	
	public void SetCamera(Transform newCamera)
	{
		_cam = newCamera;
		_originalPosition = newCamera.localPosition;
	}
	
	void Update()
	{
		if (!_cam) return;
		if (Time.timeScale < 0.1f) return;
		if (_magnitude > 0f)
		{
			_cam.localPosition = _originalPosition + Random.insideUnitSphere * _magnitude;
			_magnitude -= _decayRate * Time.deltaTime;
		}
		else 
		{
			_cam.localPosition = _originalPosition;
		}
	}
}
