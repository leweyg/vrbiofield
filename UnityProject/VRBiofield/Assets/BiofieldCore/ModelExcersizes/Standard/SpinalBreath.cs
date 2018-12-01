using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpinalBreath : ExcersizeActivityInst {

	private ParticleSystem ParRenderer;
	//public BodyLandmarks Body;
	public NamedLines LineToShow = NamedLines.SpinalBreathing;
	public int ParticleCountBase = 10;
	private ParticleSystem.Particle[] Particles;
	private ParticleSpan CoreSpan;
	private float UnitAnimationSpeed = 1.0f;
	protected int ClaimedCount = 0;
	private float DefaultRadius = 0.2f;

	private List<ParticleSpan> AllSpans = new List<ParticleSpan> ();
	private ParticleSpan ClosestInfoSpan = null;

	private class ParticleSpan {
		public SpinalBreath Owner;
		public LinesThroughPoints Line;
		public int First, Count;
		public float LatestAlpha;
		public Vector3[] FieldCache;

		public ParticleSpan(SpinalBreath sb, int count, LinesThroughPoints ln) {
			this.Owner = sb;
			this.Line = ln;
			this.First = sb.ClaimedCount;
			this.Count = count;
			this.LatestAlpha = 1.0f;
			sb.ClaimedCount += count;
			sb.AllSpans.Add(this);
		}

		public int IndexOf(int i) {
			Debug.Assert (i < this.Count);
			Debug.Assert (i >= 0);
			return (i + First);
		}
	}


	public enum NamedLines {
		LeftLegToDanTien,
		RightLegToDanTien,
		SpinalBreathing,
		GroundingBreath,
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
		this.ApplyBodyPositioning ();
		this.ParRenderer = this.GetComponent<ParticleSystem> ();

		int cnt = ((LineToShow == NamedLines.SpinalBreathing) ? 1 : this.ParticleCountBase);
		this.ParticleCountBase = cnt;

		this.SetupCoreLine ();

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

	[ContextMenu("Test Energy Flow")]
	public void TestEnergyFlow() {
		FlowMeshAroundLine fm = this.GetComponentInChildren<FlowMeshAroundLine> ();
		fm.SetupLine (this.SpanLeftLegToDanTien.Line);
	}

	private Transform GetCommonParent(Transform a, Transform b) {
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

	private List<Vector3> LineBetweenTransforms(Transform a, Transform b) {
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

	private ParticleSpan SpanLeftLegToDanTien;
	private ParticleSpan SpanRightLegToDanTien;
	private ParticleSpan SpanCrownToDanTien;

	void SetupCoreLine() {
		switch (this.LineToShow) {
		case NamedLines.SpinalBreathing:
			{
				var pnts = this.LineBetweenTransforms (this.Body.SpineStart, this.Body.SpineEnd);
				this.CoreSpan = new ParticleSpan(this, 1, new LinesThroughPoints(pnts.ToArray()));
			}
			break;
		case NamedLines.GroundingBreath:
			{
				{
					var pnts = this.LineBetweenTransforms (this.Body.LeftLegEnd, this.Body.LeftLegStart);
					pnts.Add (this.Body.Chakras.AllChakras[1].transform.position);
					this.SpanLeftLegToDanTien = new ParticleSpan (this, this.ParticleCountBase, new LinesThroughPoints (pnts.ToArray ()));
				}
				{
					var pnts = this.LineBetweenTransforms (this.Body.RightLegEnd, this.Body.RightLegStart);
					pnts.Add (this.Body.Chakras.AllChakras[1].transform.position);
					this.SpanRightLegToDanTien = new ParticleSpan (this, this.ParticleCountBase, new LinesThroughPoints (pnts.ToArray ()));
				}
				{
					//SpanCrownToDanTien
					var pnts = this.LineBetweenTransforms (
						this.Body.Chakras.AllChakras[6].transform, 
						this.Body.Chakras.AllChakras[1].transform);
					this.SpanCrownToDanTien = new ParticleSpan (this, (int)(0.75f * this.ParticleCountBase), new LinesThroughPoints (pnts.ToArray ()));
				}
			}
			break;
		default:
			Debug.Assert (false, "Unknown line: " + this.LineToShow);
			break;
		}

	}

	void UpdateParticles_Spinal() {
		float toffset = 0; //UnitAnimationSpeed * Time.time * (1.0f / this.Particles.Length);
		bool isOut = (this.Breath.BreathIndex % 2)==1;
		var sp = this.CoreSpan;
		for (int i = 0; i < sp.Count; i++) {
			float t = this.Breath.UnitTimeInBreath;
			if (isOut) {
				t = (1.0f - t);
			}
			var pos = this.CoreSpan.Line.SampleAtUnitLength (Mathf.Repeat( t + toffset, 1.0f));
			var a = Mathf.Clamp01 (Breath.UnitTo010f(t) * 3.1f);
			var clr = ((!isOut) ? Color.blue : Color.red);
			this.Particles [i].position = pos;
			this.Particles [i].startColor = ColorWithAlpha (clr, a);
		}
	}

	void UpdateParticles_GroundingSpan(ParticleSpan ps, float toffset) {
		for (int i = 0; i < ps.Count; i++) {
			float ba = this.Breath.UnitBreathInPct;
			float fi = ((float)i) / ((float)(ps.Count - 1));
			float mt = Mathf.Repeat (fi + toffset, 1.0f);
			float a = ba * Mathf.Clamp01 (Breath.UnitTo010f (mt) * 5.1f);
			var pos = ps.Line.SampleAtUnitLength (mt);
			bool isSpine = (ps == SpanCrownToDanTien);
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
			ps.LatestAlpha = a;

			var pi = ps.IndexOf(i);
			this.Particles [pi].position = pos;
			this.Particles [pi].startColor = ColorWithAlpha (clr, a);
		}
	}

	void UpdateParticles_Grounding() {
		float toffset = UnitAnimationSpeed * Breath.UnitTimeSinceStart;// * (1.0f / this.Particles.Length);
		this.UpdateParticles_GroundingSpan(this.SpanLeftLegToDanTien, toffset);
		this.UpdateParticles_GroundingSpan(this.SpanRightLegToDanTien, toffset);
		this.UpdateParticles_GroundingSpan (this.SpanCrownToDanTien, toffset);

	}

	public override Vector3 CalcVectorField (DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor)
	{
		Vector3 res = Vector3.zero;
		primaryColor = Color.white;
		foreach (var s in this.AllSpans) {
			if (s.LatestAlpha > 0.0f) {
				if (s.FieldCache == null) {
					s.FieldCache = new Vector3[model.CellCount];
					for (int c = 0; c < model.CellCount; c++) {
						var cPos = model.FieldsCells.Array[c].Pos;
						var cField = Vector3.zero;
						for (int i = 1; i < s.Line.Points.Length; i++) {
							var fm = s.Line.Points [i - 1];
							var to = s.Line.Points [i];
							var fld = DynamicFieldModel.ChakraFieldAlongLineV4 (cPos, fm, to, false);
							cField += fld;
						}
						s.FieldCache [c] = cField;
					}
				}
				primaryColor = ((s == this.SpanCrownToDanTien) ? Color.white : Color.green);
				res += s.FieldCache [posIndex] * Mathf.Pow( s.LatestAlpha, 0.75f );
			}
		}
		return res;
	}

	Color ColorWithAlpha(Color c, float a) {
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
	
	// Update is called once per frame
	void Update () {

		this.Breath.CurrentBreathsPerRep = 2;

		if (this.IsInfoAvatar) {
			this.UpdateInfoModel ();
		}

		switch (this.LineToShow) {
		case NamedLines.SpinalBreathing:
			this.UpdateParticles_Spinal ();
			break;
		case NamedLines.GroundingBreath:
			this.UpdateParticles_Grounding ();
			break;
		default:
			Debug.Assert (false, "Unknown Update line: " + this.LineToShow);
			break;
		}

		this.ParRenderer.SetParticles (this.Particles, this.Particles.Length);
	}
}
