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
		if (GameManager.Instance.levelTeardown) return;
		_impact = false;
	}
	
	IEnumerator OnCollisionEnter(Collision col) {
		if (!_impact) {
			_impact = true;
			Transform i = impactPrefab.Spawn(transform.position, transform.rotation);
			i.parent = col.transform;
			col.transform.SendMessage("Damage", _projectile.damage, SendMessageOptions.DontRequireReceiver); // damage info on Projectile component
			transform.Recycle();
			yield return new WaitForSeconds(10f);
			i.Recycle();
		}
		yield return new WaitForEndOfFrame();
	}
}
