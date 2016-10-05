  Shader "Volume Rendering / BACKUP - Per-Pixel Random Sampler" {
      Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _MainVol("VolTexture", 3D) = "white" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float4 color : COLOR;
          float3 screenPos ;
          float2 uv_MainTex;
      };
      
       sampler2D _MainTex;
       sampler3D _MainVol;
      
       float customRand3(float3 co)
		 {
		     return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
		 }
		 
      float4 sampleSphere2(float2 coord) {
      
      		float2 orig = (coord - float2(0.5,0.5)) * 2.0;
      		float dist = 1.0 - ( dot( orig, orig ) );
      		return float4( dist, dist, dist, 1.0 );
      }
      
      float4 sampleSphere3(float3 coord) {
      
      	float radius = dot(coord, coord);
      	
      	float dist = ((radius > 0.6) && (radius < 1.0)) ? 1.0 : 0.0;
      
      		//float dist = max(0.0, 1.0 - ( abs( dot( coord, coord ) - 0.8 ) ) );
      		return float4( dist, dist, dist, 1.0 );
      }

      
      void surf (Input IN, inout SurfaceOutput o) {
          //o.Albedo = float4(0.5, 0.5, 0.5, 0.5); //Mathf.PerlinNoise();
          
          float2 texPos = IN.uv_MainTex.xy;
          float4 texColor = tex2D( _MainTex, texPos ).rgba;
          
          float4 random = customRand3( IN.screenPos ).rrrr; 
          
          //float4 colour = sampleSphere2( IN.uv_MainTex.xy );
          float2 coordUnit = IN.uv_MainTex.xy; //((IN.uv_MainTex.xy) - float2(0.5,0.5)) * 2.0;
          float3 coord3 = float3( coordUnit.x, coordUnit.y, random.r );
          float4 colour = sampleSphere3( coord3 );
          
          //float test = colour; //((random.r < colour.r) ? 1.0 : 0.0);
          o.Albedo = colour * texColor;
          
            
      	  float4 volColor = tex3Dlod( _MainVol, float4( coord3, 0 ) );
      	   o.Albedo = volColor;
   
      }
      
      ENDCG
    }
    Fallback "Diffuse"
  }