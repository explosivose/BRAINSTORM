using UnityEngine;
using System.Collections;

public class CTRLcontainer : MonoBehaviour {

	public bool animate;
	public float moveSpeed = 4f;
	public float rotateSpeed = 4f;
	public float scaleSpeed = 4f;

	private Vector3 	_targetPosition;
	private Quaternion	_targetRotation;
	private Vector3		_targetScale;
	private bool 		_complete  = false;
	
	void Awake() {
		
		if (Player.localPlayer) 
			transform.parent = Player.localPlayer.transform;
			
		_targetPosition = transform.localPosition;
		_targetRotation = transform.localRotation;
		_targetScale 	= transform.localScale;
		if (animate) {
			transform.localPosition += Random.onUnitSphere * 10f;
			transform.localRotation = Random.rotation;
			transform.localScale = Vector3.zero;
		}
	}
	
	void OnEnable() {
		CTRL.level++;
	}
	
	void OnDisable(){
		_complete = false;
		CTRL.level--;
	}
	

	void Update() {

		if (animate && !_complete) {
			transform.localPosition = Vector3.Slerp(
				transform.localPosition,
				_targetPosition,
				CTRL.deltaTime * moveSpeed);
			
			transform.localRotation = Quaternion.Slerp(
				transform.localRotation,
				_targetRotation,
				CTRL.deltaTime * rotateSpeed);
				
			transform.localScale = Vector3.Slerp(
				transform.localScale,
				_targetScale,
				CTRL.deltaTime * scaleSpeed);
			
			float distance = Vector3.Distance(transform.localPosition, _targetPosition);
			float angle = Quaternion.Angle(transform.localRotation, _targetRotation);
			float difference = Vector3.Distance(transform.localScale, _targetScale);
			
			if (distance < 0.05f && angle < 1f && difference < 0.05f) 
				_complete = true;
		}

	}
	
}
