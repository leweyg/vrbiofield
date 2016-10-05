using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Cubic<T>
{
	private T[] mArray;

	public Cubic() {
		mArray = new T[3];
	}

	public Cubic(T a, T b, T c) {
		mArray = new T[]{ a, b, c };
	}

	public static Cubic<T> Create(T a, T b, T c) {
		return new Cubic<T> (a, b, c);
	}

	public static Cubic<T> CreateSame(T a) {
		return new Cubic<T> (a, a, a);
	}

	public T X {
		get { return mArray [0]; }
		set { mArray [0] = value; }
	}

	public T Y {
		get { return mArray [1]; }
		set { mArray [1] = value; }
	}

	public T Z {
		get { return mArray [2]; }
		set { mArray [2] = value; }
	}

	public Cubic<X> Select<X>(Func<T,X> selector) {
		return new Cubic<X> (selector (this.X), selector (this.Y), selector (this.Z));
	}

	public Cubic<Y> Select2<X,Y>(Cubic<X> other, Func<T,X,Y> selector) {
		return new Cubic<Y> (
			selector (this.X, other.X),
			selector (this.Y, other.Y),
			selector (this.Z, other.Z));
	}

	public T Aggregate(Func<T,T,T> combiner) {
		return combiner (combiner (this.X, this.Y), this.Z);
	}

    public override string ToString()
    {
        return "{" + this.Select(k => k.ToString()).Aggregate((a, b) => a + "," + b) + "}";
    }
}

public class VolumeHeader {
	public readonly Int3 Size;
	public readonly int TotalCount;

	public VolumeHeader(Int3 sz) {
		this.Size = sz;
		this.TotalCount = sz.X * sz.Y * sz.Z;
	}

	public int CubicToLinear(Int3 pos) {
		return (pos.X + (pos.Y * Size.X) + (pos.Z * this.Size.X * this.Size.Y));
	}
	
	public Int3 LinearToCubic(int ndx) {
		return new Int3 (
			((ndx / (1)) % this.Size.X),
			((ndx / (this.Size.X)) % this.Size.Y),
			((ndx / (this.Size.X * this.Size.Y)) % this.Size.Z));
	}

	public bool IsSafe(Int3 ndx) {
		return ((ndx.X >= 0) && (ndx.Y >= 0) && (ndx.Z >= 0)
			&& (ndx.X < Size.X) && (ndx.Y < Size.Y) && (ndx.Z < Size.Z));
	}
}

public class VolumeBuffer<T> {
	public VolumeHeader Header;
	public T[] Array { get; private set; }

	public VolumeBuffer(Cubic<int> sz) {
		this.Header = new VolumeHeader (new Int3(sz));
		int count = sz.X * sz.Y * sz.Z;
		this.Array = new T[count];
	}

    public Cubic<int> Size { get { return this.Header.Size.AsCubic(); } }

	public Cubic<int> UnprojectIndex(int ndx) {
		return new Cubic<int> (
			((ndx / (1)) % this.Size.X),
			((ndx / (this.Size.X)) % this.Size.Y),
			((ndx / (this.Size.X * this.Size.Y)) % this.Size.Z));
	}

	public IEnumerable<Int3> AllIndices3() {
		for (int i=0; i<this.Array.Length; i++) {
			var pnt = this.UnprojectIndex(i);
			yield return new Int3(pnt);
		}
	}

	public bool IsSafe(Int3 pos) {
		return this.Header.IsSafe (pos);
	}

	public T this[Int3 ndx] {
		get {
			if (this.Header.IsSafe(ndx)) 
				return this.Array[this.Header.CubicToLinear(ndx)];
			return default(T);
		}
		set {
			if (this.Header.IsSafe(ndx))
				this.Array[this.Header.CubicToLinear(ndx)] = value;
			else {
				//throw new IndexOutOfRangeException();
			}
		}
	}

	public bool IsSafe(Cubic<int> pos) {
		return pos.Select2 (this.Size, (i,sz) => ((i >= 0) && (i < sz))).AllTrue ();
	}

	public bool IsEdge(Cubic<int> pos) {
		return pos.Select2 (this.Size, (i,sz) => ((i == 0) || ((i + 1) == sz))).AnyTrue();
	}

	public T Read(Cubic<int> pos) {
		if (this.IsSafe (pos)) {
			return this.Array [this.Header.CubicToLinear(new Int3(pos))];
		} else {
			return default(T);
		}
	}

	public void Write(Cubic<int> pos, T val) {
		if (this.IsSafe (pos)) {
			this.Array [this.Header.CubicToLinear(new Int3 (pos))] = val;
		}
	}

