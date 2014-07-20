using UnityEngine;
using System.Collections;

[RequireComponent(typeof(WeaponEquip))]
public class WeaponLauncher : MonoBehaviour {

	public Transform projectile;
	public float timeBetweenShots;

	private bool _firing = false;
	private Transform _weaponNozzle;
	private WeaponEquip _equip;

	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(projectile);
		_weaponNozzle = transform.FindChild("Nozzle");
		_equip = GetComponent<WeaponEquip>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton("Fire1") && !_firing && _equip.equipped) {
			StartCoroutine( Fire() );
		}
	}
	
	IEnumerator Fire() {
		_firing = true;
		Transform i = projectile.Spawn(_weaponNozzle.position, _weaponNozzle.rotation);
		i.SendMessage("SetDamageSource", transform.parent.parent); // this is dodgy... assuming t.p.p is player
		yield return new WaitForSeconds(timeBetweenShots);
		_firing = false;
	}
}
