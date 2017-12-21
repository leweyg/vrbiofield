using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ApplyPostShader : MonoBehaviour {

	public Material ShaderMaterial;
	public bool UseEffect = true;

	// Creates a private material used to the effect
	void Awake ()
	{
	}
	
	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		if ((ShaderMaterial==null) || (!UseEffect))
		{
			Graphics.Blit (source, destination);
			return;
		}

		//ShaderMaterial.SetFloat("_bwBlend", intensity);
		Graphics.Blit (source, destination, ShaderMaterial);
	}
}