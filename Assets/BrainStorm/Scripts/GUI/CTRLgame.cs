using UnityEngine;
using System.Collections;

public class CTRLgame : CTRLelement {

	public enum Action {
		resume, restart, quit, noclip
	}
	public Action action;
	
	protected override void OnMouseEnter ()
	{
		base.OnMouseEnter ();
	}
	
	protected override void OnMouseExit ()
	{
		base.OnMouseExit ();
	}

	protected override void OnMouseUpAsButton ()
	{
		base.OnMouseUpAsButton ();
		switch(action) {
		case Action.resume:
			GameManager.Instance.paused = false;
			break;
		case Action.restart:
			Application.LoadLevel(Application.loadedLevel);
			break;
		case Action.quit:
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
			break;
		case Action.noclip:
			Player.Instance.noclip = !Player.Instance.noclip;
			break;
		default:
			break;
		}
	}
	

}
