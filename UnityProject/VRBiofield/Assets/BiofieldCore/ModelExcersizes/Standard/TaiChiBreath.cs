using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaiChiBreath : ExcersizeActivityInst {

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public override void OnStateEnter ()
	{
		this.EnsureSetup ();

	}

	public override void OnStateLeave ()
	{
		base.OnStateLeave ();
		if (mChangedModel) {
			mChangedModel.ParticleFlowRate = 1.0f;
		}

	}

	public override void EnsureSetup ()
	{
		base.EnsureSetup ();
	}

	private Vector3[] CachedRandom = null;
	private DynamicFieldModel mChangedModel = null;

	public override Vector3 CalcVectorField (DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor)
	{
		primaryColor = Color.yellow;
		//var spinePos = this.Body.SpineStart.position;// Vector3.Lerp (this.Body.SpineStart.position, this.Body.SpineEnd.position, 0.3f);
		var spinePos = Vector3.Lerp (this.Body.SpineStart.position, this.Body.SpineEnd.position, 0.51f);
		//return DynamicFieldModel.ChakraFieldV3 (pos, spinePos, Quaternion.identity, false);

		model.ParticleFlowRate = 0.3f;
		mChangedModel = model;

		if (this.Breath.BreathIndex % 2 == 0) {

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
