using UnityEngine;
using System.Collections;

public class ArmoryManager : MonoBehaviour {

	public Transform[] weaponPrefabs;
	
	public Transform[] equipmentPrefabs;
	
	private int _weaponsSpawned = 0;
	private int _equipmentSpawned = 0;
	
	void Awake() {
		foreach (Transform t in weaponPrefabs)
			ObjectPool.CreatePool(t);
		foreach (Transform t in equipmentPrefabs)
			ObjectPool.CreatePool(t);
	}
	
	
	void OnEnable () {
		StartCoroutine( SpawnWeapons() );
		StartCoroutine( SpawnEquipment() );
	}
	
	
	IEnumerator SpawnWeapons() {
		yield return new WaitForSeconds(1f);
		for (int i = _weaponsSpawned; i < weaponPrefabs.Length; i++) {
			Transform w = weaponPrefabs[i].Spawn(transform.position - transform.right * i * 2);
			w.parent = GameManager.Instance.activeScene;
			_weaponsSpawned++;
			yield return new WaitForSeconds(0.3f);
		}
	}
	
	IEnumerator SpawnEquipment() {
		yield return new WaitForSeconds(1f);
		for (int i = _equipmentSpawned; i < equipmentPrefabs.Length; i++) {
			Transform e = equipmentPrefabs[i].Spawn(transform.position + transform.right * i * 2);
			e.parent = GameManager.Instance.activeScene;
			_equipmentSpawned++;
			yield return new WaitForSeconds(0.3f);
		}
	}
}
