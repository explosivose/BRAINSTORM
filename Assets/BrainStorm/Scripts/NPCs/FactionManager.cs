using UnityEngine;
using System.Collections;

public class FactionManager : MonoBehaviour {

	public int NPCCount {
		get { return _pinkCount + _purpleCount; }
	}
	
	public static FactionManager Instance;
	
	public NPCFaction.State initialState;
	
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
	
	public void NPCDeath(NPC.Type faction) {
		if (faction == NPC.Type.Team1) 
			_pinkCount--;
		else if (faction == NPC.Type.Team2)
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
		Transform instance;
		NPCFaction f;
		for (int i = 0; i < waveSize; i++) {
			NPCIndex = Random.Range(0, NPCPrefabs.Length);
			instance = NPCPrefabs[NPCIndex].Spawn(pinkSpawnAreas[pinkSpawnIndex].position);
			instance.parent = GameManager.Instance.activeScene;
			instance.name = "Pink" + NPCPrefabs[NPCIndex].name;
			f = instance.GetComponent<NPCFaction>();
			f.type = NPC.Type.Team1;
			f.FactionInit();
			f.advancePosition = purpleSpawnAreas[purpleSpawnIndex].position;
			f.state = initialState;
			_pinkCount++;
			
			yield return new WaitForSeconds(0.1f);
			
			instance = NPCPrefabs[NPCIndex].Spawn(purpleSpawnAreas[purpleSpawnIndex].position);
			instance.parent = GameManager.Instance.activeScene;
			instance.name = "Purple" + NPCPrefabs[NPCIndex].name;
			f = instance.GetComponent<NPCFaction>();
			f.type = NPC.Type.Team2;
			f.FactionInit();
			f.advancePosition = pinkSpawnAreas[pinkSpawnIndex].position;
			f.state = initialState;
			_purpleCount++;
			
			yield return new WaitForSeconds(0.4f);
		}
		_spawning = false;
	}
}
