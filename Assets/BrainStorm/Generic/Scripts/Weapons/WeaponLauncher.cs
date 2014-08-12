using UnityEngine;
using System.Collections;

[RequireComponent(typeof(WeaponEquip))]
public class WeaponLauncher : MonoBehaviour {

	public Transform projectile;
	public float timeBetweenShots;
	public float range;

	private bool _firing = false;
	private Transform _weaponNozzle;
	private WeaponEquip _equip;

	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(projectile);
		_weaponNozzle = transform.FindChild("Nozzle");
		_equip = GetComponent<WeaponEquip>();
	}
	
	void Update () {
		if (!_equip.equipped) return;
		
		// Point the weapon
		Vector3 screenPoint = new Vector3 (
			x: Screen.width/2f,
			y: Screen.height/2f
		);
		Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, range)) {
			transform.LookAt(hit.point);
			Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red);
			Debug.DrawLine(transform.position, hit.point, Color.yellow);
		}
		else {
			Quaternion rotation = Quaternion.Euler(_equip.defaultRotation);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, Time.deltaTime);
			// fake a hit.point
			hit.point = transform.position + _weaponNozzle.forward.normalized * range;
			Debug.DrawLine(transform.position, hit.point, Color.yellow);
		}
		// Fire the gun
		if (Input.GetButton("Fire1") && !_firing) {
			
			StartCoroutine( Fire(hit.transform, hit.point) );
		}
	}
	

	
	IEnumerator Fire(Transform target, Vector3 hit) {
		_firing = true;
		
		BroadcastMessage("FireEffect", SendMessageOptions.DontRequireReceiver);
		
		Transform i = projectile.Spawn(_weaponNozzle.position, _weaponNozzle.rotation);
		
		i.SendMessage("SetDamageSource", transform.parent.parent); // this is dodgy... assuming t.p.p is player
		
		if (target!=null)
			i.SendMessage("SetTarget", target, SendMessageOptions.DontRequireReceiver);
		
		i.SendMessage("HitPosition", hit, SendMessageOptions.DontRequireReceiver);
		
		yield return new WaitForSeconds(timeBetweenShots);
		_firing = false;
	}
}
