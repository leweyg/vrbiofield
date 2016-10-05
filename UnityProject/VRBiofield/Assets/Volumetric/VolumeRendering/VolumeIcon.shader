Shader "Volume Rendering / Volume Icon" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    _MainVol("VolTexture", 3D) = "white" {}
    _RandomSeedColor("Random Seed Color", Color) = (1, 1, 1, 1)
	_ScrollOffset("_ScrollOffset", vector) = (0,0,0,0)
	_ScrollScale("_ScrollScale", vector) = (1,1,1,1)
	_SampleTransparency("_SampleTransparency", Float) = 1
    }
        SubShader{
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Pass{
        CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 
#pragma target 5.0

#include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler3D _MainVol;
    fixed4 _RandomSeedColor;
	fixed4 _ScrollOffset;
    fixed4 _ScrollScale;
	float _SampleTransparency;
	float4x4 _UnitToVolMatrix;



    const bool perEye = false;
    const bool singleSample = false;


    struct vertexInput {
        float4 vertex : POSITION;
        float4 texcoord : TEXCOORD0;
    };

    struct vertexOutput {
        float4 pos : SV_POSITION;
        float4 tex : TEXCOORD0;
        float4 objPosStart : TEXCOORD1;
        float4 objPosEnd : TEXCOORD2;
    };

    vertexOutput vert(vertexInput input)
    {
        vertexOutput output;

        //float4 worldPosUnProj = mul( _Object2World, input.vertex );
        float4 screenPosUnProj = mul(UNITY_MATRIX_MVP, input.vertex);


        float3 tocamera = ObjSpaceViewDir(input.vertex);

        const float maxDistance = 1.7;

        float3 objPos = (input.vertex.xyz);
        objPos = mul( _UnitToVolMatrix, float4( objPos, 1.0 ) );
        output.objPosStart = float4(objPos, 1);
        output.objPosEnd = float4(objPos - (normalize(tocamera) * maxDistance), 1);

        output.tex = input.texcoord;
        output.pos = screenPosUnProj;


        return output;
    }


    float customRand3(float3 co)
    {
        return frac(sin(dot(co.xyz + _RandomSeedColor.xyz,float3(12.9898,78.233,45.5432))) * 43758.5453);
    }

    float4 sampleSphere2(float2 coord) {

        float2 orig = (coord - float2(0.5,0.5)) * 2.0;
        float dist = 1.0 - (dot(orig, orig));
        return float4(dist, dist, dist, 1.0);
    }

    float4 sampleSphere3(float3 coord) {

        float radius = dot(coord, coord);

        float dist = ((radius > 0.6) && (radius < 1.0)) ? 1.0 : 0.0;

        //float dist = max(0.0, 1.0 - ( abs( dot( coord, coord ) - 0.8 ) ) );
        return float4(dist, dist, dist, 1.0);
    }

    float4 volTrace(vertexOutput input, float traceLerpFactor) {

        float3 start = input.objPosStart;
        float3 end = input.objPosEnd;

        //if (perEye) 
		{
            float3 tocamera = normalize(ObjSpaceViewDir(float4(start.xyz,1)).xyz);
            end = start - tocamera;
        }

        float3 coord3 = lerp(start, end, traceLerpFactor);
        float3 offset = float3(0.5,0.5,0.5);
        coord3 += offset;

		float3 unitCoord = coord3;

		// Apply Scroll and Offset:
		coord3 = ((coord3.xyz* _ScrollScale.xyz) + _ScrollOffset.xyz);

		// Sample volume:
        float4 volColor = tex3Dlod( _MainVol, float4( coord3, 0 ) );
        //float4 volColor = tex3D( _MainVol, float4( coord3, 0 ) );
        //float4 volColor = float4( coord3, 0.5 );
        //float4 volColor = tex3D( _MainVol, float3(1,1,1) * 0.5 );// float4( coord3, 0 ));
        //volColor.a = 0.5;
        //float4 volColor = tex2Dlod(_MainTex, float4(coord3.xy, 0, 0));
        //float4 volColor = tex3D( _MainVol, coord3.xyz );

		//volColor.a *= _SampleTransparency; 

		// Black and White to Alpha transfer:
		//float bw = volColor.g;
		//volColor = float4( 1.0, 1.0, 1.0, bw );

		// Hide if outside the unit cube:
		float inRange = (((unitCoord == saturate(unitCoord))) ? 1.0 : 0.0);
		volColor = (volColor * inRange);

        return volColor;
    }

    float4 blendOver(float4 dst, float4 src) {
		float sAlpha = src.a * _SampleTransparency;
		//float4 comb = saturate( dst + float4( src.xyz * sAlpha, src.a ) );
		float4 comb = saturate( (dst * (1.0 - sAlpha * sAlpha )) + float4( src.xyz * sAlpha, src.a ) );
		comb.a = max( dst.a, sAlpha );
		return comb;

        float4 clr = ((src * src.a) + (dst * (1.0 - src.a)));
		//clr.a = lerp( src.a, dst.a, src.a );
		clr.a = max( src.a, dst.a );
		return clr;
    }

    float4 volTraceMultiple(vertexOutput input, float traceLerpFactor) {

			const int maxLoops = 60;
			const float maxDist = 1.7;
			const float stepDelta = maxDist / ((float)maxLoops);
			float curStart = maxDist;
			float4 curColor = float4(0,0,0,0);
			for (int i=0; i<maxLoops; ++i) {
				float4 c = volTrace( input, curStart - ( stepDelta * traceLerpFactor ) );
				curColor = blendOver( curColor, c );

				curStart -= stepDelta;
			}

			return curColor;

    }


    float4 frag(vertexOutput input) : COLOR
    {
    
    	float4 coord3 = float4(input.objPosStart.xyz,0.5);
    	float4 volColor = tex3D( _MainVol, coord3.xyz );
    	
    	float4 finalColor = float4( volColor.rgb, 1.0 );
    	//clip(volColor.a - 0.01);
    	
    	return finalColor;
    
        //float2 texPos = input.tex.xy;
        //float4 texColor = tex2D(_MainTex, texPos).rgba;

        //float4 random = customRand3(input.objPosStart).rrrr;


        //float2 coordUnit = input.tex.xy; //((IN.uv_MainTex.xy) - float2(0.5,0.5)) * 2.0;
        //float3 coord3 = float3( coordUnit.x, coordUnit.y, random.r );
        //float4 colour = sampleSphere3( coord3 );

        //return volTraceMultiple(input, random.r);

        //          float3 coord3 = lerp( input.objPosStart, input.objPosEnd, random );
        //             float3 offset = float3(0.5,0.5,0.5);
        //             coord3 += offset;
        //          float4 result;
        //      	  float4 volColor = tex3Dlod( _MainVol, float4( coord3, 0 ) );
        //      	  result = float4( volColor.rgbr );
        //   			return result;
    }

        ENDCG
    }
    }
        Fallback "Diffuse"
}
