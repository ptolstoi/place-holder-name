Shader "Custom/Flat" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Pass {
			Tags { "RenderType"="Opaque" }
			Cull Back
			LOD 200
		
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
            
			uniform sampler2D _MainTex;
			uniform half4 _Color;

			float4 frag(v2f_img i) : COLOR {
				return tex2D(_MainTex, i.uv) * _Color;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
