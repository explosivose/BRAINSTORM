using UnityEngine;
using System.Collections;

public class TerrorManager : MonoBehaviour {

	public static TerrorManager Instance;
	public int shrinkTerrainPasses = 10;
	public int shrinkTerrainSectionSize = 20;
	public Material meadowSkybox;
	private TerrainData _terrainData;
	private float[,] _originalHeights;
	private DetailPrototype[] _terrainDetails;
	
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
		_terrainData = Terrain.activeTerrain.terrainData;
		int width = _terrainData.heightmapWidth;
		int height = _terrainData.heightmapHeight;
		_originalHeights = Terrain.activeTerrain.terrainData.GetHeights(0, 0, width, height);;
	}
	
	// Update is called once per frame
	public void StopTerror() {
		StartCoroutine( TerrorToMeadowTransformation() );
		StartCoroutine( MeadowRenderSettings() );
		
	}
	
	IEnumerator TerrorToMeadowTransformation() {
		// Shrinking terrain heightmap with details is laggy as fuck so
		// remove terrain details beforehand.
		yield return StartCoroutine ( RemoveTerrainDetails() );
		yield return StartCoroutine ( ShrinkTerrain() );
		// Then restore the terrain details :-)
		yield return StartCoroutine ( RestoreTerrainDetails() );
	}
	
	IEnumerator ShrinkTerrain() {
		int width = _terrainData.heightmapWidth;
		int height = _terrainData.heightmapHeight;
		float[,] heights = _terrainData.GetHeights(0, 0, width, height);
		
		Debug.Log ("Heights: " + heights.Length);
		
		// this loop structure moves a square around the heightmap and
		// shrinks the heights inside the square. The amount to shrink by
		// is a lerp between the original height and some fraction (shrinkTo)
		// of the original height. This creates the shrink over time effect
		float shrinkTo = 1f/32f;
		int s = shrinkTerrainSectionSize;
		for (int i = 1; i <= shrinkTerrainPasses; i++) {
			float t = (float)(i)/(float)shrinkTerrainPasses;
			for (int X = 0; X < width; X+=s) {
				for (int Y = 0; Y < height; Y+=s) {
					for (int x = 0; x < s; x++) {
						for (int y = 0; y < s; y++) {
							if (X+x < width && Y+y < height) {
								float o = _originalHeights[X+x,Y+y];
								heights[X+x,Y+y] = Mathf.Lerp(o, o * shrinkTo, t);
							} // box loop
						} // little y loop
					} // little x loop
					_terrainData.SetHeights(0, 0, heights);
					yield return new WaitForEndOfFrame();
				} // big Y loop
			} // big X loop
		} // passes loop
	}
	
	IEnumerator RemoveTerrainDetails() {
		// store details
		_terrainDetails = _terrainData.detailPrototypes;
		
		// shrink detail heights
		float shrinkTime = 2f;
		int steps = 4;
		for (int i = 1; i <= steps; i++) {
			float t = (float)i/(float)steps;
			for (int p = 0; p < _terrainData.detailPrototypes.Length; p++) {
				_terrainData.detailPrototypes[p].minHeight = 
					Mathf.Lerp(_terrainDetails[p].minHeight, 0f, t);
				_terrainData.RefreshPrototypes();
				_terrainData.detailPrototypes[p].maxHeight = 
					Mathf.Lerp(_terrainDetails[p].maxHeight, 0f, t);
				_terrainData.RefreshPrototypes();
			}
			yield return new WaitForSeconds(shrinkTime/(float)steps);
		}
		
		// destroy the details
		_terrainData.detailPrototypes = null;
	}
	
	IEnumerator RestoreTerrainDetails() {
		// restore details
		_terrainData.detailPrototypes = _terrainDetails;
		
		// set terrain detail color to meadowy
		for (int p = 0; p < _terrainData.detailPrototypes.Length; p++) {
			_terrainData.detailPrototypes[p].healthyColor = Color.green;
			_terrainData.RefreshPrototypes();
			_terrainData.detailPrototypes[p].dryColor = Color.yellow;
			_terrainData.RefreshPrototypes();
		}
		
		// grow detail heights
		float growTime = 2f;
		int steps = 10;
		for (int i = 1; i <= steps; i++) {
			float t = (float)i/(float)steps;
			for (int p = 0; p < _terrainData.detailPrototypes.Length; p++) {
				_terrainData.detailPrototypes[p].minHeight =
					Mathf.Lerp(0f, _terrainDetails[p].minHeight, t);
				_terrainData.detailPrototypes[p].maxHeight =
					Mathf.Lerp(0f, _terrainDetails[p].maxHeight, t);
			}
			yield return new WaitForSeconds(growTime/(float)steps);
		}
	}
	
	IEnumerator MeadowRenderSettings() {
		RenderSettings.fog = false;
		RenderSettings.skybox = meadowSkybox;
		yield return new WaitForSeconds(1);
	}

	void OnDestroy() {
		_terrainData.SetHeights(0, 0, _originalHeights);
		if (_terrainData.detailPrototypes==null) {
			_terrainData.detailPrototypes = _terrainDetails;
		}
	}
}
