﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Player")]
[RequireComponent(typeof(CharacterMotorC))]
[RequireComponent(typeof(ScreenFade))]
public class Player : Photon.MonoBehaviour {

	public static Player localPlayer;
	
	
	[System.Serializable]
	public class AudioLibrary {
		public float volume;
		public AudioClip hurt;
		public AudioClip jump;
		public AudioClip land;
	}

	public int maxHealth;
	public float hurtEffectDuration = 0.1f;
	
	public AudioLibrary sounds = new AudioLibrary();
	
	public bool isLocalPlayer {
		get; private set;
	}
	public float health01 {
		get { return (float)_health/(float)maxHealth; }
	}
	public bool screenEffects { 
		get; set; 
	}
	public bool noclip { 
		get {
			return _noclip;	
		}
		set {
			if (!isLocalPlayer) return;
			_noclip = value;
			motor.enabled = !value;
		}
	}
	public CharacterMotorC motor {
		get; private set;
	}
	public GameObject mainCamera {
		get {
			return _mainCamera;
		}
	}
	public PlayerInventory inventory {
		get; private set;
	}
	
	private MouseLook _mouseLook;
	private GameObject _mainCamera;
	private int _health;
	private bool _dead = false;
	private bool _noclip = false;
	private ScreenFade _fade;
	private Color _hurtOverlay;
	private float _lastHurtTime; 
	
	void Awake() {
		motor = GetComponent<CharacterMotorC>();
		inventory = GetComponent<PlayerInventory>();
		_mouseLook = GetComponent<MouseLook>();
		_mainCamera = transform.Find("Main Camera").gameObject;
		_fade = GetComponent<ScreenFade>();
		_hurtOverlay = Color.Lerp(Color.red, Color.clear, 0.25f);
	}
	
	void Start() {
		Spawn();
	}
	
	void Spawn() {
		if (!PhotonNetwork.inRoom) SetLocalPlayer(true);
		_health = maxHealth;
		_dead = false;
		SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
	}
	
	void SetLocalPlayer(bool value) {
		isLocalPlayer = value;
		
		motor.enabled = value;
		inventory.enabled = value;
		_mouseLook.enabled = value;
		_mainCamera.SetActive(value);
		_fade.enabled = value;
		
		if (value) {
			localPlayer = this;
			name = "Local Player";
			gameObject.layer = LayerMask.NameToLayer("Player");
			GUIController.Instance.InitializeGUI();
			ScreenShake.Instance.SetCamera(_mainCamera.transform);
		}
	}
	
	void Update() {
		
		if(!isLocalPlayer) return;
		
		// do not allow motor control while in noclip mode
		if (noclip) motor.enabled = false;
		
		if (_dead) return;
		
		if (_lastHurtTime + hurtEffectDuration <= Time.time && screenEffects)
			_fade.StartFade(Color.clear, hurtEffectDuration);
			
		// Get the input vector from keyboard or analog stick
		Vector3 directionVector;
		directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		if (directionVector != Vector3.zero) {
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			float directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;
		}
		
		if (noclip) {
			directionVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
			directionVector *= motor.movement.maxForwardSpeed;
			transform.position += _mainCamera.transform.rotation * directionVector * CTRL.deltaTime;
		}
		else {
			// Apply the direction to the CharacterMotor
			motor.inputMoveDirection = transform.rotation * directionVector;
			motor.inputLookDirection = _mainCamera.transform.rotation * directionVector;
			motor.inputJump = Input.GetButton("Jump");
			motor.inputSprint = Input.GetButton("Sprint");
		}
	}
	
	void OnJump() {
		PlaySound(sounds.jump);
	}
	
	void OnLand() {
		PlaySound(sounds.land);
	}
	
	void PlaySound(AudioClip clip) {
		audio.clip = clip;
		audio.loop = false;
		audio.volume = sounds.volume;
		audio.Play();
	}
	
	public void Killed(Transform victim) {
		Debug.Log ("Player killed something :O");
	}
	
	[RPC]
	void Damage(int damage) {
		// something in my game has damaged this player
		if (photonView.isMine) {
			_health -= damage;
			Debug.Log ("Player got " + damage + " damage.");
			AudioSource.PlayClipAtPoint(sounds.hurt, transform.position);
			ScreenShake.Instance.Shake(Mathf.Min(0.5f,(float)damage/(float)maxHealth), 0.3f);
			_lastHurtTime = Time.time;
			if (screenEffects)
				_fade.StartFade(_hurtOverlay, hurtEffectDuration);
			if (_health < 0) StartCoroutine ( Death() );
		}
		else {
			photonView.RPC("Damage", photonView.owner, damage);
		}
	}

	IEnumerator Death() {
		_dead = true;
		if (screenEffects)
			_fade.StartFade(Color.black, 1f);
		float vol = AudioListener.volume;
		AudioListener.volume = 0f;
		SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(1.5f);
		AudioListener.volume = vol;
		if (!PhotonNetwork.inRoom) {
			GameManager.Instance.ChangeScene( Scene.Tag.Lobby );
		}
		
		Spawn();
	}
}
