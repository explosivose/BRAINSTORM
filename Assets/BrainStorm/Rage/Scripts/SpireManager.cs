using UnityEngine;
using System.Collections;

public class SpireManager : MonoBehaviour {

	public int virusCount;
	public Transform[] virusPrefabs;
	public Transform[] spawnPoints;
	
	public Material calm;
	
	void Start() {
		foreach (Transform t in virusPrefabs) {
			ObjectPool.CreatePool(t);
		}
		StartCoroutine(SpawnVirus());
	}
	
	IEnumerator SpawnVirus() {
		for (int i = 0; i < virusCount; i++) {
			int prefabIndex = Random.Range(0, virusPrefabs.Length);
			int spawnIndex = Random.Range(0, spawnPoints.Length);
			Transform v = virusPrefabs[prefabIndex].Spawn(spawnPoints[spawnIndex].position);
			v.SendMessage("Defend", this.transform);
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	void Damage(DamageInstance damage) {
		if (damage.source.tag == "Player") {
			FactionManager.Instance.initialState = NPCFaction.State.Calm;
			Terrain.activeTerrain.materialTemplate = calm;
		}
	}
}