    public T SampleSpan(Cubic<SpanI> span, T defVal, Func<T,T,T> combiner)
    {
        bool hasFirst = false;
        T res = defVal;
        for (int iz = span.Z.From; iz <= span.Z.To; iz++)
        {
            for (int iy = span.Y.From; iy <= span.Y.To; iy++)
            {
                for (int ix = span.X.From; ix <= span.X.To; ix++)
                {
                    Cubic<int> curNdx = new Cubic<int>(ix, iy, iz);
                    if (this.IsSafe(curNdx))
                    {
                        T val = this.Read(curNdx);
                        if (hasFirst)
                        {
                            res = combiner(res, val);
                        }
                        else
                        {
                            hasFirst = true;
                            res = val;
                        }
                    }
                }
            }
        }
        return res;
    }

	public int Length
	{
		get { 
			return this.Array.Length; 
		}
	}

    public void ClearAll(T val)
    {
        for (int i = 0; i < this.Array.Length; i++)
        {
            this.Array[i] = val;
        }
    }

    public void ClearEdges(T val)
    {
        for (int i = 0; i < this.Array.Length; i++)
        {
            var ndx = this.UnprojectIndex(i);
            if (this.IsEdge(ndx))
            {
                this.Array[i] = val;
            }
        }
    }
}

public abstract class VolumeBufferUtil
{
//	public static IEnumerable<Int3> AllIndices_pXpYpZ(VolumeHeader header) {
//		for (int i=0; i<header.TotalCount; i++) {
//			var pnt = header.LinearToCubic(i);
//			yield return new Int3(pnt);
//		}
//
//	}
	
	public static void ConvertOpacityToDistanceBuffer(VolumeBuffer<float> opac) 
	{
//		VolumeBuffer<Int3> nearest = new VolumeBuffer<Int3> (opac.Size);
//		Int3 prevPos = new Cubic<int> (0, 0, 0);
//		float prevOpac = opac.Read (prevPos);
//		foreach (var i in AllIndices_pXpYpZ(opac.Header)) {
//			var curOpac = opac.Read(i);
//			if (curOpac >= prevOpac) {
//				prevPos = i;
//				prevOpac = curOpac;
//				nearest.Write(i,i);
//			}
//			else {
//				nearest.Write(i, prevPos);
//			} 
//		}
		
	}
}

public struct Int3 {
	public int X, Y, Z;

	public Int3(int a, int b, int c) {
		X = a; Y = b; Z = c;
	}
	public Int3(Cubic<int> v) : this(v.X, v.Y, v.Z) { }

	public Int3 Select(Func<int,int> f) {
		return new Int3 (f (X), f (Y), f (Z));
	}

	public Int3 SelectWith(Int3 v, Func<int,int,int> f) {
		return new Int3 (f (X, v.X), f (Y, v.Y), f (Z, v.Z));
	}

	public Int3 Add(Int3 v) {
		return this.SelectWith (v, (a,b) => (a + b));
	}

	public bool Any(Func<int,bool> f) {
		return f (X) || f (Y) || f (Z);
	}
	
	public Int3 ShiftDownOne() {
		return this.Select (k => (k >> 1));
	}

	public bool AnyZero() {
		return this.Any(k => (k == 0));
	}

	public Cubic<int> AsCubic() {
		return new Cubic<int> (X, Y, Z);
	}
}

public class VolumeBufferMip<T> 
{
	public VolumeBuffer<T>[] Mips;

	public VolumeBufferMip(Int3 size) {
		int h = MipHeight (size.X);
		Mips = new VolumeBuffer<T>[h];
		var curSize = size;
		for (int i=0; i<h; i++) {
			Mips[i] = new VolumeBuffer<T>(curSize.AsCubic());
			curSize = curSize.ShiftDownOne();
		}
	}

	public void SpreadValuesUp(int mip, Func<T,T,T> comb) {
		if (mip + 1 >= this.Mips.Length)
			return;
		var big = this.Mips [mip];
		var sml = this.Mips [mip + 1];
		var h2 = new VolumeHeader (new Int3 (2, 2, 2));
		foreach (var si in sml.AllIndices3()) {
			var res = big.Read(si.AsCubic());
			for (int n=1; n<h2.TotalCount; n++) {
				var ni = si.Add(h2.LinearToCubic(n));
				var sec = big.Read(ni.AsCubic());
				res = comb(res,sec);
			}
			sml.Write(si.AsCubic(),res);
		}
	}

	public void SpreadValuesUp(Func<T,T,T> comb) {
		for (int i=1; i<Mips.Length; i++) {
			this.SpreadValuesUp(i, comb);
		}
	}

