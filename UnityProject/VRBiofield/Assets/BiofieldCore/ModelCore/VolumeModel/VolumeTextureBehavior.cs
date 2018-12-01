using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[RequireComponent(typeof(VolumeSourceBehavior))]
public class VolumeTextureBehavior : MonoBehaviour {

	VolumeBuffer<Color> mColorBuffer;
	VolumeSourceBehavior mVolumeSource;
	int mFrameTest = 1;
	public bool IsLEWFlowField = false;
	public bool RefreshCache = false;

    public bool IsMultiChakras = false;
	public bool IsAura = false;
	public bool IsSphereMode = false;
	public int ChakraPetals = 0;
    public Color ChakraColor = Color.white;
	public Color ColorX = Color.white;
	public Color ColorY = Color.green;
    public bool ChakraOneWay = false;
	public bool RefreshDistances = false;
    private bool IsOnlyShowMultiChakra = false;

    private IEnumerator SlowlyExecuting = null;

    private VolumeTextureBehavior[] MultiChakras = null;

	public VolumeSourceBehavior VolumeSources {get {return this.mVolumeSource;}}

	// Use this for initialization
	void Start () {
        this.EnsureSetup();
		if (this.mVolumeSource.IsSlowerPlatform ()) {
			this.GetComponent<MeshRenderer> ().enabled = false;
		}
    }

    public void EnsureSetup() {
        if (this.mColorBuffer != null) return; // check if already setup

        Debug.Log("Setting up volume '" + this.gameObject.name + "'");

		if (mVolumeSource == null) {
			this.mVolumeSource = this.GetComponent<VolumeSourceBehavior>();
			this.mVolumeSource.EnsureSetup();
		}

		if (mVolumeSource.IsSlowerPlatform ())
			return;

		var fullsize = this.mVolumeSource.VolumeSize;


        if (IsOnlyShowMultiChakra && (!this.IsMultiChakras))
        {
            this.mColorBuffer = new VolumeBuffer<Color>(fullsize);
            this.mColorBuffer.ClearAll(Color.clear);
            this.UpdateTextureFromBuffer();
            return;
        }

        if ((!this.RefreshCache) && TryLoadCached())
        {
            this.UpdateTextureFromBuffer();
        }
        else
        {
		    this.mColorBuffer = new VolumeBuffer<Color> (fullsize);

			if (this.IsLEWFlowField) {
				this.SlowlyExecuting = this.DoFlowField().GetEnumerator();
				this.UpdateSlowlyExecuting();
			} else
			if (this.IsAura) {
				this.SlowlyExecuting = this.UpdateAura().GetEnumerator();
				this.UpdateSlowlyExecuting();
			}
            else if (this.IsMultiChakras)
            {
                this.SlowlyExecuting = this.UpdateMultiChakras().GetEnumerator();
                this.UpdateSlowlyExecuting();
            }
            else
            {
                this.DoFieldUpdate();
            }
        }
	}

    public bool TryLoadCached()
    {
        var sz = this.mVolumeSource.VolumeSize;
        var path = VolumeBufferFile.CacheFilePath(this.gameObject.name, sz);
        if (VolumeBufferFile.CacheFileExists(path))
        {
            Debug.Log("Loading from cache '" + path + "'");
            this.mColorBuffer = VolumeBufferFile.ReadVolFromFile(sz, path);
            return (this.mColorBuffer != null);
        }
        return false;
    }

    public void TrySaveToCache()
    {
        var sz = this.mVolumeSource.VolumeSize;
        var path = VolumeBufferFile.CacheFilePath(this.gameObject.name, sz);
        Debug.Log("Saving to cache '" + path + "'");
        if (this.mColorBuffer != null)
        {
            VolumeBufferFile.SaveColorVolToFile(this.mColorBuffer, path);
        }
    }

    public static Vector3 ToVector3(Cubic<double> vec)
    {
        return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
    }

