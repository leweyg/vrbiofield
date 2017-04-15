using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VolumeTetrahedraSurfacer {

	public static Mesh GenerateSurfaceVolume<T>(VolumeBuffer<T> vol, Func<T,float> signTest, Vector3 relativeCameraPos, DynamicFieldModel optionalModel = null) {
		int tetCorners = 4;
		Dictionary<int,int> ndxToVertex = new Dictionary<int, int> ();
		List<int> triangles = new List<int> ();
		List<int> quads = new List<int> ();
		float[] signes = new float[tetCorners];
		List<int> edgesInTetra = new List<int> ();
		VolumeHeader h2 = new VolumeHeader (new Int3 (2, 2, 2));
		foreach (var ndx in vol.AllIndices3()) {
			if ((ndx.X != 0) && (ndx.Y != 0) && (ndx.Z != 0)) {
				var start = ndx.Add (new Int3 (-1, -1, -1));

				foreach (var ts in TetrasInCube) {
					int countPos=0, countNeg=0;
					int signKey = 0;
					for (int cornerIndex=0; cornerIndex<ts.Length; cornerIndex++) {
						var lndx = ts[cornerIndex];
						var cur = AddInvertTileOffset3 (start, (h2.LinearToCubic (lndx)));
						var sv = signTest (vol.Read (cur));
						if (sv >= 0.0f) {
							countPos++;
							signKey |= (1 << cornerIndex);
						} else {
							countNeg++;
						}
						signes [cornerIndex] = sv;
					}
					if ((countPos > 0) && (countNeg > 0)) {
						edgesInTetra.Clear ();
						// we have something in this tetrahedra!
						for (int ei = 0; ei < VolumeTetrahedraSurfacer.EdgesInTetra.Length; ei += 2) {
							var ef = VolumeTetrahedraSurfacer.EdgesInTetra [ei + 0];
							var et = VolumeTetrahedraSurfacer.EdgesInTetra [ei + 1];
							if (((signes [ef])*(signes [et])) < 0.0f) {
								// we have something on this edge!
								var tf = ts[ ef ];
								var tt = ts[ et ];
								var vf = AddInvertTileOffset3 (start, h2.LinearToCubic (tf));
								var vt = AddInvertTileOffset3 (start, h2.LinearToCubic (tt));
								int encoded = PackVoxelEdgeIdSorted(vol.Header, vf, vt);
								edgesInTetra.Add (encoded);
							}
						}
						int prevVid = -1, prevPrevVid = -1, pppv = -1;
						foreach (var ee in edgesInTetra) {
							int vid;
							if (ndxToVertex.ContainsKey (ee)) {
								vid = ndxToVertex [ee];
							} else {
								vid = ndxToVertex.Count;
								ndxToVertex.Add (ee, vid);
							}
							if (edgesInTetra.Count == 3) {
								triangles.Add (vid);
							} else if (edgesInTetra.Count == 4) {
								quads.Add (vid);
							} else {
								Debug.Assert (false, "Really?? c=" + edgesInTetra.Count);
							}
							pppv = prevPrevVid;
							prevPrevVid = prevVid;
							prevVid = vid;
						}
					}
				}
			}
		}

		List<Vector3> vertices = new List<Vector3> ();
		List<Vector3> vertexSigns = new List<Vector3> ();
		List<Vector4> vertexTangents = null;
		if (optionalModel != null) {
			vertexTangents = new List<Vector4> ();
		}
		Vector3 invScale = new Vector3 (1.0f / (vol.Size.X-1), 1.0f / (vol.Size.Y-1), 1.0f / (vol.Size.Z-1));
		foreach (var kv in ndxToVertex) {
			// setup vertices from edge data:
			var packed = kv.Key;
			var vid = kv.Value;
			while (vertices.Count <= vid) {
				vertices.Add (Vector3.zero);
				vertexSigns.Add (Vector3.zero);;
				if (optionalModel != null) {
					vertexTangents.Add(Vector4.zero);
				}
			}
			Int3 a3, b3;
			UnpackVoxelEdgeId (vol.Header, packed, out a3, out b3);
			var wa = signTest (vol.Read (a3));
			var wb = signTest (vol.Read (b3));
			var wab = Mathf.Abs (wa) / (Mathf.Abs (wa) + Mathf.Abs (wb));
			var pos = Vector3.Lerp (a3.AsVector3 (), b3.AsVector3 (), wab); //0.5f); // TODO: weight value based on signed root
			var upos = new Vector3(pos.x * invScale.x, pos.y * invScale.y, pos.z * invScale.z) - (Vector3.one * 0.5f);
			vertices [vid] = upos;
			vertexSigns [vid] = (a3.AsVector3 () - b3.AsVector3 ()) * Mathf.Sign (wa - wb);
			if (optionalModel != null) {
				var ta = CalcFlowTangent(optionalModel, optionalModel.FieldsCells.Read(a3));
				var tb = CalcFlowTangent(optionalModel, optionalModel.FieldsCells.Read(b3));
				vertexTangents [vid] = Vector4.Lerp (ta, tb, wab); // ((ta + tb) *0.5f);
			}
		}
		for (int qi = 0; qi < quads.Count; qi+=4) {
			
			// ensure quad covers whole space (a and b must be furthest from each other):
			var a = vertices[quads[qi+0]];
			var b = vertices[quads[qi+1]];
			var c = vertices[quads[qi+2]];
			var d = vertices[quads[qi+3]];

			// add triangle a-b-c:
			triangles.Add(quads[qi+0]);
			triangles.Add(quads[qi+1]);
			triangles.Add(quads[qi+2]);

			triangles.Add(quads[qi+1]);
			triangles.Add(quads[qi+3]);
			triangles.Add(quads[qi+2]);

		}
		var trisToSort = new List<SortTri> ();
		for (int i = 0; i < triangles.Count; i += 3) {
			// ensure triangle is oriented correctly:
			var a = vertices[triangles[i+0]];
			var b = vertices[triangles[i+1]];
			var c = vertices[triangles[i+2]];
			var n = Vector3.Cross (b - a, c - b);
			if (Vector3.Dot (vertexSigns [triangles [i + 0]], n) >= 0.0f) {
				SwapListValues (triangles, i + 1, i + 2);
			}

			// add triangle to list
			SortTri tri;
			tri.I0 = triangles [i + 0];
			tri.I1 = triangles [i + 1];
			tri.I2 = triangles [i + 2];
			tri.DistFromCam = (relativeCameraPos - ((a + b + c) * (1.0f / 3.0f))).magnitude;
			trisToSort.Add (tri);
		}
		// sort the triangles:
		if (true)
		{
			trisToSort.Sort ((a, b) => -(a.DistFromCam.CompareTo (b.DistFromCam)));
			triangles.Clear ();
			foreach (var t in trisToSort) { 
				triangles.Add (t.I0);
				triangles.Add (t.I1);
				triangles.Add (t.I2);
			}
		}

		Mesh result = new Mesh ();
		//Debug.Log ("Meshing info: verts=" + vertices.Count + " tris=" + triangles.Count);
		result.SetVertices( vertices );
		if (optionalModel != null) {
			result.SetTangents(vertexTangents);
		}
		result.triangles = ( triangles.ToArray() );
		result.RecalculateNormals ();
		return result;
	}

	private static Vector4 CalcFlowTangent(DynamicFieldModel model, DynamicFieldModel.DynFieldCell cell) {
		var dir = cell.Direction.normalized;// / model.UnitMagnitude;
		var repeatScaler = 6.0f;
		return new Vector4 (dir.x, dir.y, dir.z, repeatScaler);
		//var offset = Vector3.Dot (cell.Pos, dir) * repeatScaler;
		//return new Vector4 (cell.Direction.magnitude / model.UnitMagnitude, offset, 0, 0);
	}

	private struct SortTri {
		public int I0, I1, I2;
		public float DistFromCam;


	}

	private static void SwapListValues<T>(List<T> list, int ia, int ib) {
		var tmp = list [ia];
		list [ia] = list [ib];
		list [ib] = tmp;
	}

	public static int InvertTileOffset(int s, int o) {
		if ((s % 2) == 0)
			return o;
		else
			return 1 - o;
	}

	public static Int3 InvertTileOffset3(Int3 start, Int3 h2offset) {
		return new Int3 (
			InvertTileOffset (start.X, h2offset.X),
			InvertTileOffset (start.Y, h2offset.Y),
			InvertTileOffset (start.Z, h2offset.Z));
	}

	public static Int3 AddInvertTileOffset3(Int3 start, Int3 h2offset) {
		return start.Add (InvertTileOffset3(start, h2offset));
	}

	public static int PackVoxelEdgeId(VolumeHeader hd, Int3 a3, Int3 b3) {
		int a = hd.CubicToLinear (a3);
		int b = hd.CubicToLinear (b3);
		return (a + (hd.TotalCount * b));
	}

	public static int PackVoxelEdgeIdSorted(VolumeHeader hd, Int3 a3, Int3 b3) {
		int a = hd.CubicToLinear (a3);
		int b = hd.CubicToLinear (b3);
		if (a < b)
			return PackVoxelEdgeId (hd, a3, b3);
		else
			return PackVoxelEdgeId (hd, b3, a3);
	}

	public static void UnpackVoxelEdgeId(VolumeHeader hd, int packed, out Int3 a3, out Int3 b3) {
		int a = (packed % hd.TotalCount);
		int b = (packed / hd.TotalCount);
		a3 = hd.LinearToCubic (a);
		b3 = hd.LinearToCubic (b);
	}
		
	public static int[][] TetrasInCube = new int[][] {
		new int[]{ 0, 1, 3, 5 },
		new int[]{ 0, 2, 3, 6 },
		new int[]{ 0, 3, 5, 6 },
		new int[]{ 0, 4, 5, 6 },
		new int[]{ 3, 5, 6, 7 },
	};
	public static int[] EdgesInTetra = new int[6*2]{
		0, 1,
		0, 2,
		0, 3,
		1, 2,
		1, 3,
		2, 3
	};

	public struct TetraCorner {
		Vector3 Pos3d;
		int NdxLinear;
		float SurfaceSign;
		Vector3 Torque;
	}

}
