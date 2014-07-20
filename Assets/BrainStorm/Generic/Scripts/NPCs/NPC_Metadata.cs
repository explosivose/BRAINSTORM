using UnityEngine;
using System.Collections;

// generic data structure for common types of character stats
[System.Serializable]
public class CharacterStats {

	
	public int maxHealth;
	[System.NonSerialized]
	public int health;
	
	public bool immune = false;
	
	public float attackRange;
}

[System.Serializable]
public class CharacterMaterials {
	[System.NonSerialized]
	public Material normal;
	public Material attacking;
	public Material hurt;
	public Material dead;
}

[System.Serializable]
public class CharacterAudio {
	public AudioClip[] attack;
	public AudioClip[] hurt;
	public AudioClip[] death;
}