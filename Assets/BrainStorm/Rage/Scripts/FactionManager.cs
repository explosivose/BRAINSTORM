using UnityEngine;
using System.Collections;

public class FactionManager : MonoBehaviour {

	public int NPCCount {
		get { return _pinkCount + _purpleCount; }
	}
	
	public static FactionManager Instance;
	
	public int maxNPCs;
	public int waveSize;
	public float waveTime;
	
	public Transform[] NPCPrefabs;
	public Transform[] pinkSpawnAreas;
	public Transform[] purpleSpawnAreas;
	
	private float _waveTimer;
	private bool _spawning;
	private int _pinkCount;
	private int _purpleCount;
	
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
		for (int i = 0; i < NPCPrefabs.Length; i++) {
			ObjectPool.CreatePool(NPCPrefabs[i]);
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
		int purpleSpawnIndex = Random.Range(0, purpleSpawnAreas.Length);
		
		int NPCIndex;
		Transform NPC;
		NPCFaction f;
		for (int i = 0; i < waveSize; i++) {
			NPCIndex = Random.Range(0, NPCPrefabs.Length);
			NPC = NPCPrefabs[NPCIndex].Spawn(pinkSpawnAreas[pinkSpawnIndex].position);
			NPC.name = "Pink" + NPCPrefabs[NPCIndex].name;
			f = NPC.GetComponent<NPCFaction>();
			f.team = NPCFaction.Faction.Pink;
			f.advancePosition = purpleSpawnAreas[purpleSpawnIndex].position;
			_pinkCount++;
			
			yield return new WaitForSeconds(0.1f);
			
			NPC = NPCPrefabs[NPCIndex].Spawn(purpleSpawnAreas[purpleSpawnIndex].position);
			NPC.name = "Purple" + NPCPrefabs[NPCIndex].name;
			f = NPC.GetComponent<NPCFaction>();
			f.team = NPCFaction.Faction.Purple;
			f.advancePosition = pinkSpawnAreas[pinkSpawnIndex].position;
			_purpleCount++;
			
			yield return new WaitForSeconds(1f);
		}
		_spawning = false;
	}
}
