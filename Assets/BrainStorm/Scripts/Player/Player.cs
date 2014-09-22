using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		get { return photonView.isMine; }
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
	public Transform head {
		get; private set;
	}
	public PlayerInventory inventory {
		get; private set;
	}
	public GUIController hud {
		get; private set;
	}
	
	// variables used when photonView.isMine
	private int _health;
	private bool _dead = false;
	private bool _noclip = false;
	private GameObject _mainCamera;
	private MouseLook _bodyTurn;
	private MouseLook _headTilt;
	private ScreenFade _fade;
	private Color _hurtOverlay;
	private float _lastHurtTime; 
	
	// variables used when !photonView.isMine
	private Vector3 latestCorrectPos;
	private Vector3 onUpdatePos;
	private float fraction;
	
	void Awake() {
		
		head = transform.Find("Head");
		_mainCamera = head.Find("Main Camera").gameObject;
		
		if (photonView.isMine) {
			motor = GetComponent<CharacterMotorC>();
			inventory = GetComponent<PlayerInventory>();
			_bodyTurn = GetComponent<MouseLook>();
			_headTilt = head.GetComponent<MouseLook>();
			hud = GetComponentInChildren<GUIController>();
			_fade = GetComponent<ScreenFade>();
			_hurtOverlay = Color.Lerp(Color.red, Color.clear, 0.25f);
		}
		
	}
	
	
	void Start() {
		
		if (photonView.isMine) {
			
			motor.enabled = true;
			inventory.enabled = true;
			_bodyTurn.enabled = true;
			_headTilt.enabled = true;
			_mainCamera.SetActive(true);
			ScreenShake.Instance.SetCamera(_mainCamera.transform);
			_fade.enabled = true;
			screenEffects = true;
			hud.enabled = true;
			localPlayer = this;
			name = "Local Player";
			gameObject.layer = LayerMask.NameToLayer("Player");
			hud.InitializeGUI();
			Spawn();
		}
		else {
			gameObject.layer = LayerMask.NameToLayer("Character");
			latestCorrectPos = transform.position;
			onUpdatePos = transform.position;
		}
		
	}
	
	void Spawn() {
	
		if (PhotonNetwork.inRoom)
			transform.position = PlayerSpawn.Multiplayer.randomSpawnPosition;
		
		_health = maxHealth;
		_dead = false;
		SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
	
	}
	

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 pos = transform.localPosition;
			Quaternion rot = transform.localRotation;
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		}
		else
		{
			// Receive latest state information
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			
			latestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
			onUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
			fraction = 0;                           // reset the fraction we alreay moved. see Update()
			
			transform.localRotation = rot;          // this sample doesn't smooth rotation
		}
	}
	
	void Update() {
		
		if(!photonView.isMine) {
			// We get 10 updates per sec. sometimes a few less or one or two more, depending on variation of lag.
			// Due to that we want to reach the correct position in a little over 100ms. This way, we usually avoid a stop.
			// Lerp() gets a fraction value between 0 and 1. This is how far we went from A to B.
			//
			// Our fraction variable would reach 1 in 100ms if we multiply deltaTime by 10.
			// We want it to take a bit longer, so we multiply with 9 instead.
			
			fraction = fraction + Time.deltaTime * 9;
			transform.localPosition = Vector3.Lerp(onUpdatePos, latestCorrectPos, fraction);    // set our pos between A and B
			return;
			// everything else in update is performed by owner
		}
		
		// do not allow motor control while in noclip mode
		if (noclip) motor.enabled = false;
		
		if (_dead) return;
		
		// unfade hurt effects
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
