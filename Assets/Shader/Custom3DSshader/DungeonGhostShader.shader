// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "N3DS/Effects/Dungeon Geometry" {
    Properties {
        [Space(10)]_Color ("Color", Color) = (1,1,1,1)
        _EmissiveIntensity ("Emissive Intensity", Range(0, 5)) = 0.5
        [Space(10)]_FresnelStrength ("Fresnel Strength", Range(0, 10)) = 0.5
        [MaterialToggle] _InvertFresnel ("Invert Fresnel", Float ) = 0
		_MainTex ("Mask (A)", 2D) = "white" {}
		_Intensity ("Intensity", Range (0, 2) ) = 1
		_Frequency ("Frequency", Range(0.1, 2)) = 0.4
		[HideInInspector]_BaseColor ("Base", Color) = (0,0,0,1)
    }
    SubShader {
        Tags {
            "Queue"="Geometry"
            
        }

		/*Pass 
		{
			Name "FORWARD"
            Cull Back

			SetTexture[_MainTex] { ConstantColor[_BaseColor] combine constant }
		}*/

        Pass {
            Name "FORWARD"
            Cull Back
			//Blend One One // Additive         
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0
            #include "UnityCG.cginc"
            uniform float _FresnelStrength;
            uniform float4 _Color;
            uniform float _EmissiveIntensity;
            uniform fixed _InvertFresnel;
			float _Intensity;
			float _Frequency;

			float4 _IlluminColor;

			sampler2D _MainTex;
			float4 _MainTex_ST;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
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

            VertexOutput vert (VertexInput v) {
                VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 normalDir = normalize(mul(unity_ObjectToWorld, v.normal));
                float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );

				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);
                float3 normalDirection = normalDir;
				
				float node_5849 = pow(1.0-max(0,dot(normalDirection, viewDirection)),_FresnelStrength);
                float node_5454 = (node_5849*node_5849*node_5849);
                float3 emissive = ((_Color.rgb*lerp( node_5454, (node_5454*-1.0+1.0), _InvertFresnel ))*_EmissiveIntensity);
				
				o.color = fixed4(emissive,_Color.a) * (FastSin(_Time.g * _Frequency) + 2) / 4 * _Intensity;;

				o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

                return o;
            }

            float4 frag(VertexOutput i) : COLOR {
				
				fixed4 c = tex2D(_MainTex, i.texcoord);
				
				return i.color + c;			             
            }
            ENDCG

			//SetTexture[_MainTex]{ combine texture * primary}
        }
    }
}
