using UnityEngine;
using System.Collections;

public class PrimaryFireProjectile : MonoBehaviour {

	public bool equipped = false;
	public Transform projectile;
	public float timeBetweenShots;
	public Vector3 localPosition;
	
	

	private bool _firing = false;
	private Transform _weaponNozzle;

	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(projectile);
		_weaponNozzle = transform.FindChild("Nozzle");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton("Fire1") && !_firing && equipped) {
			StartCoroutine( Fire() );
		}
	}
	
	IEnumerator Fire() {
		_firing = true;
		projectile.Spawn(_weaponNozzle.position, _weaponNozzle.rotation);
		yield return new WaitForSeconds(timeBetweenShots);
		_firing = false;
	}
}
