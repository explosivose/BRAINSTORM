using UnityEngine;
using System.Collections;

public class CTRLcontainer : MonoBehaviour {

	public bool animate;

	private Vector3 	_targetPosition;
	private Quaternion	_targetRotation;
	private Vector3		_targetScale;
	private bool 		_complete  = false;
	
	void Awake() {
		_targetPosition = transform.position;
		_targetRotation = transform.rotation;
		_targetScale 	= transform.localScale;
		if (animate) {
			transform.position += Random.onUnitSphere * 10f;
			transform.rotation = Random.rotation;
			transform.localScale = Vector3.zero;
		}
	}
	
	void OnEnable() {

	}
	
	void OnDisable(){
		_complete = false;
	}
	

	void Update() {
		if (animate && !_complete) {
			transform.position = Vector3.Slerp(
				transform.position,
				_targetPosition,
				CTRL.deltaTime * 4f);
			
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				_targetRotation,
				CTRL.deltaTime * 4f);
				
			transform.localScale = Vector3.Slerp(
				transform.localScale,
				_targetScale,
				CTRL.deltaTime * 4f);
			
			float distance = Vector3.Distance(transform.position, _targetPosition);
			float angle = Quaternion.Angle(transform.rotation, _targetRotation);
			float difference = Vector3.Distance(transform.localScale, _targetScale);
			
			if (distance < 0.05f && angle < 1f && difference < 0.05f) 
				_complete = true;
		}
	}
	
}
