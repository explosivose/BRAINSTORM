using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BoidFlockSpawner : MonoBehaviour {

	public int flockSize = 20;
	public GameObject prefab;
	
	void Start()
	{

		for (var i=0; i<flockSize; i++)
		{
			Vector3 position = new Vector3 (
				Random.value * collider.bounds.size.x,
				Random.value * collider.bounds.size.y,
				Random.value * collider.bounds.size.z
				) - collider.bounds.extents;
			
			GameObject b = Instantiate(prefab, transform.position + position, Random.rotation) as GameObject;
			b.SendMessage("SetTarget", this.transform);
		}
	}

	
}
