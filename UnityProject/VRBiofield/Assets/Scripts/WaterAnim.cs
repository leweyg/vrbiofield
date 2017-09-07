using UnityEngine;
using System.Collections;

public class WaterAnim : MonoBehaviour {

	private MeshRenderer Renderer;
	public float RateScalarOverDefault = 1.0f;

	// Use this for initialization
	void Start () {
		this.Renderer = this.GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		this.Renderer.material.mainTextureOffset += new Vector2 (0.0f, Time.deltaTime * -0.0035f * RateScalarOverDefault);
	}
}
