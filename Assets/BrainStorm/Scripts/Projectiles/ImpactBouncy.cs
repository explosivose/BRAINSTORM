using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Projectile))]
public class ImpactBouncy : MonoBehaviour {

	public int bounces;
	public Transform bouncePrefab;
	public Transform impactPrefab;
	public bool addVelocityOnBounce;
	
	private int b;
	private Projectile _projectile;
	
	void Start() {
		ObjectPool.CreatePool(impactPrefab);
		ObjectPool.CreatePool(bouncePrefab);
		_projectile = GetComponent<Projectile>();
	}
	
	void OnEnable() {
		if (GameManager.Instance.levelTeardown) return;
		b = bounces;
		rigidbody.isKinematic = false;
	}
	
	IEnumerator OnCollisionEnter(Collision col) {
		if (b <= 0) {
			Transform i = impactPrefab.Spawn(transform.position, transform.rotation);
			i.parent = col.transform;
			rigidbody.isKinematic = false;
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.isKinematic = true;
			col.transform.SendMessage("Damage", _projectile.damage, SendMessageOptions.DontRequireReceiver); // damage info on Projectile component
			yield return new WaitForSeconds(10f);
			i.Recycle();
			transform.Recycle();
		}
		else {
			b--;
			ContactPoint contact = col.contacts[0];
			if (addVelocityOnBounce)
				rigidbody.AddForce(contact.normal * col.relativeVelocity.magnitude, ForceMode.VelocityChange);
			transform.rotation = Quaternion.LookRotation(contact.normal);
			col.transform.SendMessage("Damage", _projectile.damage, SendMessageOptions.DontRequireReceiver);
			Transform i = bouncePrefab.Spawn(contact.point, Quaternion.LookRotation(contact.normal));
			i.parent = GameManager.Instance.activeScene.instance;
			i.particleSystem.time = 0f;
			i.particleSystem.Play();
			yield return new WaitForSeconds(i.particleSystem.duration);
			i.Recycle();
		}
	}
}
