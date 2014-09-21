using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BuildingMaker : MonoBehaviour {

	[System.Serializable]
	public class Floor {
		public Transform[] prefabs;
		[Range(0.0f, 1.0f)]
		public float variation;
		[Range(0.0f, 1.0f)]
		public float openness;
		public float maxSpacing;
		public float minSpacing;
		public Vector3 maxScale;
		public Vector3 minScale;
		
		public int index {get; private set;}
		public Transform prefab {
			get {
				return prefabs[index];
			}
		}
		public float spacing {get; set;}
		public Vector3 scale {get; set;}
		public Vector3 position {get; set;}
		public float height {
			get {
				return 1f;//prefab.renderer.bounds.size.y;
			}
		}
		
		public int NewIndex() {
			index = Random.Range(0, prefabs.Length);
			return index;
		}
	}
	
	[System.Serializable]
	public class Column {
		public Transform[] prefabs;
		public Vector3 maxScale;
		public Vector3 minScale;
		
		public int index {get; private set;}
		public Transform prefab {
			get {
				return prefabs[index];
			}
		}
		public Vector3 scale {get; set;}
		public float height {
			get {
				return prefab.renderer.bounds.size.y;
			}
		}
		
		public int NewIndex() {
			index = Random.Range(0, prefabs.Length);
			return index;
		}
	}

	public float maxBuildingHeight;
	public float minBuildingHeight;
	public float maxLeanAngle;
	public Floor floor;
	public Column column;
	
	private BoxCollider _box;
	
	void Start() {
		_box = GetComponent<BoxCollider>();
		
		if (PhotonNetwork.inRoom) {
			Random.seed = GameManager.Instance.activeScene.seed;
			Random.seed += Mathf.RoundToInt(transform.position.magnitude);
		}
		
		BuildImmediate();
		//StartCoroutine(BuildRoutine());
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.Lerp(Color.clear, Color.cyan, 0.75f);
		Gizmos.DrawCube(
			GetComponent<BoxCollider>().center + transform.position, 
			GetComponent<BoxCollider>().size
		);
	}
	
	// running this routine in separate machines results in different buildings
	// even with the same seed set in Start()
	IEnumerator BuildRoutine() {
		Debug.Log (Random.seed);
		GroundFloor();
		Debug.Log (Random.seed);
		float height = Random.Range(minBuildingHeight, maxBuildingHeight);
		Debug.Log (Random.seed);
		while (floor.position.y - transform.position.y < height) {
			NextFloor();
			float roll = Random.value;
			if (roll < floor.variation) GenerateFloorData();
			yield return new WaitForSeconds(0.1f);
			Debug.Log (Random.seed);
		}
		
		// lean
		Vector3 rot = new Vector3(
			(Random.value - 0.5f) * 2f * maxLeanAngle,
			0f,
			(Random.value - 0.5f) * 2f * maxLeanAngle
			);
		transform.Rotate(rot);
 		yield return new WaitForEndOfFrame();
 		
 		SendMessage("Combine");
	}
	
	void BuildImmediate() {
		
		GroundFloor();
		
		float height = Random.Range(minBuildingHeight, maxBuildingHeight);
		
		while (floor.position.y - transform.position.y < height) {
			NextFloor();
			float roll = Random.value;
			if (roll < floor.variation) GenerateFloorData();
		}
		
		// lean
		Vector3 rot = new Vector3(
			(Random.value - 0.5f) * 2f * maxLeanAngle,
			0f,
			(Random.value - 0.5f) * 2f * maxLeanAngle
			);
		transform.Rotate(rot);
		
		SendMessage("Combine");
	}
	
	void GroundFloor() {
		floor.spacing = Random.Range(floor.minSpacing, floor.maxSpacing);
		floor.scale = _box.size;
		floor.position = transform.position + _box.center;
		NextFloor();
	}
	
	void NextFloor() {
		floor.position += transform.up * floor.spacing;
		Transform instance = floor.prefab.Spawn(
			floor.position,
			transform.rotation
			);
		instance.parent = this.transform;
				
		// open or closed?
		float roll = Random.value; 
		if (roll < floor.openness) {
			// open floor
			instance.localScale = floor.scale;	
		}
		else {
			// closed floor
			instance.position += Vector3.up * floor.spacing/2f;
			Vector3 scale = floor.scale;
			scale.y *= floor.spacing/floor.height;
			instance.localScale = scale;
		}
		
		// random 180deg rotaton
		// 90deg or 270deg rotation would have to factor in swapping x and z scales 
		roll = Random.value; 
		while (roll > 0f) {
			instance.Rotate(transform.up, 180f);
			roll -= 0.5f;
		}
		
		SpawnColumns();
		// use different floor next time
		floor.NewIndex();
	}
	
	void GenerateFloorData() {
		floor.spacing = Random.Range(floor.minSpacing, floor.maxSpacing);
		
		Vector3 previousScale = floor.scale;
		
		floor.scale = RandomVector3InRange(floor.minScale, floor.scale);
		
		float xmax = previousScale.x - floor.scale.x;
		float zmax = previousScale.z - floor.scale.z;
		
		floor.position += transform.right * Random.Range(-xmax/2f, xmax/2f);
		floor.position += transform.forward * Random.Range(-zmax/2f, zmax/2f);
		
		// use different columns
		column.NewIndex();
	}
	
	void SpawnColumns() {
		
		// center column
		Vector3 location = new Vector3(
			floor.position.x,
			floor.position.y - floor.spacing/2f,
			floor.position.z
		);
		SpawnColumn(location);
		
		// 1st column
		location = new Vector3(
			floor.position.x + floor.scale.x/2f,
			floor.position.y - floor.spacing/2f,
			floor.position.z + floor.scale.z/2f
		);
		SpawnColumn(location);
		
		// 2nd column
		location = new Vector3(
			floor.position.x - floor.scale.x/2f,
			floor.position.y - floor.spacing/2f,
			floor.position.z + floor.scale.z/2f
		);
		SpawnColumn(location);
		
		// 3rd column
		location = new Vector3(
			floor.position.x - floor.scale.x/2f,
			floor.position.y - floor.spacing/2f,
			floor.position.z - floor.scale.z/2f
		);
		SpawnColumn(location);
		
		// 4th column
		location = new Vector3(
			floor.position.x + floor.scale.x/2f,
			floor.position.y - floor.spacing/2f,
			floor.position.z - floor.scale.z/2f
		);
		SpawnColumn(location);
	}
	
	void SpawnColumn(Vector3 location) {
		Transform instance = column.prefab.Spawn (location);
		instance.parent = this.transform;
		Vector3 scale = instance.localScale;
		scale.y *= 1.05f * floor.spacing/column.height;
		instance.localScale = scale;
	}
	
	Vector3 RandomVector3InRange(Vector3 min, Vector3 max) {
		float x = Random.Range(min.x, max.x);
		float y = Random.Range(min.y, max.y);
		float z = Random.Range(min.z, max.z);
		return new Vector3(x, y, z);
	}
}
