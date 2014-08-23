using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Weapon/Launcher")]
public class WeaponLauncher : MonoBehaviour {

	public Transform projectile;
	public float timeBetweenShots;
	public float range;
	public LayerMask raycastMask;

	private bool _firing = false;
	private Transform _weaponNozzle;
	private Equipment _equip;
	private Transform _crosshair;
	
	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(projectile);
		_weaponNozzle = transform.FindChild("Nozzle");
		_equip = GetComponent<Equipment>();
		_crosshair = transform.FindChild("Crosshair");
	}
	
	void Update () {
		_crosshair.gameObject.SetActive(_equip.equipped);
		if (!_equip.equipped) return;
		
		// Point the weapon
		Vector3 screenPoint = new Vector3 (
			x: Screen.width/2f,
			y: Screen.height/2f
		);
		Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		RaycastHit hit;
		Transform target = null;
		if (Physics.Raycast(ray, out hit, range, raycastMask)) {
			if (!hit.collider.isTrigger) {
				transform.LookAt(hit.point);
				_crosshair.position = hit.point;
				Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red);
				Debug.DrawLine(transform.position, hit.point, Color.yellow);
				/* hit.transform != hit.collider.transform
					hit.transform is the parent transform of child colliders.
					hit.collider.transform is the transform of the collider 
					we actually hit regardless of position in hierarchy.
					
					This next statement is necessary, for example, the spider
					which has arms that act as shields tagged as "Invulnerable".
					Without this check, the spider will be damaged when it shouldn't.
				*/
				if (hit.collider.transform.tag != "Invulnerable") {
					target = hit.transform;
				}
			}

		}
		else {
			_crosshair.position = Camera.main.transform.position + Camera.main.transform.forward;
			Quaternion rotation = Quaternion.Euler(_equip.defaultRotation);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, Time.deltaTime);
			// fake a hit.point
			hit.point = transform.position + _weaponNozzle.forward.normalized * range;
			Debug.DrawLine(transform.position, hit.point, Color.yellow);
		}
		
		_crosshair.rotation = transform.rotation;
		
		// Fire the gun
		if (Input.GetButton("Fire1") && !_firing) {
			StartCoroutine( Fire(target, hit.point) );
		}
	}
	
	IEnumerator Fire(Transform target, Vector3 hit) {
		_firing = true;
		
		BroadcastMessage("FireEffect", SendMessageOptions.DontRequireReceiver);
		
		Transform i = projectile.Spawn(_weaponNozzle.position, _weaponNozzle.rotation);
		i.parent = GameManager.Instance.activeScene;
		i.SendMessage("SetDamageSource", transform.parent.parent); // this is dodgy... assuming t.p.p is player
		
		if (target!=null) {
			Debug.Log (name + " hit " + target.name);// + " with " + i.name + ".");
			i.SendMessage("SetTarget", target, SendMessageOptions.DontRequireReceiver);
		}
			
		
		i.SendMessage("HitPosition", hit, SendMessageOptions.DontRequireReceiver);
		
		yield return new WaitForSeconds(timeBetweenShots);
		_firing = false;
	}
}
