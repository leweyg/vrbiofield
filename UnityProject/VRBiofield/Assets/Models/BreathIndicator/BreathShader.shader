Shader "ChiVR/Breath Indicator Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorOut("Color Out", Color) = (0,0,1,1)
		_ColorIn("Color In", Color) = (0,1,0,1)
		_BreathInPct ("Breath In Pct", Float) = 0.5
	}
	SubShader
	{
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _BreathInPct;
			float4 _ColorOut;
			float4 _ColorIn;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float pct = ((i.uv.y < _BreathInPct) ? 1 : 0);
				float4 col = lerp( _ColorOut, _ColorIn, pct );
				// sample the texture
				col.a *= tex2D(_MainTex, i.uv).a;
				//clip( alpha - 0.5 );
				// apply fog
				return col;
			}
			ENDCG
		}
	}
}
