using UnityEngine;
using System.Collections;

public class NPCFaction : MonoBehaviour {

	public Vector3 advancePosition {
		get { return _advancePosition;  }
		set { _advancePosition = value; }
	}

	public enum Faction {
		Pink, Purple
	}
	public Faction team = Faction.Pink;

	private Vector3 _advancePosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
