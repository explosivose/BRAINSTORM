using UnityEngine;
using System.Collections;

public class CTRLtext : CTRLelement {

	// latenight thought about these tooltips:
	// make a tooltip prefab and reference it in equipment component
	// instantiate via equipment component 
	// this ensures that all tooltips are le same
	public bool tooltip = false;
	public float tooltipTime = 0.5f;

	public enum Source {
		None, Name, ParentName, Developers, Assets, PhotonPlayerName
	}
	public Source source;

	private bool inspected = false;
	private float inspectTime = 0f;

	protected override void OnEnable ()
	{
		base.OnEnable ();
		textMesh.color = Color.clear;
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
		case Source.PhotonPlayerName:
			text = GetComponentInParent<PhotonView>().owner.name;
			break;
		case Source.None:
		default:
			break;
		}
	}
	
	void OnInspectStart() {
		if (tooltip && !inspected) {
			inspectTime = Time.time;
			inspected = true;
		}
	}
	
	void OnInspectStop() {
		if (tooltip) {
			inspected = false;
		}
	}
	
	protected override void Update ()
	{
		if (Application.isLoadingLevel) return;
		if (!Camera.main) return;
		if (!Player.localPlayer) return;
		base.Update ();
		if (tooltip) {
			if (inspected && inspectTime + tooltipTime < Time.time) {
				// ensure player name is up to date
				if (source == Source.PhotonPlayerName) {
					text = GetComponentInParent<PhotonView>().owner.name;
				}
				// rotate text to face camera
				Transform cam = Camera.main.transform;
				Quaternion rotation = Quaternion.LookRotation(transform.position - cam.position);
				transform.rotation = Quaternion.Lerp(
					transform.rotation, 
					rotation, 
					Time.deltaTime * 4f
				);
				// fade in text
				textMesh.color = Color.Lerp(
					textMesh.color,
					textColor,
					Time.deltaTime * 6f
				);
			}
			else {
				// fade out text
				textMesh.color = Color.Lerp(
					textMesh.color,
					Color.clear,
					Time.deltaTime * 4f
				);
			}
		}
	}
	
}
