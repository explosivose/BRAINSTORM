using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Player")]
[RequireComponent(typeof(CharacterMotorC))]
public class Player : MonoBehaviour {

	public static Player Instance;
	
	public int maxHealth;
	
	public float hurtEffectDuration = 0.15f;
	
	public float health01 {
		get { return (float)_health/(float)maxHealth; }
	}
	
	// probably move this and an hasJetpack bool to PlayerInventory
	public float jetpack01 {
		get { return _motor.jetpack.fuel/_motor.jetpack.maxJetpackFuel; }
	}
	
	public float sprint01 {
		get { return _motor.sprint.stamina/_motor.sprint.sprintLength; }
	}
	
	private int _health;
	private bool _dead = false;
	private Color _hurtOverlay;
	private float _lastHurtTime; 
	private CharacterMotorC _motor;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		_motor = GetComponent<CharacterMotorC>();
		_health = maxHealth;
		_hurtOverlay = Color.Lerp(Color.red, Color.clear, 0.25f);
	}
	
	void Update() {
		if (_dead) return;
		if (_lastHurtTime + hurtEffectDuration <= Time.time)
			ScreenFade.Instance.StartFade(Color.clear, hurtEffectDuration);
	}
	
	void Damage(DamageInstance damage) {
		_health -= damage.damage;
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
