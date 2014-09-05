using UnityEngine;
using System.Collections;

public class Spire : MonoBehaviour {

	public static Spire Instance;

	public int virusCount = 10;
	public Transform[] virusPrefabs;

	public Transform top { get; private set; }
	
	
	void Awake () {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		top = transform.Find("Top");
	}
	
	void Start() {
		StartCoroutine( SpawnVirus() );
	}
	
	IEnumerator SpawnVirus() {
		for (int i = 0; i < virusCount; i++) {
			int prefabIndex = Random.Range(0, virusPrefabs.Length);
			Transform v = virusPrefabs[prefabIndex].Spawn(top.position);
			v.parent = GameManager.Instance.activeScene;
			v.SendMessage("Defend", this.transform);
			yield return new WaitForSeconds(0.5f);
		}
	}
}
