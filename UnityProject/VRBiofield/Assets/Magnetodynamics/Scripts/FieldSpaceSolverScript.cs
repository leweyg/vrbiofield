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

	void AddFieldAtPoint(Vector3 pos) {
		Matrix4x4 data;
		Controller.MagneticFieldLocalSpaceX (pos, out data, isNormalized: true);
		Debug.DrawLine (data.MultiplyPoint (Vector3.zero), data.MultiplyPoint (new Vector3 (StepSize, 0, 0)), Color.black);
		Debug.DrawLine (data.MultiplyPoint (Vector3.zero), data.MultiplyPoint (new Vector3 (0, StepSize, 0)), Color.black);
		Debug.DrawLine (data.MultiplyPoint (Vector3.zero), data.MultiplyPoint (new Vector3 (0, 0, StepSize)), Color.black);
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
		}
	}
	
	// Update is called once per frame
	void UpdateSpace() {
		ActivePoints.Clear ();
		NextPoints.Clear ();
		LineList.Clear ();

		Matrix4x4 startPoint;
		Controller.MagneticFieldLocalSpaceX (this.transform.position, out startPoint, isNormalized: true);
		NextPoints.Enqueue (startPoint);
		float startIntensity = Controller.MagneticField (this.transform.position).magnitude;


		while ((ActivePoints.Count < MaxCellPoints) && (NextPoints.Count > 0)) {
			var curMtx = NextPoints.Dequeue ();
			var curPnt = curMtx.MultiplyPoint (Vector3.zero);
			bool isTooClose = false;
			LineNeighbors.Clear ();

			var curStepSize = StepSize;
			if (ScaleFromStartIntensity) {
				var curIntensity = Controller.MagneticField (curPnt).magnitude;
				curStepSize = StepSize * (startIntensity / curIntensity);
			}

			foreach (var otherMtx in ActivePoints) {
				var otherPnt = otherMtx.MultiplyPoint (Vector3.zero);
				var dist = ((curPnt - otherPnt).magnitude) / curStepSize;
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
					Debug.DrawLine (curPnt, npos, Color.black);
					LineList.Add (new KeyValuePair<Vector3, Vector3> (curPnt, npos));
				}

				Matrix4x4 actualMtx;
				this.Controller.MagneticFieldLocalSpaceX (curPnt, out actualMtx, isNormalized: true);

				NextPoints.Enqueue (NeighborMatrix(actualMtx, new Vector3 (curStepSize, 0, 0)));
				NextPoints.Enqueue (NeighborMatrix(actualMtx, new Vector3 (0, curStepSize, 0)));
				NextPoints.Enqueue (NeighborMatrix(actualMtx, new Vector3 (0, 0, curStepSize)));
				NextPoints.Enqueue (NeighborMatrix(actualMtx, new Vector3 (-curStepSize, 0, 0)));
				NextPoints.Enqueue (NeighborMatrix(actualMtx, new Vector3 (0, -curStepSize, 0)));
				NextPoints.Enqueue (NeighborMatrix(actualMtx, new Vector3 (0, 0, -curStepSize)));

			}
		}
	}
}
