using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeridianOrgans : ExcersizeActivityInst {

	public override void EnsureSetup ()
	{
		base.EnsureSetup ();

	}

	public override void ApplyBodyPositioning ()
	{
		base.ApplyBodyPositioning ();

	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		this.Breath.CurrentBreathsPerRep = 10;
		var mc = this.Body.Meridians;
		foreach (var m in mc.Meridians) {
			bool showMer = ( (m.MeditationOrder-1) == (this.Breath.BreathIndex % mc.Meridians.Length));
			m.gameObject.SetActive( showMer);
			if (showMer) {
				m.SetMeridianOpacity (Breath.UnitFadeInPct, Breath.UnitFadeInPct);
			}
		}
	}
}
