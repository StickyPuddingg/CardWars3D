// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "N3DS/Special/Flags" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "bump" {}
        _Amplitude ("Amplitude", Float ) = 0.1
        _Windspeed ("Wind speed", Float ) = 0.1
        _Ambient_color ("Ambient_color", Color) = (0.8014706,0.8014706,0.8014706,1)
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            
			Cull Off

			Blend SrcAlpha OneMinusSrcAlpha            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0
            #include "UnityCG.cginc"
            uniform float4 _TimeEditor;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float _Amplitude;
            uniform float _Windspeed;
            uniform float4 _Ambient_color;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
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

                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor * _Ambient_color * 4;

                float4 node_7734 = _Time + _TimeEditor;
                float2 node_4736 = (mul(unity_ObjectToWorld, v.vertex).rgb.rgb+(_Windspeed*node_7734.g)*float2(1,1));
                
				float4 __var = float4(TRANSFORM_TEX(node_4736, _Diffuse),0.0,0);
				__var.r = FastCos(__var.r);
				__var.g = FastSin(__var.g);

                v.vertex.xyz += (__var.rgb*(_Amplitude*o.vertexColor.rgb));
                
				o.pos = UnityObjectToClipPos(v.vertex );

				o.vertexColor = _Ambient_color * 2;
                
				return o;
            }

            float4 frag(VertexOutput i) : COLOR {
                
				return tex2D(_Diffuse,TRANSFORM_TEX(i.uv0, _Diffuse)) * i.vertexColor;
            }
            ENDCG
        }
    }
}
