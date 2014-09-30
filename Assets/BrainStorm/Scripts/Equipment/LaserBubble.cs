using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Equipment))]
public class LaserBubble : MonoBehaviour {

	public float maxRadius;			// how large is the bubble when active?
	public float minRadius;			// how large is the bubble when inactive?
	public int damage;				// amount of damage in one instance
	public float force;
	public LayerMask targetMask;	// which objects are valid to affect?
	public float cooldown;			// how long until we can use it again?
	public float maxExpandedTime;	// how long can the bubble be expanded for?
	public float rechargeTime;		// how long does it take to recharge?
	
	private bool _expanded;
	private float _radius;
	private DamageInstance _damage = new DamageInstance();
	private Equipment _equipment;
	private MeshRenderer _renderer;
	private Color _initColor;
	private float _charge;
	private float _lastUseTime = -999f;
	
	public bool expanded {
		get { return _expanded; }
		private set {
			_expanded = value;
			if (!_expanded) _lastUseTime = Time.time;
		}
	}
	
	public bool canFire {
		get {
			if (!_equipment.equipped) return false;
			return _charge > 0f && _lastUseTime + cooldown < Time.time;	
		}
	}
	
	public float charge01 {
		get {
			return _charge/maxExpandedTime;
		}
	}
	
	void Awake() {
		_equipment = GetComponent<Equipment>();
		_renderer = GetComponentInChildren<MeshRenderer>();
		_initColor = _renderer.material.GetColor("_TintColor");
		_damage.damage = damage;
		transform.localScale = Vector3.one;
	}
	
	void OnEquip() {
		audio.Play();
		_damage.viewId = _equipment.owner.photonView.viewID;
	}
	
	void OnDrop() {
		audio.Stop();
		transform.localScale = Vector3.one;
	}
	
	void OnDisable() {
		_renderer.material.SetColor ("_TintColor", _initColor);
	}

	void Update() {
		if (!_equipment.equipped) return;
		
		if (Input.GetButton("Fire1")) {
			if (canFire && !expanded) 
				expanded = true;
		}
		
		float noise = (Random.value - 0.5f)/5f; // +/- 10%
		Color color = _initColor;
		
		if(expanded) {
			_lastUseTime = Time.time;
			// expand
			_radius += Time.deltaTime * 8f;
			_radius = Mathf.Min (_radius, maxRadius);
			_radius += _radius * noise;
			transform.localScale = Vector3.one * _radius;
			// discharge
			_charge -= Time.deltaTime;
			// set inactive if no charge left
			expanded = _charge > 0f;
		}
		else {
			// collapse
			_radius -= Time.deltaTime * 16f;
			_radius = Mathf.Max(minRadius, _radius);
			_radius += _radius * noise * 0.1f;
			transform.localScale = Vector3.one * _radius;
			// charge
			_charge += Time.deltaTime * maxExpandedTime/rechargeTime;
			_charge = Mathf.Min (_charge, maxExpandedTime);
			// ready to fire feedback
			if (!canFire) color = Color.clear;
		}

		audio.volume = _radius/maxRadius;
		audio.pitch = charge01 + 0.5f;
		
		_renderer.material.SetColor("_TintColor", Color.Lerp(
			Color.clear,
			color,
			(_charge * noise) +  _charge/maxExpandedTime
			));
	}
	
	void FixedUpdate() {
		if (expanded) Attack();
	}
	
	void Attack() {
		Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, targetMask);
		foreach(Collider c in colliders) {
			if (c.isTrigger) continue;
			if (c.transform == this.transform) continue;
			if (c.transform.tag == "Player") continue;
			_damage.damage = Mathf.RoundToInt(charge01 * damage);
			c.transform.SendMessage("Damage", _damage, SendMessageOptions.DontRequireReceiver);
			if (c.transform.rigidbody) {
				Debug.DrawLine(transform.position, c.transform.position, Color.red);
				c.transform.rigidbody.AddExplosionForce(force * charge01, transform.position, _radius);
			}
			else {
				Debug.DrawLine(transform.position, c.transform.position, Color.grey);
			}
		}
	}
}
