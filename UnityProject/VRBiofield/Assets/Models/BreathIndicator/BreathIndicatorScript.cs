using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathIndicatorScript : MonoBehaviour {

	private ExcersizeBreathController Breather;
	private Material MatInst;
	private Transform ChildShape = null;

	// Use this for initialization
	void Start () {
		Breather = GameObject.FindObjectOfType<ExcersizeBreathController> ();
		var mr = this.GetComponent<MeshRenderer> ();
		mr.material.SetFloat ("MakeInstance", 1.0f);
		this.MatInst = mr.material;

		if (this.transform.childCount > 0) {
			ChildShape = this.transform.GetChild (0);
		}
	}
	
	// Update is called once per frame
	void Update () {
		this.MatInst.SetFloat ("_BreathInPct", this.Breather.UnitBreathInPct);
		if (ChildShape) {
			var z = (this.Breather.UnitBreathInPct - 0.5f) * -2.0f * 5.0f;
			ChildShape.localPosition = new Vector3 (0, 0, z);
				
			ChildShape.localRotation = (this.Breather.IsBreathingIn) ?
				Quaternion.AngleAxis(90, Vector3.forward) : Quaternion.AngleAxis(90, Vector3.right);
		}
	}
}