    public static Cubic<SpanF> MultiplySpanByMatrix(Cubic<SpanF> span, Matrix4x4 mat)
    {
        SpanAPI.WeakTODO();

        // slow but accurate way
        bool doAllCorners = false;
        if (doAllCorners)
        {
            var ans = span
                .Select(k => k.EachOfFromAndTo()) // seperate from and to
                .EachPermutation() // generate all corner permutations
                .Select(k => ToVector3(k))
                .Select(k => mat.MultiplyPoint(k)) // transform by matrix
                .Select(k => k.ToCubic().Select(j => SpanAPI.spanExactly(j))) // back into spans
                .Aggregate((a, b) => a.Select2(b, (c, d) => SpanAPI.spanUnion(c, d))); // take union of all
            return ans;
        }

        // only doing the corners, ideally do all:
        var cornerAM = ToVector3(span.Select(k => k.From));
        var cornerBM = ToVector3(span.Select(k => k.To));

        var cornerAW = mat.MultiplyPoint(cornerAM);
        var cornerBW = mat.MultiplyPoint(cornerBM);

        return new Cubic<SpanF>(
            SpanAPI.spanUnsorted(cornerAW.x, cornerBW.x),
            SpanAPI.spanUnsorted(cornerAW.y, cornerBW.y),
            SpanAPI.spanUnsorted(cornerAW.z, cornerBW.z)
            );

    }

    public static Color MixColors(Color a, Color b) 
    {
        if (a.a > b.a) {
            return a;
        } else {
            return b;
        }
    }

	public static float CubicIntDist(Cubic<int> v1, Cubic<int> v2) {
		var c = v1.Select2 (v2, (a,b) => ((a - b) * (a - b))).Aggregate ((a,b) => (a + b));
		return Mathf.Sqrt ((float)c);
	}

    public void RequestAuraUpdate()
    {
        if (this.SlowlyExecuting == null)
        {
            this.SlowlyExecuting = this.UpdateAura().GetEnumerator();
            this.UpdateSlowlyExecuting();
        }
    }

    public static Color Colors_Scale(Color c, float s)
    {
        return new Color(c.r * s, c.g * s, c.b * s, c.a );
    }

    public static Color Colors_BlendOver(ChakraAuraSettings config, Color dst, Color src)
    {
        float sAlpha = src.a * config.SampleTranparency;
        Color comb = (Colors_Scale(dst, (1.0f - sAlpha)) + Colors_Scale(src, sAlpha));
        comb.a = Mathf.Max(dst.a, sAlpha);
        return comb;

    }

