using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeridianController : MonoBehaviour {

	public MeridianPath[] Meridians = null;

	public void EnsureSetup() {
		if (Meridians.Length > 0) {
			return;
		}

		Meridians = this.transform.GetComponentsInChildren<MeridianPath> ();
		Meridians = Meridians.OrderBy (k => k.MeditationOrder).ToArray ();
		// by default turn them all off:
		foreach (var m in Meridians) {
			m.EnsureSetup ();
			m.gameObject.SetActive (false);
		}
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
