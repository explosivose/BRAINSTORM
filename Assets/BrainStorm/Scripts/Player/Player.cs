using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Player")]
[RequireComponent(typeof(CharacterMotorC))]
public class Player : MonoBehaviour {

	public static Player Instance;

	[System.Serializable]
	public class AudioLibrary {
		public float volume;
		public AudioClip hurt;
		public AudioClip jump;
		public AudioClip land;
	}
			
	public int maxHealth;
	
	public float hurtEffectDuration = 0.15f;
	
	public AudioLibrary sounds = new AudioLibrary();
	
	public float health01 {
		get { return (float)_health/(float)maxHealth; }
	}
	
	private CharacterMotorC _motor;
	
	private int _health;
	private bool _dead = false;
	private Color _hurtOverlay;
	private float _lastHurtTime; 
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		_motor = GetComponent<CharacterMotorC>();
		_hurtOverlay = Color.Lerp(Color.red, Color.clear, 0.25f);
		Spawn();
	}
	
	void Spawn() {
		_health = maxHealth;
		_dead = false;
		SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
	}
	
	void Update() {
		if (_dead) return;
		if (_lastHurtTime + hurtEffectDuration <= Time.time)
			ScreenFade.Instance.StartFade(Color.clear, hurtEffectDuration);
			
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
		
		// Apply the direction to the CharacterMotor
		_motor.inputMoveDirection = transform.rotation * directionVector;
		_motor.inputJump = Input.GetButton("Jump");
		_motor.inputSprint = Input.GetButton("Sprint");
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
	
	void Damage(DamageInstance damage) {
		_health -= damage.damage;
		AudioSource.PlayClipAtPoint(sounds.hurt, transform.position);
		ScreenShake.Instance.Shake(0.5f, (float)maxHealth/(float)damage.damage);
		_lastHurtTime = Time.time;
		ScreenFade.Instance.StartFade(_hurtOverlay, hurtEffectDuration);
		if (_health < 0) StartCoroutine ( Death() );
	}
	
	IEnumerator Death() {
		_dead = true;
		ScreenFade.Instance.StartFade(Color.black, 1f);
		float vol = AudioListener.volume;
		AudioListener.volume = 0f;
		SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(1.5f);
		AudioListener.volume = vol;
		GameManager.Instance.ChangeScene( Scene.Tag.Lobby );
		Spawn();
	}
}
