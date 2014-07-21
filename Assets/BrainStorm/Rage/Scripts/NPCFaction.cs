using UnityEngine;
using System.Collections;

public class NPCFaction : MonoBehaviour {

	public Vector3 advancePosition {
		get { return _advancePosition;  }
		set { 
			_advancePosition = value;
			SendMessage("Advance");
		}
	}

	public enum Faction {
		Pink, Purple
	}
	public Faction team {
		get { return _team; }
		set { 
			_team = value;
			SendMessage("ChangeFaction");
		}
	}
	private Faction _team;

	public CharacterMaterials pinkWardrobe = new CharacterMaterials();
	public CharacterMaterials purpleWardrobe = new CharacterMaterials();

	private Vector3 _advancePosition;
	
}
