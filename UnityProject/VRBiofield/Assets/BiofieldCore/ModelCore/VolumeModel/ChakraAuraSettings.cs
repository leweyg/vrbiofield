using UnityEngine;
using System.Collections;

public class ChakraAuraSettings : MonoBehaviour {

    [Range(0.0f, 2.0f)]
    public float WholeScaleDistances = 1.0f;
    [Range(0.0f, 1.5f)]
    public float SampleTranparency = 1.0f;
    public Color[] AuraColors;
    [Range(0.0f, 0.2f)]
    public float[] AuraDistances;

    public bool IsPauseCalculation = false;

    public bool DoRecalc = false;
	public bool UseChakraAuraDistance = false;
    private VolumeTextureBehavior AuraBehavior = null;

    private Material MyMaterial = null;

	// Use this for initialization
	void Start () {
        this.AuraBehavior = this.GetComponent<VolumeTextureBehavior>();
        var mr = this.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            this.MyMaterial = mr.material;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (this.DoRecalc && (this.AuraBehavior != null))
        {
            this.DoRecalc = false;
            this.AuraBehavior.RequestAuraUpdate();
        }
        if (this.MyMaterial != null)
        {
            this.MyMaterial.SetFloat("_SampleTransparency", this.SampleTranparency);
        }
	}
}
