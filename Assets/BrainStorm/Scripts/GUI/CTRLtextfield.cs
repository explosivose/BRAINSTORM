using UnityEngine;
using System.Collections;

public class CTRLtextfield : CTRLelement {

	private bool fillin	 = false;
	private bool newName = false;
	
	
	protected override void OnEnable ()
	{
		finalText = PhotonNetwork.playerName;
		base.OnEnable ();
	}
	
	protected override void OnMouseUpAsButton ()
	{
		base.OnMouseUpAsButton ();
		fillin = true;
		newName = false;
	}
	
	void Update() {
		if (!fillin) {
			text = PhotonNetwork.playerName;
			return;
		}
		MouseLook.freeze = true;
		if (Input.GetKey(KeyCode.Escape)) {
			fillin = false;
			MouseLook.freeze = false;
		}
		if (Input.GetKey(KeyCode.Delete)) {
			text = "";
		}
		foreach(char c in Input.inputString) {
			if (c == '\b') {
				if (text.Length != 0) {
					text = text.Substring(0, text.Length-1);
				}
			}
			else {
				if (c == '\n' || c == '\r') {
					fillin = false;
					newName = false;
					MouseLook.freeze = false;
					PhotonNetwork.playerName = text;
					Options.Save();
				}
				else {
					if (!newName) {
						text = "";
						newName = true;
					}
					text += c;
				}
			}
		}
	}
}
