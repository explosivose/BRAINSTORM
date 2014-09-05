using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Pylon : MonoBehaviour {

	public Transform nextPylon;
	
	public Transform myNode {
		get; private set;
	}
	
	public Transform nextNode {
		get; private set;
	}
	
	private LineRenderer _lr;
	
	// Use this for initialization
	void Start () {
		myNode = transform.Find("Node");
		if (!myNode) Debug.LogError("Pylon requires Node.");
		
		nextNode = nextPylon.Find("Node");
		if (!nextNode) Debug.LogError("Next pylon has no node.");
		
		_lr = GetComponent<LineRenderer>();
		_lr.SetVertexCount(2);
		if (nextNode && myNode) {
			_lr.SetPosition(0, myNode.position);
			_lr.SetPosition(1, nextNode.position);
		}
	}
}