	public static int MipHeight(int sz) {
		int c = 0;
		while (sz > 0) {
			sz = (sz >> 1);
			c++;
		}
		return c;
	}


	public VolumeBuffer<T> GetMip(int i) {
		return this.Mips [i];
	}
}

public class VolumeBufferFile
{
    public static string CacheFilePath(string baseName, Cubic<int> sz)
    {
        baseName = baseName.Replace(" ", "_");
        string szName = "_" + sz.X + "_" + sz.Y + "_" + sz.Z;
        var path = UnityEngine.Application.dataPath + "/SavedVolumes/" + baseName + szName + ".Colors";
        return path;
    }

    public static bool CacheFileExists(string filepath)
    {
        return System.IO.File.Exists(filepath);
    }

    public static void SaveColorVolToFile(VolumeBuffer<Color> vol, string filename)
    {
        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.FileStream(filename, System.IO.FileMode.Create));
        for (int i = 0; i < vol.Array.Length; i++)
        {
            var c = vol.Array[i];
            bw.Write(c.r);
            bw.Write(c.g);
            bw.Write(c.b);
            bw.Write(c.a);
        }
        bw.Close();
    }

    public static VolumeBuffer<Color> ReadVolFromFile(Cubic<int> size, string filename)
    {
        System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.FileStream(filename, System.IO.FileMode.Open));
        VolumeBuffer<Color> ans = new VolumeBuffer<Color>(size);
        for (int i = 0; i < ans.Array.Length; i++)
        {
            Color c = new Color();
            c.r = br.ReadSingle();
            c.g = br.ReadSingle();
            c.b = br.ReadSingle();
            c.a = br.ReadSingle();
            ans.Array[i] = c;
        }
        br.Close();
        return ans;
    }

	public static void SaveFloatVolToFile(VolumeBuffer<float> vol, string filename)
	{
		System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.FileStream(filename, System.IO.FileMode.Create));
		for (int i = 0; i < vol.Array.Length; i++)
		{
			var c = vol.Array[i];
			bw.Write(c);
		}
		bw.Close();
	}
	
	public static VolumeBuffer<float> ReadFloatVolFromFile(Cubic<int> size, string filename)
	{
		System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.FileStream(filename, System.IO.FileMode.Open));
		VolumeBuffer<float> ans = new VolumeBuffer<float>(size);
		for (int i = 0; i < ans.Array.Length; i++)
		{
			float c = br.ReadSingle();
			ans.Array[i] = c;
		}
		br.Close();
		return ans;
	}
}

public class SpanOf<T> {
	public T From { get; set; }
	public T To { get; set; }

	public SpanOf() {}
	public SpanOf(T _f, T _t) { 
		this.From = _f;
		this.To = _t;
	}

	public override string ToString ()
	{
		return string.Format ("{0}~{1}", From, To);
	}
}

public class SpanF : SpanOf<double> {
	public SpanF(double f, double t) {
		this.From = f;
		this.To = t;
	}
	public static SpanF Create(double f, double t) {
        // could call spanCheck here...
		return new SpanF (f, t);
	}
    public static SpanF CreateInvalid(double f, double t)
    {
        // intentionally don't call 'spanCheck' here.
        return new SpanF(f, t);
    }

	public static SpanF exactly(double f) {
		return SpanAPI.spanExactly (f);
	}

	public SpanF add(SpanF other) {
		return SpanAPI.spanAdd (this, other);
	}

	public SpanF minus(SpanF other) {
		return SpanAPI.spanSubtract (this, other);
	}
	
	public SpanF times(SpanF other) {
		return SpanAPI.spanMult (this, other);
	}
	
	public SpanF cos() {
		return SpanAPI.spanCos (this);
	}

	public SpanF boundedBy(SpanF range) {
		return SpanAPI.spanBounded (this, range);
	}

	public Cubic<float> ToColorRGB() {
		return SpanAPI.spanColorSignedToRGB(this); 
	}
}

public class SpanI : SpanOf<int> {
	public SpanI(int f, int t) {
		this.From = f;
		this.To = t;
	}

	public static SpanI Create(int f, int t) {
		return new SpanI (f, t);
	}

	public IEnumerable<int> Steps() {
		for (int i=this.From; i<this.To; i++) {
			yield return i;
		}
	}

    public static SpanI FromSpanF(SpanF _s)
    {
        SpanAPI.spanCheck(_s);
        return new SpanI((int)_s.From, (int)_s.To);
    }
}

public class SpanAPI {

	// Span Creation Methods:
	
