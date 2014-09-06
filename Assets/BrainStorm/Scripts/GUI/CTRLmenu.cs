using UnityEngine;
using System.Collections;

public class CTRLmenu : CTRLelement {

	public enum Action {
		menu, credits, back
	}
	public Action action;
	public Transform menuPrefab;
	private Transform menuInstance;

	protected override void OnEnable ()
	{
		base.OnEnable ();
		if (action == Action.credits) {
			text = Credits.developers;
		}
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
			SpawnMenu();
			break;
		case Action.back:
			transform.parent.Recycle();
			break;
		case Action.credits:
		default:
			break;
		}
	}
	
	void SpawnMenu() {
		Transform cam = Camera.main.transform;
		Vector3 position = cam.position + (cam.forward + cam.right) * 5f;
		Quaternion rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
		menuInstance = menuPrefab.Spawn(position, rotation);
	}
}
