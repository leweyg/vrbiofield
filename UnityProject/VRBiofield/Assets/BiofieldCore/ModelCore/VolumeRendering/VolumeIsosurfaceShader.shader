// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Volume Rendering / Volume Isosurface" {
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
        ZWrite Off
        Pass{
        CGPROGRAM

#pragma vertex vert  
#pragma fragment frag 
#pragma target 4.0

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
        float4 tangent : TANGENT;
        float3 normal : NORMAL;
    };

    struct vertexOutput {
        float4 pos : SV_POSITION;
        //float4 tex : TEXCOORD0;
        float4 worldPos : TEXCOORD0;
        float4 tangent : TEXCOORD1;
        float3 extraXShade : TEXCOORD2;
    };

    vertexOutput vert(vertexInput input)
    {
        vertexOutput output;

        float4 worldPos = mul( unity_ObjectToWorld, float4( input.vertex.xyz, 1) );
        float4 screenPosUnProj = UnityObjectToClipPos(float4( input.vertex.xyz, 1 ) );
        float4 worldNormal = normalize( mul(unity_ObjectToWorld, float4( input.normal.xyz, 0 ) ) );
        float3 viewDir = normalize( worldPos -  _WorldSpaceCameraPos );

        //output.tex = input.texcoord;
        output.worldPos = worldPos;
        output.pos = screenPosUnProj;
        output.tangent = input.tangent;
        output.extraXShade = float3( abs(dot( worldNormal, viewDir)), 0, 0 );

        return output;
    }




    float4 frag(vertexOutput input) : COLOR
    {
    	float4 baseColor = float4(0.7, 1.0, 0.7, 0.75 );    
    	//float rippleFader = ( 1.0f + sin( ( _Time.y * -input.tangent.x ) + input.tangent.y ) ) * 0.5f;
    	float timeOffset = ( frac( _Time.y ) * 2.0f ) - 1.0f;
    	float3 dir = -normalize( input.tangent.xyz );
    	float ripplePos = ( dot( dir, ( input.worldPos.xyz + ( dir * timeOffset ) ) ) );
    	float rippleFader = ( 1.0f + sin( ripplePos * 3.14159 ) ) * 0.5f;
    	float magFader = saturate( input.tangent.w );
    	float edgePct = input.extraXShade.r;
    	float edgeFader = rippleFader * edgePct * magFader; // 1.0f; // (1.0f - (input.extraXShade.r * 1.0f )); //0.5f));
    	//float4 fnlColor = float4((baseColor.rgb * input.extraXShade.r), baseColor.a);
    	float3 rgbColor = lerp(float3(1,1,1), baseColor.rgb, rippleFader ); // baseColor.rgb * rippleFader;
    	float4 fnlColor = float4(rgbColor, edgeFader * baseColor.a);
		return fnlColor;
    }

        ENDCG
    }
    }
        Fallback "Diffuse"
}
