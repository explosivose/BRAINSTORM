using UnityEngine;
using System.Collections;

public class TerrorManager : MonoBehaviour {

	public static TerrorManager Instance;
	public int shrinkTerrainPasses = 10;
	public int shrinkTerrainSectionSize = 20;
	private float[,] _originalHeights;
	
	// Use this for initialization
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
		
	}
	
	void Start() {
		TerrainData terrainData = Terrain.activeTerrain.terrainData;
		int width = terrainData.heightmapWidth;
		int height = terrainData.heightmapHeight;
		_originalHeights = Terrain.activeTerrain.terrainData.GetHeights(0, 0, width, height);;
	}
	
	// Update is called once per frame
	public void StopTerror() {
		StartCoroutine( ShrinkTerrain() );
		
	}
	
	IEnumerator ShrinkTerrain() {
		TerrainData terrainData = Terrain.activeTerrain.terrainData;
		int width = terrainData.heightmapWidth;
		int height = terrainData.heightmapHeight;
		float[,] heights = terrainData.GetHeights(0, 0, width, height);
		
		Debug.Log ("Heights: " + heights.Length);
		
		int s = shrinkTerrainSectionSize;
		for (int i = 1; i <= shrinkTerrainPasses; i++) {
			float t = (float)(i)/(float)shrinkTerrainPasses;
			for (int X = 0; X < width; X+=s) {
				for (int Y = 0; Y < height; Y+=s) {
					for (int x = 0; x < s; x++) {
						for (int y = 0; y < s; y++) {
							if (X+x < width && Y+y < height) {
								float o = _originalHeights[X+x,Y+y];
								heights[X+x,Y+y] = Mathf.Lerp(o, o * 0.03125f, t);
							} // box loop
						} // little y loop
					} // little x loop
					terrainData.SetHeights(0, 0, heights);
					yield return new WaitForEndOfFrame();
				} // big Y loop
			} // big X loop
		} // passes loop

	}
	
	void OnDestroy() {
		Terrain.activeTerrain.terrainData.SetHeights(0, 0, _originalHeights);
	}
}