	private IEnumerable<bool> UpdateAura() 
	{
		Debug.Log("UPDATING AURA.");
		
		var m2w = this.transform.localToWorldMatrix;
		
		bool debugMe = true;
		
		var proj = SpanAPI.spanProjFromCubicDomainAndRange(
			mColorBuffer.Size.Select(k => SpanAPI.span(0, k-1)),
			Cubic<SpanF>.CreateSame( SpanAPI.span(-0.5, 0.5))
			);
		var invProj = SpanAPI.spanProjFromCubicDomainAndRange(
			Cubic<SpanF>.CreateSame(SpanAPI.span(-0.5, 0.5)),
			mColorBuffer.Size.Select(k => SpanAPI.span(0, k-1))
			);
		var invProjForMath = SpanAPI.spanProjFromCubicDomainAndRange(
			Cubic<SpanF>.CreateSame(SpanAPI.span(-0.5,0.5)),
			Cubic<SpanF>.CreateSame(SpanAPI.span(-1.0, 1.0))
			);

		VolumeBuffer<float> distances = null;
		bool needsDistances = false;
		bool needsSurface = true;
		var sz = this.mColorBuffer.Size;
		var path = VolumeBufferFile.CacheFilePath(this.gameObject.name + "_sdf", sz);
		var pathOpacity = VolumeBufferFile.CacheFilePath(this.gameObject.name + "_opacity", sz);
		if ((!this.RefreshDistances) && VolumeBufferFile.CacheFileExists(path)) {
			distances = VolumeBufferFile.ReadFloatVolFromFile(sz, path);
		}
		if ((!this.RefreshDistances) && VolumeBufferFile.CacheFileExists(pathOpacity)) {
			distances = VolumeBufferFile.ReadFloatVolFromFile(sz, pathOpacity);
			needsDistances = true;
			needsSurface = false;
		}
		if (distances == null) {
			distances = new VolumeBuffer<float>(sz);
			distances.ClearAll(-1.0f);
			needsDistances = true;
		}

		if (needsDistances) {

			if (needsSurface) {
				Debug.Log("AURA SURFACES...");
				// First see which points are touching:
				Color defaultVal = Color.clear;
				for (int i = 0; i < this.mColorBuffer.Length; i++) {
					var ndx = this.mColorBuffer.UnprojectIndex (i);
					var mpos = SpanAPI.spanProjCubicInt (proj, ndx);
					Color curColor = defaultVal;
					float curDistance = -1.0f;
				
					if (this.mColorBuffer.IsEdge (ndx)) {
						// it's an edge, ignore it
					}else {
						var center = mpos.Select (k => (k.From + k.To) * 0.5f).AsVector ();
						var radius = mpos.Select (k => Mathf.Abs ((float)((k.From - k.To) * 0.5f))).Aggregate ((a,b) => ((a + b) * 0.5f));
						var wcenter = m2w.MultiplyPoint (center);
						var wradius = m2w.MultiplyVector (Vector3.one * radius).magnitude;
						if (Physics.CheckSphere (wcenter, wradius)) {
							//Debug.Log ("Touched something");
							curColor = Color.white;
							curColor.a = 0.2f;
							curDistance = 0.0f;
						}
					}
				
					this.mColorBuffer.Write (ndx, curColor);
					distances.Write(ndx, curDistance );
				
					if (((i % 100)) == 99) {
						this.UpdateTextureFromBuffer ();
						yield return false;
					}			
				}
				VolumeBufferFile.SaveFloatVolToFile(distances, pathOpacity);
			}

			// Now calculate the actual distances:
			Debug.Log("AURA DISTANCES...");
			if (false)
			{
				// if this works:
				VolumeBufferUtil.ConvertOpacityToDistanceBuffer(distances);
			}
			else
			{
				// this works but is super slow:
				float maxDist = CubicIntDist( Cubic<int>.CreateSame(0), distances.Size );
				for (int i = 0; i < this.mColorBuffer.Length; i++) {
					var ndx = this.mColorBuffer.UnprojectIndex (i);

					var centerDist = distances.Read(ndx);
					if (centerDist < 0.0f) {
						// needs recalc, slowest possible way O(n^6):

						float closestDistance = -1.0f;
						for (int j=0; j<distances.Array.Length; j++) {
							if ( distances.Array[j] == 0.0f ) {
								float dist = CubicIntDist( distances.UnprojectIndex(j), ndx ) / maxDist;
								if ((closestDistance < 0.0f) || (dist < closestDistance)) {
									closestDistance = dist;
								}
							}
						}
						distances.Write(ndx, closestDistance);

						Color testColor = Color.white;
						testColor.a = closestDistance;
						this.mColorBuffer.Write (ndx, testColor);					
						if (((i % 100)) == 99) {
							this.UpdateTextureFromBuffer ();
							yield return false;
						}			

					}
				}
			}

			// Write out the results:
			VolumeBufferFile.SaveFloatVolToFile(distances, path);
		}

		ChakraAuraSettings auraSettings = this.GetComponent<ChakraAuraSettings>();


		if (auraSettings.UseChakraAuraDistance) {
			Debug.Log("AURA-TO-CHAKRA DISTANCES...");

			// modify distances based on chakra locality:
			var allChakras = this.gameObject
				.GetComponentInParent<ChakraControl>()
					.AllPoints
					.Where(k => ((!k.IsAura) && (!k.IsMultiChakras)))
					.Where(k => (!k.ChakraOneWay))
					.Select(k => Matrix4x4.TRS(k.transform.position, k.transform.rotation, Vector3.one).inverse)
					.ToArray();

			Debug.Log("Chakra Count = " + allChakras.Length);

			for (int li=0; li<distances.Array.Length; li++) {
				var ndx = this.mColorBuffer.UnprojectIndex (li);
				var mpos = SpanAPI.spanProjCubicInt (proj, ndx).Select(k => (k.From+k.To)*0.5f).AsVector();
				var wpos = m2w.MultiplyPoint(mpos);
				var dist = distances.Array[li];
				var origdist = dist;

				foreach (var c in allChakras){
					var lpos = c.MultiplyPoint(wpos);
					var ldist = Mathf.Sqrt((lpos.x * lpos.x) + (lpos.z * lpos.z)) / (Mathf.Abs(lpos.y) * 1.5f);
					ldist = 0.5f - (ldist * ldist);
					if (ldist > dist) {
						dist = ldist; //(origdist - ldist);
					}
				}

				//if (origdist == dist) dist = 0.0f; // HACK REMOVE

				distances.Array[li] = dist;

				if (((li % 100)) == 99) {
					float pct = ((float)li) / ((float)distances.Array.Length);
					Debug.Log("Updating aura-chakra distance (" + (pct*100.0f) + "%)");
					yield return false;
				}	
			}
		}


		// Calculate the colors from the distances:
        float[] idealDistances = auraSettings.AuraDistances;// { 0.0165f, 0.05f, 0.1f };
        Color[] idealColors = auraSettings.AuraColors; // { Color.red, Color.green, new Color(0.0f, 1.0f, 1.0f, 1.0f) };
		//float IdealDistance = 0.0165f;
		//float AuraWidth = 0.0125f;
		Debug.Log ("AURA COLORS...");
        float previousDist = 0.0f;
		for (int i = 0; i < this.mColorBuffer.Length; i++) {
			var ndx = this.mColorBuffer.UnprojectIndex (i);

			var dist = distances.Read(ndx);

            dist = dist * dist; // 1.0f / (dist * dist);

            Color resColor = Color.clear;
            for (int layerNdx = idealDistances.Length - 1; layerNdx >= 0 ; layerNdx--)
            {
                var edgeDist = idealDistances[layerNdx] * auraSettings.WholeScaleDistances;

                if (dist < edgeDist)
                {
                    var edgeColor = idealColors[layerNdx];
                    var nextDist = 0.0f; // ((layerNdx > 0) ? (idealDistances[layerNdx - 1] * auraSettings.WholeScaleDistances) : 0.0f);

                    float strength = Mathf.Clamp01(1.0f - (Mathf.Abs(dist - edgeDist) / (edgeDist - nextDist)));
                    Color curColor = edgeColor;
                    curColor.a = curColor.a * strength;

                    resColor = curColor;// Colors_BlendOver(auraSettings, resColor, curColor);
                }
                previousDist = edgeDist;
            }

			this.mColorBuffer.Write(ndx, resColor );
			if (((i % 100)) == 99) {
				Debug.Log("cur = " + dist);
				this.UpdateTextureFromBuffer ();
				yield return false;
			}

            while (auraSettings.IsPauseCalculation)
            {
                yield return false;
            }
		}
		this.mColorBuffer.ClearEdges (Color.clear);
		
		//return;
		
		this.UpdateTextureFromBuffer();
		this.TrySaveToCache();
		
		Debug.Log("AURA UPDATED!");
	}

