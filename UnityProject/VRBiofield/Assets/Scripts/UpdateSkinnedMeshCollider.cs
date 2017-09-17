using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class UpdateSkinnedMeshCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.UpdateCollider ();
	}

	public void UpdateCollider() {
		var sm = this.GetComponent<SkinnedMeshRenderer> ();
		var mc = this.GetComponent<MeshCollider> ();

		Mesh colliderMesh = new Mesh();
		sm.BakeMesh(colliderMesh);
		mc.sharedMesh = null;
		mc.sharedMesh = colliderMesh;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
