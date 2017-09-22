Shader "Unlit/CustomWaterShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorSky ("Sky color", Color) = (.34, .85, .92, 1) // color
		_ColorHorizonA ("Sky Horizon A", Color) = (.34, .85, .92, 1) // color
		_ColorHorizonB ("Sky Horizon B", Color) = (.34, .85, .92, 1) // color
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
				float2 uvOrig : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ColorSky;
			float4 _ColorHorizonA;
			float4 _ColorHorizonB;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uvOrig = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 dir1 = tex2D(_MainTex, i.uv).xyz;
				float3 dir2 = tex2D(_MainTex, lerp(i.uvOrig,i.uv,0.2)).xyz;

				float mix = max( dir1.b, dir2.b );

				//float3 dirX = cross(dir2, float3(0, 0, 1));
				//float3 dirY = cross(dir2, dirX);
				//float3 dir = normalize( ((dirX * dir1.x) + (dirY * dir1.y) + (dir2 * dir2.z)) );
				//float mix = dir.b;

				//float3 dir = normalize(dir1 + (dir2 - ((0.2).xxxx)) );

				//return float4(dir,1);

				float4 col = lerp( _ColorHorizonA, _ColorSky, mix );
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
