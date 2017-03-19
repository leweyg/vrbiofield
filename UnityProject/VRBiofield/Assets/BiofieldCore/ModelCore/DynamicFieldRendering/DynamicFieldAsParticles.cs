using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicFieldAsParticles : MonoBehaviour {

	private ParticleSystem PartSystem = null;
	private ParticleSystem.Particle[] PartData = null;
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

	void UpdateFieldParticles(bool isFirst = false) {
		var cells = this.Model.FieldsCells;
		int count = cells.Header.TotalCount;
		var camPos = Camera.main.transform.position;
		this.PartData = new ParticleSystem.Particle[count];
		for (int i = 0; i < count; i++) {
			var p = this.PartData [i];
			var c = cells.Array [i];

			if (isFirst) {
				p.position = c.Pos;
				p.color = Color.green;
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

			this.PartData [i] = p;
		}
		this.PartSystem.SetParticles (this.PartData, this.PartData.Length);
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {

		this.UpdateFieldParticles (true);

		//var cells = this.Model.FieldsCells;
		//var cnt = cells.Header.TotalCount;
		//for (int i = 0; i < cnt; i++) {
		//	var c = cells.Array [i];
		//	Debug.DrawLine (c.Pos, c.Pos + (c.Direction * 0.1f), Color.green);
		//}
	}
}
