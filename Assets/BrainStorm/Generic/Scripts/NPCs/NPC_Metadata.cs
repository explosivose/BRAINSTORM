using UnityEngine;
using System.Collections;

// generic data structure for common types of character stats
[System.Serializable]
public class CharacterStats {
	
	public int health;

	public bool immune = false;
	
	public float attackRange;

}

[System.Serializable]
public class CharacterMaterials {
	public Material normal;
	public Material attacking;
	public Material hurt;
	public Material dead;
	public Material lingerie;
}

[System.Serializable]
public class CharacterAudio {
	public AudioClip[] attack;
	public AudioClip[] hurt;
	public AudioClip[] death;
}