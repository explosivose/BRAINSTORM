using UnityEngine;
using System.Collections;


public class CTRLelement : MonoBehaviour {

	public Color textColor = Color.white;
	public Color textHoverColor = Color.cyan;

	// width and height are defined by the sprite width and height
	
	public int width {
		get {
			return Mathf.RoundToInt(_boxCollider.size.x * 100f);
		}
	}
	
	public int height {
		get {
			return Mathf.RoundToInt(_boxCollider.size.y * 100f);
		}
	}
	
	// zero means top edge meets the top of the CTRL
	public int top {
		get {
			return Mathf.RoundToInt(-transform.localPosition.y * 100f);
		}
		set {
			Vector3 localpos = transform.localPosition;
			localpos.y = -(float)value/100f;
			transform.localPosition = localpos;
		}
	}
	
	public int left {
		get {
			return Mathf.RoundToInt(transform.localPosition.x * 100f);
		}
		set {
			Vector3 localpos = transform.localPosition;
			localpos.x = -(float)value/100f;
			transform.localPosition = localpos;
		}
	}
	
	public string text {
		get {
			return textMesh.text;
		}
		set {
			textMesh.text = value;
		}
	}
	
	private BoxCollider _boxCollider;
	private Vector3 _initialPosition;
	protected TextMesh textMesh { get; private set; }
	
	protected virtual void Awake () {
		_boxCollider = GetComponent<BoxCollider>();
		_initialPosition = transform.localPosition;
		textMesh = transform.Find("text").GetComponent<TextMesh>();
		if (!textMesh) Debug.LogError("Required child object with TextMesh is missing.");
	}
	
	protected virtual void OnEnable() {
		textMesh.color = textColor;
	}
	
	protected virtual void Update() {
	
	}
	
	protected virtual void OnDisable() {
		transform.localPosition = _initialPosition;
	}
	
	protected virtual void OnMouseEnter() {
		textMesh.color = textHoverColor;
	}
	
	protected virtual void OnMouseExit() {
		textMesh.color = textColor;
	}
	
	protected virtual void OnMouseDown() {
		transform.localPosition = _initialPosition + Vector3.forward * 0.1f;
	}
	
	protected virtual void OnMouseUp() {
		transform.localPosition = _initialPosition;
	}
	
	protected virtual void OnMouseUpAsButton() {
		
	}
	
	public void SetPosition(int Top, int Left) {
		top = Top;
		left = Left;
	}
}
