using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
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
			return _text.text;
		}
		set {
			_text.text = value;
		}
	}
	
	private BoxCollider _boxCollider;
	private SpriteRenderer _background;
	private TextMesh _text;
	
	protected virtual void Awake () {
		_boxCollider = GetComponent<BoxCollider>();
		_background = GetComponent<SpriteRenderer>();
		_text = transform.Find("text").GetComponent<TextMesh>();
	}
	
	protected virtual void OnEnable() {
		_text.color = textColor;
	}
	
	protected virtual void OnDisable() {
		
	}
	
	protected virtual void OnMouseEnter() {
		_text.color = textHoverColor;
	}
	
	protected virtual void OnMouseExit() {
		_text.color = textColor;
	}
	
	protected virtual void OnMouseDown() {
		
	}
	
	protected virtual void OnMouseUpAsButton() {
		
	}
	
	public void SetPosition(int Top, int Left) {
		top = Top;
		left = Left;
	}
}
