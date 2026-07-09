Shader "N3DS/Unlit/Vegetation/Windy Leaves"
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Intensity ("Intensity", Range (0, 1.5) ) = 0.3
		_Cutoff("Alpha cutoff", Range(0.000000,1.000000)) = 0.1
    }
    SubShader {
        
		LOD 200

		Pass {
			Name "FORWARD"
			
			Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }

			Cull Off

			AlphaTest Greater [_Cutoff]
			
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

            float _Intensity;
			
			float4 _Color;	

			float _TimeRange;

			fixed _Cutoff;
			
			#define SUPPORT_TILING

			sampler2D _MainTex;				

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

			float FastSin (float val) {
				val = val - floor(val * 0.15915494309) * 6.28318530718 - 3.14159265359; // scale to range: -pi to pi  make it cyclic
				// powers for taylor series
				float x2 = val * val;
				float x3 = x2 * val;
				float x5 = x3 * x2;
				float x7 = x5 * x2;
 
				// sin
				return (val - x3 * 0.16161616 + x5 * 0.0083333 - x7 * 0.00019841);
			}

			float FastCos (float val) {
				val = val - floor(val * 0.15915494309) * 6.28318530718 - 3.14159265359; // scale to range: -pi to pi  make it cyclic
				// powers for taylor series
				float x2 = val * val;
				float x4 = x2 * x2;
				float x6 = x4 * x2;
				float x8 = x6 * x2;
 
				// cos
				return (1 - x2 * 0.5 + x4 * 0.041666666 - x6 * 0.0013888889 + x8 * 0.000024801587);
			}

            VertexOutput vert (VertexInput v) {

                VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv0 = v.texcoord0.xy;

                o.vertexColor = v.vertexColor * _Color;

				float sinval = FastSin(mul(unity_ObjectToWorld, v.vertex).r);
                
				float node_5818 = sinval * _Intensity * o.uv0.g * FastCos(_Time.g + sinval);
                float3 vert = v.vertex.xyz + float3(node_5818,0.0,node_5818);

				o.pos = UnityObjectToClipPos( vert );

                return o;
            }

            fixed4 frag(VertexOutput i) : SV_Target 
			{
                fixed4 col = tex2D(_MainTex, i.uv0) * i.vertexColor;
				
				clip (col.a = col.a - _Cutoff);
                
				return col;
            }
            ENDCG

            /*SetTexture [_MainTex] {
                combine texture * primary DOUBLE, texture * primary
            }*/
        }

		/*Pass {
			Name "FORWARD"
			
			 Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

			 Cull Off
			 ZWrite Off
			 ZTest Less
         
			 AlphaTest LEqual [_Cutoff]

			 Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

            float _Intensity;
			
			float4 _Color;	

			float _TimeRange;

			fixed _Cutoff;
			
			#define SUPPORT_TILING

			sampler2D _MainTex;				

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

			float FastSin (float val) {
				val = val - floor(val * 0.15915494309) * 6.28318530718 - 3.14159265359; // scale to range: -pi to pi  make it cyclic
				// powers for taylor series
				float x2 = val * val;
				float x3 = x2 * val;
				float x5 = x3 * x2;
				float x7 = x5 * x2;
 
				// sin
				return (val - x3 * 0.16161616 + x5 * 0.0083333 - x7 * 0.00019841);
			}

			float FastCos (float val) {
				val = val - floor(val * 0.15915494309) * 6.28318530718 - 3.14159265359; // scale to range: -pi to pi  make it cyclic
				// powers for taylor series
				float x2 = val * val;
				float x4 = x2 * x2;
				float x6 = x4 * x2;
				float x8 = x6 * x2;
 
				// cos
				return (1 - x2 * 0.5 + x4 * 0.041666666 - x6 * 0.0013888889 + x8 * 0.000024801587);
			}

            VertexOutput vert (VertexInput v) {

                VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv0 = v.texcoord0.xy;

                o.vertexColor = v.vertexColor * _Color;

				float sinval = FastSin(mul(unity_ObjectToWorld, v.vertex).r);
                
				float node_5818 = sinval * _Intensity * o.uv0.g * FastCos(_Time.g + sinval);
                float3 vert = v.vertex.xyz + float3(node_5818,0.0,node_5818);

				o.pos = UnityObjectToClipPos( vert );

                return o;
            }

            fixed4 frag(VertexOutput i) : SV_Target 
			{
                fixed4 col = tex2D(_MainTex, i.uv0) * i.vertexColor;
				
				clip (col.a = col.a - _Cutoff);
                
				return col;
            }
            ENDCG

            /*SetTexture [_MainTex] {
                combine texture * primary DOUBLE, texture * primary
            }*/
        }*/
    }
}
