using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;

public class ExportChakraMeshes : MonoBehaviour {

	public ChakraBreath ChakraExcersize;

	[ContextMenu("Export One Chakra Mesh")]
	public void DoExport() {
		var dict = this.ChakraExcersize.MeshDictionary;

		foreach (var kv in dict) {
			this.ExportChakraMesh (kv.Key, kv.Value);
			return; // only one for now
		}
	}

	[ContextMenu("Export Yogi Mesh")]
	public void ExportYogiMesh() {
		var bodyRoot = this.ChakraExcersize.Body.LeftLegEnd;
		while (bodyRoot.parent != null)
			bodyRoot = bodyRoot.parent;
		var smr = bodyRoot.GetComponentInChildren<SkinnedMeshRenderer> ();
		Mesh m = new Mesh ();
		smr.BakeMesh (m);

		var str = ObjExporterScript.MeshToString (m, this.transform, null);

		string toPath = (Application.dataPath + "/ExportedMeshes/YogiMesh_SimpleLotus.obj");
		Debug.Log ("Saving mesh to: " + toPath);
		StreamWriter sw = new StreamWriter (toPath);
		sw.Write (str);
		sw.Close ();

	}

	[ContextMenu("Export All Chakra Meshes")]
	public void DoExportAll() {
		var dict = this.ChakraExcersize.MeshDictionary;

		foreach (var kv in dict) {
			this.ExportChakraMesh (kv.Key, kv.Value);
		}
	}

	public int ChakraIndex(ChakraPosition p) {
		var all = this.ChakraExcersize.Body.Chakras.AllChakras;
		for (int i = 0; i < all.Length; i++) {
			if (all [i] == p) {
				return i;
			}
		}
		Debug.LogError ("Couldn't find this one: " + p.name);
		return -1;
	}

	public void ExportChakraMesh(ChakraPosition cp, Mesh m) {
		ObjExporterScript ex = new ObjExporterScript ();
		ObjExporterScript.Start ();
		string str = ObjExporterScript.MeshToString (m, this.transform, null);
		int index = this.ChakraIndex (cp);

		string toPath = (Application.dataPath + "/ExportedMeshes/ChakraMesh_" + (index+1) + "_" + cp.name.Trim ().Replace (" ", "_") + ".obj");
		Debug.Log ("Saving mesh to: " + toPath);
			StreamWriter sw = new StreamWriter (toPath);
		sw.Write (str);
		sw.Close ();
	}
}
