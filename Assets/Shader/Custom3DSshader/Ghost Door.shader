// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "N3DS/Effects/Ghost Door" {
    Properties {
		_IlluminColor ("Illumination Color", Color) = (1,1,1,1)
		_MainTex ("Mask (A)", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "Queue"="Geometry+1"
            
        }
        Pass {
            Name "FORWARD"
            Cull Back
			Blend One One // Additive         
            
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

                o.pos = UnityObjectToClipPos(v.vertex );

				o.color = _IlluminColor;

				o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

                return o;
            }

            float4 frag(VertexOutput i) : COLOR {
				
				fixed4 c = tex2D(_MainTex, i.texcoord);
				
				return c.a * i.color;
            }
            ENDCG

        }
    }
}
