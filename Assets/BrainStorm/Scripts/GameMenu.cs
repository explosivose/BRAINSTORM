using UnityEngine;
using System.Collections;

public class GameMenu : Singleton<GameMenu> {

	
	private bool _showMenu;
	public bool showMenu {
		get { return _showMenu; }
		set { _showMenu = value; }
	}
	
	void OnGUI() {
		if (!_showMenu) return;
		if (GUI.Button(new Rect(10, 10, 150, 100), "resume")) {
			GameManager.Instance.paused = false;
		}
	}
}
