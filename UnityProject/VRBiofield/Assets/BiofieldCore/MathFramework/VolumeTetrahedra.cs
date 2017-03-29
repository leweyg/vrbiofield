using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VolumeTetrahedraSurfacer {

	public static Mesh GenerateSurfaceVolume<T>(VolumeBuffer<T> vol, Func<T,float> signTest) {
		int tetCorners = 4;
		Dictionary<int,int> ndxToVertex = new Dictionary<int, int> ();
		List<int> triangles = new List<int> ();
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
								if (ee != edgesInTetra [3]) {
									triangles.Add (vid);
								} else {
									Debug.Log ("Four sided");
									triangles.Add (prevVid);
									triangles.Add (prevPrevVid);
									triangles.Add (vid);

									triangles.Add (pppv);
									triangles.Add (prevPrevVid);
									triangles.Add (vid);

									triangles.Add (pppv);
									triangles.Add (prevVid);
									triangles.Add (vid);
								}
							} else {
								Debug.Assert (false, "Really??");
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
		foreach (var kv in ndxToVertex) {
			var packed = kv.Key;
			var vid = kv.Value;
			while (vertices.Count <= vid) {
				vertices.Add (Vector3.zero);
			}
			Int3 a3, b3;
			UnpackVoxelEdgeId (vol.Header, packed, out a3, out b3);
			var pos = Vector3.Lerp (a3.AsVector3 (), b3.AsVector3 (), 0.5f); // TODO: weight value based on signed root
			vertices [vid] = pos;
		}
		Mesh result = new Mesh ();
		Debug.Log ("Meshing info: verts=" + vertices.Count + " tris=" + triangles.Count);
		result.vertices = vertices.ToArray ();
		result.triangles = triangles.ToArray ();
		return result;
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
