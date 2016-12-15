using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ChakraType = VolumeTextureBehavior;

public class ChakraGazeSelector : MonoBehaviour {

	public ChakraMesh Mesher = null;
	public ChakraMesh BackMesher = null;
	public ChakraControl MainControls = null;
	private ChakraType CurrentChakra = null;
	public bool IsShowBreathing = false;
	private Dictionary<ChakraType,Mesh> CachedMeshes = new Dictionary<ChakraType, Mesh>();
	private Dictionary<ChakraType,Mesh> CachedMeshesBack = new Dictionary<ChakraType, Mesh>();
	private Vector3 MesherLocalScaleOrig;

	// Use this for initialization
	void Start () {
		if (MainControls == null) {
			MainControls = GameObject.FindObjectOfType<ChakraControl> ();

		}
		if (Mesher == null) {
			Mesher = GameObject.FindObjectOfType<ChakraMesh> ();
		}
		this.MesherLocalScaleOrig = this.Mesher.transform.localScale;
	}

	private List<ChakraType> CommonChakras = new List<ChakraType>();

	private bool IsCommonChakra(ChakraType c) {
		return ((!c.IsAura) && (!c.IsMultiChakras) && (!c.IsLEWFlowField));
	}

	ChakraType FindClosestChakra() {
		var ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward.normalized);
		var bestDot = -100.0f;
		var bestChakra = this.CurrentChakra;
		foreach (var c in this.MainControls.AllPoints) {
			if (IsCommonChakra(c)) {
				var d = Vector3.Dot (ray.direction, (c.transform.position - ray.origin).normalized);
				if ((d > bestDot) && (d > 0.98)) {
					bestChakra = c;
					bestDot = d;
				}
			}
		}
		return bestChakra;
	}

	
	// Update is called once per frame
	void Update () {
		var cur = this.FindClosestChakra ();
		if (this.IsShowBreathing) {

			if (CommonChakras.Count < 1) {
				foreach (var c in this.MainControls.AllPoints) {
					if (IsCommonChakra (c)) {
						this.CommonChakras.Add (c);
					}
				}
				this.CommonChakras.Sort ((a, b) => (a.transform.position.y > b.transform.position.y) ? 1 : -1);
				int n = this.CommonChakras.Count;
				for (int i = n - 2; i > 0; i--) {
					this.CommonChakras.Add (this.CommonChakras [i]);
				}
			}
			//then
			if (CommonChakras.Count > 0) {
				float timePerBreath = 9.0f;
				float ftime = (Time.time / timePerBreath);
				float timeFrac = ((ftime - ((int)ftime)));
				var curLevel = ((int)ftime) % this.CommonChakras.Count;
				cur = this.CommonChakras [curLevel];


				var alpha = 1.0f - Mathf.Clamp01( Mathf.Abs((timeFrac - 0.5f) * 2.0f) );
				//var alpha = Mathf.Min( Mathf.Min(timeFrac * 4.0f, 1.0f), Mathf.Clamp01(((1.0f-timeFrac) * 4.0f) - 0.25f));
				this.Mesher.SetChakraAlpha (alpha);
				if (this.BackMesher != null) {
					this.BackMesher.SetChakraAlpha (alpha);
				}
			}
		}
		if (cur != this.CurrentChakra) {
			this.CurrentChakra = cur;
			if (cur != null) {
				this.Mesher.transform.position = cur.transform.position;
				this.Mesher.transform.rotation = cur.transform.rotation;
				this.Mesher.SetChakraColor (cur.ChakraColor);
				if (this.BackMesher != null) {
					this.BackMesher.transform.position = cur.transform.position;
					this.BackMesher.transform.rotation = cur.transform.rotation;
					this.BackMesher.transform.Rotate (new Vector3 (180, 0, 0));
					this.BackMesher.SetChakraColor (cur.ChakraColor);
				}

				if (this.CachedMeshes.ContainsKey (cur)) {
					this.Mesher.SetMesh (this.CachedMeshes [cur]);
				} else {
					this.Mesher.MeshController.ResultMesh = null;
					this.Mesher.MeshController.BuildSingleLevel (Mathf.Min (30, cur.ChakraPetals));
					this.CachedMeshes.Add (cur, this.Mesher.MeshController.ResultMesh);
				}
				if (this.BackMesher != null) {
					this.BackMesher.SetMesh (this.CachedMeshes [cur]);
					if (cur.ChakraOneWay) {
						var lc = this.MesherLocalScaleOrig;
						float sc = 0.2f;
						var lcs = new Vector3 (lc.x * sc, lc.y, lc.z * sc);
						this.BackMesher.transform.localScale = lcs;
					} else {
						this.BackMesher.transform.localScale = this.MesherLocalScaleOrig;
					}
				}
			}
		}
	}
}
