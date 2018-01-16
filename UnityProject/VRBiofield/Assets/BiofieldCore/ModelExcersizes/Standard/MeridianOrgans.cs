﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeridianOrgans : ExcersizeActivityInst {

	public MeridianPath ActiveMeridian { get; set; }

	public override void EnsureSetup ()
	{
		base.EnsureSetup ();

	}

	public override void ApplyBodyPositioning ()
	{
		base.ApplyBodyPositioning ();

	}

	public override Vector3 CalcVectorField (DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor)
	{
		if (ActiveMeridian) {
			primaryColor = Color.Lerp (ActiveMeridian.MeridanColor, Color.white, 0.5f);
			return DynamicFieldModel.ChakraFieldV3 (pos, ActiveMeridian.OrganCenterPos, Quaternion.identity, false);
		}
		return base.CalcVectorField (model, posIndex, pos, out primaryColor);
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		var mc = this.Body.Meridians;
		this.Breath.CurrentBreathsPerRep = mc.Meridians.Length;
		foreach (var m in mc.Meridians) {
			bool showMer = ( (m.MeditationOrder-1) == ((this.Breath.BreathIndex) % mc.Meridians.Length));
			m.gameObject.SetActive( showMer);
			if (showMer) {
				ActiveMeridian = m;
				m.SetMeridianOpacity (Breath.UnitFadeInPct, Breath.UnitBreathInPct);//UnitFadeInPct);
			}
		}
	}
}
