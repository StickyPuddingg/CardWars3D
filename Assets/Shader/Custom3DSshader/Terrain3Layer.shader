// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "N3DS/Terrain/3-Layer" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_TerrainTex1 ("Base (Mask A)", 2D) = "white" {}
		_TerrainTex2 ("Base (Mask A)", 2D) = "white" {}
		_TerrainTex3 ("Base (Mask A)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200	

		/*Pass {
		
			SetTexture[_TerrainTex1]{
				combine texture, texture
			}
			SetTexture[_TerrainTex2]{
				combine previous, texture
			}
			SetTexture[_TerrainTex2] {
				combine previous lerp(previous) texture
			}
			SetTexture[_TerrainTex3]{
				combine previous, texture
			}
			SetTexture[_TerrainTex3] {
				combine previous lerp(previous) texture
			}
		}*/
		

		Pass {

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0
            #include "UnityCG.cginc"
            
            uniform float4 _Color;

			sampler2D _TerrainTex1;
			float4 _TerrainTex1_ST;
			sampler2D _TerrainTex2;
			float4 _TerrainTex2_ST;
			sampler2D _TerrainTex3;
			float4 _TerrainTex3_ST;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
				o.texcoord0 = TRANSFORM_TEX(v.texcoord.xy, _TerrainTex1);
				o.texcoord1 = v.texcoord.xy;
				o.color = _Color;

				o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            float4 frag(VertexOutput i) : COLOR {
				
				fixed4 a1 = tex2D(_TerrainTex1, i.texcoord1);
				fixed4 a2 = tex2D(_TerrainTex2, i.texcoord1);
				fixed4 a3 = tex2D(_TerrainTex3, i.texcoord1);

				fixed4 col1 = tex2D(_TerrainTex1, i.texcoord0) * i.color * 2;

				fixed4 col2 = tex2D(_TerrainTex2, i.texcoord0) * i.color * 2;

				fixed4 col3 = tex2D(_TerrainTex3, i.texcoord0) * i.color * 2;

				fixed4 col = col1 * a1;

				col.rgb = lerp(col.rgb, col2.rgb, a2.a);
				col.rgb = lerp(col.rgb, col3.rgb, a3.a);

				return col;
            }
            ENDCG	
		}
	}

	Fallback Off
}