	public static SpanF span(double from, double to) {
		var ans = SpanF.Create (from, to);
		spanCheck (ans);
		return ans;
	}

	public static void spanCheck(SpanF chk) {
		if (double.IsInfinity (chk.From) || double.IsInfinity (chk.To)) {
			return;
		}
		if (double.IsNaN (chk.From) || double.IsNaN (chk.To)) {
			return;
			//throw new ArgumentOutOfRangeException("Span contains NaN: " + chk.ToString() );
		}
		if ((chk.To >= chk.From)) {
		} else {
			throw new ArgumentOutOfRangeException("Not a valid span!: " + chk.ToString() );
		}

	}

	public static bool BreakOnWeakTODO = false;
	public static void WeakTODO() {
		if (BreakOnWeakTODO) {
			throw new InvalidOperationException("Weak TODO");
		}
	}

	public static SpanF spanUnsorted(double _a,double _b)
	{
		if (_a <= _b) return span(_a, _b);
		else return span(_b, _a);
	}

    public static SpanF spanCreateInvalid(double _a, double _b)
    {
        return SpanF.CreateInvalid(_a, _b);
    }

	public static SpanF spanUnsorted4(double _a, double _b, double _c, double _d) {
		var mn = Math.Min (Math.Min (_a, _b), Math.Min (_c, _d));
		var mx = Math.Max (Math.Max (_a, _b), Math.Max (_c, _d));
		return span (mn, mx);
	}
	
	public static SpanF spanAround(double _v, double _delta) {
		var ad = Math.Abs(_delta);
		return span(_v - ad, _v + ad);
	}
	
	public static SpanF spanExactly(double _val) {
		return span(_val, _val);
	}

	
	public static SpanF spanOffsetAndSize(double _offset,double _size)
	{
		return span(_offset, _offset + _size);
	}
	
	public static SpanF spanFromInt(int _ival)
	{
		return span(_ival, _ival + 1);
	}

    public static SpanF spanUnion(SpanF _a, SpanF _b)
    {
        // TODO: could impliment this better
        return spanUnsorted4(_a.From, _a.To, _b.From, _b.To);
    }
	
	// Span Arithmetic
	
	public static SpanF spanAdd(SpanF _sa,SpanF  _sb)
	{
		spanCheck(_sa);
		spanCheck(_sb);
		return span(_sa.From + _sb.From, _sa.To + _sb.To);
	}
	
	public static SpanF spanMult(SpanF _sa,SpanF  _sb)
	{
		spanCheck(_sa);
		spanCheck(_sb);
		return spanUnsorted(_sa.From * _sb.From, _sa.To * _sb.To);
	}

	public static SpanF spanDivide(SpanF _sa,SpanF  _sb)
	{
		spanCheck(_sa);
		spanCheck(_sb);
		return spanUnsorted4 (_sa.From / _sb.From, _sa.From / _sb.To, _sa.To / _sb.From, _sa.To / _sb.To);
		//return spanUnsorted(_sa.From / _sb.From, _sa.To / _sb.To);
	}
	
	public static SpanF spanNegative(SpanF _s) {
		spanCheck(_s);
		return span(-(_s.To), -(_s.From));
	}
	
	public static SpanF spanSubtract(SpanF _sa,SpanF  _sb) {
		spanCheck(_sa);
		spanCheck(_sb);
		return spanAdd(_sa, spanNegative(_sb));
	}

	public static SpanF spanAbs(SpanF _s) {
		var ans = spanUnsorted (Math.Abs (_s.From), Math.Abs (_s.To));
		if (spanContains (_s, 0.0)) {
			ans = spanIncluding(ans, 0.0);
		}
		return ans;
	}
	
	public static bool spanContains(SpanF _sa,double _val) {
		spanCheck(_sa);
		return ((_sa.From <= _val) && (_sa.To > _val));
	}

	public static double spanClampValue(double v, SpanF range) {
		spanCheck (range);
		return Math.Min (Math.Max (v, range.From), range.To);
	}

	public static SpanF spanClampRange(SpanF v, SpanF range) {
		spanCheck (v);
		spanCheck (range);
		return span (
			spanClampValue (v.From, range), 
			spanClampValue (v.To, range));

	}

	private static double util_bounded(double _a, double _min, double _max) {
		return Math.Max(_min, Math.Min(_max, _a));
	}
	
	public static SpanF spanBounded(SpanF _sa, SpanF _val) {
		spanCheck(_sa);
		spanCheck(_val);
		return span(
			util_bounded(_sa.From, _val.From, _val.To),
			util_bounded(_sa.To, _val.From, _val.To));
	}

