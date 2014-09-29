using UnityEngine;
using System.Collections;

public class CTRLtextfield : CTRLelement {

	private bool fillin;
	
	protected override void OnEnable ()
	{
		base.OnEnable ();
		text = PhotonNetwork.playerName;
	}
	
	protected override void OnMouseUpAsButton ()
	{
		base.OnMouseUpAsButton ();
		fillin = true;
	}
	
	void Update() {
		if (!fillin) return;
		MouseLook.freeze = true;
		if (Input.GetKey(KeyCode.Escape)) {
			fillin = false;
			MouseLook.freeze = false;
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
					MouseLook.freeze = false;
					PhotonNetwork.playerName = text;
					Options.Save();
				}
				else {
					text += c;
				}
			}
		}
	}
}
