using UnityEngine;
using System.Collections;

public class CTRLsliderhandle : CTRLelement {

	public enum Action {
		AudioVolume,
		MouseSensivity
	}
	
	public float value {
		get {
			return 100 * left/(parent.width - width);
		}
		set {
			left = Mathf.RoundToInt(value) * (parent.width - width) / 100;
		}
	}
	
	public Action action;
	
	private bool slide;
	private float startPos;
	private CTRLelement parent;
	
	protected override void Awake ()
	{
		base.Awake ();
		parent = transform.parent.GetComponent<CTRLelement>();
	}
	
	protected override void OnEnable ()
	{
		base.OnEnable ();
		switch(action) {
		default:
		case Action.AudioVolume:
			value = AudioListener.volume * 100f;
			break;
		case Action.MouseSensivity:
			value = MouseLook.sensitivity * 5f;
			break;
		}
	}
	
	protected override void OnDisable ()
	{
		base.OnDisable ();
		Options.Save();
	}
	
	void Update() {
		if (slide) {
			float move = startPos - Input.mousePosition.x;
			int nextPos = left - Mathf.RoundToInt(move);
			nextPos = Mathf.Min(nextPos, parent.width - width);
			nextPos = Mathf.Max(nextPos, 0);
			left = nextPos;
			startPos = Input.mousePosition.x;
			
			switch(action) {
			default:
			case Action.AudioVolume:
				AudioListener.volume = value/100f;
				break;
			case Action.MouseSensivity:
				MouseLook.sensitivity = value/5f;
				break;
			}
		}
	}

	protected override void OnMouseDown ()
	{
		slide = true;
		MouseLook.freeze = true;
		startPos = Input.mousePosition.x;
	}
	
	protected override void OnMouseUp ()
	{
		slide = false;
		MouseLook.freeze = false;
	}
}
