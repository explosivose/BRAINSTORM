using UnityEngine;
using System.Collections;

[System.Serializable]
public class DamageInstance {
	public int damage;
	public int viewId;
	
	public DamageInstance() {
		
	}
	
	public DamageInstance(int dmg, int id) {
		damage = dmg;
		viewId = id;
	}
}