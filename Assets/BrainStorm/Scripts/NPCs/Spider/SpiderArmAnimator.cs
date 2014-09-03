using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/Spider/Spider")]
public class SpiderArmAnimator : MonoBehaviour {

	public Transform spiderBody;
	public Transform target, 	idleTarget;
	public Transform elbow,		idleElbow;
	public float armMoveSpeed;
	public float attackSpeed;
	public float primingTime 	= 0.5f;
	public float strikeTime 	= 1f;
	public float cooldownTime 	= 1.5f;
	public Material eyesMaterial;
	public Color eyesVulnerable;
	
	private enum State {
		idle, priming, striking, cooldown
	}
	
	private State _state;
	private State state {
		get {return _state; }
		set {
			_lastStateChangeTime = Time.time;
			_state = value;
		}
	}
	private float _lastStateChangeTime;
	private Spider _spider;
	private Vector3 _updateTarget, _updateElbow;
	private Vector3 _attackTarget;
	private Color _eyesInvuln;
	
	void Awake() {
		idleTarget.position = target.position;
		idleElbow.position = elbow.position;
		_spider = spiderBody.GetComponent<Spider>();
		_eyesInvuln = eyesMaterial.GetColor("_Color");
	}
	
	void OnEnable() {
		state = State.idle;
		_spider.attacking = false;
		_spider.invulnerable = true;
	}
	
	void OnDestroy() {
		eyesMaterial.SetColor("_Color", _eyesInvuln);
	}
	
	void Update () {
		
		// set eyes color
		float t = Time.time - _lastStateChangeTime;
		Color c;
		switch(state) {
		case State.priming:
			t /= primingTime;
			c = Color.Lerp (_eyesInvuln, eyesVulnerable, t);
			break;
		default:
			c = _eyesInvuln;
			break;
		}
		
		eyesMaterial.SetColor("_Color", c);
		
		t = attackSpeed;
		if (state == State.idle) {
			_updateTarget = idleTarget.position;
			_updateElbow = idleElbow.position;
			t = armMoveSpeed;
		}
		
		target.position = Vector3.Lerp(target.position, _updateTarget, t * Time.deltaTime);
		elbow.position = Vector3.Lerp(elbow.position, _updateElbow, t * Time.deltaTime);
	}
	
	void Attack(Vector3 position) {
		_attackTarget = position;
		if (state == State.idle) StartCoroutine(AttackRoutine());
	}
	
	void CancelAttack() {
		state = State.idle;
	}
	
	IEnumerator AttackRoutine() {
		state = State.priming;
		_spider.attacking = true;
		_spider.invulnerable = false;
		_updateTarget = _attackTarget + Vector3.up * 10f;
		yield return new WaitForSeconds(primingTime);
		state = State.striking;
		_spider.invulnerable = true;
		_updateTarget = _attackTarget;
		yield return new WaitForSeconds(strikeTime);
		state = State.cooldown;
		_spider.invulnerable = true;
		yield return new WaitForSeconds(cooldownTime);
		_spider.attacking = false;
		state = State.idle;
	}

}