	public static SpanF spanGreaterThan(SpanF _sa, SpanF _sb) {
		WeakTODO(); // the line below is not good
		var cmpr = spanSubtract (_sa, _sb);
		var d = ((((cmpr.From + cmpr.To) * 0.5) > 0.0) ? 1.0 : 0.0);
		return span (d, d);
	}
	
	public static SpanF spanIncluding(SpanF _s, double _val) {
		spanCheck(_s);
		return span(Math.Min(_s.From, _val), Math.Max(_s.To, _val));
	}
	
	public static double util_sqrtSigned(double _val) {
		var sign = ((_val >= 0) ? 1.0 : -1.0);
		return Math.Sqrt(_val * sign) * sign;
	}
	
	public static SpanF spanSqrt(SpanF _s) {
		spanCheck(_s);
		return span(util_sqrtSigned(_s.From), util_sqrtSigned(_s.To));
	}
	
	public static SpanF spanCos(SpanF _s) {
		spanCheck(_s);
		
		var ans = spanUnsorted(Math.Cos(_s.From), Math.Cos(_s.To));
		
		// TODO: Optimize, this walks through the critcal points ensuring they are in the interval:
		var cycleLength = Math.PI / 4.0;
		var unitFrom = _s.From / cycleLength;
		var unitTo = Math.Min( _s.To / cycleLength, unitFrom + (cycleLength * 6) );
		while (Math.Floor(unitFrom) < Math.Floor(unitTo)) {
			unitFrom = Math.Floor(unitFrom) + 1;
			var curVal = Math.Cos(unitFrom * cycleLength);
			ans = spanIncluding(ans, curVal);
		}
		
		return ans;
	}

	public static SpanF spanSin(SpanF _s) {
		spanCheck(_s);
		
		var ans = spanUnsorted(Math.Sin(_s.From), Math.Sin(_s.To));
		
		// TODO: Optimize, this walks through the critcal points ensuring they are in the interval:
		var cycleLength = Math.PI / 4.0;
		var unitFrom = _s.From / cycleLength;
		var unitTo = Math.Min( _s.To / cycleLength, unitFrom + (cycleLength * 6) );
		while (Math.Floor(unitFrom) < Math.Floor(unitTo)) {
			unitFrom = Math.Floor(unitFrom) + 1;
			var curVal = Math.Sin(unitFrom * cycleLength);
			ans = spanIncluding(ans, curVal);
		}
		
		return ans;
	}

	public static SpanF spanATan2(SpanF _y, SpanF _x) {

		WeakTODO ();

		return spanUnsorted4(
			Math.Atan2(_y.From, _x.From),
			Math.Atan2(_y.From, _x.To),
			Math.Atan2(_y.To, _x.From),
			Math.Atan2(_y.To, _x.To)
			);
	}

	// Span Vector Operations

	public static Cubic<SpanF> spanVector(SpanF x, SpanF y, SpanF z) {
		return new Cubic<SpanF> (x, y, z);
	}

	public static Cubic<SpanF> spanVectorAdd(Cubic<SpanF> a, Cubic<SpanF> b) {
		return new Cubic<SpanF> (
			spanAdd (a.X, b.X),
			spanAdd (a.Y, b.Y),
			spanAdd (a.Z, b.Z));
	}

	public static Cubic<SpanF> spanVectorSubtract(Cubic<SpanF> a, Cubic<SpanF> b) {
		return new Cubic<SpanF> (
			spanSubtract (a.X, b.X),
			spanSubtract (a.Y, b.Y),
			spanSubtract (a.Z, b.Z));
	}

	public static SpanF spanVectorDot(Cubic<SpanF> a, Cubic<SpanF> b) {
		return spanAdd (spanAdd (
			spanMult (a.X, b.X), spanMult (a.Y, b.Y)), spanMult (a.Z, b.Z));
	}

	public static Cubic<SpanF> spanVectorScale(Cubic<SpanF> a, Cubic<SpanF> b) {
		return spanVector (
			spanMult (a.X, b.X),
			spanMult (a.Y, b.Y),
			spanMult (a.Z, b.Z));
	}

	public static  Cubic<SpanF> spanVectorCross(Cubic<SpanF> a, Cubic<SpanF> b) {
		return new Cubic<SpanF> (
			spanSubtract (spanMult (a.Y, b.Z), spanMult (a.Z, b.Y)),
            spanSubtract (spanMult (a.Z, b.X), spanMult (a.X, b.Z)),
			spanSubtract (spanMult (a.X, b.Y), spanMult (a.Y, b.X)));
	}

