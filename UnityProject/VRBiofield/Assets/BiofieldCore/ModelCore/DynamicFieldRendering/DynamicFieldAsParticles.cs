using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFieldAsParticles : MonoBehaviour {

	private ParticleSystem PartSystem = null;
	private ParticleSystemRenderer PartRenderer = null;
	private Material PartMatInst = null;
	private ParticleSystem.Particle[] PartData = null;
	private List<Vector4> PartCustom = null;
	public DynamicFieldModel Model = null;
	public bool IsDebugParticles = false;
	public bool IsDebugGrid = false;
	[Range(0.0f, 2.0f)]
	public float ParticleSizeScalar = 1.0f;
	private bool DataPushed { get; set; }

	private bool isSetup = false;
	public void EnsureSetup() {
		if (isSetup)
			return;
		isSetup = false;

		if (this.Model == null) {
			this.Model = this.gameObject.GetComponentInParent<DynamicFieldModel> ();
		}
		if (this.PartSystem == null) {
			this.PartSystem = this.gameObject.GetComponent<ParticleSystem> ();
		}
		this.PartRenderer = this.PartSystem.GetComponent<ParticleSystemRenderer> ();

		this.Model.EnsureSetup ();
		this.UpdateFieldParticles (isFirst:true);

		Model.OnPausedChanged += (bool isNowPaused) => {
			this.gameObject.SetActive(!isNowPaused);
		};
	}

	public static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 refSign) {
		var angle = Vector3.Angle (a, b);
		if (Vector3.Dot (refSign, b) < 0) {
			angle *= -1.0f;
		}
		return angle;
	}

	public static Color ColorWithAlpha(Color c, float alpha) {
		Color res = c;
		res.a = alpha;
		return res;
	}

	void UpdateFieldParticles(bool isFirst = false) {
		var cells = this.Model.FieldsCells;
		int count = cells.Header.TotalCount;
		var camPos = Camera.main.transform.position;
		if (this.PartData == null) {
			this.PartData = new ParticleSystem.Particle[count];
			this.PartCustom = new List<Vector4> (count);
			while (this.PartCustom.Count < count) {
				this.PartCustom.Add (Vector4.zero);
			}
		}
		for (int i = 0; i < count; i++) {
			var p = this.PartData [i];
			var c = cells.Array [i];
			var s = this.PartCustom [i];

			if (isFirst) {
				p.position = c.Pos;
				p.startColor = Color.green;
				p.startSize3D = Vector3.one * 0.2f * ParticleSizeScalar;
				p.remainingLifetime = 10000.0f;
				p.randomSeed = (uint)i;
			}

			if (false) {
				var tt = Time.timeSinceLevelLoad;
				var t = (tt - Mathf.Floor (tt));
				var ht = (t - 0.5f) * 2.0f;
			
				p.position = c.Pos + (c.Direction * ht * 0.01f);
			}

			Vector3 fwd = (p.position - camPos);
			Vector3 rght = Vector3.Cross (fwd, Vector3.up);
			Vector3 up = Vector3.Cross (fwd, rght);
			float angle = SignedAngleBetween(up, c.Direction, rght);
			p.axisOfRotation = fwd;
			p.rotation = angle;
			float timeAlpha = Mathf.Clamp01 (Mathf.Pow (c.Direction.magnitude / Model.UnitMagnitude, 2.0f));
			p.startColor = ColorWithAlpha (c.LatestColor, timeAlpha * Model.FieldOverallAlpha);
			s = new Vector4 ((float)i, UnityEngine.Random.value, 0, 0);

			this.PartData [i] = p;
			this.PartCustom [i] = s;
		}
		this.PartSystem.SetParticles (this.PartData, this.PartData.Length);
		this.PartSystem.SetCustomParticleData (this.PartCustom, ParticleSystemCustomData.Custom1);
		this.DataPushed = true;
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}

	private Vector3 DebugCellOffset(DynamicFieldModel.DynFieldCell c) {
		var offset = ((c.Direction / Model.UnitMagnitude) * 0.2f);
		return offset;
	}
	
	// Update is called once per frame
	void Update () {

		if (Model.IsPaused)
			return;
		if (Model.IsStaticLayout && this.DataPushed) {
			if (!PartMatInst) {
				PartRenderer.material.SetFloat ("CreateInstance", 1);
				PartMatInst = PartRenderer.material;
			}
			PartMatInst.SetFloat ("_CustomAlpha", this.Model.FieldOverallAlpha);
			return;
		}

		this.UpdateFieldParticles (true);

		if (this.IsDebugParticles || this.IsDebugGrid) {
			var cells = this.Model.FieldsCells;
			var cnt = cells.Header.TotalCount;
			for (int i = 0; i < cnt; i++) {
				var c = cells.Array [i];
				var offset = DebugCellOffset (c); //((c.Direction / Model.UnitMagnitude) * 0.1f);
				var tipScl = offset.magnitude * 0.2f;
				if (this.IsDebugParticles) {
					Debug.DrawLine (c.Pos, c.Pos + (offset * 1.0f), Color.green);
					Debug.DrawLine (c.Pos, c.Pos + (new Vector3 (0, tipScl, 0)), Color.green);
				}
				if (this.IsDebugGrid) {
					var ndx = cells.Header.LinearToCubic (i);
					var from = c.Pos + offset;
					//var justOne = new Int3(1,0,0);
					if (cells.Header.IsSafe (ndx.Add (Int3.One))) 
					{
						var two = VolumeHeader.Two;
						for (int j = 0; j < two.TotalCount; j++) 
						{
							var toCell = cells.Read (ndx.Add (two.LinearToCubic (j)));
								//var toCell = cells.Read(ndx.Add(justOne)); //new Int3(1, 0, 0)));
							var toPos = toCell.Pos + DebugCellOffset (toCell);
							Debug.DrawLine (from, toPos, Color.green);
						}
					}
				}
			}
		}

	}
}
