using UnityEngine;
using System.Collections;
using System.Linq;

public class ChakraControl : MonoBehaviour {

    public int VoxelResolution = 16;
    public VolumeTextureBehavior MultiChakras = null;
    public bool EnableMultiChakra = true;
    public VolumeTextureBehavior EnableOnlyThisChakra = null;
    private VolumeTextureBehavior PreviousEnableOnly = null;
    private bool PreviousEnableMulti = false;

	public VolumeTextureBehavior[] AllPoints { get; private set; }


    void Awake()
    {
        this.AllPoints = this.gameObject.GetComponentsInChildren<VolumeTextureBehavior>();
        if (this.MultiChakras == null)
        {
            this.MultiChakras = this.AllPoints.First(k => k.IsMultiChakras);
        }

        this.EnableCorrectChakras();
    }


	// Use this for initialization
    void EnableCorrectChakras()
    {
        this.PreviousEnableOnly = this.EnableOnlyThisChakra;
        this.PreviousEnableMulti = this.EnableMultiChakra;
        if (this.EnableOnlyThisChakra != null)
        {
            this.AllPoints.ForeachDo(k => k.gameObject.SetActive(false));
            this.EnableOnlyThisChakra.gameObject.SetActive(true);
        }
        else
        {
            this.AllPoints.ForeachDo(k => k.gameObject.SetActive(true));
            this.MultiChakras.gameObject.SetActive(this.EnableMultiChakra);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if ((this.PreviousEnableOnly != this.EnableOnlyThisChakra)
            || (this.PreviousEnableMulti != this.EnableMultiChakra))
        {
            this.EnableCorrectChakras();
        }
	}
}
