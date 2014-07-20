using UnityEngine;
using System.Collections;

public class FactionManager : MonoBehaviour {

	public static FactionManager Instance;

	public int maxNPCs;
	public int waveSize;
	public float waveTimer;
	
	public Transform[] pinkSpawnAreas;
	public Transform[] pinkNPCPrefabs;
	public Transform[] purpleSpawnAreas;
	public Transform[] purpleNPCPrefabs;
	
	void Awake() {
		// singleton design pattern
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
