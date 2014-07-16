using UnityEngine;
using System.Collections;

// generic data structure for common types of character stats
[System.Serializable]
public class CharacterStats {
	public float maxMoveSpeed;
	[System.NonSerialized]
	public float moveSpeed;
	
	public float maxRotationSpeed;
	[System.NonSerialized]
	public float rotationSpeed;
	
	public float maxHealth;
	[System.NonSerialized]
	public float health;
	
}
