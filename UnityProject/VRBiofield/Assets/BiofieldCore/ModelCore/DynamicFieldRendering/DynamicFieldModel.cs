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


	private bool isSetup = false;
	public void EnsureSetup() {
		if (isSetup)
			return;
		isSetup = true;

		this.Body.EnsureSetup ();
		int sideRes = 7;
		this.FieldsCells = new VolumeBuffer<DynFieldCell> (Cubic<int>.CreateSame (sideRes));
		var l2w = this.transform.localToWorldMatrix;
		var cntr = Body.Chakras.AllChakras [3].transform.position;//  l2w.MultiplyPoint (Vector3.zero);
		foreach (var nd in this.FieldsCells.AllIndices3()) {
			var cell = this.FieldsCells.Read (nd.AsCubic());
			cell.Pos = this.transform.localToWorldMatrix.MultiplyPoint (FieldsCells.Header.CubicToDecimalUnit(nd) - (Vector3.one * 0.5f));
			cell.Direction = cntr - cell.Pos;
			cell.Twist = 0.0f;
			this.FieldsCells.Write (nd.AsCubic(), cell);
		}
	}

	// Use this for initialization
	void Start () {
		
	}

	void UpdateCellFieldDir() {
		var chakras = this.Body.Chakras.AllChakras;
		var chakra = chakras [((int)(Time.time * 2.0f)) % chakras.Length];

		var cells = this.FieldsCells;
		var cnt = cells.Header.TotalCount;
		for (int i = 0; i < cnt; i++) {
			var c = cells.Array [i];
			c.Direction = chakra.transform.position - c.Pos;
			cells.Array [i] = c;
		}
	}
	
	// Update is called once per frame
	void Update () {
		this.UpdateCellFieldDir	();

	}
}
