using UnityEngine;
using System.Collections;

public class VolumeIconBehavior : MonoBehaviour {

	public VolumeSourceBehavior VolumeSource;
	public Material IconMaterial;
	public ChakraControl Chakras;

	public bool IsOverrideTime = false;
	[Range(0.0f,1.0f)] public float OverrideTimeTo = 0.0f;

	// Use this for initialization
	void Start () {
		if (this.IconMaterial == null) {
			this.IconMaterial = this.GetComponent<MeshRenderer>().material;
		}
	}

	static Matrix4x4 MatTranslate(Vector3 offset) {
		return Matrix4x4.TRS (offset, Quaternion.identity, Vector3.one);
	}

	static Matrix4x4 MatScale(Vector3 scl) {
		return Matrix4x4.TRS (Vector3.zero, Quaternion.identity, scl);
	}

	static float SignedPower(float v, float s) {
		if (v >= 0.0f)
			return Mathf.Pow (v, s);
		else
			return -Mathf.Pow (-v, s);
	}

	static Vector3 NthAxes(int ia) {
		Vector3 ans = Vector3.zero;
		ans [ia] = 1.0f;
		return ans;
	}

	static Matrix4x4 MatSwapAxes(int ix, int iy, int iz) {
		Matrix4x4 ans = Matrix4x4.identity;
		ans.SetRow (0, NthAxes (ix));
		ans.SetRow (1, NthAxes (iy));
		ans.SetRow (2, NthAxes (iz));
		return ans;
	}

	[Range(0,2)]
	public int SelectN0 = 0;
	[Range(0,2)]
	public int SelectN1 = 1;

	[Range(0,2)]
	public int SelectN2 = 2;

	
	// Update is called once per frame
	void Update () {

		Matrix4x4 mat = Matrix4x4.identity;


		var baseTime = (Time.timeSinceLevelLoad * 0.61f * 0.61f * 0.61f);
		if (this.IsOverrideTime) {
			baseTime = this.OverrideTimeTo;
		}
		var unitTime = baseTime - Mathf.Floor (baseTime);
		var signedTime = Mathf.Abs (unitTime - 0.5f) * 2.0f;

		var offset = SignedPower (signedTime, 0.75f) * 0.5f - 0.25f;	



		mat = mat * MatTranslate (
			Vector3.one * 0.5f + (Vector3.forward * (-0.5f + offset)));
		mat = mat * MatScale (new Vector3 (1, -1, 1));

		//mat = MatSwapAxes (2, 1, 0) * mat; // forward

		mat = MatSwapAxes (SelectN0, SelectN1, SelectN2)
			* mat; // forward


		this.IconMaterial.SetMatrix ("_UnitToVolMatrix", mat);

		//this.Chakras.EnableOnlyThisChakra.EnsureSetup ();
		//var tex = this.Chakras.EnableOnlyThisChakra.VolumeSources.VolumeTexture;
		var tex = this.VolumeSource.VolumeTexture;
		this.IconMaterial.SetTexture ("_MainVol",tex);
	}
}
