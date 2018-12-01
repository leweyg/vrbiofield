// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Biofield / Chakra Mesh Shader" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    _RandomSeedColor("Random Seed Color", Color) = (1, 1, 1, 1)
	_ScrollOffset("_ScrollOffset", vector) = (0,0,0,0)
	_ScrollScale("_ScrollScale", vector) = (1,1,1,1)
	_CustomColor("CustomColor",Color) = (0,1,0,0)
	_CustomColor2("CustomColor2",Color) = (0,1,0,0)
	_CustomAlpha("CustomAlpha",Float) = 1
	_SampleTransparency("_SampleTransparency", Float) = 1
    }
        SubShader{
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
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
	fixed4 _CustomColor2;
	float _CustomAlpha;


    const bool perEye = false;
    const bool singleSample = false;


    struct vertexInput {
        float4 vertex : POSITION;
        float4 texcoord : TEXCOORD0;
        float4 vcolor : COLOR;
    };

    struct vertexOutput {
        float4 pos : SV_POSITION;
        float4 tex : TEXCOORD0;
        float4 localPos : TEXCOORD1;
        float4 vcolor : TEXCOORD2;
    };

    vertexOutput vert(vertexInput input)
    {
        vertexOutput output;

        float4 screenPosUnProj = UnityObjectToClipPos(input.vertex);

        output.tex = input.texcoord;
        output.pos = screenPosUnProj;
        output.localPos = input.vertex;
        output.vcolor = input.vcolor;

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
        float2 centeredUV = ( input.tex.xy + ((0.3).xx) );
        float2 unitUV = (( centeredUV - ((0.5).xx) ) * 3 ) + ((0.5).xx);
        //return tex2D(_MainTex, unitUV).rgba;

        unitUV.y *= 1.2;
        float2 crossUV = float2(-unitUV.y, unitUV.x);
        float2 fwdUV = float2(1,1) / sqrt(2);
       	float2 midPnt = fwdUV * dot(fwdUV, unitUV);
        float alphaSide = 1 - (abs(dot(unitUV - midPnt, crossUV)) * 4);
        //return float4(alphaSide.xxx, 1);

        float alphaAlong = (1 - length(unitUV)*0.7);
        float leafAlpha = (alphaAlong * alphaSide);
        //return float4( leafAlpha.xxxx );

        float al = length(input.tex.xy);
        float unitAl = length(unitUV);
        float rt = ((_Time.y * 1.7) + (al * 24.0f));
        float c = remap( (cos(rt)), -1, 1, 0.0, 1.0 );
        //float3 clr = lerp(float3(1,1,1),_CustomColor.rgb,al); 
        float3 clr = lerp(float3(1,1,1),input.vcolor.rgb,al);
        float distAlpha = saturate(1.0 - (unitAl * 0.85));
        return float4(clr.rgb,distAlpha * c * _CustomAlpha);
        //return float4(clr.rgb,leafAlpha * c * _CustomAlpha);

    }

        ENDCG
    }
    }
        Fallback "Diffuse"
}
