using UnityEngine;
using System.Collections;

public class GameMenu : MonoBehaviour {
	
	public static GameMenu Instance;

	public bool showVersion = true;
	
	public GUIStyle labelVersionStyle;
	public GUIStyle buttonStyle;
	
	private bool _showMenu;
	public bool showMenu {
		get { return _showMenu; }
		set { _showMenu = value; }
	}
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}
	
	void OnGUI() {
		int left, top, width, height;
		if (showVersion) {
			GUIController.Alignment versionAlign = GUIController.Alignment.UpperCenter;
			height = labelVersionStyle.fontSize + 4;
			width = 200;
			left = GUIController.CalcLeft(width, 0, versionAlign);
			top = GUIController.CalcTop(height, 0, versionAlign);
			GUI.Label(new Rect(left, top, width, height), GameManager.GameVersion, labelVersionStyle);
		}

		if (!_showMenu) return;
		GUIController.Alignment buttonAlign = GUIController.Alignment.MiddleCenter;
		height = buttonStyle.fontSize + 4;
		width = 200;
		left = GUIController.CalcLeft(width, 0, buttonAlign);
		top = GUIController.CalcTop(height, 0, buttonAlign);
		if (GUI.Button(new Rect(left, top, width, height), "Resume", buttonStyle)) {
			GameManager.Instance.paused = false;
		}
	}
}
