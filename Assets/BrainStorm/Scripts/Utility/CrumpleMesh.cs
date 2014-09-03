using UnityEngine;
using System.Collections;

public class CrumpleMesh : MonoBehaviour {

	public float scale = 1f;
	public float speed = 1f;
	public bool recalculateNormals = false;
	
	private Vector3[] _baseVertices;
	private Perlin _noise = new Perlin();
	private Mesh _mesh;	

	void Start() {
		_mesh = GetComponent<MeshFilter>().mesh;
		_baseVertices = _mesh.vertices;
	}

	// Update is called once per frame
	void Update () {
		Vector3[] vertices = new Vector3[_baseVertices.Length];
		
		float timex = Time.time * speed * 0.1365143f;
		float timey = Time.time * speed * 1.21688f;
		float timez = Time.time * speed * 2.5564f;
		
		for(int i = 0; i < vertices.Length; i++) {
			Vector3 vertex = _baseVertices[i];
			
			vertex.x += _noise.Noise(timex + vertex.x, timex + vertex.y, timex + vertex.z) * scale;
			vertex.y += _noise.Noise(timey + vertex.x, timey + vertex.y, timey + vertex.z) * scale;
			vertex.z += _noise.Noise(timez + vertex.x, timez + vertex.y, timez + vertex.z) * scale;
			
			vertices[i] = vertex;
		}
		
		_mesh.vertices = vertices;
		
		if (recalculateNormals) _mesh.RecalculateNormals();
		
		_mesh.RecalculateBounds();
	}
}