	public static Cubic<SpanF> spanVectorScale(Cubic<SpanF> a, SpanF scl) {
		return new Cubic<SpanF> (
			spanMult (a.X, scl),
			spanMult (a.Y, scl),
			spanMult (a.Z, scl));
	}

	public static SpanF spanVectorMagnitude( Cubic<SpanF> a ) {
		return spanSqrt (spanVectorDot (a, a));
	}

	public static Cubic<SpanF> spanVectorNormalize( Cubic<SpanF> a ) {
		var mag = spanVectorMagnitude (a);
		var scl = spanDivide (spanExactly(1.0), mag);
		return spanVectorScale (a, scl);
	}
	
	// Span Projections
	
	public static void spanProjCheck(SpanOf<SpanF> _s) {
		//spanCheck(_s.From);
		spanCheck(_s.To);
	}
	
	public static SpanOf<SpanF> spanProjFromDomainAndRange(SpanF _sdomain, SpanF _srange)
	{
		spanCheck(_sdomain);
		spanCheck(_srange);
		var delta = (_srange.To - _srange.From) / (_sdomain.To - _sdomain.From);
		return new SpanOf<SpanF>(
            spanCreateInvalid(_sdomain.From, _srange.From), 
            spanExactly(delta));
	}

    public static Cubic<SpanOf<SpanF>> spanProjFromCubicDomainAndRange(Cubic<SpanF> _sdomain, Cubic<SpanF> _srange)
    {
        return _sdomain.Select2(_srange, (dmn, rng) => spanProjFromDomainAndRange(dmn, rng));
    }
	
	public static SpanF spanProjSpan(SpanOf<SpanF> _sproj,SpanF _sindex) {
		spanProjCheck(_sproj);
		spanCheck(_sindex);
        return spanAdd(spanExactly(_sproj.From.To),
            spanMult(_sproj.To, spanAdd(_sindex, spanExactly(-_sproj.From.From))));
		//return spanAdd(_sproj.From, spanMult(_sproj.To, _sindex));
	}

    public static Cubic<SpanF> spanProjCubicSpan(Cubic<SpanOf<SpanF>> _sproj, Cubic<SpanF> _spanNdx)
    {
        return new Cubic<SpanF>(
            spanProjSpan(_sproj.X, _spanNdx.X),
            spanProjSpan(_sproj.Y, _spanNdx.Y),
            spanProjSpan(_sproj.Z, _spanNdx.Z));
    }
	
	public static SpanF spanProjInt(SpanOf<SpanF> _sproj, int _intIndex)
	{
		return spanProjSpan(_sproj, spanFromInt(_intIndex));
	}

    public static Cubic<SpanF> spanProjCubicInt(Cubic<SpanOf<SpanF>> _sproj, Cubic<int> _intIndex3)
    {
        return new Cubic<SpanF>(
            spanProjInt(_sproj.X, _intIndex3.X),
            spanProjInt(_sproj.Y, _intIndex3.Y),
            spanProjInt(_sproj.Z, _intIndex3.Z));
    }

	// Color System:

	public static SpanF spanColorSignedForGreen(SpanF v) {
		return v.boundedBy(span(-1.0, 1.0)).times(SpanF.exactly(Math.PI)).cos().times(SpanF.exactly(0.5)).add(SpanF.exactly(0.5));
	}

	public static SpanF spanColorSignedForRed(SpanF v) {
		return spanColorSignedForGreen(v.add(SpanF.exactly(-0.5)).times(SpanF.exactly(1.0)));
	}
	
	public static SpanF spanColorSignedForBlue(SpanF v) {
		return spanColorSignedForGreen(v.add(SpanF.exactly(0.5)).times(SpanF.exactly(1.0)));
	}

	public static float spanColorChannelToFloat(SpanF v) {
		return ((float)(v.To + v.From) * 0.5f);
	}

	public static Cubic<float> spanColorSignedToRGB(SpanF v) {
		return Cubic<float>.Create (
			spanColorChannelToFloat(spanColorSignedForRed (v)),
			spanColorChannelToFloat(spanColorSignedForGreen (v)), 
            spanColorChannelToFloat(spanColorSignedForBlue (v)));
	}

	/*
	function spanColorSignedForGreen(v) {
		return v.boundedBy(span(-1.0, 1.0)).times(exactly(Math.PI)).cos().times(exactly(0.5)).add(exactly(0.5));
	}
	
	function spanColorSignedForRed(v) {
		return spanColorSignedForGreen(v.add(exactly(-0.5)).times(exactly(1.0)));
	}
	
	function spanColorSignedForBlue(v) {
		return spanColorSignedForGreen(v.add(exactly(0.5)).times(exactly(1.0)));
	}
	*/

