using UnityEngine;
using System.Collections;

public class AloneTransition : MonoBehaviour {

	public static AloneTransition Instance;
	
	public Transform sun;
	public Material sadRoads;
	public Material sadBuildings;
	public Material joyRoads;
	public Material joyBuildings;
	
	private Component[] _renRoads;
	private Component[] _renBuildings;

	// Use this for initialization
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}
	
	void Start() {
		
		Transform r = transform.FindChild("Roads");
		Transform b = transform.FindChild("Buildings");
		
		_renRoads = r.GetComponentsInChildren(typeof(MeshRenderer));
		_renBuildings = b.GetComponentsInChildren(typeof(MeshRenderer)) ;
		
		
	}
	
	void OnEnable() {
		StartCoroutine(Transitioner());
	}
	
	IEnumerator Transitioner() {
		float t = NPCCarrot.frenzyFactor;
		while (true) {
			if (t != NPCCarrot.frenzyFactor) {
				t = NPCCarrot.frenzyFactor;
				yield return StartCoroutine( UpdateMaterialColors(t) );
				sun.light.intensity = Mathf.Lerp(0.1f, 0.4f, t);
			}
			yield return new WaitForSeconds(0.05f);
		}

	}
	
	IEnumerator UpdateMaterialColors(float t) {
		Color sad, joy;
		foreach(MeshRenderer r in _renRoads) {
			sad = sadRoads.GetColor("_Color");
			joy = joyRoads.GetColor("_Color");
			r.material.SetColor("_Color", Color.Lerp(sad, joy, t));
			yield return new WaitForEndOfFrame();
			sad = sadRoads.GetColor("_OutlineColor");
			joy = joyRoads.GetColor("_OutlineColor");
			r.material.SetColor("_OutlineColor", Color.Lerp(sad, joy, t));
			yield return new WaitForEndOfFrame();
		}
		foreach(MeshRenderer r in _renBuildings) {
			sad = sadBuildings.GetColor("_Color");
			joy = joyBuildings.GetColor("_Color");
			r.material.SetColor("_Color", Color.Lerp(sad, joy, t));
			yield return new WaitForEndOfFrame();
			sad = sadBuildings.GetColor("_OutlineColor");
			joy = joyBuildings.GetColor("_OutlineColor");
			r.material.SetColor("_OutlineColor", Color.Lerp(sad, joy, t));
			yield return new WaitForEndOfFrame();
		}
	}
}
