using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleExcersize : ExcersizeActivityInst {

	protected ParticleSystem ParRenderer;
	public int ParticleCountBase = 10;
	protected ParticleSystem.Particle[] Particles;
	protected ParticleSpan CoreSpan;
	protected float UnitAnimationSpeed = 1.0f;
	protected int ClaimedCount = 0;
	protected float DefaultRadius = 0.2f;

	protected List<ParticleSpan> AllSpans = new List<ParticleSpan> ();
	protected ParticleSpan ClosestInfoSpan = null;

	protected class ParticleSpan {
		public ParticleExcersize Owner;
		public LinesThroughPoints Line;
		public int First, Count;
		private Vector3[] RandomUnitOffsets = null;
		public Vector3[] FlowFieldCache = null;
		public float LatestOverallAlpha = 1.0f;

		public ParticleSpan(ParticleExcersize sb, int count, LinesThroughPoints ln) {
			this.Owner = sb;
			this.Line = ln;
			this.First = sb.ClaimedCount;
			this.Count = count;
			sb.ClaimedCount += count;
			sb.AllSpans.Add(this);
		}

		public Vector3[] EnsureRandomOffsets() {
			if (this.RandomUnitOffsets != null)
				return this.RandomUnitOffsets;
			this.RandomUnitOffsets = new Vector3[this.Count];
			for (int i = 0; i < this.Count; i++) {
				this.RandomUnitOffsets [i] = Random.onUnitSphere;
			}
			return this.RandomUnitOffsets;
		}

		public int IndexOf(int i) {
			Debug.Assert (i < this.Count);
			Debug.Assert (i >= 0);
			return (i + First);
		}
	}

	protected void InnerStart() {
		this.EnsureSetup ();
		this.ParRenderer = this.GetComponent<ParticleSystem> ();

		int cnt = this.ParticleCountBase;
		this.ParticleCountBase = cnt;

		this.VirtualSetupCoreLine ();

		Particles = new ParticleSystem.Particle[this.ClaimedCount];
		for (int i = 0; i < Particles.Length; i++) {
			Particles [i].position = Vector3.one * 0.1f * ((float)i);
			Particles [i].startColor = Color.green;
			Particles [i].velocity = Vector3.zero;
			Particles [i].startSize = DefaultRadius;
			Particles [i].startSize3D = Vector3.one * DefaultRadius;
			Particles [i].remainingLifetime = (float)( 60 * 60 * 24 ); // 1 day
		}
		this.ParRenderer.SetParticles (this.Particles, this.Particles.Length);
	}

	// Use this for initialization
	void Start () {
		this.InnerStart ();
	}

	protected Transform GetCommonParent(Transform a, Transform b) {
		var apars = new List<Transform> ();
		var bpars = new List<Transform> ();
		while (a != null) {
			apars.Insert (0, a);
			a = a.parent;
		}
		while (b != null) {
			bpars.Insert (0, b);
			b = b.parent;
		}
		Transform result = null;
		for (int i=0; i< Mathf.Min (apars.Count, bpars.Count); i++) {
			if (apars [i] == bpars [i]) {
				result = apars [i];
			} else {
				return result;
			}
		}
		return result;
	}

	protected List<Vector3> LineBetweenTransforms(Transform a, Transform b) {
		List<Vector3> pnts = new List<Vector3> ();
		Transform common = GetCommonParent (a, b);
		Debug.Assert (common != null);
		while (a != common) {
			pnts.Add (a.position);
			a = a.parent;
		}
		var aend = pnts.Count;
		while (b != common) {
			pnts.Insert (aend, b.position);
			b = b.parent;
		}
		return pnts;
	}


	protected virtual void VirtualSetupCoreLine() {
		Debug.LogError ("Override this method");
	}


	protected void UpdateParticles_GeneralSpan(ParticleSpan ps, float toffset) {
		for (int i = 0; i < ps.Count; i++) {
			float ba = this.Breath.UnitBreathInPct;
			float fi = ((float)i) / ((float)(ps.Count - 1));
			float mt = Mathf.Repeat (fi + toffset, 1.0f);
			float a = ba * Mathf.Clamp01 (Breath.UnitTo010f (mt) * 5.1f);
			var pos = ps.Line.SampleAtUnitLength (mt);
			bool isSpine = false; //(ps == SpanCrownToDanTien);
			var clr = (isSpine) ? Color.white : Color.green;
			if (this.IsInfoAvatar) {
				if (ps == this.ClosestInfoSpan) {
					// leave alpha as is
				} else {
					a = 0.0f;
				}
			} else if (isSpine != ((Breath.BreathIndex % 2) == 1)) {
				a = 0.0f;
			}

			var pi = ps.IndexOf(i);
			this.Particles [pi].position = pos;
			this.Particles [pi].startColor = ColorWithAlpha (clr, a);
		}
	}


	protected Color ColorWithAlpha(Color c, float a) {
		return new Color (c.r, c.g, c.b, a);
	}

	void UpdateInfoModel() {
		var ray = FocusRay.main.CurrentRay;
		float bestScore = 0.7f;
		ParticleSpan bestSpan = null;
		foreach (var ps in this.AllSpans) {
			float score = Vector3.Dot ((ps.Line.AveragePoint - ray.origin).normalized, ray.direction.normalized);
			if (score > bestScore) {
				bestSpan = ps;
				bestScore = score;
			}
		}
		this.ClosestInfoSpan = bestSpan;
	}

	protected virtual void VirtualUpdate() {
		foreach (var s in this.AllSpans) {
			this.UpdateParticles_GeneralSpan (s, 0.0f);
		}
	}

	public override Vector3 CalcVectorField (DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor)
	{
		Vector3 res = Vector3.zero;
		primaryColor = Color.white;
		foreach (var ps in this.AllSpans) {
			if (ps.LatestOverallAlpha > 0.0f) {
				if (ps.FlowFieldCache != null) {
					// ready to go
				} else {
					ps.FlowFieldCache = new Vector3[model.CellCount];
					for (int c = 0; c < model.CellCount; c++) {
						var cPos = model.FieldsCells.Array [c].Pos;
						var cField = Vector3.zero;
						for (int i = 1; i < ps.Line.Points.Length; i++) {
							var fm = ps.Line.Points [i - 1];
							var to = ps.Line.Points [i];
							var fld = DynamicFieldModel.ChakraFieldAlongLineV4 (cPos, fm, to, false);
							cField += fld;
						}
						ps.FlowFieldCache [c] = cField;
					}
				}
				res += ps.FlowFieldCache [posIndex] * ps.LatestOverallAlpha;
			}
		}
		return res;
	}

	// Update is called once per frame
	void Update () {
		if (this.IsInfoAvatar) {
			this.UpdateInfoModel ();
		}

		this.VirtualUpdate ();

		this.ParRenderer.SetParticles (this.Particles, this.Particles.Length);
	}
}
