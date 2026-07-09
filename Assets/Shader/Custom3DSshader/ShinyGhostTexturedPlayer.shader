// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "N3DS/Effects/Shiny Ghost (Texture-Player)" {
    Properties {
        [Space(10)]_Color ("Color", Color) = (1,1,1,1)
        _EmissiveIntensity ("Emissive Intensity", Range(0, 5)) = 0.5
        [Space(10)]_FresnelStrength ("Fresnel Strength", Range(0, 10)) = 0.5
        [MaterialToggle] _InvertFresnel ("Invert Fresnel", Float ) = 0
		_MainTex ("Mask (A)", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "Queue"="Geometry+1"
            
        }

        Pass {
            Name "FORWARD"
            Cull Back
			Blend One DstColor // Additive         
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0
            #include "UnityCG.cginc"
            uniform float _FresnelStrength;
            uniform float4 _Color;
            uniform float _EmissiveIntensity;
            uniform fixed _InvertFresnel;

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
				
				o.color = fixed4(emissive,_Color.a);

				o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

                return o;
            }

            float4 frag(VertexOutput i) : COLOR {
				
				fixed4 c = tex2D(_MainTex, i.texcoord);
				
				fixed4 col = i.color + c;
				col.a = i.color.a;

				return col;
            }
            ENDCG

			//SetTexture[_MainTex]{ combine texture + primary, primary}
        }
    }
}
