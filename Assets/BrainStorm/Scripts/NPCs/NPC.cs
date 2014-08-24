using UnityEngine;
using System.Collections;

[AddComponentMenu("Character/NPC")]
public class NPC : MonoBehaviour {

	public enum Type {
		None		= 0x00,
		Native 		= 0x01,		// healthy NPCs (friendly to player)
		Infected 	= 0x02,		// (faction NPCs, Spider, etc)
		Virus		= 0x04,		// Virus' are friendly to one another, hostile to most other things
				//	= 0x08
				//	= 0x10
				//	= 0x20
				//	= 0x40
				//	= 0x80
		All			= 0xFF
	}
	
	public Type type;
}
