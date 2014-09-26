using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Player/Weapon/Launcher")]
public class WeaponLauncher : Photon.MonoBehaviour {

	[System.Serializable]
	public class Spread {
		public bool enabled = false;
		public float minAngle = 8f;			// angle spread
		public float deterioration = 1f;	// amount to add/shot to spread
		public float recoveryRate = 1f;			// amount to subtract/second to spread
		public float angle {get; set;}
	}
	
	[System.Serializable]
	public class Deterioration {
		public bool enabled = true;
		public float amount = 1f;		// amount to subtract from rateOfFire
		public float cooldown = 1f;		// if we fire during cooldown then subtract amount from rateOfFire
		public float recoveryRate = 1f;	// how much rateOfFire to recover in one second
		// autoproperties are hidden from unity inspector
		public float initRateOfFire { get; set; }
	}
	
	[System.Serializable]
	public class Recoil {
		public bool enabled = true;
		public float amount = 0.5f;		// lerp amount
		// autoproperties are hidden from unity inspector
		public Vector3 recoilPosition { get; set; }
		public Quaternion recoilRotation { get; set; }
		public float t { get; set; }
	}
	
	[System.Serializable]
	public class Kickback {
		public bool enabled = false;
		public float amount = 1f;
	}
	
	[System.Serializable]
	public class Shake {
		public bool enabled = false;
		public float amount = 0.1f;
	}
	
	[System.Serializable]
	public class Zoom {
		public bool enabled = false;
		public float level = 15f;
		public float originalLevel {get; set;}
		public bool zoomed {get; set;}
	}
	
	public Transform projectile;		// projectile prefab
	public int shots = 1;				// how many shots in one fire?
	public float rateOfFire;			// how many fires in one second?	
	public Spread spread = new Spread();
	public Deterioration deteriorate = new Deterioration();
	public Recoil recoil = new Recoil();
	public Shake shake = new Shake();
	public Zoom zoom = new Zoom();
	public Kickback kickback = new Kickback();
	public float range;					// how long is our raycast?
	public LayerMask raycastMask;		// which layers can we hit?
	public float minXhairSize;			// controls size of crosshair on screen
	public float readyTime;				// how much wait time after equiping?

	private bool _firing = false;
	private bool _ready = false;
	private Equipment _equip;
	private Transform _crosshair;
	private Transform _weaponNozzle;	// launch from this position
	private Transform _handle; 			// recoil around this
	private Transform _target;
	private RaycastHit _hit;
	private float _lastFireTime;
	private float _nextPossibleFireTime; 

	
	// Use this for initialization
	void Start () {
		ObjectPool.CreatePool(projectile);
		_weaponNozzle = transform.Find("Nozzle");
		_handle = transform.Find("Handle");
		if (!_handle) _handle = transform;
		_equip = GetComponent<Equipment>();
		_crosshair = transform.FindChild("Crosshair");
		deteriorate.initRateOfFire = rateOfFire;
		spread.angle = spread.minAngle;
		zoom.originalLevel = 60f;
	}
	
	IEnumerator OnEquip() {
		yield return new WaitForSeconds(readyTime);
		
		_ready = true;
		_crosshair.gameObject.SetActive(true);
	}
	
	void OnDrop() {
		_ready = false;
		_crosshair.gameObject.SetActive(false);
	}
	
	void OnHolster() {
		_ready = false;
		_crosshair.gameObject.SetActive(false);
	}
	
	void Update () {

		if (!_equip.equipped) return;
		if (GameManager.Instance.paused) return;
		AimWeapon();
		if (!_ready) return;
		if (recoil.enabled) WeaponRecoil();
		
		// Fire the gun
		if (Input.GetButton("Fire1") && !_firing) {
			photonView.RPC("FireRPC", PhotonTargets.All, Random.seed, spread.angle);
		}
		
		// zoooomo
		if (Input.GetButton("Fire2") && zoom.enabled) {
			Camera.main.fieldOfView = zoom.level;
			zoom.zoomed = true;
		}
		else {
			Camera.main.fieldOfView = zoom.originalLevel;
			zoom.zoomed = false;
		}
		
		// recover rate of fire over time
		if (deteriorate.enabled) {
			if (rateOfFire < deteriorate.initRateOfFire) {
				rateOfFire += Time.deltaTime * deteriorate.recoveryRate;
			}
		}
		
		if (spread.enabled) {
			spread.angle -= spread.recoveryRate * Time.deltaTime;
			spread.angle = Mathf.Max(spread.angle, spread.minAngle);
		}
	}
	
