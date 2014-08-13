﻿using UnityEngine;
using System.Collections;

public class SpiderArmAnimator : MonoBehaviour {

	public Transform spiderBody;
	public Transform target, 	idleTarget;
	public Transform elbow,		idleElbow;
	public float armMoveSpeed;
	public float attackSpeed;
	
	
	private Vector3 _updateTarget, _updateElbow;
	private bool _attacking;
	private bool _cooldown;
	private Vector3 _attackTarget;
	
	void Awake() {
		idleTarget.position = target.position;
		idleElbow.position = elbow.position;
	}
	
	void Update () {

		float t = attackSpeed;
		if (!_attacking) {
			_updateTarget = idleTarget.position;
			_updateElbow = idleElbow.position;
			t = armMoveSpeed;
		}
		
		target.position = Vector3.Lerp(target.position, _updateTarget, t * Time.deltaTime);
		elbow.position = Vector3.Lerp(elbow.position, _updateElbow, t * Time.deltaTime);
	}
	
	void Attack(Vector3 position) {
		_attackTarget = position;
		if (!_cooldown) StartCoroutine(AttackRoutine());
	}
	
	IEnumerator AttackRoutine() {
		_attacking = true;
		_cooldown = true;
		_updateTarget = _attackTarget + Vector3.up * 10f;
		yield return new WaitForSeconds(0.4f);
		_updateTarget = _attackTarget;
		yield return new WaitForSeconds(1f);
		_attacking = false;
		yield return new WaitForSeconds(2f);
		_cooldown = false;
	}

}
