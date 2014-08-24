using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[AddComponentMenu("Player/Weapon/Launcher")]
public class WeaponLauncher : MonoBehaviour {

	[System.Serializable]
	public class Deterioration {
		public bool enabled = true;
		public float amount = 1f;		// amount to subtract from rateOfFire
		public float cooldown = 1f;		// if we fire during cooldown then subtract amount from rateOfFire
		public float recoveryRate = 1f;	// how much rateOfFire to recover in one second
		[System.NonSerialized]
		public float lastFireTime;
		[System.NonSerialized]
		public float initRateOfFire;
	}
	
	public Transform projectile;		// projectile prefab
	public float rateOfFire;			// how many shots in one second?	
	public Deterioration deteriorate = new Deterioration();
	public float range;					// how long is our raycast?
	public LayerMask raycastMask;		// which layers can we hit?

	private bool _firing = false;	
	private Equipment _equip;
	private Transform _crosshair;
	private Transform _weaponNozzle;	// launch from this position
	private Transform _target;
	private RaycastHit _hit;

	
	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(projectile);
		_weaponNozzle = transform.FindChild("Nozzle");
		_equip = GetComponent<Equipment>();
		_crosshair = transform.FindChild("Crosshair");
		deteriorate.initRateOfFire = rateOfFire;
	}
	
	void Update () {
		_crosshair.gameObject.SetActive(_equip.equipped);
		if (!_equip.equipped) return;
		
		AimWeapon();
		
		// Fire the gun
		if (Input.GetButton("Fire1") && !_firing) {
			StartCoroutine( Fire() );
		}
		
		// recover rate of fire over time
		if (deteriorate.enabled) {
			if (rateOfFire < deteriorate.initRateOfFire) {
				rateOfFire += Time.deltaTime * deteriorate.recoveryRate;
			}
		}
	}
	
	void AimWeapon() {
		// Point the weapon
		Vector3 screenPoint = new Vector3 (
			x: Screen.width/2f,
			y: Screen.height/2f
			);
		Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		_target = null;
		// Point at what the player is looking at
		if (Physics.Raycast(ray, out _hit, range, raycastMask)) {
			Quaternion rotation = Quaternion.LookRotation(_hit.point 
			                                              - transform.position, Player.Instance.transform.up );
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 4f);
			//transform.LookAt(hit.point);
			_crosshair.position = _hit.point;
			Debug.DrawLine(Camera.main.transform.position, _hit.point, Color.red);
			Debug.DrawLine(transform.position, _hit.point, Color.yellow);
			/* hit.transform != hit.collider.transform
				hit.transform is the parent transform of child colliders.
				hit.collider.transform is the transform of the collider 
				we actually hit regardless of position in hierarchy.
				
				This next statement is necessary, for example, the spider
				which has arms that act as shields tagged as "Invulnerable".
				Without this check, the spider will be damaged when it shouldn't.
			*/
			if (_hit.collider.transform.tag != "Invulnerable") {
				_target = _hit.transform;
			}
		}
		// Fake a hit point if we haven't found an object to look at infront of us
		else {
			_crosshair.position = Camera.main.transform.position + Camera.main.transform.forward;
			Quaternion rotation = Quaternion.Euler(_equip.defaultRotation);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, Time.deltaTime);
			_hit.point = transform.position + _weaponNozzle.forward * range;
			Debug.DrawLine(transform.position, _hit.point, Color.yellow);
		}
		
		_crosshair.rotation = transform.rotation;
	}
	
	IEnumerator Fire() {
		_firing = true;
		if (deteriorate.enabled && 
		    deteriorate.lastFireTime + 
		    deteriorate.cooldown +
		    1f/rateOfFire > Time.time) {
		    rateOfFire -= deteriorate.amount;
		}
		
		deteriorate.lastFireTime = Time.time;
		
		BroadcastMessage("FireEffect", SendMessageOptions.DontRequireReceiver);
		
		Transform i = projectile.Spawn(_weaponNozzle.position, _weaponNozzle.rotation);
		i.parent = GameManager.Instance.activeScene;
		i.SendMessage("SetDamageSource", Player.Instance.transform);
		
		if (_target!=null) {
			Debug.Log (name + " hit " + _target.name);// + " with " + i.name + ".");
			i.SendMessage("SetTarget", _target, SendMessageOptions.DontRequireReceiver);
		}
			
		
		i.SendMessage("HitPosition", _hit.point, SendMessageOptions.DontRequireReceiver);
		
		yield return new WaitForSeconds(1f/rateOfFire);
		
		_firing = false;
	}
}
