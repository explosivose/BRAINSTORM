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

	public enum SpawnPosition {
		Inherited,
		RandomInColliderBounds,
		Unchanged
	}
	public enum SpawnRotation {
		Inherited,
		Randomly,
		Unchanged
	}

	public Transform prefab;
	public string resourcesSubpath;
	public bool useObjectPool = true;
	public bool networkSpawn = false;
	public bool spawnOnStart = true;
	public float amountToSpawn = 10f;
	public float variationOnAmount = 1f;
	public SpawnPosition position = SpawnPosition.Inherited;
	public SpawnRotation rotation = SpawnRotation.Inherited;
	public float timeBeforeFirstSpawn;
	public float timeBetweenInstantiations;
	public float variationOnTime = 1f;
	
	private float _amountToSpawn;
	private int _spawned = 0;
	

	
	void Start() {
		if (useObjectPool)
			ObjectPool.CreatePool(prefab);
	
		if (amountToSpawn < Mathf.Infinity && amountToSpawn > 0f) {
			float vary = (Random.value-0.5f) * amountToSpawn * variationOnAmount;
			_amountToSpawn = Mathf.RoundToInt(amountToSpawn + vary);
		}
		else {
			_amountToSpawn = amountToSpawn;
		}

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
		// only spawn if we're local-only or master in a networked game
		if ((networkSpawn && PhotonNetwork.isMasterClient && PhotonNetwork.inRoom) || !networkSpawn) {
			StartCoroutine( SpawnRoutine () );
		}
	}

	IEnumerator SpawnRoutine() {
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(timeBeforeFirstSpawn);
		for (int i = _spawned; i < _amountToSpawn; i++) {
			Transform t;
			if (networkSpawn && PhotonNetwork.isMasterClient) {
				t = PhotonNetwork.InstantiateSceneObject(
					resourcesSubpath + prefab.name,
					transform.position,
					prefab.rotation,
					0,
					null
				).transform;
			}
			else {
				t = prefab.Spawn(transform.position);
			}
			
			
			switch(position) {
				default:
				case SpawnPosition.Inherited:
					t.position = transform.position;
					break;
				case SpawnPosition.RandomInColliderBounds:
					Vector3 pos = randomPositionIn(collider.bounds);
					t.position = pos;
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
				case SpawnRotation.Unchanged:
					break;
			}
			
			t.parent = GameManager.Instance.activeScene.instance;
			_spawned++;
			float vary = (Random.value-0.5f) * timeBetweenInstantiations * variationOnTime;
			float wait = timeBetweenInstantiations + vary;
			yield return new WaitForSeconds(wait);
		}
	}
}
