using UnityEngine;
using System.Collections;

public class Spire : MonoBehaviour {

	public static Spire Instance;

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
	
}
