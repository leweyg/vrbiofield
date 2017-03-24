using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFieldModel : MonoBehaviour {

	public BodyLandmarks Body;

	public VolumeBuffer<DynFieldCell> FieldsCells = null;
	[Range(0,6)]
	public int CurrentFocusChakra = 0;
	public float UnitMagnitude = 45.0f;
	public float DEBUG_AvgMagnitude = 0.0f;

	public struct DynFieldCell
	{
		public Vector3 Pos;
		public Vector3 Direction;
		public Color LatestColor;
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
		int sideRes = 8;
		int chakraToShow = CurrentFocusChakra; //3;
		this.FieldsCells = new VolumeBuffer<DynFieldCell> (Cubic<int>.CreateSame (sideRes));
		var scl = OneOver (this.FieldsCells.Header.Size.AsVector3 ());
		var l2w = this.transform.localToWorldMatrix;
		var cntr = Body.Chakras.AllChakras [chakraToShow].transform.position;//  l2w.MultiplyPoint (Vector3.zero);
		foreach (var nd in this.FieldsCells.AllIndices3()) {
			var cell = this.FieldsCells.Read (nd.AsCubic());
			cell.Pos = this.transform.localToWorldMatrix.MultiplyPoint (FieldsCells.Header.CubicToDecimalUnit(nd) - (Vector3.one * 0.5f));
			cell.Pos += Scale (Random.insideUnitSphere, scl); // add random offset
			cell.Direction = cntr - cell.Pos;
			cell.LatestColor = Color.white;
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

	public static Vector3 ChakraDipoleField(Vector3 pos, Vector3 opos, Vector3 odip, bool isOneWay) {
		Vector3 r = (pos - opos);
		Vector3 rhat = r.normalized;

		float radiusScale = 3.0f; //3.0f;
		float localDot = Vector3.Dot(rhat, odip);
		float coreDot = localDot;
		float localDotScale = Mathf.Lerp (0.2f, 1.0f, Mathf.Pow (Mathf.Abs (localDot), 2));
		float sideScale = ((!isOneWay) ? (localDotScale) : ((localDot < 0) ? (localDotScale) : (localDotScale * 0.1f)));
		float distScale = sideScale / Mathf.Pow (r.magnitude, 3);
		Vector3 res = (radiusScale * ((float)((coreDot )) * rhat) - odip) * distScale;// * 1e-7f;
		res *= -Mathf.Sign(localDot); // flow in in both ways.
		return res;
	}

	void UpdateCellFieldDir() {
		var chakras = this.Body.Chakras.AllChakras;
		var chakra = chakras [((int)(Time.time * 0.5f)) % chakras.Length];
		chakra = chakras[CurrentFocusChakra];

		var cpos = chakra.transform.position;
		var cdir = -chakra.transform.up;
		var cOneWay = chakra.ChakraOneWay;

		var avgSum = 0.0f;
		var avgCnt = 0;

		var cells = this.FieldsCells;
		var cnt = cells.Header.TotalCount;
		for (int i = 0; i < cnt; i++) {
			var c = cells.Array [i];
			//c.Direction = chakra.transform.position - c.Pos;
			//c.Direction = MagneticDipoleField(c.Pos, cpos, cdir) / UnitMagnitude;
			var newDir = ChakraDipoleField(c.Pos, cpos, cdir, cOneWay);
			var newClr = chakra.ChakraColor;
			var lf = Time.deltaTime * 1.0f;
			c.Direction = Vector3.Lerp (c.Direction, newDir, lf);
			c.LatestColor = Color.Lerp (c.LatestColor, newClr, lf);
			cells.Array [i] = c;

			avgSum += c.Direction.magnitude;
			avgCnt += 1;
		}

		this.DEBUG_AvgMagnitude = (avgSum / ((float)avgCnt));
	}
	
	// Update is called once per frame
	void Update () {
		this.UpdateCellFieldDir	();

	}
}
