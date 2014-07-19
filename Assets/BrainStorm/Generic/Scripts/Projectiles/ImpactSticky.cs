using UnityEngine;
using System.Collections;

public class ImpactSticky : MonoBehaviour {

	public Transform impactPrefab;
	
	private bool impact = false;
	
	void Start() {
		ObjectPool.CreatePool(impactPrefab);
	}
	
	void OnEnable() {
		impact = false;
	}
	
	IEnumerator OnCollisionEnter(Collision col) {
		if (!impact) {
			impact = true;
			Transform i = impactPrefab.Spawn(transform.position, transform.rotation);
			i.parent = col.transform;
			transform.Recycle();
			col.transform.BroadcastMessage("Damage", SendMessageOptions.DontRequireReceiver); // damage info on Projectile component
			yield return new WaitForSeconds(10f);
			i.Recycle();
		}
		yield return new WaitForEndOfFrame();
	}
}
