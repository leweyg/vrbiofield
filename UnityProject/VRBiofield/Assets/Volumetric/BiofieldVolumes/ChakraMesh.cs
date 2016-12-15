﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChakraMesh : MonoBehaviour {

	public Mesh PieceMesh;

	private ChakraMeshBuilder Builder;
	private Material Mat = null;
	public Color LatestColor { get; set; }

	// Use this for initialization
	void Start () {
		this.Builder = new ChakraMeshBuilder (this);
		this.Builder.BuildSingleLevel (12);

		var mf = this.GetComponent<MeshFilter> ();
		mf.mesh = this.Builder.ResultMesh;

		var renderer = this.GetComponent<MeshRenderer> ();
		this.Mat = renderer.material;
		this.Mat.SetFloat ("_CustomAlpha", 1.0f);
		renderer.material = this.Mat;
	}

	public void SetMesh(Mesh m) {
		this.GetComponent<MeshFilter> ().mesh = m;
	}

	public void SetChakraAlpha(float f) {
		this.Mat.SetFloat ("_CustomAlpha", f);
	}

	public void SetChakraColor(Color c) {
		this.LatestColor = c;
		this.Mat.SetColor("_CustomColor", c );
		//this.GetComponent<MeshRenderer> ().material = this.Mat;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.RotateAround (this.transform.position,
			this.transform.up, Time.deltaTime * -3.0f);
	}

	public ChakraMeshBuilder MeshController {get {return this.Builder;}}

	public class ChakraMeshBuilder {
		public List<Vector3> Vertices = new List<Vector3>();
		public List<Vector2> VertexTCoords = new List<Vector2> ();
		public List<int> Indices = new List<int>();
		public ChakraMesh FullObj;
		public Mesh ResultMesh;
		public int CurrentPetals;
		public float CurrentPetalArc;
		public float CurrentPetalStart;
		public float CurrentAuricSlope;

		public ChakraMeshBuilder(ChakraMesh cm) {
			this.FullObj = cm;
		}

		private void ClearCache() {
			this.Vertices.Clear ();
			this.VertexTCoords.Clear ();
			this.Indices.Clear ();
		}

		public Vector3 PetalVertex(float petalX, float petalY) {
			float extraScaleWidth = 3.8f;
			float extraHeight = 2.0f;
			float r = this.CurrentPetalStart + ((0.5f-petalX) * (this.CurrentPetalArc * extraScaleWidth));
			float y = (petalY * this.CurrentAuricSlope * extraHeight);
			float x = Mathf.Sin (r) * y;
			float z = Mathf.Cos (r) * y;
			return new Vector3 (x, y, z);
		}

		private void AddPetalMesh() {
			var pieceMesh = this.FullObj.PieceMesh;
			var startVert = this.Vertices.Count;

			foreach (var pv in pieceMesh.vertices) {
				this.Vertices.Add (PetalVertex (pv.x, pv.y));
			}
			this.VertexTCoords.AddRange (pieceMesh.uv);
			var fromTris = pieceMesh.GetIndices (0);
			for (int ii = 0; ii < fromTris.Length; ii++) {
				this.Indices.Add (startVert + fromTris[ii]);
			}
		}

		private void BuildChakraLevel(int petals, float auricLevel) {
			float petalArc = (Mathf.PI * 2.0f) / petals;
			this.CurrentPetals = petals;
			this.CurrentPetalArc = petalArc;
			this.CurrentAuricSlope = 1.0f + ((auricLevel - 2.0f) * 0.25f);
			for (int pi = 0; pi < petals; pi++) {
				this.CurrentPetalStart = this.CurrentPetalArc * pi;

				this.AddPetalMesh ();
				//Vertices.Add (PetalVertex (0, 0.5f));
				//Vertices.Add (PetalVertex (0, 1));
				//Vertices.Add (PetalVertex (1, 1));
			}
			ResultMesh.Clear (true);
			ResultMesh.SetVertices (Vertices);
			ResultMesh.SetUVs (0, this.VertexTCoords);
			ResultMesh.triangles = this.Indices.ToArray ();
			this.FullObj.SetMesh (this.ResultMesh);
		}

		public void BuildSingleLevel(int petals) {
			if (this.ResultMesh == null)
				this.ResultMesh = new Mesh ();
			this.ClearCache ();
			this.BuildChakraLevel (petals, 1.0f);
		}


	}
}