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
		public float 		volume;
		public AudioClip 	hurt;
		public AudioClip 	jump;
		public AudioClip 	land;
		public AudioClip	hitNotice;
	}

	public int 			maxHealth;
	public float 		fov;
	public Transform	deadBody;
	public Material 	blinkMaterial;
	public Material		cloakMaterial;
	public float 		hurtEffectDuration = 0.1f;
	
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
	
	
	private int 			_health;
	private bool 			_dead = false;
	private bool 			_noclip = false;
	private bool			_hitNoticePlaying = false;
	private GameObject 		_mainCamera;
	private MeshRenderer 	_ren;
	private TrailRenderer	_trail;
	private Material 		_originalMaterial;
	private MouseLook 		_bodyTurn;
	private MouseLook 		_headTilt;
	private ScreenFade 		_fade;
	private Color 			_hurtOverlay;
	private float 			_lastHurtTime; 
	
	// members used for network sync
	private Vector3 		latestCorrectPos;
	private Vector3 		onUpdatePos;
	private Quaternion 		latestCorrectRot = Quaternion.identity;
	private Quaternion 		onUpdateRot = Quaternion.identity;
	private Quaternion 		latestCorrectHeadRot = Quaternion.identity;
	private Quaternion 		onUpdateHeadRot = Quaternion.identity;
	private float 			fraction;
	
	void Awake() {
		
		head = transform.Find("Head");
		_mainCamera = head.Find("Main Camera").gameObject;
		_ren = GetComponentInChildren<MeshRenderer>();
		_originalMaterial = _ren.material;
		_trail = GetComponentInChildren<TrailRenderer>();
		
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
		
		name = photonView.owner.name;
		
		if (photonView.isMine) {
			inventory.enabled = true;
			_bodyTurn.enabled = true;
			_headTilt.enabled = true;
			_mainCamera.SetActive(true);
			ScreenShake.Instance.SetCamera(_mainCamera.transform);
			_fade.enabled = true;
			screenEffects = true;
			hud.enabled = true;
			localPlayer = this;
			name = photonView.owner.name;
			gameObject.layer = LayerMask.NameToLayer("Player");
			hud.InitializeGUI();
			Spawn();
		}
		else {
			gameObject.layer = LayerMask.NameToLayer("Character");
			latestCorrectPos = transform.position;
			onUpdatePos = transform.position;
			transform.Find("Name").gameObject.SetActive(true);
			
		}
		
	}
	
	[RPC]
	void SpawnRPC() {
		_ren.material = _originalMaterial;
		_dead = false;
		if (photonView.isMine) Spawn();
	}
	
	void Spawn() {
	
		if (PhotonNetwork.inRoom)
			transform.position = PlayerSpawn.Multiplayer.randomSpawnPosition;

		motor.enabled = true;
		_health = maxHealth;
		AudioListener.volume = PlayerPrefs.GetFloat(Options.keyVolume);
		Camera.main.fieldOfView = fov;
		SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
		
	}
	

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			Vector3 pos = transform.localPosition;
			Quaternion rot = transform.localRotation;
			Quaternion headRot = head.localRotation;
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			stream.Serialize(ref headRot);
		}
		else
		{
			// Receive latest state information
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			Quaternion headRot = Quaternion.identity;
			
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			stream.Serialize(ref headRot);
			
			latestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
			onUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
			latestCorrectRot = rot;
			onUpdateRot = transform.localRotation;
			latestCorrectHeadRot = headRot;
			onUpdateHeadRot = head.localRotation;
			fraction = 0;                           // reset the fraction we alreay moved. see Update()
			

		}
	}
	
	void Update() {
		
		if (_dead) return;
		
		if(!photonView.isMine) {
			// We get 10 updates per sec. sometimes a few less or one or two more, depending on variation of lag.
			// Due to that we want to reach the correct position in a little over 100ms. This way, we usually avoid a stop.
			// Lerp() gets a fraction value between 0 and 1. This is how far we went from A to B.
			//
			// Our fraction variable would reach 1 in 100ms if we multiply deltaTime by 10.
			// We want it to take a bit longer, so we multiply with 9 instead.
			
			fraction = fraction + Time.deltaTime * 9;
			transform.localPosition = Vector3.Lerp(onUpdatePos, latestCorrectPos, fraction);    // set our pos between A and B
			transform.localRotation = Quaternion.Lerp(onUpdateRot, latestCorrectRot, fraction);
			head.localRotation = Quaternion.Lerp(onUpdateHeadRot, latestCorrectHeadRot, fraction);
			return;
			// everything else in update is performed by owner
		}
		
		// do not allow motor control while in noclip mode
		if (noclip) motor.enabled = false;
		
		
		
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
	
	[RPC]
	public void OnBlinkStartRPC() {
		_ren.material = blinkMaterial;
		_trail.time = 1f;
		SendMessage("OnBlinkStart");
	}
	
	[RPC]
	public void OnBlinkStopRPC() {
		_ren.material = _originalMaterial;
		_trail.time = 0f;
		SendMessage("OnBlinkStop");
	}
	
	[RPC]
	public void OnCloakStartRPC() {
		_ren.material = cloakMaterial;
		SendMessage("OnCloakStart");
	}
	
	[RPC]
	public void OnCloakStopRPC() {
		_ren.material = _originalMaterial;
		SendMessage("OnCloakStop");
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
	
	public void HitNotice() {
		if (!_hitNoticePlaying) StartCoroutine( HitNoticeRoutine() );
	}
	
	IEnumerator HitNoticeRoutine() {
		_hitNoticePlaying = true;
		yield return new WaitForSeconds(0.05f);
		AudioSource.PlayClipAtPoint(sounds.hitNotice, transform.position);
		yield return new WaitForSeconds(0.1f);
		_hitNoticePlaying = false;
	}
	
	[RPC]
	void Damage(int damage) {
		
		if (photonView.isMine) {
			_health -= damage;
			Debug.Log ("Player got " + damage + " damage.");
			AudioSource.PlayClipAtPoint(sounds.hurt, transform.position);
			ScreenShake.Instance.Shake(Mathf.Min(0.5f,(float)damage/(float)maxHealth), 0.3f);
			_lastHurtTime = Time.time;
			if (screenEffects)
				_fade.StartFade(_hurtOverlay, hurtEffectDuration);
			if (_health < 0)
				photonView.RPC("DeathRPC", PhotonTargets.All);
		}
		else {
			photonView.RPC("Damage", photonView.owner, damage);
		}
	}

	[RPC]
	void DeathRPC() {
		StartCoroutine(Death ());
	}

	IEnumerator Death() {
		_dead = true;
		if (screenEffects)
			_fade.StartFade(Color.black, 1f);
		
		
		Vector3 position = transform.position;
		
		//Vector3 velocity = motor.movement.velocity;
		// nope, motor is disabled on remote clients
		transform.position = Vector3.down * 100f;
		yield return new WaitForEndOfFrame();
		Transform body = deadBody.Spawn(position, transform.rotation);
		yield return new WaitForEndOfFrame();
		body.rigidbody.isKinematic = false;
		//body.rigidbody.velocity = velocity;
		
		if (photonView.isMine) {
			AudioListener.volume = 0f;
			motor.enabled = false;
		}

		SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
		
		yield return new WaitForSeconds(1.5f);
		
		if (!PhotonNetwork.inRoom) {
			GameManager.Instance.ChangeScene( Scene.Tag.Lobby );
		}
		
		photonView.RPC ("SpawnRPC", PhotonTargets.All);
		
		
	}
}
