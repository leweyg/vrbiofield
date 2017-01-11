using UnityEngine;
using System.Collections;

public class FieldDebugLineScript : MonoBehaviour {

	public ElectromagneticFieldControllerScript Controller;
	public int CubeSize = 4;
	public bool UseDebugLine = true;
	public bool UseDebugMesh = true;
	public bool UpdateEveryFrame = true;
	private VolumeBuffer<Vector3> FieldValues;// = new VolumeBuffer<Vector3> (new Cubic<int> (8, 8, 8));
	private Mesh SurfaceMesh;
	private Vector3[] MeshVertices;
	private Vector3[] MeshNormals;
	public float SharedScale = 0.15f;
	public float FrameMaxField = 0.15f;
	public float GoalFieldMag = 0.0000749632f; //0.0002349632f; 
	public bool IsNormalizeVectors = false;

	// Use this for initialization
	void Start () {
		if (this.Controller==null) {
			this.Controller = GameObject.FindObjectOfType<ElectromagneticFieldControllerScript> ();
		}

		FieldValues = new VolumeBuffer<Vector3> (Cubic<int>.CreateSame (CubeSize));

		if (this.UseDebugMesh) {
			var mf = this.GetComponent<MeshFilter> ();
			if (mf == null) {
				this.UseDebugMesh = false;
				Debug.Assert (mf != null, "Please setup a MeshFilter and MeshRenderer on this object");
			}
			SurfaceMesh = new Mesh ();
			MeshVertices = new Vector3[FieldValues.Length * 3];
			MeshNormals = new Vector3[FieldValues.Length * 3];
			mf.mesh = SurfaceMesh;
		}
	}

	Vector3 DrawFieldAtPoint(int vi, Vector3 pos, int depth=0, bool isFirst = true) {
		var rawmf = this.Controller.MagneticField (pos);
		var mf = rawmf.normalized * Mathf.Pow(rawmf.magnitude / FrameMaxField, 0.2f); // rawmf.normalized;
		var cr = ((rawmf.magnitude > (FrameMaxField*0.25f)) ? Color.red : Color.green);
		if (IsNormalizeVectors) {
			mf = rawmf.normalized;
		}
		float scale = this.SharedScale;
		var endPos = pos + (mf * scale);
		if (this.UseDebugLine) {
			Debug.DrawLine (pos, endPos, cr);
			if (depth > 0) {
				//DrawFieldAtPoint (endPos, depth - 1, false);
			}
		}
		if (isFirst) {
			Vector3 curl, ddx, ddy, ddz;
			this.Controller.MagneticFieldDerivatives (pos, out curl, out ddx, out ddy, out ddz);
			Vector3 dmf = (ddx * mf.x) + (ddy * mf.y) + (ddz * mf.z);
			Vector3 sideDir = Vector3.Cross (dmf.normalized, mf.normalized);
			Vector3 upDir = Vector3.Cross (sideDir, mf.normalized).normalized;

			Vector3 sidePos = pos + (sideDir.normalized * scale * 0.25f);
			Vector3 upPos = pos + (upDir.normalized * scale * 0.25f);
			Vector3 anglePos = pos + ((upDir + sideDir).normalized * scale * 0.25f);
			if (this.UseDebugLine) {
				Debug.DrawLine (pos, anglePos, cr);
			}

			if (this.UseDebugMesh) {
				if (vi >= 0) {
					var ti = vi * 3;
					var w2l = this.transform.worldToLocalMatrix;
					var nl = w2l.MultiplyVector (upDir).normalized;
					this.MeshVertices [ti + 0] = w2l.MultiplyPoint (pos);
					this.MeshVertices [ti + 1] = w2l.MultiplyPoint (anglePos);
					this.MeshVertices [ti + 2] = w2l.MultiplyPoint (endPos);
					this.MeshNormals [ti + 0] = nl;
					this.MeshNormals [ti + 1] = nl;
					this.MeshNormals [ti + 2] = nl;
				}
			}

			//if (depth > 0) {
			//	var curMag = rawmf.magnitude;
			//	var dgoal = (GoalFieldMag - curMag);
			//	var npos = pos + (upDir.normalized * scale * (dgoal * 5000.0f));
			//	DrawFieldAtPoint (-1, npos, depth - 1, false);
			//}
		}
		return rawmf;
	}
	
	// Update is called once per frame
	void Update () {
		if ((!UpdateEveryFrame) && (Time.frameCount != 2))
			return;

		DrawFieldAtPoint (-1, this.transform.position);
		float nextFrameMax = 0.0f;
		var l2w = this.transform.localToWorldMatrix;
		{
			for (int vi = 0; vi < this.FieldValues.Header.TotalCount; vi++) {
				var i3 = this.FieldValues.Header.LinearToCubic (vi);
				var f3 = i3.AsVector3() / ((float)this.FieldValues.Header.Size.X);
				f3 -= (Vector3.one * 0.5f);
				var wpos = l2w.MultiplyPoint (f3);
				var mfield = DrawFieldAtPoint (vi, wpos, 2);
				this.FieldValues.Array [vi] = mfield;
				var mfieldMag = mfield.magnitude;
				nextFrameMax = Mathf.Max (nextFrameMax, mfieldMag);
			}
		}
		this.FrameMaxField = nextFrameMax;

		if (this.UseDebugMesh) {
			this.SurfaceMesh.vertices = this.MeshVertices;
			this.SurfaceMesh.normals = this.MeshNormals;
			if (!HasSetIndices) {
				HasSetIndices = true;
				var nv = FieldValues.Length * 3;
				int[] indices = new int[nv * 2];
				for (int i = 0; i < nv; i++)
					indices [i] = i;
				for (int i = 0; i < nv; i++)
					indices [(nv) + i] = ((i / 3) * 3) + (((i % 3) == 2) ? 2 : (1 - (i % 3)));
				SurfaceMesh.triangles = indices;

				var uvs = new Vector2[nv];
				for (int i = 0; i < nv; i++) {
					Vector2 uv;
					switch ((i % 3)) {
					case 0:
					default:
						uv = new Vector2 (1, 0);
						break;
					case 1:
						uv = new Vector2 (0, 1);
						break;
					case 2:
						uv = new Vector2 (0, 0);
						break;
					}
					uvs [i] = uv;
				}
				SurfaceMesh.uv = uvs;
				this.gameObject.GetComponent<MeshRenderer> ().enabled = true;
			}
			this.SurfaceMesh.UploadMeshData (false);
		}
	}
	private bool HasSetIndices = false;

	public class DebugVectorLine {
		public Vector3[] points3;

		public DebugVectorLine(string name, Vector3[] vecs) {
			points3 = vecs;
		}

		public void Draw3D() {
			for (int i = 1; i < points3.Length; i++) {
				Debug.DrawLine (points3 [i - 1], points3 [i]);
			}
		}
	}
}
