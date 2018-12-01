using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChiBallBreath : ParticleExcersize {

	public BodyLandmarks ReferencePoseForChi;
	private ParticleSpan HandToHandSpan;
	private ParticleSpan LeftSideSpan;
	private ParticleSpan RightSideSpan;
	private float SphereRadius = 0.1f;

	public override void ApplyBodyPositioning ()
	{
		this.EnsureSetup ();
		this.ReferencePoseForChi.EnsureSetup ();
		this.Body.EnsureBodyPositioning ().CopyPositioningFrom (this.ReferencePoseForChi.EnsureBodyPositioning ());
	}

	protected override void VirtualSetupCoreLine ()
	{
		this.ApplyBodyPositioning ();
		//this.ParticleCountBase = 4;

		this.SphereRadius = Vector3.Distance (this.Body.EstLeftHandKnuckle.position, this.Body.EstRightHandKnuckle.position) * 0.4f;
		{
			var dantien = this.Body.Chakras.AllChakras [1].transform.position;
			var pnts = new Vector3[]{ (this.Body.EstLeftHandKnuckle.position + this.Body.EstRightHandKnuckle.position)*0.5f, dantien };
			this.HandToHandSpan = new ParticleSpan (this, this.ParticleCountBase, new LinesThroughPoints (pnts));
		}
		{
			var pnts = this.LineBetweenTransforms (this.Body.LeftArmStart, this.Body.EstLeftHandKnuckle);
			this.LeftSideSpan = new ParticleSpan (this, this.ParticleCountBase, new LinesThroughPoints (pnts.ToArray ()));
		}
		{
			var pnts = this.LineBetweenTransforms (this.Body.RightArmStart, this.Body.EstRightHandKnuckle);
			this.RightSideSpan = new ParticleSpan (this, this.ParticleCountBase, new LinesThroughPoints (pnts.ToArray ()));
		}
	}

	void UpdateParticles_GeneralChi(ParticleSpan ps, float toffset) {
		
		for (int i = 0; i < ps.Count; i++) {
			float ba = this.Breath.UnitBreathInPct;
			float fi = ((float)i) / ((float)(ps.Count - 1));
			float mt = Mathf.Repeat (fi + toffset, 1.0f);
			float a = ba * Mathf.Clamp01 (Breath.UnitTo010f (mt) * 5.1f);
			var pos = ps.Line.SampleAtUnitLength (mt);
			bool isSpine = false;
			var clr = Color.green;
			if (this.IsInfoAvatar) {
				if (ps == this.ClosestInfoSpan) {
					// leave alpha as is
				} else {
					a = 0.0f;
				}
			}
			ps.LatestOverallAlpha = a;
			var pi = ps.IndexOf(i);
			this.Particles [pi].position = pos;
			this.Particles [pi].startColor = ColorWithAlpha (clr, a);
		}
	}

	void UpdateParticles_HandToHandSpan(ParticleSpan ps, float toffset) {
		var offsets = ps.EnsureRandomOffsets ();
		for (int i = 0; i < ps.Count; i++) {
			float bt = this.Breath.UnitTimeInBreath;
			float ba = this.Breath.UnitBreathInPct;
			float fi = ((float)i) / ((float)(ps.Count - 1));
			float mt = Mathf.Repeat (fi + toffset, 1.0f);
			float a = ba * Mathf.Clamp01 (Breath.UnitTo010f (mt) * 5.1f);


			var basePose = ps.Line.SampleAtLength (Mathf.Clamp01 ((bt - 0.5f) * 2.0f));
			var pos = basePose + (offsets[i] * SphereRadius 
				* Breath.UnitTo010f(bt) 
				* Mathf.Sin(Time.timeSinceLevelLoad*4.2f + (offsets[i].y*20)));
			bool isSpine = false;
			var clr = Color.green;
			if (this.IsInfoAvatar) {
				if (ps == this.ClosestInfoSpan) {
					// leave alpha as is
				} else {
					a = 0.0f;
				}
			}
			ps.LatestOverallAlpha = a;

			var pi = ps.IndexOf(i);
			this.Particles [pi].position = pos;
			this.Particles [pi].startColor = ColorWithAlpha (clr, a);
			this.Particles [pi].startSize3D = Vector3.one * (DefaultRadius * 2.0f);
		}
	}

	public override Vector3 CalcVectorField (DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor)
	{
		var res = base.CalcVectorField (model, posIndex, pos, out primaryColor);
		primaryColor = Color.green;
		return res;
	}

	protected override void VirtualUpdate ()
	{
		this.Breath.CurrentBreathsPerRep = 1;

		float toffset = UnitAnimationSpeed * Breath.UnitTimeSinceStart;
		this.UpdateParticles_HandToHandSpan(this.HandToHandSpan, toffset);
		this.UpdateParticles_GeneralChi (this.LeftSideSpan, toffset);
		this.UpdateParticles_GeneralChi (this.RightSideSpan, toffset);
	}
}
