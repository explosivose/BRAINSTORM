using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Equipment/Blink")]
public class BlinkTarget : Equipment {

	public Transform 	blinkTargetPrefab;
	public float 		blinkSpeed;
	public float 		maxRange;					// how long is our raycast?
	public LayerMask 	raycastMask;			// which layers can we hit?
	private Transform 	_blinkTarget;
	private bool 		_blinking;
	private Transform	_player;
	
	void Awake() {
		blinkTargetPrefab.CreatePool();
	}
	
	protected override void Start ()
	{
		base.Start ();
		_player = Player.Instance.transform;
	}
	
	void OnEquip() {
		PlayerInventory.Instance.hasBlink = true;
	}
	
	void OnDrop() {
		PlayerInventory.Instance.hasBlink = false;
		if (_blinkTarget) _blinkTarget.Recycle();
	}
	
	// Update is called once per frame
	void Update () {
		if (!equipped) return;
		
		if (_blinking) {
			Blink();
			return;
		}
		
		if (Input.GetButtonDown("Sprint")) {
			_blinkTarget = blinkTargetPrefab.Spawn();
		}
		if (Input.GetButtonUp("Sprint")) {
			StartBlink();
		}
		if (_blinkTarget) {
			PositionTarget();
		}
	}
	
	void StartBlink() {
		_blinking = true;
		Player.Instance.motorEnabled = false;
	}
	
	void StopBlink() {
		_blinking = false;
		Player.Instance.motorEnabled = true;
		_blinkTarget.Recycle();
	}
	
	void Blink() {
		_player.position = Vector3.Slerp(
			_player.position, 
			_blinkTarget.position,
			Time.deltaTime * blinkSpeed
		);
		if (Vector3.Distance(_player.position, _blinkTarget.position) < 1f) {
			StopBlink();
		}
	}

	void PositionTarget() {
		Vector3 screenPoint = new Vector3 (
			x: Screen.width/2f,
			y: Screen.height/2f
			);
		Ray ray = Camera.main.ScreenPointToRay(screenPoint);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, range, raycastMask)) {
			_blinkTarget.position = hit.point + hit.normal * 2f;
		}
		else {
			_blinkTarget.position = transform.position +
				ray.direction * range;
		}
	}
}
