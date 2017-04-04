using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class VolumeSurfaceMesh : MonoBehaviour {

	public bool UpdateNow = false;

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

		SetMeshFromVolume (VolumeTetrahedraSurfacer.GenerateSurfaceVolume (test, (t => (t ? 3.0f : -1.0f))));
		Debug.Log ("Done meshing.");
	}

	void SetMeshFromVolume(Mesh m) {
		var mf = this.GetComponent<MeshFilter> ();
		mf.mesh = m;
		var mr = this.GetComponent<MeshRenderer> ();
		mr.enabled = true;
	}

	[Range(0.0f,30.0f)]
	public float IsosurfaceRootValue = 5.0f;
	[ContextMenu("From Dynamic Field")]
	void FromDynamicField() {
		var df = this.GetComponentInParent<DynamicFieldModel> ();
		var cells = df.FieldsCells;

		SetMeshFromVolume (VolumeTetrahedraSurfacer.GenerateSurfaceVolume (cells, (t => t.Direction.magnitude - IsosurfaceRootValue)));

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (UpdateNow) {
			UpdateNow = false;
			this.FromDynamicField ();
		}
	}
}
