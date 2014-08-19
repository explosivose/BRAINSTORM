using UnityEngine;
using System.Collections;

[System.Serializable]
public class GUIBarPlacement {
	public Transform prefab;
	public GUIController.Alignment alignment;
	public int horizontalOffset;
	public int verticalOffset;
	public GUIBarScript SpawnAndPlace() {
		Transform instance = prefab.Spawn();
		instance.parent = Player.Instance.transform;
		GUIBarScript bar = instance.GetComponent<GUIBarScript>();
		int width = bar.Background.width;
		int height = bar.Background.height;
		bar.Position.x = GUIController.CalcLeft(width, horizontalOffset, alignment);
		bar.Position.y = GUIController.CalcTop(height, verticalOffset, alignment);
		return bar; 
	}
}

public class GUIController : MonoBehaviour {

	
	public GUIBarPlacement healthBar;
	
	private GUIBarScript _healthBarInstance;
	
	public enum Alignment {
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft, 
		LowerCenter,
		LowerRight
	}
	
	public static int CalcTop(int height, int offset, Alignment alignment) {
		switch (alignment)
		{
		case Alignment.UpperCenter: 
		case Alignment.UpperLeft: 
		case Alignment.UpperRight:
		default:
			return offset;
			
		case Alignment.MiddleCenter: 
		case Alignment.MiddleLeft: 
		case Alignment.MiddleRight:
			return Mathf.RoundToInt(Screen.height/2 - height/2) + offset;
			
		case Alignment.LowerCenter: 
		case Alignment.LowerLeft: 
		case Alignment.LowerRight:
			return Screen.height - height - offset;
			
		}
	}
	
	public static int CalcLeft(int width, int offset, Alignment alignment) {
		switch (alignment)
		{
		case Alignment.LowerLeft: 
		case Alignment.MiddleLeft: 
		case Alignment.UpperLeft:
		default:
			return offset;
			
		case Alignment.LowerCenter: 
		case Alignment.MiddleCenter: 
		case Alignment.UpperCenter:
			return Mathf.RoundToInt(Screen.width/2 - width/2) + offset;
			
		case Alignment.LowerRight: 
		case Alignment.MiddleRight: 
		case Alignment.UpperRight:
			return Screen.width - width - offset;
			
		}
	}
	
	// Use this for initialization
	void Start () {
		
		// spawn healthbar and place on bottom right
		_healthBarInstance = healthBar.SpawnAndPlace();
		
	}
	
	// Update is called once per frame
	void Update () {
		_healthBarInstance.Value = Player.Instance.health01;
	}
	
}