    public bool MultiChakraFullEval = false;

    public IEnumerable<bool> UpdateMultiChakras()
    {
        this.MultiChakras = this.gameObject.transform.parent
			.GetComponentsInChildren<VolumeTextureBehavior>()
				//.Where(k => (k.IsAura)||(k.name.Contains("Solar"))).ToArray()
				;
        List<Matrix4x4> worldToChakras = new List<Matrix4x4>();

        foreach (var c in this.MultiChakras)
        {
            c.EnsureSetup();
            worldToChakras.Add(c.transform.worldToLocalMatrix);
        }

        Debug.Log("MULTI CHAKRA from " + this.MultiChakras.Length + " children.");

        var m2w = this.transform.localToWorldMatrix;

        bool debugMe = true;

        var proj = SpanAPI.spanProjFromCubicDomainAndRange(
            mColorBuffer.Size.Select(k => SpanAPI.span(0, k-1)),
            Cubic<SpanF>.CreateSame( SpanAPI.span(-0.5, 0.5))
            );
        var invProj = SpanAPI.spanProjFromCubicDomainAndRange(
            Cubic<SpanF>.CreateSame(SpanAPI.span(-0.5, 0.5)),
            mColorBuffer.Size.Select(k => SpanAPI.span(0, k-1))
            );
        var invProjForMath = SpanAPI.spanProjFromCubicDomainAndRange(
            Cubic<SpanF>.CreateSame(SpanAPI.span(-0.5,0.5)),
            Cubic<SpanF>.CreateSame(SpanAPI.span(-1.0, 1.0))
            );

        //var proj = SpanAPI.Example_SetupProjection (mColorBuffer.Size.X, mColorBuffer.Size.Y, mColorBuffer.Size.Z);
        //var invProj = SpanAPI.Example_SetupSignedUnitToIntProjection(mColorBuffer.Size.X, mColorBuffer.Size.Y, mColorBuffer.Size.Z);
        Color defaultVal = Color.clear;
        for (int i = 0; i < this.mColorBuffer.Length; i++)
        {
            var ndx = this.mColorBuffer.UnprojectIndex(i);
            var mpos = SpanAPI.spanProjCubicInt(proj, ndx);
            Color curColor = defaultVal;

            if (this.mColorBuffer.IsEdge(ndx))
            {
                // it's an edge, ignore it
            }
            else
            {

                //foreach (var chakra in this.MultiChakras)
                for (int ci = 0; ci < this.MultiChakras.Length; ci++ )
                {
                    var chakra = this.MultiChakras[ci];
                    var w2c = worldToChakras[ci];

                    var cpos = MultiplySpanByMatrix(mpos, w2c * m2w);// m2w * w2c);
                    //cpos = cpos.Select(k => SpanAPI.spanMult(k, SpanAPI.spanExactly(1.5)));

                    if (MultiChakraFullEval && (!chakra.IsAura))
                    {
                        var suSpan = SpanAPI.spanProjCubicSpan(invProjForMath, cpos);
                        var suVal = chakra.ChakraFieldFromSignedUnit(suSpan);
                        var suColor = chakra.ColorFromChakraField(suVal);
                        curColor = MixColors(curColor, suColor);
                        continue;
                    }


                    var fspan = SpanAPI.spanProjCubicSpan(invProj, cpos);
                    fspan = fspan.Select(k => SpanAPI.spanMult(k, SpanAPI.spanExactly(1.01)));
                    var ispan = fspan.Select(k => SpanI.FromSpanF(k));
                    var sample = chakra.mColorBuffer.SampleSpan(ispan, Color.clear, (a, b) => MixColors(a, b));

                    if (debugMe)
                    {
                        debugMe = false;
                        Debug.Log("proj = " + proj);
                        Debug.Log("invProj = " + invProj);

                        Debug.Log("ndx = " + ndx);
                        Debug.Log("mpos = " + mpos);
                        var testInvPos = SpanAPI.spanProjCubicSpan(invProj, mpos);
                        Debug.Log("testInvPos = " + testInvPos);
                        var testISpan = testInvPos.Select(k => SpanI.FromSpanF(k));
                        Debug.Log("testISpan = " + testISpan);

                        Debug.Log("mpos = " + mpos);
                        Debug.Log("cpos = " + cpos);
                        Debug.Log("fspan = " + fspan);
                        Debug.Log("ispan = " + ispan);
                        Debug.Log("sample = " + sample);
                    }

                    curColor = MixColors(curColor, sample);
                }
            }

            this.mColorBuffer.Write(ndx, curColor);

            if (((i % 100)) == 99)
            {
                this.UpdateTextureFromBuffer();
                yield return false;
            }

        }

        //return;

        this.UpdateTextureFromBuffer();
        this.TrySaveToCache();

        Debug.Log("MULTI CHAKRAS UPDATED!");
    }

