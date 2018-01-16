Shader "Biofield / Meridian Quill Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CustomAlpha("CustomAlpha",Float) = 1
		_PulsationPct("Pulsation Pct", Float) = 0
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off
        ZTest Off

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
			 	float4 col:COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 col : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _CustomAlpha;
			float _PulsationPct;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.col = v.col;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = i.col;

				float t = _Time.y;
				float v = i.uv.y;
				float waveSigned = (cos(((t*1.5f) - (v*7.0f))*(3.14159)));
				float unitWave = lerp( 0.25, 0.75, ((waveSigned * 0.5f) + 0.5f) );
				float wavePct = lerp( 1, unitWave, _PulsationPct );

				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				col.a *= _CustomAlpha * wavePct;
				return col;
			}
			ENDCG
		}
	}
}
