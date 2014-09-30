using UnityEngine;
using System.Collections;

public class CTRLsliderhandle : CTRLelement {

	public enum Action {
		AudioVolume,
		MouseSensivity
	}
	
	public float value {
		get {
			float oldMax = parent.width - width;
			float oldMin = 0;
			float oldRange = oldMax - oldMin;
			float newRange = maxValue - minValue;
			return (((left - oldMin) * newRange) / oldRange) + minValue;
		}
		set {
			float oldRange = maxValue - minValue;
			float newMax = parent.width - width;
			float newMin = 0;
			float newRange = newMax - newMin;
			left = Mathf.RoundToInt((((value - minValue) * newRange) / oldRange) + newMin);
		}
	}
	
	public Action action;
	public float minValue = 0f;
	public float maxValue = 1f;
	
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
			value = AudioListener.volume;
			break;
		case Action.MouseSensivity:
			value = MouseLook.sensitivity;
			break;
		}
	}
	
	protected override void OnDisable ()
	{
		base.OnDisable ();
		Options.Save();
	}
	
	void Update() {
		parent.text = value.ToString("F2");
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
				AudioListener.volume = value;
				break;
			case Action.MouseSensivity:
				MouseLook.sensitivity = value;
				break;
			}
		}
		else {
			switch(action) {
			default:
			case Action.AudioVolume:
				value = AudioListener.volume;
				break;
			case Action.MouseSensivity:
				value = MouseLook.sensitivity;
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
