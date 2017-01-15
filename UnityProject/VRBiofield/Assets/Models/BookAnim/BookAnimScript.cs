using UnityEngine;
using System.Collections;

public class BookAnimScript : MonoBehaviour {

	public Texture[] Pages;
	public int CurrentPage = 0;
	public float TurnDuration = 2.0f;
	public bool UseArrowKeys = false;

	public Color ButtonColorPassive = Color.grey;
	public Color ButtonColorActive = Color.green;

	public GameObject PageRotateOrigin;
	public GameObject PageSinkOrigin;
	public MeshRenderer Left, Back, Front, Right;

	public event OnPageChangedEvent OnPageChanged;
	public delegate void OnPageChangedEvent(int newPage);

	private bool TurnIsAnimating = false;
	private bool TurnAutoActive = false;
	private bool TurnHoverActive = false;
	private float TurnTimeAmount;
	private float TurnStart;
	private int TurnDirection;
	private int TurnNextLeft;
	private int TurnHoverUpdateFrame = -1;

	public void SetIsHovering(bool isOver, bool isForward) {
		this.TurnHoverUpdateFrame = Time.frameCount;
		if (this.TurnIsAnimating) {
			if ((!isOver) && (this.TurnHoverActive)) {
				if (this.TurnTimeAmount > (this.TurnDuration * 0.8f)) {
					this.TurnAutoActive = true;
				}
			}
			this.TurnHoverActive = isOver;
		} else if (isOver) {
			this.ChangePageSimple (isForward, false);
		}
	}

	public bool CanChangePageInDirection(bool isForward) {
		if (isForward) {
			return ((this.CurrentPage + 2) < Pages.Length);
		} else {
			return ((this.CurrentPage - 2) >= 0);
		}
	}

	public void ChangePageSimple(bool isForward, bool isAuto=true) {
		if ((!isForward) && (this.CurrentPage <= 0)) {
			return;
		}
		if ((isForward && (this.CurrentPage + 2 >= Pages.Length))) {
			return;
		}
		this.ChangePageSlowly (this.CurrentPage + (isForward ? 2 : -2), isAuto);
	}

	// Use this for initialization
	void Start () {
		this.SetupPages ();
	}

	private Texture GetPageTex(int i) {
		return this.Pages[((i + (1000*Pages.Length)) % Pages.Length)];
	}

	void SetupPages() 
	{
		PageRotateOrigin.SetActive (false);
		Left.material.mainTexture = GetPageTex (this.CurrentPage + 0);
		Right.material.mainTexture = GetPageTex (this.CurrentPage + 1);
	}

	public void ChangePageSlowly(int toPageLeft, bool isAuto=true) {

		toPageLeft = ((toPageLeft + Pages.Length) % Pages.Length);

		this.TurnDirection = (toPageLeft - this.CurrentPage);
		this.TurnAutoActive = isAuto;
		this.TurnHoverActive = !isAuto;
		this.TurnStart = Time.time;
		this.TurnNextLeft = toPageLeft;
		this.TurnIsAnimating = true;

		PageRotateOrigin.SetActive (true);
		if (this.TurnDirection > 0) {
			Back.material.mainTexture = Right.material.mainTexture;
			Front.material.mainTexture = GetPageTex (toPageLeft + 0);
			Right.material.mainTexture = GetPageTex (toPageLeft + 1);
		} else {
			Front.material.mainTexture = Left.material.mainTexture;
			Back.material.mainTexture = GetPageTex (toPageLeft + 1);
			Left.material.mainTexture = GetPageTex (toPageLeft + 0);
		}
		this.UpdatePageTurn ();
	}

	void UpdatePageTurn() {
		if (!(this.TurnAutoActive || this.TurnHoverActive)) {
			if (TurnTimeAmount > 0) {
				TurnTimeAmount -= Time.deltaTime;
				if (TurnTimeAmount <= 0.0f) {
					TurnTimeAmount = 0.0f;
					TurnIsAnimating = false;
					this.SetupPages ();
				}
			} else {
				return;
			}
		} else {
			TurnTimeAmount += Time.deltaTime;
		}

		float t = (TurnTimeAmount / this.TurnDuration);
		t = Mathf.Max (t, 0.001f);
		if (t >= 1.0f) {
			this.TurnAutoActive = false;
			this.TurnIsAnimating = false;
			this.CurrentPage = this.TurnNextLeft;
			this.TurnTimeAmount = 0.0f;
			this.SetupPages ();
			if (OnPageChanged != null) {
				OnPageChanged (this.CurrentPage);
			}
			return;
		}
		t = Mathf.Pow (t, 2.5f);
		if (this.TurnDirection > 0) {
			t = 1.0f - t;
		}
		this.PageRotateOrigin.transform.localRotation = Quaternion.Euler (0, 0, t * -180.0f);
		var mt = (1.0f - Mathf.Abs((t * 2.0f) - 1.0f));
		this.PageSinkOrigin.transform.localPosition = new Vector3 (mt * 2.72f, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
		if (UseArrowKeys) {
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				this.ChangePageSimple (true);
			}
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				this.ChangePageSimple (false);
			}
			if (Input.GetKey (KeyCode.LeftBracket)) {
				this.SetIsHovering (true, false);
			}
			if (Input.GetKey (KeyCode.RightBracket)) {
				this.SetIsHovering (true, true);
			}
		}
		if (TurnHoverUpdateFrame < Time.frameCount - 2) {
			// auto unhover if we aren't getting and update
			this.SetIsHovering (false, false);
		}

		this.UpdatePageTurn ();
	}
}