	// Example Math Stuff:

	public static bool Example_EvaluateUnitCircle(SpanF qx, SpanF qy, SpanF qz) {

		//return qz.To > qy.To;
		//return spanContains (spanAdd (spanMult (qx, qx), spanMult (qy, qy)), 1);
		return spanContains (
			spanAdd ( spanAdd (
				spanMult (qx, qx), 
				spanMult (qy, qy)
			), spanMult( qz, qz ) )
			, 1);

	}

	public static SpanF Example_ChakraV1(float countPetals, Cubic<SpanF> qv, bool isOneWay) {

		var xy = spanSqrt( spanAdd(spanMult (qv.X, qv.X), spanMult (qv.Z, qv.Z)));
		var z = spanSqrt( spanMult(qv.Y,qv.Y) );
        if (isOneWay && (qv.Y.To < 0.0))
        {
            return span(0, 0);
        }
		if (z.To < 0.0) {
			return span (0,0);
		}

		var cone = spanGreaterThan(spanMult(z,spanExactly(2.0)), xy);

        var rot = spanATan2(qv.X, qv.Z);
        var rotTwist = spanAdd( rot, spanMult( z, spanExactly(0.9) ) );
		var fade = spanCos (spanMult (rot, spanExactly (0.5 * countPetals)));
		var petals = spanAbs (fade);
        petals = spanMult(petals, petals);

		var awayness = spanSubtract (spanExactly (1), z ); //spanMult(z,z));
		var dist = spanClampRange (spanMult (awayness, spanExactly (2.0)), span (0, 1));

		var fnl = spanMult (spanMult (petals, cone), dist);
		return fnl;
	}

	public static bool Example_EvaluateUnitTorus(SpanF qx, SpanF qy, SpanF qz) {
//		var qv = spanVectorScale (
//			spanVector (qx, qy, qz), 
//			spanExactly (1.0)
//			);
		var scaler = spanExactly (1.75);
		var qv = spanVector(
			spanMult( qx, scaler),
			spanMult( qy, scaler),
			spanMult(qz, scaler) );

		var plane = spanVectorNormalize (spanVector (qv.X, qv.Y, spanExactly(0) ));
		var delta = spanVectorMagnitude (spanVectorSubtract (qv, plane));

		return spanContains (delta, 1.0);


//		var r = spanDivide (spanExactly (1.0), spanVectorMagnitude (qv));
//		var up = spanVector (spanExactly (0), spanExactly (1), spanExactly (0));
//		var c = spanVectorCross (qv, up);
//		var m = spanVectorMagnitude (c);
//
//		var f = spanMult (m, r);
//		return spanContains (f, 1.0);
	}

	public static Cubic<SpanF> Example_FieldFromParticle(Cubic<SpanF> pos, Cubic<SpanF> particle, SpanF charge) {
		var r1 = spanVectorSubtract (pos, particle);
		var invSqrR1 = spanDivide (spanExactly (1), spanVectorDot (r1, r1));
		var nr = spanVectorNormalize (r1);
		var ans = spanVectorScale (nr, spanMult (charge, invSqrR1));
		return ans;
	}

	public static SpanF Example_MagneticCore(Cubic<SpanF> pos) {

		var q1 = spanVector (spanExactly (0), spanExactly (0), spanExactly ( 0.5));
		var q2 = spanVector (spanExactly (0), spanExactly (0), spanExactly (-0.5));

		var c1 = spanExactly (0.75);
		var c2 = spanExactly (-0.75);

		var f1 = Example_FieldFromParticle (pos, q1, c1);
		var f2 = Example_FieldFromParticle (pos, q2, c2);
		var sumF = spanVectorAdd(f1, f2);

		var mag = spanVectorMagnitude (sumF);

		var c = spanExactly (6.0);
		var sm = spanMult (mag, c);
		var sn = spanSin (sm);

		return sn;
	}

	public static float Util_ScaleSingle(double v, double from, double to) {
		var rv = ((v - from) / (to - from));
		return (float)rv;
	}

	public static Color Example_SingleToColor(double f) {
		return new Color (
			Util_ScaleSingle (f, 0.0, -1.0),
			1.0f - Math.Abs ((float)f),
			Util_ScaleSingle (f, 0.0, 1.0),
			(float)Math.Pow( 1.0f - Math.Abs ((float)f), 2.0f ) );

	}

	public static double util_spanCenter(SpanF _s) {
		return ((_s.From + _s.To) * 0.5f);
	}