    public Color ColorFromChakraField(SpanF pct)
    {
        Color curColor = this.ChakraColor;
        curColor.a = Mathf.Clamp01((float)(((pct.From + pct.To) * 0.5) * 1.0));
        return curColor;
    }

    public SpanF ChakraFieldFromSignedUnit(Cubic<SpanF> pos)
    {
        return SpanAPI.Example_ChakraV1(this.ChakraPetals, pos, this.ChakraOneWay);
    }

	
	private static Color ColorWithAlpha(Color c, float a) {
		return new Color (c.r, c.g, c.b, a);
	}


	private FlowVertexNode[] CachedFlowVerts = null;

	public Cubic<SpanF> spanVectorFromUnity(Vector3 pos, float radius) {
		return new Cubic<SpanF> (
			new SpanF (pos.x - radius, pos.x + radius),
			new SpanF (pos.y - radius, pos.y + radius),
			new SpanF (pos.z - radius, pos.z + radius));
	}

	private double PreviousRY = -100.0;

	private Color EvalFlowVolume(Vector3 center, float radius, Cubic<SpanF> modelPos) {
		float forces = 0.0f;
		float forceM = 0.0f;
		float chargeE = 0.008f;
		float chargeM = 0.014f;
		Vector3 flowDir = Vector3.zero;

		var centerQ = spanVectorFromUnity (center, radius);
		var torqueQ = spanVectorFromUnity (Vector3.zero, 0.0f);
		float flowVertRadius = 0.04f;
		float flowVertFwdRadius = 0.02f;
		var magTorqueCharge = SpanF.exactly (0.021);

		if (false) {
			var rv = modelPos.Y.times(SpanF.exactly(2.0));
			var rc = SpanAPI.spanColorSignedToRGB(rv);
			var clr = new Color(rc.X, rc.Y, rc.Z, 0.9f);
			if (PreviousRY != rv.To) {
				Debug.Log("M/G: " + rv + "    " + rc.Y );
				PreviousRY = rv.To;
			}
			return clr;
		}

		if (true) {

			foreach (var fv in this.CachedFlowVerts) {


				var dist = (fv.Location - center).magnitude;
				var oneOverDistSqr = (1.0f / (dist * dist));
				var fe = chargeE * oneOverDistSqr;

				if (false)
				{
					var locQ = spanVectorFromUnity (fv.Location, flowVertRadius);
					var fQ = spanVectorFromUnity (fv.ForwardDir, flowVertFwdRadius);
					var rQ = SpanAPI.spanVectorSubtract (centerQ, locQ);
					var cQ = SpanAPI.spanVectorCross (rQ, fQ);
					//var cQ = SpanAPI.spanVectorDot(rQ, fQ);
					var scQ = magTorqueCharge.times (SpanF.exactly (oneOverDistSqr));
					cQ = SpanAPI.spanVectorScale (cQ, scQ);
					torqueQ = SpanAPI.spanVectorAdd (torqueQ, cQ);
				}

				var fm = chargeM * oneOverDistSqr * Vector3.Dot (fv.Location - center, -fv.ForwardDir);

				forces += fe;
				flowDir += fv.ForwardDir * fe;
				//forceM += fm;
			}
		}

		if (false) {
			var torqueMagQ = SpanAPI.spanVectorMagnitude (torqueQ);
			var rawVal = torqueMagQ;
			Debug.Log ("Val = " + rawVal);
			//var toColorRangeOffset = SpanAPI.spanExactly (1.0);
			var toColorRangeScale = SpanAPI.spanExactly (1.0);
			//var rgb = torqueMagQ.add(toColorRangeOffset).times(toColorRangeScale).ToColorRGB ();

			var valInRange = rawVal.times(SpanAPI.spanExactly(2.0f/2.6f)).add(SpanAPI.spanExactly(-1.0f));
			var rgb = valInRange.ToColorRGB ();
			//rgb = new Cubic<float> (0, (float)SpanAPI.spanColorSignedForGreen (valInRange).To, 0);
			var alpha = rgb.Y; // Mathf.Max(Mathf.Max(rgb.X,rgb.Y),rgb.Z) * 0.5f;
			return new Color (rgb.X, rgb.Y, rgb.Z, alpha);
		}

		flowDir.y *= 10.0f;
		var yval = Mathf.Clamp01 (Mathf.Abs (flowDir.normalized.y));
		var dirColor = Color.Lerp (this.ColorX, this.ColorY, yval);

		var clearColor = this.ChakraColor;
		var fullColor = dirColor; //ColorWithAlpha (dirColor, 1.0f);
		return Color.Lerp (clearColor, fullColor, Mathf.Clamp01( forces ));
	}

