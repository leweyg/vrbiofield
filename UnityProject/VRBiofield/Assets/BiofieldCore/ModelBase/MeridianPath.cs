using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeridianPath : MonoBehaviour {

	public string MeridianName;
	public EMeridian MeridianId = EMeridian.Unknown;
	public Color MeridanColor = Color.white;
	public int MeditationOrder = 0;
	public GameObject Organ;
	public GameObject MeridianLine;
	private bool mIsSetup = false;
	public Material MatInstOrgan { get; set; }
	public Material MatInstLine { get; set; }
	public Vector3 OrganCenterPos { get; set; }


	public enum EMeridian
	{
		Unknown,
		Lung,
		Kidney,
		Liver,
		Heart,
		Spleen,
	}

	public void SetMeridianOpacity(float organ_alpha, float line_alpha) {
		string name = "_CustomAlpha";
		MatInstOrgan.SetFloat (name, organ_alpha);
		MatInstLine.SetFloat (name, line_alpha);
		MatInstLine.SetFloat ("_PulsationPct", 1.0f);
	}

	public void EnsureSetup() {
		if (mIsSetup)
			return;
		mIsSetup = true;

		this.OrganCenterPos = this.transform.position; // just to start with, fixed later

		for (int ci = 0; ci < this.transform.childCount; ci++) {
			var ob = (this.transform.GetChild (ci));
			var isMer = (ob.name.Contains ("mer"));
			if (isMer) {
				if (this.MeridianLine == null) {
					this.MeridianLine = ob.gameObject;
					this.MatInstLine = GetMaterialInst (this.MeridianLine);
				}
			} else {
				if (this.Organ == null) {
					this.Organ = ob.gameObject;
					this.MatInstOrgan = GetMaterialInst (this.Organ);

					var mf = this.Organ.GetComponent<Renderer> ();
					if (mf) {
						// actually use bounds
						this.OrganCenterPos = mf.bounds.center;
					}
				}
			}
		}
	}

	private Material GetMaterialInst(GameObject ob) {
		var rnd = ob.GetComponent<Renderer> ();
		if (rnd) {
			rnd.material.SetFloat ("_CustomAlpha", 1.0f);
			return rnd.material;
		}
		return null;
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}

}
