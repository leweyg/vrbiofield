// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Biofield / Flow Mesh Shader" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    _RandomSeedColor("Random Seed Color", Color) = (1, 1, 1, 1)
	_ScrollOffset("_ScrollOffset", vector) = (0,0,0,0)
	_ScrollScale("_ScrollScale", vector) = (1,1,1,1)
	_CustomColor("CustomColor",Color) = (0,1,0,0)
	_CustomAlpha("CustomAlpha",Float) = 1
	_SampleTransparency("_SampleTransparency", Float) = 1
    }
        SubShader{
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off
        ZTest Off
        Pass{
        CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 
#pragma target 3.0

#include "UnityCG.cginc"

    sampler2D _MainTex;
    fixed4 _RandomSeedColor;
	fixed4 _ScrollOffset;
    fixed4 _ScrollScale;
	float _SampleTransparency;
	float4x4 _UnitToVolMatrix;
	fixed4 _CustomColor;
	float _CustomAlpha;


    const bool perEye = false;
    const bool singleSample = false;


    struct vertexInput {
        float4 vertex : POSITION;
	float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
    };

    struct vertexOutput {
        float4 pos : SV_POSITION;
        float4 tex : TEXCOORD0;
	float3 wpos : TEXCOORD1;
        float3 normal : NORMAL;
    };

    vertexOutput vert(vertexInput input)
    {
        vertexOutput output;

        float4 screenPosUnProj = mul(UNITY_MATRIX_MVP, input.vertex);

        output.tex = input.texcoord;
        output.pos = screenPosUnProj;
	output.wpos = mul( unity_ObjectToWorld, input.vertex ).xyz;
        output.normal = normalize( mul( unity_ObjectToWorld, float4( input.normal.xyz, 0 ) ) );

        return output;
    }

    float remap(float v, float fs, float fe, float ts, float te) {
    	float t = ((v - fs)/(fe - fs));
    	return lerp(ts, te, t);
    }

    float4 frag(vertexOutput input) : COLOR
    {
        //float2 texPos = input.tex.xy;
        //float4 texColor = tex2D(_MainTex, texPos).rgba;

        //return float4(input.tex.xy * 2.0,1,1);

	float3 dir = normalize( input.wpos.xyz - _WorldSpaceCameraPos.xyz );
	float NdotL = pow( -dot( normalize(input.normal.xyz), dir ), 4);
	return float4(0.5,0.5,0.5,NdotL);

        //float3 xyzToC = length(input.localPos.xyz);
        //float3 finalRgb = xyzToC - floor(xyzToC);
        //return float4(finalRgb,1);
    }

        ENDCG
    }
    }
        Fallback "Diffuse"
}
