using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFieldAsParticles : MonoBehaviour {

	private ParticleSystem PartSystem = null;
	private ParticleSystem.Particle[] PartData = null;
	private List<Vector4> PartCustom = null;
	public DynamicFieldModel Model = null;

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

		this.Model.EnsureSetup ();
		this.UpdateFieldParticles (isFirst:true);
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
				p.startSize3D = Vector3.one * 0.1f;
				p.remainingLifetime = 10000.0f;
				p.randomSeed = (uint)i;
			}

			Vector3 fwd = (p.position - camPos);
			Vector3 rght = Vector3.Cross (fwd, Vector3.up);
			Vector3 up = Vector3.Cross (fwd, rght);
			float angle = SignedAngleBetween(up, c.Direction, rght);
			p.axisOfRotation = fwd;
			p.rotation = angle;
			p.startColor = ColorWithAlpha (c.LatestColor, Mathf.Clamp01( c.Direction.magnitude / Model.UnitMagnitude ));
			s = new Vector4 ((float)i, 0, 0, 0);

			this.PartData [i] = p;
			this.PartCustom [i] = s;
		}
		this.PartSystem.SetParticles (this.PartData, this.PartData.Length);
		this.PartSystem.SetCustomParticleData (this.PartCustom, ParticleSystemCustomData.Custom1);
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {

		this.UpdateFieldParticles (true);

		if (true) {
			var cells = this.Model.FieldsCells;
			var cnt = cells.Header.TotalCount;
			for (int i = 0; i < cnt; i++) {
				var c = cells.Array [i];
				var offset = ((c.Direction / Model.UnitMagnitude) * 0.1f);
				var tipScl = offset.magnitude * 0.2f;
				Debug.DrawLine (c.Pos, c.Pos + offset, Color.green);
				Debug.DrawLine (c.Pos, c.Pos + (new Vector3(0,tipScl,0)), Color.green);
			}
		}

	}
}
