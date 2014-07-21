using UnityEngine;
using System.Collections;

public class FactionManager : MonoBehaviour {

	public static int NPCCount {
		get { return _pinkCount + _purpleCount; }
	}
	
	public static FactionManager Instance;
	
	public int maxNPCs;
	public int waveSize;
	public float waveTime;
	
	public Transform[] pinkSpawnAreas;
	public Transform[] pinkNPCPrefabs;
	public Transform[] purpleSpawnAreas;
	public Transform[] purpleNPCPrefabs;
	
	private float _waveTimer;
	private bool _spawning;
	private static int _pinkCount;
	private static int _purpleCount;
	
	public void NPCDeath(NPCFaction.Faction faction) {
		if (faction == NPCFaction.Faction.Pink) 
			_pinkCount--;
		else 
			_purpleCount--;
	}	
	
	void Awake() {
		// singleton design pattern
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}
	
	void OnLevelWasLoaded() {
		_pinkCount = 0;
		_purpleCount = 0;
	}
	
	// Update is called once per frame
	void Update () {
		_waveTimer -= Time.deltaTime;
		if (_waveTimer < 0f && !_spawning && NPCCount < maxNPCs) {
			_waveTimer = waveTime;
			StartCoroutine( SpawnWave() );
		}
	}
	
	IEnumerator SpawnWave() {
		_spawning = true;
		int pinkSpawnIndex = Random.Range(0, pinkSpawnAreas.Length);
		int purpleSpawnIndex = Random.Range(0, purpleNPCPrefabs.Length);
		
		for (int i = 0; i < waveSize; i++) {
			int pinkNPCIndex = Random.Range(0, pinkNPCPrefabs.Length);
			Transform pinkNPC = pinkNPCPrefabs[pinkNPCIndex].Spawn(pinkSpawnAreas[pinkSpawnIndex].position);
			pinkNPC.GetComponent<NPCFaction>().advancePosition = purpleSpawnAreas[purpleSpawnIndex].position;
			_pinkCount++;
			yield return new WaitForSeconds(0.1f);
			int purpleNPCIndex = Random.Range(0, purpleNPCPrefabs.Length);
			Transform purpleNPC = purpleNPCPrefabs[purpleNPCIndex].Spawn(purpleSpawnAreas[purpleSpawnIndex].position);
			purpleNPC.GetComponent<NPCFaction>().advancePosition = pinkSpawnAreas[pinkSpawnIndex].position;
			_purpleCount++;
			
			yield return new WaitForSeconds(1f);
		}
		_spawning = false;
	}
}
