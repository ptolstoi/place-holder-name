Shader "Custom/Glow" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
		_Alpha ("Alpha", Float) = 1
	}
	SubShader {
		Pass {
			Tags { "RenderType"="Transparent" "Queue"="Transparent + 10"  }
			Blend One One
			Cull Back
			LOD 200
            Lighting Off
			ZWrite Off
            ZTest Less
			ColorMask RGB
            Offset 100, -1
		
			CGPROGRAM
			#pragma vertex vert alpha
			#pragma fragment frag alpha
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};
            
			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Color;
			float _Alpha;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;
				return o;
			}

			float3 frag(v2f i) : COLOR {
				float4 tex = tex2D(_MainTex, i.texcoord);
				return float3(tex.a, tex.a, tex.a) * i.color.r * 2.1 * _Color.rgb * _Alpha;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
