using UnityEngine;
using System.Collections;

public class CTRLtext : CTRLelement {

	// latenight thought about these tooltips:
	// make a tooltip prefab and reference it in equipment component
	// instantiate via equipment component 
	// this ensures that all tooltips are le same
	public bool tooltip = false;

	public enum Source {
		None, Name, ParentName, Developers, Assets
	}
	public Source source;

	protected override void OnEnable ()
	{
		base.OnEnable ();
		switch(source) {
		case Source.Assets:
			text = Strings.assets;
			break;
		case Source.Developers:
			text = Strings.developers;
			break;
		case Source.Name:
			text = Strings.OmitCloneSuffix(transform.name);
			break;
		case Source.ParentName:
			text = Strings.OmitCloneSuffix(transform.parent.name);
			break;
		case Source.None:
		default:
			break;
		}
	}
	
	protected override void Update ()
	{
		if (Application.isLoadingLevel) return;
		if (!Camera.main) return;
		if (!Player.localPlayer) return;
		base.Update ();
		if (tooltip) {
			Transform cam = Camera.main.transform;
			Quaternion rotation = Quaternion.LookRotation(transform.position - cam.position);
			transform.rotation = rotation;
			float playerDistance = Vector3.Distance(Player.localPlayer.transform.position, transform.position);
			float lerp = (4f-playerDistance);
			textMesh.color = Color.Lerp(Color.clear, textColor, lerp);
		}
	}
	
}
