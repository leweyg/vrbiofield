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




    float4 frag(vertexOutput input) : COLOR
    {
		return float4(0.7, 1.0, 0.7, 0.5 );    
    }

        ENDCG
    }
    }
        Fallback "Diffuse"
}
