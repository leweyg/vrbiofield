using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



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
		//var rotTwist = spanAdd( rot, spanMult( z, spanExactly(0.9) ) );
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

