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

	public Transform EstRightHandKnuckle {
		get { return this.RightArmHand.GetChild (1); }
	}
	public Transform EstLeftHandKnuckle {
		get { return this.LeftArmHand.GetChild (1); }
	}

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

	private MeridianController mMeridians = null;
	public MeridianController Meridians {
		get {
			if (mMeridians)
				return mMeridians;
			mMeridians = this.GetComponentInChildren<MeridianController> ();
			if (mMeridians) {
				mMeridians.EnsureSetup ();
				return mMeridians;
			}
			return null;
		}
	}

	public void EnsureSetup() {
		this.Chakras.EnsureSetup ();
		if (this.Meridians) {
			this.Meridians.EnsureSetup ();
		}
	}

	private BodyPositioning mBodyPosition;
	public BodyPositioning EnsureBodyPositioning() {
		this.EnsureSetup ();
		if (this.mBodyPosition == null) {
			this.mBodyPosition = this.gameObject.GetComponent<BodyPositioning> ();
		}
		if (this.mBodyPosition == null) {
			this.mBodyPosition = this.gameObject.AddComponent<BodyPositioning> ();
		}
		Debug.Assert (this.mBodyPosition != null);
		return this.mBodyPosition;
	}
}
