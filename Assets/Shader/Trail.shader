	Shader "Custom/Trail" {
		Properties {
			_Color ("Color", Color) = (1,1,1,1)
			_MainTex ("Main", 2D) = "white" {}
			_LightTex ("Light", 2D) = "white" {}
		}
				
		SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 colorTexcoord : TEXCOORD0;
				half2 lightTexcoord : TEXCOORD1;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			
			sampler2D _LightTex;
			fixed4 _LightTex_ST;
			
			fixed4 _Color;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.colorTexcoord = TRANSFORM_TEX(v.texcoord, _MainTex);				
				o.lightTexcoord = TRANSFORM_TEX(v.texcoord, _LightTex);
				o.color = v.color;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.colorTexcoord);
				
				fixed4 light1 = tex2D (_LightTex, i.lightTexcoord);
				fixed4 light = light1 + tex2D (_LightTex, -i.lightTexcoord);
				
				color = color * _Color + light;
				
				color *= i.color;
				
				return color;
			}
		ENDCG
	}
}

}