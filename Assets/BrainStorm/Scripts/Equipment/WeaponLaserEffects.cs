using UnityEngine;
using System.Collections;

public class WeaponLaserEffects : MonoBehaviour {

	public Transform[] startPoints;
	public Transform laserEffect;
	
	private Transform _nozzle;
	
	void Awake() {
		_nozzle = transform.FindChild("Nozzle");
		ObjectPool.CreatePool(laserEffect);
	}
	
	void FireEffect() {
		foreach(Transform t in startPoints) {
			// these lasers need to be using local space
			Transform i = laserEffect.Spawn(t.position);
			i.parent = this.transform;
			i.SendMessage("HitPosition", _nozzle.position);
		}
	}
}
