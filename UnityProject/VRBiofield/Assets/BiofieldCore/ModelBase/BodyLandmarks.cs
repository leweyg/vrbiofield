using UnityEngine;
using System.Collections;

public class BodyLandmarks : MonoBehaviour {

	public Transform SpineStart;
	public Transform SpineShoulders;
	public Transform SpineEnd;

	public Transform LeftLegStart;
	public Transform LeftLegEnd;

	public Transform RightLegStart;
	public Transform RightLegEnd;

	public Transform LeftArmStart;
	public Transform LeftArmHand;

	public Transform RightArmStart;
	public Transform RightArmHand;


	private ChakraControl mChakras;
	public ChakraControl Chakras {
		get {
			if (this.mChakras != null) {
				return this.mChakras;
			} else {
				this.mChakras = this.gameObject.GetComponent<ChakraControl> ();
			}
			return this.mChakras;
		}
	}

	public void EnsureSetup() {
		this.Chakras.EnsureSetup ();
	}
}
