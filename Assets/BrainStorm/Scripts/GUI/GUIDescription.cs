using UnityEngine;
using System.Collections;

[AddComponentMenu("GUI/Inspect Description")]
public class GUIDescription : MonoBehaviour {
	
	// Probably smarter to have several types of descriptions
	// i.e. weapon: transform.name + weaponString
	// where weapon string is defined in one place and might sensibly be
	//		weaponString = "Press E to pickup"
	public string description;
	public int height = 20;
	public int width = 200;
	public float minimumShowTime = 0.1f;
	public bool inspect = false;
	
	private bool inspectReq;
	
	void OnInspect() {
		if (!inspect) StartCoroutine( Uninspect() );
		inspect = true;
	}
	
	IEnumerator Uninspect() {
		yield return new WaitForSeconds(minimumShowTime);
		inspect = false;
	}
	
	void OnEnable() {
		inspect = false;
	}
	
	// having a lot of OnGUI is probably horrible
	// maybe think about a GUIDescription implementation that 
	// uses GUIText or some other gameObject in world space
	void OnGUI() {
		if (!inspect || GameManager.Instance.paused) return;
	 	Vector3 worldPos = transform.position;
		Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
		
		int left = Mathf.RoundToInt(screenPos.x - (float)width/2f);
		int top = Mathf.RoundToInt(Screen.height - screenPos.y - (float)height/2f);
		
		Rect position = new Rect(left, top, width, height);
		GUI.Button (position, description);
	}
}
