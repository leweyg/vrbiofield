using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFieldModel : MonoBehaviour {

	public BodyLandmarks Body;

	public VolumeBuffer<DynFieldCell> FieldsCells = null;

	public struct DynFieldCell
	{
		public Vector3 Pos;
		public Vector3 Direction;
		public float Twist;
	};

	static Vector3 OneOver(Vector3 v) {
		return new Vector3 (1.0f / v.x, 1.0f / v.y, 1.0f / v.z);
	}

	static Vector3 Scale(Vector3 a, Vector3 scl) {
		return new Vector3 (a.x * scl.x, a.y * scl.y, a.z * scl.z);
	}

	private bool isSetup = false;
	public void EnsureSetup() {
		if (isSetup)
			return;
		isSetup = true;

		this.Body.EnsureSetup ();
		int sideRes = 7;
		this.FieldsCells = new VolumeBuffer<DynFieldCell> (Cubic<int>.CreateSame (sideRes));
		var scl = OneOver (this.FieldsCells.Header.Size.AsVector3 ());
		var l2w = this.transform.localToWorldMatrix;
		var cntr = Body.Chakras.AllChakras [3].transform.position;//  l2w.MultiplyPoint (Vector3.zero);
		foreach (var nd in this.FieldsCells.AllIndices3()) {
			var cell = this.FieldsCells.Read (nd.AsCubic());
			cell.Pos = this.transform.localToWorldMatrix.MultiplyPoint (FieldsCells.Header.CubicToDecimalUnit(nd) - (Vector3.one * 0.5f));
			cell.Pos += Scale (Random.insideUnitSphere, scl); // add random offset
			cell.Direction = cntr - cell.Pos;
			cell.Twist = 0.0f;
			this.FieldsCells.Write (nd.AsCubic(), cell);
		}
	}

	// Use this for initialization
	void Start () {
		
	}

	public static Vector3 MagneticDipoleField(Vector3 pos, Vector3 opos, Vector3 odip) {

		Vector3 r = (pos - opos);
		Vector3 rhat = r.normalized;

		Vector3 res = (3.0f * ((float)(Vector3.Dot (odip, rhat))) * rhat - odip) / Mathf.Pow (r.magnitude, 3);// * 1e-7f;
		return res;
	}

	void UpdateCellFieldDir() {
		var chakras = this.Body.Chakras.AllChakras;
		var chakra = chakras [((int)(Time.time * 2.0f)) % chakras.Length];
		chakra = chakras[1];

		var cpos = chakra.transform.position;
		var cdir = chakra.transform.forward;

		var cells = this.FieldsCells;
		var cnt = cells.Header.TotalCount;
		for (int i = 0; i < cnt; i++) {
			var c = cells.Array [i];
			//c.Direction = chakra.transform.position - c.Pos;
			c.Direction = MagneticDipoleField(c.Pos, cpos, cdir);
			cells.Array [i] = c;
		}
	}
	
	// Update is called once per frame
	void Update () {
		this.UpdateCellFieldDir	();

	}
}
