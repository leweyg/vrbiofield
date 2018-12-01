using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChiHandEnergyBall : MonoBehaviour {

	public ExcersizeBreathController Breath = null;
	public ChiHandBehavior[] Hands = null;

	// Use this for initialization
	void Start () {
		if (!this.Breath) {
			this.Breath = GameObject.FindObjectOfType<ExcersizeBreathController> ();
		}
		if ((Hands == null) || (Hands.Length == 0)) {
			Hands = this.transform.parent.GetComponentsInChildren<ChiHandBehavior> ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private Vector3[] CachedRandom = null;
	public virtual Vector3 CalcVectorField(DynamicFieldModel model, int posIndex, Vector3 pos, out Color primaryColor) {
		primaryColor = Color.yellow; // GetPrimaryColorFromMeridians (); //Color.yellow;
		//var spinePos = this.Body.SpineStart.position;// Vector3.Lerp (this.Body.SpineStart.position, this.Body.SpineEnd.position, 0.3f);
		var spinePos = this.transform.position;
		//return DynamicFieldModel.ChakraFieldV3 (pos, spinePos, Quaternion.identity, false);

		model.ParticleFlowRate = 0.5f;

		if (true) { //this.Breath.BreathIndex % 2 == 0) {

			model.ParticleFlowRate = 0.5f;

			if ((CachedRandom == null) || (CachedRandom.Length != model.CellCount)) {
				CachedRandom = new Vector3[model.CellCount];
				for (int i = 0; i < model.CellCount; i++) {
					CachedRandom [i] = Random.onUnitSphere;
				}
			}

			var delta = (pos - spinePos);
			var nearestPosOnLine = spinePos;
			var r = (pos - nearestPosOnLine);
			var distScaler = 10.0f;
			var dist = (distScaler / delta.magnitude);
			var inpct = Mathf.Min (dist * 6.0f, (distScaler / r.magnitude));
			var toline = r.normalized * (-inpct);
			var tocenter = (delta).normalized * (-dist);
			var result = (toline + tocenter) * 1.0f;

			if (this.Breath.HeartBeatUnitAlpha < 0.5f) {
				result *= -1.0f;
			}

			//result = Vector3.Lerp(result.normalized, CachedRandom[posIndex], 1.0f - this.Breath.UnitTimeInBreath ) * result.magnitude;

			return result;
		} 
	}
}
