using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnergyMeshGenerator : MonoBehaviour {

	public bool DoGenerate = false;
	public bool FixComponents = false;
	public Material MaterialForLines = null;

	private List<MeshTri> MyTris = new List<MeshTri>();

	public class MeshTri {
		public FlowVertexNode A, B, C;
		public Vector3 CenterLocation;
		public float CenterRadius;

		public MeshTri(FlowVertexNode _a, FlowVertexNode _b, FlowVertexNode _c) {
			this.A = _a;
			this.B = _b;
			this.C = _c;

			var bnds = this.EachVertex()
				.Select(k => new SpanOf<Vector3>(k.Location,k.Location))
				.Aggregate((A,B) => new SpanOf<Vector3>(Vector3.Min(A.From,B.From),
					                                        Vector3.Max(A.To, B.To)));

			this.CenterLocation = (bnds.From + bnds.To) * 0.5f;//(A.Location + B.Location + C.Location)/3.0f;
			this.CenterRadius = this.EachVertex()
				.Select(k => (k.Location - this.CenterLocation).magnitude)
				.Aggregate((a,b) => Mathf.Max(a,b));
		}

		public bool ReferencesVertex(FlowVertexNode v) {
			return ((A == v) || (B == v) || (C == v));
		}

		public bool CenterSphereContainsPoint(Vector3 loc) {
			return ((loc - CenterLocation).magnitude < this.CenterRadius);
		}

		public FlowVertexNode[] EachVertex() {
			FlowVertexNode[] verts = new FlowVertexNode[3];
			verts [0] = A;
			verts [1] = B;
			verts [2] = C;
			return verts;
		}

		public Vector3 GetNormal() {
			return Vector3.Cross (B.Location - A.Location, C.Location - A.Location).normalized;
		}

		public Mesh GenerateMesh() {
			Mesh m = new Mesh ();
			var ar1 = this.EachVertex ().Select (k => (k.Location- this.CenterLocation)).ToList();
			m.vertices = ar1.Concat (ar1).ToArray ();
			m.triangles = new int[6]{ 0, 1, 2,   3, 5, 4 };
			m.RecalculateNormals ();
			return m;
		}
	}

	// Use this for initialization
	void Start () {
	
	}

	public static Vector3 Avg(Vector3 a, Vector3 b) {
		return ((a + b) * 0.5f);
	}

	public GameObject CreateMeshTriObject(MeshTri mt) {

		//GameObject s = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		//s.transform.position = mt.CenterLocation;
		//s.transform.localScale = Vector3.one * mt.CenterRadius;

		GameObject go = new GameObject ();
		go.name = "energy tri";
		go.transform.parent = this.transform;
		go.transform.position = mt.CenterLocation;
		go.AddComponent<MeshFilter> ().mesh = mt.GenerateMesh ();
		go.AddComponent<MeshRenderer> ().material = this.MaterialForLines;

		var ft = go.AddComponent<FlowTriangle> ();
		ft.NodeA = mt.A;
		ft.NodeB = mt.B;
		ft.NodeC = mt.C;

		return go;
	}

	public float DefaultLineThickness = 0.02f;

	EnergyLine CreateLineObjects(Transform parent, FlowVertexNode a, FlowVertexNode b) {
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		go.name = "energy line";
		go.transform.parent = parent;
		go.transform.position = Avg (a.Location, b.Location);
		go.transform.LookAt (b.Location);
		go.transform.localScale = new Vector3 (DefaultLineThickness, DefaultLineThickness,(b.Location - a.Location).magnitude);
		go.GetComponent<Collider> ().enabled = false;
		go.GetComponent<MeshRenderer> ().material = this.MaterialForLines;
		var line = go.AddComponent<EnergyLine> ();
		line.NodeA = a;
		line.NodeB = b;
		return line;
	}

	void SetupUniqueIndices() {
		int curNdx = 10;
		foreach (var v in GameObject.FindObjectsOfType<FlowVertexNode>()) {
			v.UniqueIndex = curNdx;
			curNdx ++;
		}
	}

	[ContextMenu ("Generate Mesh Now")]
	void ContextMenu_PerformGenerate () {
		this.PerformGenerate ();
	}

	[ContextMenu ("Fix Components")]
	void ContextMenu_PerformFixComps () {
		this.DoFixMissingComponents ();
	}
	
	void PerformGenerate() {
		this.SetupUniqueIndices ();
		var verts = GameObject.FindObjectsOfType<FlowVertexNode> ();



		for (int iv=0; iv<verts.Length; iv++) {
			var center = verts[iv];
			var nearish = verts
				.Where(k => (k.Location - center.Location).magnitude < 2.0)
				.OrderBy(k => (k.Location - center.Location).magnitude)
					.Take(20).ToList();
			for (int ik=0; ik<nearish.Count; ik++) {

				if (center.UniqueIndex < nearish[ik].UniqueIndex) {
				//CreateLineObjects(this.transform, center,
				//	                  nearish[ik]);
				}

				for (int ij=0; ij<nearish.Count; ij++) {
					var a = center;
					var b = nearish[ik];
					var c = nearish[ij];
					if ((a.UniqueIndex < b.UniqueIndex) && (a.UniqueIndex < c.UniqueIndex)
					    && (b.UniqueIndex < c.UniqueIndex))
					{
						MeshTri tri = new MeshTri(a,b,c);
						this.MyTris.Add(tri);
					}
				}
			}
		}

		if (true) // delauny simplification:
		{
			int mi = 0;
			while (mi < this.MyTris.Count) {
				var t = this.MyTris[mi];
				bool isValid = true;
				for (int vi=0; (vi<verts.Length) && isValid; vi++) {
					var iv = verts[vi];
					if (!t.ReferencesVertex(iv)) {
						if (t.CenterSphereContainsPoint(iv.Location)) {
							isValid = false;
						}
					}
				}
				if (isValid) {
					mi++;
				}
				else {
					this.MyTris.RemoveAt(mi);
				}
			}
		}

		foreach (var m in this.MyTris) {
			this.CreateMeshTriObject(m);
		}
	}

	void DoFixMissingComponents() {
		var flowNodes = GameObject.FindObjectsOfType<FlowVertexNode> ();

		for (int ci=0; ci<this.transform.childCount; ci++) {
			var c = this.transform.GetChild(ci);

			if (c.GetComponent<FlowTriangle>() != null) 
				continue;

			var mf = c.GetComponent<MeshFilter>();
			if (mf != null) {
				var m = mf.mesh;
				List<FlowVertexNode> mynodes = new List<FlowVertexNode>();
				for (int vi=0; vi<3; vi++) {
					var pos = m.vertices[vi] + c.transform.position;
					FlowVertexNode bestNode = null;
					float bestDist = 10000.0f;
					foreach (var fn in flowNodes) {
						var d = (fn.Location - pos).magnitude;
						if (d < bestDist) {
							bestNode = fn;
							bestDist = d;
						}
					}
					if (bestDist > 0.1f) {
						Debug.LogError("No node found!!!");
					}
					mynodes.Add(bestNode);
				}
				FlowTriangle ft = c.gameObject.AddComponent<FlowTriangle>();
				ft.NodeA = mynodes[0];
				ft.NodeB = mynodes[1];
				ft.NodeC = mynodes[2];
			}
		}
	}

	void UpdateNormals() {
		var verts = GameObject.FindObjectsOfType<FlowVertexNode> ();
		var tris = GameObject.FindObjectsOfType<FlowTriangle> ();

		verts.ForeachDo (k => k.TempNormal = Vector3.zero);
		tris.ForeachDo (k => {
			var n = k.CalcNormal();
			k.AllNodes().ForeachDo(j => j.TempNormal += n);
		});
		verts.ForeachDo (k => k.TempNormal = k.TempNormal.normalized);
	}

	[ContextMenu("Integrate Lines")]
	void ContextMenu_DoIntegateLines() {
		this.IntegrateLines ();
	}
	
	void IntegrateLines() {

		GameObject lines = new GameObject ();
		lines.name = "All Lines";
		lines.transform.parent = this.transform;
		lines.transform.localPosition = Vector3.zero;
		lines.transform.localRotation = Quaternion.identity;
		lines.transform.localScale = Vector3.one;
		var lt  = lines.transform;

		var tris = GameObject.FindObjectsOfType<FlowTriangle> ();
		foreach (var tri in tris) {
			this.CreateLineObjects(lt, tri.NodeA, tri.NodeB);
			this.CreateLineObjects(lt, tri.NodeB, tri.NodeC);
			this.CreateLineObjects(lt, tri.NodeC, tri.NodeA);
			//tri.enabled = false;
		}
	}

	[ContextMenu("Integrate Triangles")]
	void ContextMenu_DoIntegateMeshes() {
		this.IntegrateMeshes ();
	}

	void IntegrateMeshes() {

		var poses = new List<Vector3> ();
		var normls = new List<Vector3> ();
		var indices = new List<int> ();

		var center = this.transform.position;

		this.UpdateNormals ();

		var tris = GameObject.FindObjectsOfType<FlowTriangle> ();
		foreach (var t in tris) {
			var startI = poses.Count;

			poses.Add(t.NodeA.Location - center);
			poses.Add(t.NodeB.Location - center);
			poses.Add(t.NodeC.Location - center);
			poses.Add(t.NodeA.Location - center);
			poses.Add(t.NodeB.Location - center);
			poses.Add(t.NodeC.Location - center);

			indices.Add(startI+0);
			indices.Add(startI+1);
			indices.Add(startI+2);
			indices.Add(startI+3);
			indices.Add(startI+5); // second triangle is first flipped
			indices.Add(startI+4);

			normls.Add(t.NodeA.TempNormal);
			normls.Add(t.NodeB.TempNormal);
			normls.Add(t.NodeC.TempNormal);
			normls.Add(t.NodeA.TempNormal);
			normls.Add(t.NodeB.TempNormal);
			normls.Add(t.NodeC.TempNormal);

			
			var tmr = t.GetComponent<MeshRenderer>();
			if (tmr != null) {
				GameObject.Destroy(tmr);
			}
			var tmf = t.GetComponent<MeshFilter>();
			if (tmf != null) {
				GameObject.Destroy(tmf);
			}
		}
		var mesh = new Mesh ();
		mesh.vertices = poses.ToArray ();
		mesh.triangles = indices.ToArray ();
		mesh.normals = normls.ToArray ();
		//mesh.RecalculateNormals ();
		var mf = this.GetComponent<MeshFilter> ();
		if (mf == null) {
			mf = this.gameObject.AddComponent<MeshFilter> ();
		}
		mf.mesh = mesh;
		if (this.GetComponent<MeshRenderer> () == null) {
			var mr = this.gameObject.AddComponent<MeshRenderer> ();
			mr.material = this.MaterialForLines;
		}
	}


	// Update is called once per frame
	void Update () {
		if (this.FixComponents) {
			this.FixComponents = false;
			this.DoFixMissingComponents();
		}

		if (this.DoGenerate) {
			this.DoGenerate = false;
			this.PerformGenerate();
		}

	}
}
