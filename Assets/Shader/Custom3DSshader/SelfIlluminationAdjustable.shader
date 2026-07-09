Shader "N3DS/Unlit/Self Illumination (Adjustable)"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_IlluminColor ("Illumination Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", Range (0, 2) ) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			
			#include "UnityCG.cginc"

			struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
				UNITY_VERTEX_OUTPUT_STEREO
            };

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Intensity;			
			float4 _IlluminColor;
			float4 _Color;
			
			VertexOutput vert (VertexInput v) {

                VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv0 = TRANSFORM_TEX(v.texcoord0.xy, _MainTex);

                o.vertexColor = _IlluminColor * _Intensity;

				o.pos = UnityObjectToClipPos( v.vertex );

                return o;
            }

            fixed4 frag(VertexOutput i) : SV_Target 
			{
                fixed4 col = tex2D(_MainTex, i.uv0) * _Color * 2;
				
				fixed4 rescol = col + col.a * i.vertexColor;
                
				return rescol;
            }
            ENDCG

			//SetTexture[_MainTex]{ ConstantColor[_Color] combine texture * constant DOUBLE, texture}
			//SetTexture[_MainTex] { combine primary lerp(previous) previous}
		}
	}
}