	void AimWeapon() {
		// Point the weapon
		Vector3 screenPoint = new Vector3 (
			x: Screen.width/2f,
			y: Screen.height/2f
			);
		Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		Transform mainCam = Camera.main.transform;
		Vector3 crosshairPos;
		_target = null;
		// Point at what the player is looking at
		if (Physics.Raycast(ray, out _hit, range, raycastMask)) {
			Vector3 direction = (_hit.point - transform.position).normalized;
			Quaternion rotation = Quaternion.LookRotation(direction, Player.localPlayer.transform.up );
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 4f);
			
			crosshairPos = _hit.point;
			if (Vector3.Distance(_weaponNozzle.position, _hit.point) > minXhairSize) {
				crosshairPos = mainCam.position + mainCam.forward * minXhairSize;
			}
			
			_crosshair.position = Vector3.Lerp(_crosshair.position, crosshairPos, Time.deltaTime * 8f);
			Debug.DrawLine(mainCam.position, _hit.point, Color.red);
			Debug.DrawLine(_weaponNozzle.position, _hit.point, Color.yellow);
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
			crosshairPos = mainCam.position + mainCam.forward * minXhairSize;
			Quaternion rotation = Quaternion.Euler(_equip.defaultRotation);
			transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, Time.deltaTime);
			_hit.point = transform.position + _weaponNozzle.forward * range;
			Debug.DrawLine(transform.position, _hit.point, Color.yellow);
		}
		_crosshair.position = Vector3.Lerp(_crosshair.position, crosshairPos, Time.deltaTime * 8f);
		_crosshair.rotation = transform.rotation;
	}
	
	void WeaponRecoil() {
		recoil.t = (Time.time - _lastFireTime) / 
					(_nextPossibleFireTime - _lastFireTime);
		recoil.recoilPosition = _equip.equippedPosition + Vector3.back;
		recoil.recoilRotation = Quaternion.LookRotation(_handle.up);
		
		// recoil effect kinda works but it's not very well thought out
		// have another go at it later :-)
		
		// pull back
		if (recoil.t < recoil.amount) {
			transform.localPosition = Vector3.Lerp(transform.localPosition,
			                                        recoil.recoilPosition, recoil.t * recoil.amount);
		}
		// and ease forward
		else {
			transform.localPosition = Vector3.Lerp(transform.localPosition,
			                                        _equip.equippedPosition, recoil.t * (1f - recoil.amount));
		}

		// this really doesn't work. 
		// probably because we're already modifying the rotation
		// to aim the weapon in AimWeapon()
		//transform.localRotation=  Quaternion.Slerp(transform.localRotation,
		//										recoil.recoilRotation, recoil.t);
	}
	
	[RPC]
	void FireRPC(int seed, float spreadAngle) {
		if (_equip.owner.isLocalPlayer) {
			StartCoroutine(Fire (seed));
		}
		else {
			spread.angle = spreadAngle;
			FireProjectile(seed);
		}
	}
	
	IEnumerator Fire(int seed) {
		_firing = true;
		if (deteriorate.enabled && 
		    _lastFireTime + deteriorate.cooldown + 1f/rateOfFire > Time.time) {
		    rateOfFire -= deteriorate.amount;
		    rateOfFire = Mathf.Max(rateOfFire, 0.5f);
		}
		
		_lastFireTime = Time.time;
		
		FireProjectile(seed);
		
		if (kickback.enabled) {
			_equip.owner.motor.Kickback(-_weaponNozzle.forward * kickback.amount);
		}
		
		if (spread.enabled) {
			spread.angle += spread.deterioration;
		}
		
		float wait = 1f/rateOfFire;
		_nextPossibleFireTime = Time.time + wait;
		
		if (shake.enabled) {
			ScreenShake.Instance.Shake(shake.amount, Mathf.Min(wait, 0.4f) );
		}
		
		yield return new WaitForSeconds(wait);
		
		_firing = false;
	}
	
	void FireProjectile(int seed) {
		// figure out where we're aiming 
		
		BroadcastMessage("FireEffect", SendMessageOptions.DontRequireReceiver);
		
		_equip.AudioStart();
		
		float angle = 0;
		if (spread.enabled) {
			angle = Mathf.Deg2Rad * Mathf.Clamp(
				90f-spread.angle,
				Mathf.Epsilon,
				90f-Mathf.Epsilon);
		}

			
		float distance = Mathf.Tan(angle);
		
		for (int i = 0; i < shots; i++) {
			Vector3 dir = _weaponNozzle.forward;
			if (spread.enabled) {
				Random.seed = seed+i;
				Vector2 pointInCircle = Random.insideUnitCircle;
				dir = new Vector3(pointInCircle.x, pointInCircle.y, distance);
				dir = _weaponNozzle.rotation * dir;
			}
			Quaternion rot = Quaternion.LookRotation(dir);
			Ray ray = new Ray(_weaponNozzle.position, dir);
			if (Physics.Raycast(ray, out _hit, range, raycastMask)) {
				if (_hit.collider.transform.tag != "Invulnerable") {
					_target = _hit.transform;
				}
			}
			else {
				_hit.point = _weaponNozzle.position + dir * range;
			}
			
			Transform p = projectile.Spawn(_weaponNozzle.position, rot);
			p.parent = GameManager.Instance.activeScene.instance;
			/*
			if (_target!=null) {
				Debug.Log (name + " hit " + _target.name);// + " with " + p.name + ".");
				if (_target.tag != "Untagged")
					p.SendMessage("SetTarget", _target, SendMessageOptions.DontRequireReceiver);
			}
			p.SendMessage("HitPosition", _hit.point, SendMessageOptions.DontRequireReceiver);
			*/
		}
		
		_target = null;
	}
}
