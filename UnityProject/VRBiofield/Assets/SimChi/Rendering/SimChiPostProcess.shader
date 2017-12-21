Shader "SimChi/ChiPostProcess" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_bwBlend ("Effect blend", Range (0, 1)) = 0
		_hackyBrighten ("Hacky brighten", Range (1,10)) = 1
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			uniform float _bwBlend;
			uniform float4 _TintColor;
			uniform float _hackyBrighten;

			float pixelEdge(float4 c) {
				float4 x = abs( ddx(c) );
				float4 y = abs( ddy(c) );
				float4 d = max( x, y );
				float t = max( max(d.x, d.y), d.z );
				return 1.0 - saturate( t / 0.2f );
			}

			float4 frag(v2f_img i) : COLOR {
				float4 c = tex2D(_MainTex, i.uv);

				c = saturate( c * _hackyBrighten );

				//return c; // + ( ( ddx(c) + ddy(c) ) * 0.5f);

				//return c * pixelEdge(c);

				//float4 d = tex2D(_CameraDepthTexture, i.uv);
				//return d * 10.0;
				
				float lum = c.r*.3 + c.g*.59 + c.b*.11;
				float3 bw = float3( lum, lum, lum ) * _TintColor.rgb; 
				
				float4 result = c;
				result.rgb = lerp(c.rgb, bw, _bwBlend);
				return result;
			}
			ENDCG
		}
	}
}