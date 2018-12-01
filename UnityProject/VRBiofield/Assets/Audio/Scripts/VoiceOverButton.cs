using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceOverButton : MonoBehaviour, MenuSimpleButton.IButtonReciever {

	public VoiceOverInfo PlayThis;
	public bool IsAmbientAudio = false;
	private MenuSimpleButton SubButton;
	private VoiceOverEventManager VOMan;
	private bool IsBeforeUpdate = true;

	// Use this for initialization
	void Start () {
		VOMan = GameObject.FindObjectOfType<VoiceOverEventManager> ();
		SubButton = this.GetComponentInChildren<MenuSimpleButton> ();
		UpdateFromVOMan ();
		VOMan.OnTrackChanged += (newTrackOrNull) => {this.UpdateFromVOMan();};
		this.UpdateFromVOMan ();

	}

	void UpdateFromVOMan() {
		if (!this.IsAmbientAudio) {
			var cur = this.VOMan.CurrentTrack;
			this.SubButton.IsMode2 = (cur == this.PlayThis);
		} else {
			this.SubButton.IsMode2 = (this.VOMan.IsBackgroundMusicPlaying || IsBeforeUpdate);
		}
	}
	
	// Update is called once per frame
	void Update () {
		IsBeforeUpdate = false;
	}

	#region IButtonReciever implementation

	public void ButtonHoverChanged (MenuSimpleButton.StandardButton bt, bool isHovered)
	{
	}

	public void ButtonSelected (MenuSimpleButton.StandardButton bt)
	{
		if (bt == MenuSimpleButton.StandardButton.PlayOrPause) {
			var vman = this.VOMan;
			if (this.IsAmbientAudio) {
				vman.IsBackgroundMusicPlaying = !vman.IsBackgroundMusicPlaying;
			} else {
				
				if (vman.CurrentTrack != this.PlayThis) {
					vman.ChangeTrack (this.PlayThis);
				} else {
					if (vman.Player.isPlaying)
						vman.Player.Pause ();
					else
						vman.Player.Play ();
				}
			}
		}
	}

	#endregion
}
