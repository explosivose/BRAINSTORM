using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Projectile))]
public class ImpactBlunt : MonoBehaviour {

	public Transform impactPrefab;
	public bool destroyOnImpact;
	public float minimumVelocityForDamage = 5f;
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
		Debug.Log ("Collison");
		if (!_impact && col.relativeVelocity.magnitude > minimumVelocityForDamage) {
			_impact = true;
			Quaternion rotation = Quaternion.LookRotation(col.contacts[0].normal);
			Transform i = impactPrefab.Spawn(col.contacts[0].point, rotation);
			i.parent = col.transform;
			if(destroyOnImpact) transform.Recycle();
			col.transform.SendMessage("Damage", _projectile.Damage, SendMessageOptions.DontRequireReceiver); // damage info on Projectile component
			yield return new WaitForSeconds(10f);
			i.Recycle();
		}
		yield return new WaitForEndOfFrame();
	}
}
