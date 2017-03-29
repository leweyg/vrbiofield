using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class VolumeSurfaceMesh : MonoBehaviour {

	[ContextMenu("Test Field Meshing")]
	void TestField() {
		int s = 3;
		VolumeBuffer<bool> test = new VolumeBuffer<bool> (new Cubic<int> (s, s, s));
		test.Write (new Int3 (0, 0, 0), true);
		test.Write (new Int3 (1, 0, 0), true);
		test.Write (new Int3 (0, 1, 0), true);
		test.Write (new Int3 (0, 0, 1), true);
		test.Write (new Int3 (0, 0, 2), true);

		test.Write (new Int3 (2, 2, 2), true);
		//test.Write (new Int3 (1, 2, 2), true);

		var mf = this.GetComponent<MeshFilter> ();
		mf.mesh = VolumeTetrahedraSurfacer.GenerateSurfaceVolume (test, (t => (t ? 3.0f : -3.0f)));
		var mr = this.GetComponent<MeshRenderer> ();
		mr.enabled = true;
		Debug.Log ("Done meshing.");
	}

	void FromDynamicField() {
		// TODO
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
