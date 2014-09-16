using UnityEngine;
using System.Collections;

public class PrefabSpawner : MonoBehaviour {

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
	public bool useObjectPool = true;
	public bool spawnOnStart = true;
	public float amountToSpawn = 10f;
	public float variationOnAmount = 1f;
	public SpawnPosition position = SpawnPosition.Inherited;
	public SpawnRotation rotation = SpawnRotation.Inherited;
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
		Gizmos.color = Color.Lerp(Color.clear, Color.green, 0.75f);
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
		StartCoroutine( SpawnRoutine () );
	}

	IEnumerator SpawnRoutine() {
		yield return new WaitForEndOfFrame();
		for (int i = _spawned; i < _amountToSpawn; i++) {
			Transform t = prefab.Spawn(transform.position);
			
			switch(position) {
				default:
				case SpawnPosition.Inherited:
					t.position = transform.position;
					break;
				case SpawnPosition.RandomInColliderBounds:
					Vector3 pos = new Vector3 (
						Random.value * collider.bounds.size.x,
						Random.value * collider.bounds.size.y,
						Random.value * collider.bounds.size.z
						) - collider.bounds.extents;
					t.position = transform.position + pos;
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
			
			t.parent = GameManager.Instance.activeScene;
			_spawned++;
			float vary = (Random.value-0.5f) * timeBetweenInstantiations * variationOnTime;
			float wait = timeBetweenInstantiations + vary;
			yield return new WaitForSeconds(wait);
		}
	}
}