	[ContextMenu ("Test Color System")]
	void ContextMenu_TestColorSystem () {
		int maxi = 10;
		double dm = 1.0 / maxi;
		for (int i=-maxi; i<maxi; i++) {
			var fi = ((double)i) / ((double)(maxi/2));
			var s = SpanF.Create(fi, fi + dm);
			var g = SpanAPI.spanColorSignedForGreen(s);
			Debug.Log("s=g: " + s + "=" + g);
		}
	}

	public IEnumerable<bool> DoFlowField() {

		var verts = GameObject.FindObjectsOfType<FlowVertexNode> ();
		this.CachedFlowVerts = verts;

		
		var proj = SpanAPI.spanProjFromCubicDomainAndRange(
			mColorBuffer.Size.Select(k => SpanAPI.span(0, k-1)),
			Cubic<SpanF>.CreateSame( SpanAPI.span(-0.5, 0.5))
			);
		var m2w = this.transform.localToWorldMatrix;

		var clearWhite = new Color (1, 1, 1, 0);
		var clearBlack = new Color (0, 0, 0, 0);
		//var curColor = Color.black;
		for (int i=0; i<this.mColorBuffer.Length; i++) {
			var ndx = this.mColorBuffer.UnprojectIndex(i);
			var mpos = SpanAPI.spanProjCubicInt (proj, ndx);

			bool actMe = false;
			var clr = clearBlack; // clearWhite;
			
			var rgb = ndx.Select2(this.mColorBuffer.Size, (di,sz) => ((float)di)/((float)(sz-1)));
			//curColor = new Color(rgb.X, rgb.Y, rgb.Z);
			
			
			if (this.mColorBuffer.IsEdge(ndx)) {
				actMe = false;
			} else {
				var center = mpos.Select (k => (k.From + k.To) * 0.5f).AsVector ();
				var radius = mpos.Select (k => Mathf.Abs ((float)((k.From - k.To) * 0.5f))).Aggregate ((a,b) => ((a + b) * 0.5f));
				var wcenter = m2w.MultiplyPoint (center);
				var wradius = m2w.MultiplyVector (Vector3.one * radius).magnitude;
				clr = EvalFlowVolume(wcenter, wradius, mpos);
				//if (Physics.CheckSphere (wcenter, wradius)) {
				//	actMe = true;
				//}
				
			}
			
			//var clr = ((actMe) ? curColor : Color.clear );
			
			
			this.mColorBuffer.Array[i] = clr;

			int updateEvery = 200;
			if (((i % updateEvery)) == (updateEvery-1))
			{
				this.UpdateTextureFromBuffer();
				yield return false;
			}
		}
		
		
		this.UpdateTextureFromBuffer();
		
		this.TrySaveToCache();

		this.RefreshCache = false;
	}

