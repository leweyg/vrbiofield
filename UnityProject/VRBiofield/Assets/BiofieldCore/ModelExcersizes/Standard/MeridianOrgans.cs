using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeridianOrgans : ExcersizeActivityInst {

	//public MeridianPath ActiveMeridian { get; set; }

	public override void EnsureSetup ()
	{
		base.EnsureSetup ();

	}

	public override void ApplyBodyPositioning ()
	{
		base.ApplyBodyPositioning ();

	}

	public override void OnStateEnter ()
	{
		base.OnStateEnter ();
		this.EnsureSetup ();

		this.Body.EnsureSetup ();
		this.Body.Meridians.EnsureSetup ();
		Debug.Log ("Entering the meridians experience.");
		this.Body.Meridians.gameObject.SetActive (true);
	}

	public override void OnStateLeave ()
	{
		Debug.Log ("Leaving the meridians experience.");
		this.Body.Meridians.gameObject.SetActive (false);
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}

	MeridianPath FindClosestMeridian() {
		var ray = FocusRay.main.CurrentRay;
		var bestDot = -100.0f;
		MeridianPath bestChakra = null;//this.CurrentChakra;
		foreach (var c in this.Body.Meridians.Meridians) {// .AllPoints) {
			if (true) {
				var d = Vector3.Dot (ray.direction, (c.OrganCenterPos - ray.origin).normalized);
				float minAngle = 0.95f; // 0.98f;
				if ((d > bestDot) && (d > minAngle)) {
					bestChakra = c;
					bestDot = d;
				}
			}
		}
		return bestChakra;
	}
	
	// Update is called once per frame
	void Update () {
		var mc = this.Body.Meridians;
		this.Breath.CurrentBreathsPerRep = mc.Meridians.Length;

		foreach (var m in mc.Meridians) {
			var appState = ExcersizeAppState.main.State;
			var ms = appState.GetMeridianState (m.MeridianId);
			var dir = (ms.Direction);
			bool showMer = (dir != 0);
			if (IsInfoAvatar) {
				showMer = (ms.Id == appState.HoverMeridian); // todo
			}
			m.gameObject.SetActive( showMer);
			if (showMer) {
				//ActiveMeridian = m;
				float waveOffset = (dir < 0) ? 0.5f : 0.0f;
				float wave = Mathf.Clamp01( Mathf.Sin((Breath.UnitTimeInBreath - waveOffset)*2.0f*Mathf.PI) );
				float minShow = ((IsInfoAvatar ? 0.5f : 0.1f));
				float adjustedPct = Mathf.Lerp (minShow, 1.0f, Mathf.Pow( wave, 0.5f ) );
				if (IsInfoAvatar) {
					//adjustedPct = 1.0f;
				}
				m.SetMeridianOpacity (adjustedPct, adjustedPct);

			}
		}
	}

	public Color GetPrimaryColorFromMeridians() {
		var app = ExcersizeAppState.main.State;
		Color res = Color.yellow;
		foreach (var m in app.Meridians) {
			if (m.Direction != 0) {
				bool matchBreath = ((m.Direction > 0) == (Breath.IsBreathingIn));
				if (matchBreath) {
					foreach (var mi in this.Body.Meridians.Meridians) {
						if (mi.MeridianId == m.Id) {
							return mi.MeridanColor;
						}
					}
				}
			}
		}
		return res;
	}

	// energy fields:


	private Vector3[] CachedRandom = null;
	private DynamicFieldModel mChangedModel = null;

	public override Vector3 CalcVectorField (DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor)
	{
		primaryColor = Color.yellow; // GetPrimaryColorFromMeridians (); //Color.yellow;
		//var spinePos = this.Body.SpineStart.position;// Vector3.Lerp (this.Body.SpineStart.position, this.Body.SpineEnd.position, 0.3f);
		var spinePos = Vector3.Lerp (this.Body.SpineStart.position, this.Body.SpineEnd.position, 0.51f);
		//return DynamicFieldModel.ChakraFieldV3 (pos, spinePos, Quaternion.identity, false);

		model.ParticleFlowRate = 0.3f;
		mChangedModel = model;

		if (true) { //this.Breath.BreathIndex % 2 == 0) {

			model.ParticleFlowRate = 0.3f;

			if ((CachedRandom == null) || (CachedRandom.Length != model.CellCount)) {
				CachedRandom = new Vector3[model.CellCount];
				for (int i = 0; i < model.CellCount; i++) {
					CachedRandom [i] = Random.onUnitSphere;
				}
			}

			var delta = (pos - spinePos);
			var nearestPosOnLine = spinePos;
			var r = (pos - nearestPosOnLine);
			var distScaler = 5.0f;
			var dist = (distScaler / delta.magnitude);
			var inpct = Mathf.Min (dist * 6.0f, (distScaler / r.magnitude));
			var toline = r.normalized * (-inpct);
			var tocenter = (delta).normalized * (-dist);
			var result = (toline + tocenter) * 1.0f;

			if (!this.Breath.IsBreathingIn) {
				result = result.magnitude * Vector3.down;
			}

			//result = Vector3.Lerp(result.normalized, CachedRandom[posIndex], 1.0f - this.Breath.UnitTimeInBreath ) * result.magnitude;

			return result;
		} else {

			model.ParticleFlowRate = 0.6f;

			var delta = (pos - spinePos);
			var nearestPosOnLine = spinePos;
			var r = (pos - nearestPosOnLine);
			var distScaler = 5.0f;
			var dist = (distScaler / delta.magnitude);
			var inpct = Mathf.Min (dist * 6.0f, (distScaler / r.magnitude));
			var toline = r.normalized * (-inpct);
			var tocenter = (delta).normalized * (-dist);
			var result = (toline + tocenter) * 1.0f;

			var upways = Mathf.Lerp (0.2f, 1.0f, Mathf.Abs (Vector3.Dot (delta.normalized, Vector3.up)));
			result *= upways;

			//result = Vector3.Lerp(result.normalized, CachedRandom[posIndex], 1.0f - this.Breath.UnitTimeInBreath ) * result.magnitude;

			return result;		}

	}
}
