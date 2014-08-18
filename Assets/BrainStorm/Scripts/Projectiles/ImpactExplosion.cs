using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class ImpactExplosion : MonoBehaviour {

	public Transform impactPrefab;
	public float impactRadius;
	public float explosionForceAtCenter; 
	
	private bool _impact = false;
	private Projectile _projectile;

	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(impactPrefab);
		_projectile = GetComponent<Projectile>();
	}
	
	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		_impact = false;
		rigidbody.isKinematic = false;
	}
	
	IEnumerator OnCollisionEnter(Collision col) {
		if (!_impact) {
			_impact = true;
			// spawn explosion effect
			Transform i = impactPrefab.Spawn(transform.position, transform.rotation);
			i.parent = GameManager.Instance.activeScene;
			i.particleSystem.time = 0f;
			i.particleSystem.Play();
			
			// deal damage and physics forces to nearby objects
			
			Collider[] cols = Physics.OverlapSphere(transform.position, impactRadius);
			foreach(Collider c in cols) {
				/* this is crazy laggy
				if (c.rigidbody != null) {
					c.rigidbody.AddExplosionForce(explosionForceAtCenter, transform.position, impactRadius, 0f, ForceMode.Impulse);
					
				}
				*/
				c.SendMessage("Damage", _projectile.Damage, SendMessageOptions.DontRequireReceiver);
				Debug.DrawLine(c.transform.position, transform.position, Color.yellow, 1f);
				
			}
			
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.isKinematic = true;
			yield return new WaitForSeconds(i.particleSystem.duration);
			i.Recycle();
			transform.Recycle();
		}
	}
}
