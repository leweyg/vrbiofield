using UnityEngine;
using System.Collections;
using System.Linq;

public class ChakraControl : MonoBehaviour {

    public int VoxelResolution = 16;
    public VolumeTextureBehavior MultiChakras = null;
	public bool IsHideAll = false;
    public bool EnableMultiChakra = true;
    public VolumeTextureBehavior EnableOnlyThisChakra = null;
    private VolumeTextureBehavior PreviousEnableOnly = null;
    private bool PreviousEnableMulti = false;

	public VolumeTextureBehavior[] AllPoints { get; private set; }
	public ChakraPosition[] AllChakras { get; private set; }


    void Awake()
    {
		this.AllChakras = this.gameObject.GetComponentsInChildren<ChakraPosition> ().OrderBy (k => k.ChakraIndex).ToArray ();
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
		bool allowActive = !IsHideAll;
		this.PreviousEnableOnly = this.EnableOnlyThisChakra;
		this.PreviousEnableMulti = this.EnableMultiChakra && allowActive;
        if (this.EnableOnlyThisChakra != null)
        {
            this.AllPoints.ForeachDo(k => k.gameObject.SetActive(false));
			this.EnableOnlyThisChakra.gameObject.SetActive(true && allowActive);
        }
        else
        {
			this.AllPoints.ForeachDo(k => k.gameObject.SetActive(true && allowActive));
			this.MultiChakras.gameObject.SetActive(this.EnableMultiChakra && allowActive);
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
