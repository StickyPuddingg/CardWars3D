Shader "Custom/RimLit" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_RimColor("Rim Color", Color) = (0.5,0.5,0.5,1)
		_RimPower("Rim Power", Range(0.1, 10.0)) = 2.0
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

		SubShader{
			Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}

			CGPROGRAM
			#pragma surface surf Lambert vertex:vert
			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _RimColor;
			float _RimPower;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
				float3 worldNormal;
			};

			void vert(inout appdata_full v) {
				float rim = 1.0 - saturate(dot(v.normal, _WorldSpaceLightPos0.xyz));
				v.color.rgb += _RimColor.rgb * pow(rim, _RimPower);
			}

			void surf(Input IN, inout SurfaceOutput o) {
				// Albedo
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG
	}
		//FallBack "Diffuse"
}