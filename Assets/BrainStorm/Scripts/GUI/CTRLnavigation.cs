using UnityEngine;
using System.Collections;

public class CTRLnavigation : CTRLelement {

	public enum Action {
		menu, back
	}
	public Action action;
	public Transform menuPrefab;
	private Transform menuInstance;

	protected override void OnEnable ()
	{
		base.OnEnable ();
	}

	protected override void OnDisable ()
	{
		base.OnDisable ();
		if (menuInstance) menuInstance.Recycle();
	}
	
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
		case Action.menu:
			ShowMenu();
			break;
		case Action.back:
			transform.parent.Recycle();
			break;
		default:
			break;
		}
	}
	
	void ShowMenu() {
		// hide other branches first
		transform.parent.BroadcastMessage("HideMenu", SendMessageOptions.DontRequireReceiver);
		Transform cam = Camera.main.transform;
		Vector3 position = cam.position + (cam.forward + cam.right) * 5f;
		Quaternion rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
		menuInstance = menuPrefab.Spawn(position, rotation);
	}
	
	void HideMenu() {
		if (menuInstance) menuInstance.Recycle();
	}
}
