using UnityEngine;
using System.Collections;

public class CTRLgame : CTRLelement {

	public enum Action {
		resume, 
		restart, 
		quit, 
		noclip, 
		singleplayer, 
		multiplayer
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
			GameManager.Instance.Restart();
			break;
		case Action.quit:
			GameManager.Instance.Quit();
			break;
		case Action.noclip:
			Player.localPlayer.noclip = !Player.localPlayer.noclip;
			break;
		case Action.singleplayer:
			Application.LoadLevel("brainstorm");
			break;
		case Action.multiplayer:
			Application.LoadLevel("multiplayer");
			break;
		default:
			break;
		}
	}
	

}
