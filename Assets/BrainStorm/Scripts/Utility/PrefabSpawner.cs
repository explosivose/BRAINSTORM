using UnityEngine;
using System.Collections;

public class PrefabSpawner : MonoBehaviour {

	public static Vector3 randomPositionIn(Bounds bounds) {
		return new Vector3 (
			Random.value * bounds.size.x,
			Random.value * bounds.size.y,
			Random.value * bounds.size.z
			) - bounds.extents + bounds.center;
	}

	public static Vector3 randomPositionOnGround(Bounds bounds, out Vector3 normal) {
		Vector3 pos = randomPositionIn(bounds);
		normal = Vector3.up;
		RaycastHit hit;
		if (Physics.Raycast(pos, Vector3.down, out hit, 1000f)) {
			pos.y = hit.point.y + 2f;
			normal = hit.normal;
		}
		return pos;
	}

	public enum SpawnPosition {
		Inherited,
		RandomInColliderBounds,
		Unchanged,
		RandomSpawnLocation,
		RandomOnGround
	}
	public enum SpawnRotation {
		Inherited,
		Randomly,
		Unchanged,
		GroundNormal
	}

	public Transform[] prefabs;
	public string resourcesSubpath;
	public bool useObjectPool = true;
	public bool networkSpawn = false;
	public bool spawnOnStart = true;
	
	public float amountToSpawn = 10f;
	public float variationOnAmount = 1f;
	public SpawnPosition position = SpawnPosition.Inherited;
	public SpawnRotation rotation = SpawnRotation.Inherited;
	public bool noWaits = false;
	public bool useMasterSeed = false;
	public float timeBeforeFirstSpawn;
	public float timeBetweenInstantiations;
	public float variationOnTime = 1f;
	
	private float _amountToSpawn;
	private int _spawned = 0;
	

	
	void Start() {
		if (useObjectPool)
			foreach(Transform p in prefabs)
				p.CreatePool();
		
	

		OnEnable();
	}

	void OnDrawGizmos() {
		if (!GetComponent<BoxCollider>()) return;
		Gizmos.color = Color.Lerp(Color.clear, Color.green, 0.6f);
		Gizmos.DrawCube(
			GetComponent<BoxCollider>().center + transform.position, 
			GetComponent<BoxCollider>().size
			);
	}

	// Use this for initialization
	void OnEnable() {
		if (spawnOnStart) Spawn();
	}
	

	public void Spawn() {
		if (amountToSpawn < Mathf.Infinity && amountToSpawn > 0f) {
			float vary = (Random.value-0.5f) * amountToSpawn * variationOnAmount;
			_amountToSpawn = Mathf.RoundToInt(amountToSpawn + vary);
		}
		else {
			_amountToSpawn = amountToSpawn;
		}
		
		// only spawn if we're local-only or master in a networked game
		if ((networkSpawn && PhotonNetwork.isMasterClient && PhotonNetwork.inRoom) || !networkSpawn) {
			if (noWaits) {
				Random.seed = GameManager.Instance.activeScene.seed;
				SpawnImmediately();
			} 
			else {
				StartCoroutine( SpawnRoutine () );
			}
			
		}
	}

	void SpawnImmediately() {
		for (int i = _spawned; i < _amountToSpawn; i++) {
			SpawnOne();
		}
	}
	
	IEnumerator SpawnRoutine() {
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(timeBeforeFirstSpawn);
		for (int i = _spawned; i < _amountToSpawn; i++) {
			SpawnOne();
			float vary = (Random.value-0.5f) * timeBetweenInstantiations * variationOnTime;
			float wait = timeBetweenInstantiations + vary;
			yield return new WaitForSeconds(wait);
		}
	}
	
	void SpawnOne() {
		
		Transform t;
		int index = Random.Range(0, prefabs.Length);
		if (networkSpawn && PhotonNetwork.isMasterClient) {
			t = PhotonNetwork.InstantiateSceneObject(
				resourcesSubpath + prefabs[index].name,
				transform.position,
				prefabs[index].rotation,
				0,
				null
				).transform;
		}
		else {
			t = prefabs[index].Spawn(transform.position);
		}
		
		Vector3 normal = Vector3.up;
		
		switch(position) {
		default:
		case SpawnPosition.Inherited:
			t.position = transform.position;
			break;
		case SpawnPosition.RandomInColliderBounds:
			Vector3 pos = randomPositionIn(collider.bounds);
			t.position = pos;
			break;
		case SpawnPosition.RandomSpawnLocation:
			t.position = SpawnLocation.randomLocation;
			break;
		case SpawnPosition.RandomOnGround:
			t.position = randomPositionOnGround(collider.bounds, out normal);
			break;
		case SpawnPosition.Unchanged:
			break;
			
		}
		
		switch(rotation) {
		default:
		case SpawnRotation.Inherited:
			t.rotation = transform.rotation;
			break;
		case SpawnRotation.Randomly:
			t.rotation = Random.rotation;
			break;
		case SpawnRotation.GroundNormal:
			t.rotation = Quaternion.LookRotation(t.forward, normal);
			break;
		case SpawnRotation.Unchanged:
			break;
		}
		//t.parent = GameManager.Instance.transform;
		_spawned++;
		
	}
}
