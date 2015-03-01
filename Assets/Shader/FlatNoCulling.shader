Shader "Custom/Flat No Culling" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Pass {
			Tags { "RenderType"="Transparent" "Queue"="Transparent" }
			Cull Off
			
	ZWrite On
	ZTest Less
			LOD 200
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };
            
			uniform sampler2D _MainTex;
            float4 _MainTex_ST;
			uniform half4 _Color;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

			float4 frag(v2f i) : COLOR {
				return tex2D(_MainTex, i.texcoord) * _Color;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
