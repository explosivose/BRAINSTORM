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
	
	public static bool freeze;
	public static float sensitivity = 10f;
	
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;

	void Update ()
	{
		if (Screen.lockCursor) {
			RotateOnAxis();
		}
		else {
			LookAtCursor();
		}
	}
	
	void RotateOnAxis() {
		if (freeze) return;
		float fovFactor = Camera.main.fieldOfView / Player.localPlayer.fov;
		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivity * fovFactor;
			
			rotationY += Input.GetAxis("Mouse Y") * sensitivity * fovFactor;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivity * fovFactor, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivity * fovFactor;
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
			mousePos.y > Screen.height ||
			freeze) 
		{
				lastCall = Time.realtimeSinceStartup;
				return;
		}
		
		Transform cam = Camera.main.transform;
		Ray ray = cam.camera.ScreenPointToRay(mousePos);

		LayerMask mask = LayerMask.NameToLayer("UI");
		Quaternion rotation = cam.rotation;
		float rotationSpeed;
		RaycastHit[] hits;
		RaycastHit hit;
		hits = Physics.RaycastAll(ray, 100f, ~mask);
		if (hits.Length > 0) {
			hit = hits[0];
			foreach(RaycastHit h in hits) {
				float d1 = Vector3.Distance(hit.point, cam.position);
				float d2 = Vector3.Distance(h.point, cam.position);
				if (d2 > d1) hit = h;
			}
			rotationSpeed = 1.2f;
			rotation = Quaternion.LookRotation(hit.collider.bounds.center - cam.position);
		}
		else {
			rotation =  Quaternion.LookRotation(ray.direction);
			rotationSpeed = 0.1f;
		}

		cam.rotation = Quaternion.Lerp(cam.rotation, rotation, deltaTime * rotationSpeed);
		
		lastCall = Time.realtimeSinceStartup;
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}
	
	
}