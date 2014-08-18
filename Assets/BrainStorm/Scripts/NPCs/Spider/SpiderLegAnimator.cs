using UnityEngine;
using System.Collections;

public class SpiderLegAnimator : MonoBehaviour {

	public enum Leg {
		FrontLeft, FrontRight, BackLeft, BackRight
	}
	public Leg thisLegIs = Leg.FrontLeft;
	public LayerMask raycastMask;
	public Transform spiderBody;
	public Transform target;
	public Transform elbow;
	public float legMoveSpeed;
	public float animationSpeed;
	public float targetSearchRadius;
	public float legLength;
	
	private Vector3 updateTarget;
	private Vector3 updateElbow;

	
	void Start () {
		StartCoroutine( Animate() );
	}
	
	void Update () {
		target.position =  Vector3.Lerp(target.position, updateTarget, legMoveSpeed * Time.deltaTime);
		elbow.position = Vector3.Lerp(elbow.position, updateElbow, 1f * Time.deltaTime);
	}
	
	IEnumerator Animate() {
		if (animationSpeed == 0f) animationSpeed = 0.001f;
		yield return new WaitForSeconds(Random.value * 1f/animationSpeed);
		while(this.enabled) {
			Vector3? t = LegTarget();
			if (t.HasValue)  {
				// if target is far away enough from the body (avoids errors in ik)
				if (Vector3.Distance(t.Value, transform.position) > 4f) {
					updateTarget = t.Value; 
				}
			}
			
			updateElbow = ElbowTarget(transform.position, updateTarget);
			
			if (animationSpeed == 0f) animationSpeed = 0.001f;
			yield return new WaitForSeconds(1f/animationSpeed);
		}
	}
	
	Vector3? LegTarget() {
		Vector3 reachDirection;
		switch (thisLegIs) {
		default:
		case Leg.FrontLeft:
			reachDirection = 1f* spiderBody.forward - spiderBody.right;
			break;
		case Leg.FrontRight:
			reachDirection = 1f* spiderBody.forward + spiderBody.right;
			break;
		case Leg.BackLeft:
			reachDirection = 1.5f* -spiderBody.forward - spiderBody.right;
			break;
		case Leg.BackRight:
			reachDirection = 1.5f* -spiderBody.forward + spiderBody.right;
			break;
		}
		
		Ray ray = new Ray(transform.position, reachDirection);
		Debug.DrawRay(transform.position, reachDirection, Color.cyan, 0.5f);
		RaycastHit hit;
		if (Physics.SphereCast(ray, targetSearchRadius, out hit, legLength, raycastMask.value)) {
			return hit.point;
		}
		else {
			return null;
		}
	}
	
	Vector3 ElbowTarget(Vector3 body, Vector3 target) {
		// bring the elbow closer to the body
		Vector3 dir = (body - target).normalized;
		body += dir * Vector3.Distance(body, target)/2f;
		
		// return midpoint
		return new Vector3( 
			(body.x + target.x) / 2,
			10f + (body.y + target.y) / 2,
			(body.z + target.z) / 2
		);
	}
	
}