	public void DoFieldUpdate() {

		var proj = SpanAPI.Example_SetupProjection (mColorBuffer.Size.X, mColorBuffer.Size.Y, mColorBuffer.Size.Z);
		var curColor = Color.white;
		for (int i=0; i<this.mColorBuffer.Length; i++) {
			var ndx = this.mColorBuffer.UnprojectIndex(i);

			bool actMe;

			var rgb = ndx.Select2(this.mColorBuffer.Size, (di,sz) => ((float)di)/((float)(sz-1)));
			curColor = new Color(rgb.X, rgb.Y, rgb.Z);


			if (this.mColorBuffer.IsEdge(ndx)) {
				actMe = false;
			} else {
				if (this.ChakraPetals > 0) {
					Debug.Log("Doing chakra test 1...");
                    var qpos = SpanAPI.spanProjCubicInt(proj, ndx);
                    var pct = this.ChakraFieldFromSignedUnit(qpos);
					//var pct = SpanAPI.Example_EvaluateFieldChakraV1(proj, ndx, this.ChakraPetals, this.ChakraOneWay);
					actMe = true;
					// = SpanAPI.Example_SpanToColor(pct);
                    curColor = ColorFromChakraField(pct);
				} else if (IsSphereMode) {
					actMe = SpanAPI.Example_EvaluateField1(proj, ndx);
				} else {
					var dv = SpanAPI.Example_EvaluateField3(proj, ndx);
					actMe = true;
					curColor = SpanAPI.Example_SpanToColor(dv);
					//bool isZero = SpanAPI.spanContains(dv, 0.0);
					//var avg = (float)((dv.From + dv.To)*0.5f);
					//curColor.a = avg;
					//actMe = SpanAPI.spanContains(dv,0.5);// SpanAPI.Example_EvaluateField2(proj, ndx);
				}

			}

			var clr = ((actMe) ? curColor : Color.clear );


			this.mColorBuffer.Array[i] = clr;
		}


        this.UpdateTextureFromBuffer();

        this.TrySaveToCache();
	}

    public void UpdateTextureFromBuffer()
    {
        this.mVolumeSource.VolumeTexture.SetPixels(this.mColorBuffer.Array);
        this.mVolumeSource.VolumeTexture.Apply();

    }

    private void UpdateSlowlyExecuting()
    {
        if (this.SlowlyExecuting != null)
        {
            if (!this.SlowlyExecuting.MoveNext())
            {
                this.SlowlyExecuting = null;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		//this.mFrameTest++;
		//this.mFrameTest = (this.mFrameTest % (this.mBuffer.Length));
		//this.mBuffer.Array [0].SetActive (this.mFrameTest % 2 == 0);
        this.UpdateSlowlyExecuting();

	}
}
