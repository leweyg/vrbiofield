using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TempScripts : MonoBehaviour {

	public GameObject BaseYogi;
	public ChakraPosition[] Result;

	private void DisableComponent<T>(Component g) where T:MonoBehaviour {
		var c = g.gameObject.GetComponent<T> ();
		if ((c != null)) {
			c.enabled = false;
		}
	}

	[ContextMenu("Add Chakra Points")]
	public void AddChakraPoints() {
		List<ChakraPosition> pnts = new List<ChakraPosition> ();
		foreach (var vt in BaseYogi.gameObject.
			GetComponentsInChildren<VolumeTextureBehavior>()
			//.Where(k => !(k.IsAura || k.IsMultiChakras))
		) {
			if (!(vt.IsAura || vt.IsMultiChakras)) {
				var cp = vt.gameObject.GetComponent<ChakraPosition> ();
				if (cp == null) {
					cp = vt.gameObject.AddComponent<ChakraPosition> ();

					cp.ChakraColor = vt.ChakraColor;
					//cp.ChakraIndex = xxx;
					cp.ChakraOneWay = vt.ChakraOneWay;
					cp.ChakraPetals = vt.ChakraPetals;
				}
				pnts.Add (cp);
			}
			DisableComponent<VolumeTextureBehavior> (vt);
			DisableComponent<VolumeSourceBehavior> (vt);
			DisableComponent<VolumeMaterialAnimator> (vt);
			//DisableComponent<Collider> (vt);
			//DisableComponent<MeshRenderer> (vt);
			vt.GetComponent<MeshRenderer>().enabled = false;
			vt.GetComponent<BoxCollider> ().enabled = false;

		}
		Debug.Log ("Found " + pnts.Count);
		//if (pnts.Count == 8)
		{
			int index = 1;
			pnts = pnts.OrderBy (k => k.transform.position.y).ToList();
			foreach (var p in pnts) {
				p.ChakraIndex = index;
				index++;
			}
		}
		Result = pnts.ToArray ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
