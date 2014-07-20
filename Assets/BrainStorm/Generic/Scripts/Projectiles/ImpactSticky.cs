using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class ImpactSticky : MonoBehaviour {

	public Transform impactPrefab;
	
	private bool _impact = false;
	private Projectile _projectile;
	
	void Start() {
		ObjectPool.CreatePool(impactPrefab);
		_projectile = GetComponent<Projectile>();
	}
	
	void OnEnable() {
		_impact = false;
	}
	
	IEnumerator OnCollisionEnter(Collision col) {
		if (!_impact) {
			_impact = true;
			Transform i = impactPrefab.Spawn(transform.position, transform.rotation);
			i.parent = col.transform;
			transform.Recycle();
			col.transform.SendMessage("Damage", _projectile.Damage, SendMessageOptions.DontRequireReceiver); // damage info on Projectile component
			yield return new WaitForSeconds(10f);
			i.Recycle();
		}
		yield return new WaitForEndOfFrame();
	}
}
