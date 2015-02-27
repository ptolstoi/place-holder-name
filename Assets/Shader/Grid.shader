Shader "Custom/Grid" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Pass {
			Tags { "RenderType"="10" }
			Blend One One 
			Cull Back
			ZWrite Off
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
				half2 texcoord : TEXCOORD0;
				half2 _MorphParams01 : TEXCOORD1;
				half2 _MorphParams23 : TEXCOORD2;
			};
            
			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform half4 _Color;
			
			uniform float4 _MorphParams0;
			uniform float4 _MorphParams1;
			uniform float4 _MorphParams2;
			uniform float4 _MorphParams3;
			
			float4 convert_space(float4 params) {
				return float4(params.x / 9, params.y / 9, params.z, params.w);
			}

			float morph(float3 pos, float4 params) {

				//float2 connection = params.xy - pos.xz;
				//float l = smoothstep(2, 0, length(connection));
				//return connection / l;

				return -smoothstep(2.0, 0.0, length(params.xy - pos.xz) * 1.5) * params.w * 1.2;
			}

			v2f vert (appdata_t v)
			{
				v2f o;

				float4 pos = v.vertex;
				
				_MorphParams0 = convert_space(_MorphParams0);
				_MorphParams1 = convert_space(_MorphParams1);
				_MorphParams2 = convert_space(_MorphParams2);
				_MorphParams3 = convert_space(_MorphParams3);

				pos.y += morph(pos, _MorphParams0);
				pos.y += morph(pos, _MorphParams1);
				pos.y += morph(pos, _MorphParams2);
				pos.y += morph(pos, _MorphParams3);

				v.vertex = pos;
				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				o._MorphParams01 = half2(
					length(_MorphParams0.xy - pos.xz),
					length(_MorphParams1.xy - pos.xz)
				);

				o._MorphParams23 = half2(
					length(_MorphParams2.xy - pos.xz),
					length(_MorphParams3.xy - pos.xz)
				);

				
				return o;
			}

			float light(float len, float4 f_pos) {
				float sint = sin(_Time * 3.f);
				return smoothstep(2., 0., len);
			}

			float4 frag(v2f i) : COLOR {
				float intens = 0.;
				intens += light(i._MorphParams01.x, i.vertex);
				intens += light(i._MorphParams01.y, i.vertex);
				intens += light(i._MorphParams23.x, i.vertex);
				intens += light(i._MorphParams23.y, i.vertex);
    
				float4 fetch = tex2D(_MainTex, i.texcoord + _Time.x * 0.2);
    
				fetch = saturate(fetch * 5.) * 0.5;
    
				return fetch * intens;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
