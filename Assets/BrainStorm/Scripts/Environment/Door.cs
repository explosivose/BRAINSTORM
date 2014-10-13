using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ChangeSceneTrigger))]
public class Door : MonoBehaviour {


	private ChangeSceneTrigger _sceneTrigger;

	// Use this for initialization
	void Awake() {
		_sceneTrigger = GetComponent<ChangeSceneTrigger>();
	}
	
	void OnEnable() {
		switch (_sceneTrigger.changeTo) {
		case Scene.Tag.Rage:
			//if (GameManager.Instance.rageComplete)
				_sceneTrigger.changeTo = Scene.Tag.Calm;
			break;
		case Scene.Tag.Grief:
			//if (GameManager.Instance.griefComplete)
				_sceneTrigger.changeTo = Scene.Tag.Joy;
			break;
		case Scene.Tag.Terror:
			//if (GameManager.Instance.terrorComplete)
				_sceneTrigger.changeTo = Scene.Tag.Safety;
			break;
		}
	}
}
