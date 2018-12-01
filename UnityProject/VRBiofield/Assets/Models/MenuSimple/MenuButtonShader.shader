Shader "ChiVR/BasicSelectableMenuButton"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HighlightPct ("Highlight Pct", Float) = 0
		_SelectionPct ("Selection Pct", Float) = 0
		_DefaultColor("Default Color", Color) = (0.3,0.3,0.3,0.3)
		_BasicColor("Basic Color", Color) = (0.3,0.3,0.3,0.3)
		_Default2Color("Default 2 Color", Color) = (0.5,0.7,0.5,0.3)
		_HighlightedColor("Highlighted Color", Color) = (0.6,0.8,0.6,0.3)
		_SelectColor("Select Color", Color) = (0.8,1.0,0.8,0.8)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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

			float _HighlightPct;
			float _SelectionPct;
			float4 _DefaultColor;
			float4 _HighlightedColor;
			float4 _SelectColor;
			float4 _BasicColor;
			
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
				float4 col = _BasicColor;
				if (_HighlightPct > 0) {
					col = lerp( col, _HighlightedColor, _HighlightPct );
				}
				if ((1-i.uv.y) < _SelectionPct ) {
					col = _SelectColor;
				}
				return col;
			}
			ENDCG
		}
	}
}
