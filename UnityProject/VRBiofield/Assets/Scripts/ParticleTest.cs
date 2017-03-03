using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleTest : MonoBehaviour {

	private ParticleSystem ParRenderer;
	public BodyLandmarks Body;
	public NamedLines LineToShow = NamedLines.SpinalBreathing;
	private ParticleSystem.Particle[] Particles;
	private LinesThroughPoints CoreLine;
	private float UnitAnimationSpeed = 1.0f;
	private ExcersizeBreathController BreathTimer;


	public enum NamedLines {
		LeftLegToDanTien,
		RightLegToDanTien,
		SpinalBreathing,
	}

	// Use this for initialization
	void Start () {
		this.ParRenderer = this.GetComponent<ParticleSystem> ();
		this.BreathTimer = GameObject.FindObjectOfType<ExcersizeBreathController> ();

		int cnt = ((LineToShow == NamedLines.SpinalBreathing) ? 1 : 8);

		Particles = new ParticleSystem.Particle[cnt];
		for (int i = 0; i < Particles.Length; i++) {
			Particles [i].position = Vector3.one * 0.1f * ((float)i);
			Particles [i].startColor = Color.green;
			Particles [i].velocity = Vector3.zero;
			Particles [i].startSize = 0.1f;
			Particles [i].startSize3D = Vector3.one * 0.1f;
			Particles [i].remainingLifetime = (float)( 60 * 60 * 24 ); // 1 day
		}
		this.ParRenderer.SetParticles (this.Particles, this.Particles.Length);
		this.SetupCoreLine ();
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

	void SetupCoreLine() {
		switch (this.LineToShow) {
		case NamedLines.LeftLegToDanTien:
			{
				var pnts = this.LineBetweenTransforms (this.Body.LeftLegEnd.transform, this.Body.LeftLegStart.transform);
				pnts.Add (this.Body.SpineStart.position);
				this.CoreLine = new LinesThroughPoints (pnts.ToArray ());
			}
			break;
		case NamedLines.SpinalBreathing:
			{
				var pnts = this.LineBetweenTransforms (this.Body.SpineStart, this.Body.SpineEnd);
				//pnts.Add (this.Body.SpineStart.position);
				this.CoreLine = new LinesThroughPoints (pnts.ToArray ());
				for (int i = 0; i < this.Particles.Length; i++) {
					Particles[i].startColor = Color.red;
				}
			}
			break;
		}

	}
	
	// Update is called once per frame
	void Update () {

		float toffset = 0; //UnitAnimationSpeed * Time.time * (1.0f / this.Particles.Length);

		for (int i = 0; i < this.Particles.Length; i++) {
			float t = this.BreathTimer.UnitBreathInPct;
			var pos = this.CoreLine.SampleAtUnitLength (Mathf.Repeat( t + toffset, 1.0f));
			this.Particles [i].position = pos;
			this.Particles [i].startColor = this.BreathTimer.IsBreathingIn ? Color.blue : Color.red;
		}

		this.ParRenderer.SetParticles (this.Particles, this.Particles.Length);
	}
}
