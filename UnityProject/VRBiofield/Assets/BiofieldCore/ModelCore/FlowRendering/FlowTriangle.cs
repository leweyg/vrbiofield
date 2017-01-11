using UnityEngine;
using System.Collections;

public class FlowTriangle : MonoBehaviour {

	public FlowVertexNode NodeA;
	public FlowVertexNode NodeB;
	public FlowVertexNode NodeC;

	public Vector3 CalcNormal() {
		var n = Vector3.Cross (
			this.NodeB.Location - this.NodeA.Location,
			this.NodeC.Location - this.NodeA.Location)
			.normalized;
		if (n.y < 0) {
			n = n * -1.0f;
		}
		return n;
	}

	public FlowVertexNode[] AllNodes() {
		FlowVertexNode[] ans = new FlowVertexNode[3] { NodeA, NodeB, NodeC };
		return ans;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
