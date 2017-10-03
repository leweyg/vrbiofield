using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCustomTexture : MonoBehaviour {

	public Texture2D SaveInto;

	[ContextMenu("Generate texture into")]
	public void GenerateTexture() {
		#if UNITY_EDITOR
		var saveTo = UnityEditor.AssetDatabase.GetAssetPath (this.SaveInto);

		int w = 256;
		Texture2D tex = new Texture2D (w, w);
		tex.alphaIsTransparency = true;
		for (int y = 0; y < w; y++) {
			for (int x = 0; x < w; x++) {
				var v = (new Vector2 (x, y)) * (1.0f / w);
				v += (Vector2.one * (-0.5f));
				var p = 1 - Mathf.Clamp01( v.magnitude * 2 );
				var clr = new Color (p, p, p, p);
				tex.SetPixel (x, y, clr);
			}
		}
		tex.Apply ();

		Debug.Log ("About to write...");

		var fs = new System.IO.FileStream (saveTo, System.IO.FileMode.Create);
		System.IO.BinaryWriter bw = new System.IO.BinaryWriter (fs);
		bw.Write (tex.EncodeToPNG ());
		bw.Close ();
		fs.Close ();

		Debug.Log ("Image written to '" + saveTo + "'.");
		#endif
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
