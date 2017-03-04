using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FlowMeshAroundLine : MonoBehaviour {

	private FlowMeshBuilder Builder;
	private LinesThroughPoints Lines;
	private MeshFilter MyMesh;

	// Use this for initialization
	void Start () {
		this.MyMesh = this.GetComponent<MeshFilter> ();
	}

	void EnsureBuilder() {
		if (this.Builder == null) {
			this.Builder = new FlowMeshBuilder (this);
		}
	}

	public void SetupLine(LinesThroughPoints ln) {
		this.EnsureBuilder ();
		this.Builder.BuildForLine (ln);
		this.MyMesh.mesh = this.Builder.Result;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public class FlowMeshBuilder 
	{
		private FlowMeshAroundLine Owner;
		public LinesThroughPoints CurrentLine;
		public Mesh Result;
		public int AroundCount = 18;
		public int AlongCount = 10;
		public float FlowThickness = 0.07f;
		private List<FMVertex> Vertices = new List<FMVertex> ();
		private List<int> Indices = new List<int>();

		public FlowMeshBuilder(FlowMeshAroundLine own) {
			this.Owner = own;
		}

		public struct FMVertex {
			public Vector3 Position;
			public Vector3 Normal;
			public Vector2 Texcoord;
			public FMVertex(Vector3 p, Vector3 n, Vector2 uv) { Position = p; Normal = n; Texcoord = uv; }
		};

		public Ray GetRayAlongLine(float alongPct) {
			Vector3 p = this.CurrentLine.SampleAtUnitLength (alongPct);
			Vector3 n = this.CurrentLine.NormalAtUnitLength (alongPct);
			return new Ray (p, n);
		}

		private FMVertex GenerateVertex(float alongPct, float aroundPct, bool isTip) {
			var r = this.GetRayAlongLine (alongPct);
			var t = Vector3.Cross (Vector3.up, r.direction).normalized; // tangent direction
			var q = Quaternion.AngleAxis (aroundPct * 360, r.direction);
			var d = Matrix4x4.TRS (Vector3.zero, q, Vector3.one).MultiplyVector (t).normalized; // apply the rotation
			var dp = r.origin + (isTip ? Vector3.zero : (d * this.FlowThickness));
			Debug.DrawLine (r.origin, dp);
			return new FMVertex (
				dp, d, new Vector2(alongPct, aroundPct));
		}

		public void BuildForLine(LinesThroughPoints ln) {
			this.CurrentLine = ln;
			for (int si = 0; si < AlongCount; si++) {
				for (int ai = 0; ai < AroundCount; ai++) {
					float sf = ((float)si) / ((float)(AlongCount - 1));
					float af = ((float)ai) / ((float)(AroundCount - 1));
					var v = GenerateVertex (sf, af, ((si==0)||((si+1)==AlongCount)) );
					this.Vertices.Add (v);

					if (si > 0) {
						var i0 = ((si - 1) * AroundCount) + (ai);
						var i1 = ((si - 1) * AroundCount) + ((ai + 1) % AroundCount);
						var i2 = ((si - 0) * AroundCount) + (ai);
						var i3 = ((si - 0) * AroundCount) + ((ai + 1) % AroundCount);
						var tris = new int[]{ i0, i1, i3, i3, i2, i0 };
						this.Indices.AddRange (tris);
					}
				}
			}

			this.Result = new Mesh ();
			this.Result.vertices = this.Vertices.Select (k => k.Position).ToArray ();
			this.Result.normals = this.Vertices.Select (k => k.Normal).ToArray ();
			this.Result.uv = this.Vertices.Select (k => k.Texcoord).ToArray ();
			this.Result.triangles = this.Indices.ToArray ();
			this.Result.UploadMeshData (false);
		}
	}
}
