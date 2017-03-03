using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbacusRail : MonoBehaviour {

	public float NumberToShow { get; set; }
	public int BeadCount { get; set; }
	public bool FlipDirection { get; set; }
	public bool ForceTextDisplay { get; set; }
	public float TextDisplayUnitScalar { get; set; }

	private TextMesh MyTextDisplay;

	public void SetBeadCountAndNumber(int c, float f) {
		this.BeadCount = c;
		this.NumberToShow = f;
		this.UpdateBeads ();
	}
	public void SetBeadCountAndNumberWrapped(int c, float f) {
		var cf = (float)c;
		var v = f - (Mathf.Floor (f / cf) * cf);
		this.SetBeadCountAndNumber (c, v);
	}
	public void SetBeadCountAndNumberABA(int c, float f) {
		var cf = ((float)c);
		float floorF = (Mathf.Floor (f / cf) * cf);
		var v = f - floorF;
		this.FlipDirection = ((((int)(floorF / cf)) % 2) == 1);
		this.SetBeadCountAndNumber (c, v);
	}

	private List<AbacusBead> MyBeads = new List<AbacusBead> ();
	private Vector3 PosLow, PosHigh;
	private Abacus AbacusDefaults;
	public void EnsureSetup(Abacus ab) {
		if (this.AbacusDefaults == ab) {
			return;
		}

		this.AbacusDefaults = ab;
		ab.EnsureSetup ();

		PosLow = ab.DefaultLeft.transform.position;
		PosHigh = ab.DefaultRight.transform.position;
		PosLow.y = this.transform.position.y;
		PosHigh.y = this.transform.position.y;

		this.MyTextDisplay = this.GetComponentInChildren<TextMesh> ();
		this.TextDisplayUnitScalar = 1.0f;

		this.SetBeadCountAndNumber (0, 0);
		this.UpdateBeads ();
	}

	// Use this for initialization
	void Start () {
		if (this.AbacusDefaults == null) {
			var ab = this.gameObject.GetComponentInParent<Abacus> ();
			if (ab != null) {
				this.EnsureSetup (ab);
			}
		}
	}

	void UpdateBeads() {
		for (int bi = 0; bi < BeadCount; bi++) {
			if (bi >= this.MyBeads.Count) {
				var db = this.AbacusDefaults.DefaultBead;
				var nb = GameObject.Instantiate (db);
				this.MyBeads.Add (nb);
				nb.transform.parent = db.transform.parent;
				nb.transform.localScale = db.transform.localScale;
			}
			var cb = this.MyBeads [bi];
			cb.gameObject.SetActive (true);

			// now place the bead:
			var br = this.AbacusDefaults.BeadRadius;
			var h = (FlipDirection ? this.PosLow : this.PosHigh);
			var l = (FlipDirection ? this.PosHigh : this.PosLow);
			var dirPos = (h - l).normalized * ( br * 2.0f);
			var posLeft = h - (dirPos * (((float)bi) + 0.5f));
			var posRight = l + (dirPos * (((float)(this.BeadCount - bi)) - 0.5f));
			float pf;
			var bif = (float)bi;
			if (bi == ((int)this.NumberToShow)) {
				pf = 1.0f - (this.NumberToShow - Mathf.Floor (this.NumberToShow));
			} else if (this.NumberToShow < bi) {
				pf = 1.0f;
			} else {
				pf = 0.0f;
			}
			cb.transform.position = Vector3.Lerp(posLeft, posRight, pf);
		}
		for (int bi = BeadCount; bi < this.MyBeads.Count; bi++) {
			// hide unused beads:
			this.MyBeads [bi].gameObject.SetActive (false);
		}

		// if showing, update the text display:
		if (this.MyTextDisplay != null) {
			if (this.AbacusDefaults.UseDebugText || this.ForceTextDisplay) {
				this.MyTextDisplay.text = "" + this.BeadCount + "@" + this.NumberToShow;
				this.MyTextDisplay.gameObject.SetActive (true);
			} else {
				this.MyTextDisplay.gameObject.SetActive (false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		//this.UpdateBeads ();
	}
}
