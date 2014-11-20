using UnityEngine;
using System.Collections;

public class TerrainGenerator : MonoBehaviour {

	public static TerrainGenerator Instance;
	public float noise = 0.01f;
	public float bumpiness = 5f;
	private TerrainData _terrain;
	
	void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(this);
		}
	}
	
	public void Generate() {
		_terrain = Terrain.activeTerrain.terrainData;
		int width = _terrain.heightmapWidth;
		int height = _terrain.heightmapHeight;
		float[,] heightmap = new float[width,height];
		// there's a rare case here where x + seed or y + seed > or < int range
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				heightmap[x,y] = Mathf.PerlinNoise(
					(float)x/(float)width * bumpiness,
					(float)y/(float)height * bumpiness
					) + Random.value * noise;
			}
		}
		
		_terrain.SetHeights(0,0,heightmap);
	}
}
