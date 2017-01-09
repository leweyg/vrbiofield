using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using ChakraType = VolumeTextureBehavior;
using ChakraType = ChakraPosition;

public class ChakraGazeSelector : MonoBehaviour {

	public ChakraMesh Mesher = null;
	public ChakraMesh BackMesher = null;
	public ChakraControl MainControls = null;
	private ChakraType CurrentChakra = null;
	public bool IsShowBreathing = false;
	public bool ForceFull = false;
	private Dictionary<ChakraType,Mesh> CachedMeshes = new Dictionary<ChakraType, Mesh>();
	private Dictionary<ChakraType,Mesh> CachedMeshesBack = new Dictionary<ChakraType, Mesh>();

	// Use this for initialization
	void Start () {
		if (MainControls == null) {
			MainControls = GameObject.FindObjectOfType<ChakraControl> ();

		}
		if (Mesher == null) {
			Mesher = GameObject.FindObjectOfType<ChakraMesh> ();
		}
	}

	private List<ChakraType> CommonChakras = new List<ChakraType>();

	private bool IsCommonChakra(ChakraType c) {
		return true;
		//return ((!c.IsAura) && (!c.IsMultiChakras) && (!c.IsLEWFlowField));
	}

	ChakraType FindClosestChakra() {
		var ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward.normalized);
		var bestDot = -100.0f;
		ChakraType bestChakra = null;//this.CurrentChakra;
		foreach (var c in this.MainControls.AllChakras) {// .AllPoints) {
			if (IsCommonChakra(c)) {
				var d = Vector3.Dot (ray.direction, (c.transform.position - ray.origin).normalized);
				float minAngle = 0.95f; // 0.98f;
				if ((d > bestDot) && (d > minAngle)) {
					bestChakra = c;
					bestDot = d;
				}
			}
		}
		return bestChakra;
	}

	float CurrentBreathAlpha(ref ChakraType cur) {
		if (CommonChakras.Count < 1) {
			foreach (var c in this.MainControls.AllChakras) {
				if (IsCommonChakra (c)) {
					this.CommonChakras.Add (c);
				}
			}
			this.CommonChakras.Sort ((a, b) => (a.transform.position.y > b.transform.position.y) ? 1 : -1);
			int n = this.CommonChakras.Count;
			for (int i = n - 1; i >= 0; i--) {
				this.CommonChakras.Add (this.CommonChakras [i]);
			}
		}
		//then
		if (CommonChakras.Count > 0) {
			float timePerBreath = 9.0f;
			float ftime = (Time.time / timePerBreath);
			float timeFrac = ((ftime - ((int)ftime)));
			var curLevel = ((int)ftime) % this.CommonChakras.Count;
			if (this.IsShowBreathing) {
				cur = this.CommonChakras [curLevel];
			}


			var alpha = 1.0f - Mathf.Clamp01 (Mathf.Abs ((timeFrac - 0.5f) * 2.0f));
			return alpha;
		}

		// else
		return 1.0f;
	}

	
	// Update is called once per frame
	void Update () {
		var cur = this.FindClosestChakra ();

		// update breath:
		var breathAlpha = this.CurrentBreathAlpha (ref cur);

		// update mesh:
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
					this.Mesher.MeshController.CurrentSpinOpposite = cur.ChakraOneWay;
					this.Mesher.MeshController.BuildSingleLevel (Mathf.Min (33, cur.ChakraPetals));
					this.CachedMeshes.Add (cur, this.Mesher.MeshController.ResultMesh);
				}
				this.Mesher.SpinOpposite = cur.ChakraOneWay;
				if (this.BackMesher != null) {
					this.BackMesher.SetMesh (this.CachedMeshes [cur]);
					this.BackMesher.SpinOpposite = cur.ChakraOneWay;
					if (cur.ChakraOneWay) {
						var lc = this.BackMesher.InitialLocalScale;
						float sc = 0.2f;
						var lcs = new Vector3 (lc.x * sc, lc.y * 2.0f, lc.z * sc);
						this.BackMesher.CurrentLocalScaleBase = lcs;
					} else {
						this.BackMesher.CurrentLocalScaleBase = this.BackMesher.InitialLocalScale;
					}
					this.BackMesher.transform.localScale = this.BackMesher.CurrentLocalScaleBase;
				}
			} 
		}

		// use breath state:
		if ((cur != null) || (this.IsShowBreathing)) {
			var alpha = breathAlpha;
			var sclAlpha = Mathf.Lerp (IsShowBreathing ? 0.61f : 0.71f, 1.0f, alpha);
			var displayAlpha = Mathf.Lerp (IsShowBreathing ? 0.0f : 0.3f, 1.0f, alpha);
			if (ForceFull) {
				sclAlpha = 1.0f;
				displayAlpha = 1.0f;
			}
			this.Mesher.SetChakraAlpha (displayAlpha);
			this.Mesher.transform.localScale = this.Mesher.CurrentLocalScaleBase * sclAlpha;
			if (this.BackMesher != null) {
				this.BackMesher.SetChakraAlpha (displayAlpha);
				this.BackMesher.transform.localScale = this.BackMesher.CurrentLocalScaleBase * sclAlpha;
			}
		} else {
			this.Mesher.SetChakraAlpha (0.0f);
			this.BackMesher.SetChakraAlpha (0.0f);
		}
	}
}
