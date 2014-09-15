using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spire : MonoBehaviour {

	public static Spire Instance;
	public Transform artefactPrefab;
	public int virusCount = 10;
	public Transform[] virusPrefabs;
	public float artefactGrowthAcc = 1f;
	
	public Transform top { get; private set; }
	public Transform center { get; private set; }
	
	private bool artefactActivated = false;
	private Transform artefact;
	private float artefactPeakSize;
	private float artefactSize = 0f;
	private float artefactGrowthSpeed = -0.5f;
	
	void Awake () {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		top = transform.Find("Top");
		center = transform.Find("Center");
	}
	
	void Start() {
		StartCoroutine( SpawnVirus() );
		artefact = artefactPrefab.Spawn(center.position, Random.rotation);
		artefact.parent = GameManager.Instance.activeScene;
		artefact.rigidbody.constraints = RigidbodyConstraints.FreezePosition;
		artefact.rigidbody.angularVelocity = Random.onUnitSphere;
		artefact.localScale = Vector3.one * 0.1f;
	}
	
	IEnumerator SpawnVirus() {
		for (int i = 0; i < virusCount; i++) {
			int prefabIndex = Random.Range(0, virusPrefabs.Length);
			Transform v = virusPrefabs[prefabIndex].Spawn(center.position);
			v.parent = GameManager.Instance.activeScene;
			v.SendMessage("Defend", center.transform);
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	void Update() {
		artefactGrowthSpeed += Time.deltaTime * artefactGrowthAcc;
		artefactGrowthSpeed = Mathf.Min(artefactGrowthSpeed, 0.5f);
		artefactSize += Time.deltaTime * artefactGrowthSpeed;
		artefactSize = Mathf.Max(artefactSize, 0.1f);
		artefactSize = Mathf.Min(artefactSize, 1.5f);
		artefact.localScale = Vector3.one * artefactSize;
		artefactPeakSize = Mathf.Max(artefactSize, artefactPeakSize);
		if (artefactSize >= 1f && !artefactActivated) {
			ActivateArtefact();
			GameManager.Instance.rageComplete = true;
		}
	}
	
	void ActivateArtefact() {
		artefactActivated = true;
		artefact.SendMessage("Reveal");
		artefact.rigidbody.constraints = RigidbodyConstraints.None;
		artefact.rigidbody.AddForce(Vector3.forward * 5000f);
	}
	
	public void VirusTouch() {
		artefactGrowthSpeed -= 0.05f;
		artefactGrowthSpeed = Mathf.Max(artefactGrowthSpeed, -0.5f);
	}
}
