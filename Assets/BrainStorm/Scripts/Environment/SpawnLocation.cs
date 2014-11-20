using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnLocation : MonoBehaviour {

	public static List<SpawnLocation> list = new List<SpawnLocation>();

	public static Vector3 randomLocation {
		get {
			int index = Random.Range(0, list.Count);
			return list[index].transform.position;
		}
	}

	private int index;

	// Use this for initialization
	void Start () {
		list.Add(this);
		index = list.Count;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDestroy() {
		list.Remove(this);
	}
}
