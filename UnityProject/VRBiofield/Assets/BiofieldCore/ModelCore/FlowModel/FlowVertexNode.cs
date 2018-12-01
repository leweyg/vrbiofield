using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowVertexNode : MonoBehaviour {

	public int UniqueIndex { get; set; }

	public Vector3 Location {
		get { return this.transform.position; }
	}

	public Vector3 ForwardDir {
		get{
			return this.transform.localToWorldMatrix.
				MultiplyVector(new Vector3(0,1,0)).normalized;
		}
	}

	public Vector3 UpDir {
		get{
			return this.transform.localToWorldMatrix.
				MultiplyVector(new Vector3(1,0,0)).normalized;
		}
	}

	public Vector3 TempNormal {get; set;}


	private static List<FlowVertexNode> FlowNodesForAdd = new List<FlowVertexNode>();

	[ContextMenu ("Mark for Tri Add")]
	void ContextMenu_MarkForTriAdd () {
		FlowNodesForAdd.Add (this);
		while (FlowNodesForAdd.Count>3) {
			FlowNodesForAdd.RemoveAt(0);
		}
		if (FlowNodesForAdd.Count == 3) {
			this.ContextMenu_DoTriAdd();
		}
	}

	[ContextMenu ("Do Tri Add")]
	void ContextMenu_DoTriAdd () {
		var tri = new EnergyMeshGenerator.MeshTri (
			FlowNodesForAdd [0], FlowNodesForAdd [1], FlowNodesForAdd [2]);
		FlowNodesForAdd.Clear ();
		GameObject.FindObjectOfType<EnergyMeshGenerator> ().CreateMeshTriObject (tri);
	}
}
