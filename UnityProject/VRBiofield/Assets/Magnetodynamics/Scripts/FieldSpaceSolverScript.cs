using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldSpaceSolverScript : MonoBehaviour {

	public ElectromagneticFieldControllerScript Controller;
	public float StepSize = 0.2f;
	public int MaxCellPoints = 80;
	public bool UpdateOnlyWhenRequested = false;
	public bool UpdateNow = true;
	public float InnerRadiusScaler = 0.9f;
	public float OuterRadiusScaler = 1.1f;
	public bool ScaleFromStartIntensity = false;
	private List<Matrix4x4> ActivePoints = new List<Matrix4x4>();
	private Queue<Matrix4x4> NextPoints = new Queue<Matrix4x4>();
	private List<Vector3> LineNeighbors = new List<Vector3> ();
	private List<KeyValuePair<Vector3,Vector3>> LineList = new List<KeyValuePair<Vector3,Vector3>>();

	// Use this for initialization
	void Start () {
		if (this.Controller==null) {
			this.Controller = GameObject.FindObjectOfType<ElectromagneticFieldControllerScript> ();
		}


	}

	Matrix4x4 NeighborMatrix(Matrix4x4 space, Vector3 v) {
		// ideally: space.translation += v;
		return Matrix4x4.TRS(space.MultiplyPoint(v), Quaternion.identity, Vector3.one);
	}


	void Update() {
		if ((!UpdateOnlyWhenRequested) || (UpdateNow)) {
			UpdateNow = false;
			this.UpdateSpace ();
		} else {
			foreach (var kv in this.LineList) {
				Debug.DrawLine (kv.Key, kv.Value, Color.black);
			}
			foreach (var m in this.ActivePoints) {
				Debug.DrawLine (m.MultiplyPoint (Vector3.zero), m.MultiplyPoint (Vector3.forward));
			}
		}
	}

	private float StartingPointIntensity = 1.0f;
	void EnqueNeighbor(Matrix4x4 fromSpace, Vector3 neighOffset) {
		var pnt = fromSpace.MultiplyPoint (neighOffset);
		//Debug.DrawLine (fromSpace.MultiplyPoint (Vector3.zero), pnt, Color.red);
		var curStepSize = this.StepSize;
		if (this.ScaleFromStartIntensity) {
			float pntIntensity = Controller.MagneticField (this.transform.position).magnitude;
			curStepSize = (this.StepSize * (StartingPointIntensity / pntIntensity));
		}
		Matrix4x4 mat;
		this.Controller.MagneticFieldLocalSpaceX (pnt, out mat, true, curStepSize);
		NextPoints.Enqueue (mat);
	}

	// Update is called once per frame
	void UpdateSpace() {
		ActivePoints.Clear ();
		NextPoints.Clear ();
		LineList.Clear ();

		Matrix4x4 startPoint;
		Controller.MagneticFieldLocalSpaceX (this.transform.position, out startPoint, 
			isNormalized: true,normScale: StepSize);
		NextPoints.Enqueue (startPoint);
		float startIntensity = Controller.MagneticField (this.transform.position).magnitude;
		StartingPointIntensity = startIntensity;


		while ((ActivePoints.Count < MaxCellPoints) && (NextPoints.Count > 0)) {
			var curMtx = NextPoints.Dequeue ();
			var curPnt = curMtx.MultiplyPoint (Vector3.zero);
			bool isTooClose = false;
			LineNeighbors.Clear ();

			foreach (var otherMtx in ActivePoints) {

				var otherPnt = otherMtx.MultiplyPoint (Vector3.zero);
				var dist1 = otherMtx.inverse.MultiplyPoint (curPnt).magnitude;
				var dist2 = curMtx.inverse.MultiplyPoint (otherPnt).magnitude;
				var dist = Mathf.Min (dist1, dist2);
				//var otherPnt = otherMtx.MultiplyPoint (Vector3.zero);
				//var dist = ((curPnt - otherPnt).magnitude) / curStepSize;

				if (dist < InnerRadiusScaler) {
					isTooClose = true;
				}
				else if ((dist >= InnerRadiusScaler) && (dist < OuterRadiusScaler)) {
					LineNeighbors.Add (otherPnt);
				}
			}
			if (!isTooClose) {

				this.ActivePoints.Add (curMtx);

				foreach (var npos in LineNeighbors) {
					//Debug.DrawLine (curPnt, npos, Color.black);
					LineList.Add (new KeyValuePair<Vector3, Vector3> (curPnt, npos));
				}

				Matrix4x4 actualMtx = curMtx;
				//this.Controller.MagneticFieldLocalSpaceX (curPnt, out actualMtx, isNormalized: true);

				var curStepSize = 1.0f;
				EnqueNeighbor(actualMtx, new Vector3 (curStepSize, 0, 0));
				EnqueNeighbor(actualMtx, new Vector3 (0, curStepSize, 0));
				EnqueNeighbor(actualMtx, new Vector3 (0, 0, curStepSize));
				EnqueNeighbor(actualMtx, new Vector3 (-curStepSize, 0, 0));
				EnqueNeighbor(actualMtx, new Vector3 (0, -curStepSize, 0));
				EnqueNeighbor(actualMtx, new Vector3 (0, 0, -curStepSize));

			}
		}

		Debug.Log ("Added " + this.LineList.Count + " lines with " + this.NextPoints.Count + " extra points");
	}
}
