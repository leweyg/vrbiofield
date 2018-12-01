using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abacus : MonoBehaviour {

	public bool RunTests { get { return false; } }
	public bool UseDebugText = false;

	public AbacusRail DefaultRail;
	public AbacusRailEnd DefaultLeft, DefaultRight;
	public AbacusBead DefaultBead;
	public AbacusBead DefaultBeadForRadius;
	public List<AbacusRail> AllRails;

	public float BeadRadius { get; private set; }


	private bool isSetup = false;
	public void EnsureSetup() {
		if (isSetup) {
			return;
		}
		this.BeadRadius = (this.DefaultBead.transform.position - this.DefaultBeadForRadius.transform.position).magnitude;

		DefaultBead.gameObject.SetActive (false);
		DefaultBeadForRadius.gameObject.SetActive (false);

		foreach (var br in this.AllRails) {
			br.EnsureSetup (this);
		}
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {


		if (this.RunTests) {
			float ut = Time.time / (4.0f * 2.0f);
			float bt = 1.0f - ((Mathf.Cos (ut  * Mathf.PI * 2.0f) * 0.5f) + 0.5f);
				
			//this.AllRails [0].SetBeadCountAndNumber (1, 0.5f);
			this.AllRails [1].SetBeadCountAndNumber (1, bt);
			this.AllRails [2].SetBeadCountAndNumberABA (8, ut);
			this.AllRails [3].SetBeadCountAndNumberWrapped (6, ut / 8.0f);
		}
	}
}
