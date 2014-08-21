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
		public AudioClip sprintStart;
		public AudioClip sprintStop;
		public AudioClip jetpackStart;
		public AudioClip jetpackLoop;
		public AudioClip jetpackStop;
	}
			
	public int maxHealth;
	
	public float hurtEffectDuration = 0.15f;
	
	public AudioLibrary sounds = new AudioLibrary();
	
	public float health01 {
		get { return (float)_health/(float)maxHealth; }
	}
	
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
		_health = maxHealth;
		_hurtOverlay = Color.Lerp(Color.red, Color.clear, 0.25f);
	}
	
	void Update() {
		if (_dead) return;
		if (_lastHurtTime + hurtEffectDuration <= Time.time)
			ScreenFade.Instance.StartFade(Color.clear, hurtEffectDuration);
	}
	
	void OnJump() {
		PlaySound(sounds.jump);

	}
	
	void OnLand() {
		PlaySound(sounds.land);
	}
	
	void OnSprintStart() {
		PlaySound(sounds.sprintStart);
	}
	
	void OnSprintStop() {
		PlaySound(sounds.sprintStop);
	}
	
	void OnJetpackStart() {
		PlaySound(sounds.jetpackStart);
	}
	
	void OnJetpackStop() {
		PlaySound(sounds.jetpackStop);
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
		yield return new WaitForSeconds(1.5f);
		GameManager.Instance.ChangeScene( Scene.Tag.Lobby );
		_health = maxHealth;
		_dead = false;
	}
}
