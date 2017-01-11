using UnityEngine;
using System.Collections;

public class VolumeSourceBehavior : MonoBehaviour {

	public Texture3D VolumeTexture {get; set;}
	public Material mMaterial {get; set;}
	private MeshRenderer mRenderer = null;

	public int DefaultVoxelWidth = 16;

	public bool IsSlowerPlatform() {
		//return true; // HACK, CONSIDER ALL PLATFORMS SLOW
		if (Application.platform == RuntimePlatform.Android) {
			return true;
		}
		return false;
	}

	[HideInInspector]
	public Cubic<int> VolumeSize;

		// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}


	public void EnsureSetup() {

		if (this.mRenderer == null) {
			this.mRenderer = this.GetComponent<MeshRenderer> ();
		}

		if (this.IsSlowerPlatform ()) {
			this.mRenderer.enabled = false;
			return;
		}

		if (this.VolumeTexture != null)
			return;

		var coreSize = DefaultVoxelWidth; //64; //32 // 16
        var cc = this.gameObject.GetComponentInParent<ChakraControl>();
        if (cc != null)
        {
            coreSize = cc.VoxelResolution;
        }

		this.VolumeSize = new Cubic<int> (coreSize, coreSize, coreSize);
		Texture3D tex = new Texture3D (
			coreSize, coreSize, coreSize, 
			TextureFormat.ARGB32, true);
		this.VolumeTexture = tex;

		var numColors = coreSize * coreSize * coreSize;
		var colors = new Color[ numColors ];
		for (int i=0; i<numColors; i++) {
			//var ix = (i % coreSize);
			//var iy = ((i / coreSize) % coreSize);
			//var iz = ((i / (coreSize * coreSize)) % coreSize);

			var c = Color.black; // (((ix + iz) < iy) ? Color.grey : Color.blue);

			colors[i] = c;
		}
		colors[0] = Color.red;
		colors [(coreSize*coreSize)*1] = Color.red;
		colors [(coreSize*coreSize)*2] = Color.red;
		colors [(coreSize*coreSize)*3] = Color.red;
		colors[numColors-1] = Color.green;

		tex.SetPixels (colors);
		//tex.filterMode = FilterMode.Point;
		tex.filterMode = FilterMode.Trilinear;
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.Apply ();

		this.mMaterial = this.GetComponent<Renderer> ().material;
		this.mMaterial.SetTexture ("_MainVol", tex);


		Texture2D tex2 = new Texture2D (coreSize, coreSize);
		tex2.SetPixels (colors);
		tex2.Apply ();

		
		//this.GetComponent<Renderer> ().material.SetTexture ("_MainTex", tex2);
	}

	void UpdateMaterial(Material m) {

		var tex = this.VolumeTexture;
		m.SetTexture ("_MainVol", tex);

		float t = UnityEngine.Time.time;
		t = (t - Mathf.Floor (t));
		m.SetColor ("_RandomSeedColor", new Color (t, t, t, t));
		
		m.SetVector ("_ScrollOffset", VolumeScrollOffset);
		m.SetVector ("_ScrollScale", VolumeScrollScale);

	}

	[HideInInspector]
	public Vector4 VolumeScrollOffset = new Vector4(0,0,0,0);
	[HideInInspector]
	public Vector4 VolumeScrollScale = new Vector4 (1, 1, 1, 1);
	
	// Update is called once per frame
	void Update () {
		if (this.IsSlowerPlatform ()) {
			if (this.mRenderer != null) {
				this.mRenderer.enabled = false;
			}
			//this.gameObject.SetActive (false);
			return;
		}
		
		this.UpdateMaterial (this.mMaterial);
	}
}
