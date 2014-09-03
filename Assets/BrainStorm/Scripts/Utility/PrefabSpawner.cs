using UnityEngine;
using System.Collections;

public class PrefabSpawner : MonoBehaviour {

	public enum SpawnPosition {
		Inherited,
		RandomInColliderBounds
	}

	public Transform prefab;
	public bool useObjectPool = true;
	public bool spawnOnStart = true;
	public float amountToSpawn = 10f;
	public float variationOnAmount = 1f;
	public bool randomRotation = true;
	public Vector3 rotation;
	public SpawnPosition style = SpawnPosition.Inherited;
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

	// Use this for initialization
	void OnEnable() {
		if (spawnOnStart) StartCoroutine(Spawn ());
	}
	
	// Update is called once per frame
	IEnumerator Spawn() {
		yield return new WaitForEndOfFrame();
		for (int i = _spawned; i < _amountToSpawn; i++) {
			Transform t = prefab.Spawn(transform.position);
			
			switch(style) {
				case SpawnPosition.Inherited:
					t.position = transform.position;
					break;
				case SpawnPosition.RandomInColliderBounds:
					Vector3 position = new Vector3 (
						Random.value * collider.bounds.size.x,
						Random.value * collider.bounds.size.y,
						Random.value * collider.bounds.size.z
						) - collider.bounds.extents;
					t.position = transform.position + position;
					break;
			}
			
			if (randomRotation) t.rotation = Random.rotation;
			else t.rotation = Quaternion.Euler(rotation);
			
			t.parent = GameManager.Instance.activeScene;
			_spawned++;
			float vary = (Random.value-0.5f) * timeBetweenInstantiations * variationOnTime;
			float wait = timeBetweenInstantiations + vary;
			yield return new WaitForSeconds(wait);
		}
	}
}
