using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LinkChakrasToMagnetics : MonoBehaviour {

	private List<ChakraInfo> Chakras = new List<ChakraInfo>();
	public GameObject PrefabForChakra = null;

	class ChakraInfo {
		public VolumeTextureBehavior Vol = null;
		public ChakraInfo(VolumeTextureBehavior _vol) { Vol = _vol; }
	}

	// Use this for initialization
	void Start () {
		var core = GameObject.FindObjectOfType<ChakraControl> ();
		var allVolumes = core.AllPoints;
		foreach (var v in allVolumes) {
			if ((!v.IsAura) && (!v.IsMultiChakras)) {
				this.Chakras.Add (new ChakraInfo (v));
			}
		}
		Debug.Log ("Found " + this.Chakras.Count + " chakras to magnetize...");

		foreach (var chakra in this.Chakras) {
			var fromT = chakra.Vol.transform;
			var pole = GameObject.Instantiate (this.PrefabForChakra);
			pole.transform.position = fromT.position;
			pole.transform.rotation = fromT.rotation;
		}
	}	
	
	// Update is called once per frame
	void Update () {
	
	}
}
