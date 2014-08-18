using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public static Player Instance;
	
	public int maxHealth;
	
	public float hurtEffectDuration = 0.15f;
	
	private int _health;
	private bool _dead = false;
	private Color _hurtOverlay;
	private float _lastHurtTime;
	private GUIText _guiHealth; 
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		_health = maxHealth;
		_hurtOverlay = Color.Lerp(Color.red, Color.clear, 0.25f);
		_guiHealth = transform.FindChild("Health").guiText;
		_guiHealth.transform.parent = null;
	}
	
	void Update() {
		if (_dead) return;
		_guiHealth.text = _health.ToString() + "%";
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
