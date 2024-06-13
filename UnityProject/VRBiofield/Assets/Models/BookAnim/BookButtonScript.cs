using UnityEngine;
using System.Collections;

public class BookButtonScript : MonoBehaviour {

	public bool IsTurnForward;
	private BookAnimScript Book;
	private Collider MyCollider;
	private bool IsHovering = false;
	private MeshRenderer MyRenderer;
	public Texture WhiteTexture = null;
	private Texture ArrowTexture = null;

	// Use this for initialization
	void Start () {
		Book = this.GetComponentInParent<BookAnimScript> ();
		this.MyCollider = this.GetComponent<Collider> ();
		this.MyRenderer = this.GetComponent<MeshRenderer> ();
		this.UpdateIsHovering (false, true);
	}

	bool EffecientRayCheck(Ray r) {
		//if (Vector3.Dot ((this.transform.position - r.origin).normalized, r.direction.normalized) > 0.8f) 
		{
			RaycastHit info;
			if (this.MyCollider.Raycast (r, out info, 10000.0f)) {
				return true;
			}
		}
		return false;
	}

	Color cachedColor = Color.yellow;
	private void UpdateIsHovering(bool isOver, bool fullUpdate=false) {
		
		if ((this.IsHovering != isOver) || (fullUpdate)) {
			this.IsHovering = isOver;
		}
		var isValidArrow = Book.CanChangePageInDirection (this.IsTurnForward);
		this.MyRenderer.enabled = isValidArrow;
		this.MyCollider.enabled = isValidArrow;
		
		var clr = isValidArrow ? Book.ButtonColorActive : Color.black;
		clr = (this.IsHovering ? clr : Book.ButtonColorPassive);
		if (this.cachedColor != clr) {
			this.cachedColor = clr;
			this.MyRenderer.material.color = clr;
			if (this.WhiteTexture != null) {
				if (this.ArrowTexture == null) {
					this.ArrowTexture = this.MyRenderer.material.mainTexture;
				}
				var t = (isValidArrow ? this.ArrowTexture : this.WhiteTexture);
				this.MyRenderer.material.mainTexture = t;
			}
		}

		if (this.IsHovering && FocusRay.main.IsHeadGaze) {
			Book.SetIsHovering (true, this.IsTurnForward);
		}
	}
	
	// Update is called once per frame
	void Update () {
		var ray = FocusRay.main.CurrentRay;
		if (Input.GetMouseButtonUp (0)) {
			//var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (EffecientRayCheck (ray)) {
				Book.ChangePageSimple (IsTurnForward, true);
			}
		}

		//if (FocusRay.main.IsHeadGaze) 
		{
			if (EffecientRayCheck (ray)) {
				this.UpdateIsHovering (true);
			} else {
				this.UpdateIsHovering (false);
			}
		}
	}
}
