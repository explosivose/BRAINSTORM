using UnityEngine;
using System.Collections;

public class CTRLlevel : CTRLelement {

	public Scene.Tag level;
	
	protected override void Awake ()
	{
		 base.Awake();
		 finalText = level.ToString();
	}
	
	protected override void OnMouseUpAsButton ()
	{
		base.OnMouseUpAsButton ();
		GameManager.Instance.ChangeScene(level);
	}
}
