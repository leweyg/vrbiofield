Shader "Biofield / Dynamic Field Particle Shader" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    //_RandomSeedColor("Random Seed Color", Color) = (1, 1, 1, 1)
	//_ScrollOffset("_ScrollOffset", vector) = (0,0,0,0)
	//_ScrollScale("_ScrollScale", vector) = (1,1,1,1)
	//_CustomColor("CustomColor",Color) = (0,1,0,0)
	//_CustomAlpha("CustomAlpha",Float) = 1
	//_SampleTransparency("_SampleTransparency", Float) = 1
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
    //fixed4 _RandomSeedColor;
	//fixed4 _ScrollOffset;
    //fixed4 _ScrollScale;
	//float _SampleTransparency;
	//float4x4 _UnitToVolMatrix;
	//fixed4 _CustomColor;
	//float _CustomAlpha;


    const bool perEye = false;
    const bool singleSample = false;


    struct vertexInput {
        float4 vertex : POSITION;
        float4 vcolor : COLOR;
        float4 texcoord : TEXCOORD0;
        float4 custom1 : TEXCOORD1;
    };

    struct vertexOutput {
        float4 pos : SV_POSITION;
        float4 tex : TEXCOORD0;
        float4 localPos : TEXCOORD1;
        float4 vcolor : TEXCOORD2;
        float4 custom1 : TEXCOORD3;
    };

    vertexOutput vert(vertexInput input)
    {
        vertexOutput output;

        float4 screenPosUnProj = mul(UNITY_MATRIX_MVP, input.vertex);

        output.tex = input.texcoord;
        output.pos = screenPosUnProj;
        output.localPos = input.vertex;
        output.vcolor = input.vcolor;
        output.custom1 = input.custom1;

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

        float2 texPos = input.tex.xy;
        float4 texColor = tex2D(_MainTex, texPos).rgba;

        float tstColor = tex2D(_MainTex, texPos + float2(0,_Time.y + input.custom1.x)).a;
        float animVal = tstColor;// abs( sin( (_Time.y) + texPos.y * 4.5 ) );
 
        texColor.rgba = input.vcolor.rgba * float4(1,1,1,texColor.a);
        texColor.a *= animVal;

        return texColor;

        //return float4(input.tex.xy * 2.0,1,1);

        //float al = length(input.tex.xy);
        //float rt = _Time.y + (al * 24.0f);
        //float c = remap( (cos(rt)), -1, 1, 0.0, 1.0 );
        //float3 clr = lerp(float3(1,1,1),_CustomColor.rgb,al); 
        //float alpha = saturate(1.0 - (al*2.1)) * _CustomAlpha;
        //return float4(clr.rgb,alpha * c);

        //float3 xyzToC = length(input.localPos.xyz);
        //float3 finalRgb = xyzToC - floor(xyzToC);
        //return float4(finalRgb,1);
    }

        ENDCG
    }
    }
        Fallback "Diffuse"
}
