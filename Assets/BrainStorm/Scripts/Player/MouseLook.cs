using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Player/Mouse Look")]
public class MouseLook : MonoBehaviour {
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;

	void Update ()
	{
		//if (GameManager.Instance.paused) return;
		if (Screen.lockCursor) {
			RotateOnAxis();
		}
		else {
			LookAtCursor();
		}
	}
	
	void RotateOnAxis() {
		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}
	
	float lastCall = 0f;
	void LookAtCursor() {
		// calculate our own delta time because Time.deltaTime doesn't work
		// whilst Time.timeScale = 0f
		float deltaTime = Time.realtimeSinceStartup - lastCall;
		
		Vector3 screenCenter = new Vector3(Screen.width/2f, Screen.height/2f, 0f);
		Vector3 mousePos = Input.mousePosition;
		
		screenCenter.z = mousePos.z;
		
		// don't rotate if mouse is offscreen
		if (mousePos.x < 0 ||
		 	mousePos.x > Screen.width ||
			mousePos.y < 0 ||
			mousePos.y > Screen.height) 
		{
				lastCall = Time.realtimeSinceStartup;
				return;
		}
		
		Vector3 lookAt = Vector3.Lerp(screenCenter, mousePos, deltaTime);
		
		Ray ray = Camera.main.ScreenPointToRay(mousePos);
		Quaternion rotation =  Quaternion.LookRotation(ray.direction);
		Transform cam = Camera.main.transform;
		if (GameManager.Instance.paused)
		cam.rotation = Quaternion.Lerp(cam.rotation, rotation, deltaTime * 2f);
		
		lastCall = Time.realtimeSinceStartup;
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}
	
	
}