	public static Color Example_SpanToColor(SpanF _s) {
		var f = Example_SingleToColor (_s.From);
		var m = Example_SingleToColor (util_spanCenter (_s));
		var t = Example_SingleToColor (_s.To);

		return Color.Lerp (Color.Lerp (f, t, 0.5f), m, 0.5f);
	}

	public static SpanF Example_EvaluateField3(Cubic<SpanOf<SpanF>> proj, Cubic<int> ndx) {
		var qx = spanProjInt (proj.X, ndx.X);
		var qy = spanProjInt (proj.Y, ndx.Y);
		var qz = spanProjInt (proj.Z, ndx.Z);
		return Example_MagneticCore (spanVector (qx, qy, qz));
	}
	
	public static bool Example_EvaluateField2(Cubic<SpanOf<SpanF>> proj, Cubic<int> ndx) {
		var qx = spanProjInt (proj.X, ndx.X);
		var qy = spanProjInt (proj.Y, ndx.Y);
		var qz = spanProjInt (proj.Z, ndx.Z);
		return Example_EvaluateUnitTorus (qx, qy, qz);
	}

	public static Cubic<SpanOf<SpanF>> Example_SetupProjection(int w, int h, int d) {
		var domainx = span(0, w);
		var domainy = span(0, h);
		var domainz = span(0, d);
		var rangeUnit = span(-1, 1);
		var projx = spanProjFromDomainAndRange(domainx, rangeUnit);
		var projy = spanProjFromDomainAndRange(domainy, rangeUnit);
		var projz = spanProjFromDomainAndRange(domainz, rangeUnit);
		return new Cubic<SpanOf<SpanF>> (projx, projy, projz);
	}


    public static Cubic<SpanOf<SpanF>> Example_SetupSignedUnitToIntProjection(int w, int h, int d)
    {
        var rangex = span(0, w);
        var rangey = span(0, h);
        var rangez = span(0, d);
        var domainUnit = span(-1, 1);
        var projx = spanProjFromDomainAndRange(domainUnit, rangex);
        var projy = spanProjFromDomainAndRange(domainUnit, rangey);
        var projz = spanProjFromDomainAndRange(domainUnit, rangez);
        return new Cubic<SpanOf<SpanF>>(projx, projy, projz);
    }

	public static bool Example_EvaluateField1(Cubic<SpanOf<SpanF>> proj, Cubic<int> ndx) {
		var qx = spanProjInt (proj.X, ndx.X);
		var qy = spanProjInt (proj.Y, ndx.Y);
		var qz = spanProjInt (proj.Z, ndx.Z);
		return Example_EvaluateUnitCircle (qx, qy, qz);
	}

	public static SpanF Example_EvaluateFieldChakraV1(
		Cubic<SpanOf<SpanF>> proj, Cubic<int> ndx,
		int chakraPetals, bool isOneWay) {
		var qx = spanProjInt (proj.X, ndx.X);
		var qy = spanProjInt (proj.Y, ndx.Y);
		var qz = spanProjInt (proj.Z, ndx.Z);
		var qv = spanVector (qx, qy, qz);
		return Example_ChakraV1 (chakraPetals, qv, isOneWay);
	}
}


public static class CodeSpan
{
	public static Vector3 AsVector(this Cubic<double> c) {
		return new Vector3((float)c.X, (float)c.Y, (float)c.Z);
	}

	public static Cubic<double> ToCubic(this Vector3 vec) {
		return new Cubic<double>(vec.x, vec.y, vec.z);
	}

    public static IEnumerable<X> EachOfFromAndTo<X>(this SpanOf<X> _s)
    {
        yield return _s.From;
        yield return _s.To;
    }

	public static int DotProduct(this Cubic<int> a, Cubic<int> b) {
		return a.Select2 (b, (c,d) => (c * d)).Aggregate ((c,d) => (c + d));
	}

	public static bool AllTrue(this Cubic<bool> a) {
		return a.Aggregate ((b,c) => b && c);
	}
		
	public static bool AnyTrue(this Cubic<bool> a) {
		return a.Aggregate ((b,c) => b || c);
	}

	public static void ForeachDo<T>(this IEnumerable<T> list, Action<T> act) {
		foreach (var i in list) {
			act(i);
		}
	}

	public static IEnumerable<Cubic<T>> EachPermutation<T>(this Cubic<IEnumerable<T>> from) {
		foreach (var x in from.X) {
			foreach (var y in from.Y) {
				foreach (var z in from.Z) {
					yield return new Cubic<T>(x, y, z);
				}
			}
		}
	}
}