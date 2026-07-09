Shader "N3DS/Terrain/Builtin" {
	Properties {
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
	}

	CGINCLUDE

	struct Vertex
	{
		float4 pos : POSITION;
		float2 uv_Splat0 : TEXCOORD0;
		float2 uv_Splat1 : TEXCOORD1;
		float2 tc_Control : TEXCOORD2;
	};

	ENDCG

	// This shader is used by the editor.
	SubShader {

		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _Splat0;
			uniform sampler2D _Splat1;
			uniform sampler2D _Control;
			float4 _Splat0_ST;
			float4 _Splat1_ST;

			Vertex vert(Vertex i)
			{
				Vertex o;
				o.pos = UnityObjectToClipPos (i.pos);
				o.uv_Splat0 = TRANSFORM_TEX(i.uv_Splat0, _Splat0);
				o.uv_Splat1 = TRANSFORM_TEX(i.uv_Splat1, _Splat1);
				o.tc_Control = i.tc_Control;
				return o;
			}

			float4 frag(Vertex i) : COLOR {

				float4 color0 = tex2D(_Splat0, i.uv_Splat0);
				float4 color1 = tex2D(_Splat1, i.uv_Splat1);
				float4 control = tex2D(_Control, i.tc_Control);

				float4 o;
				o.r = (color0.r * control.r) + (color1.r * (1.0 - control.r));
				o.g = (color0.g * control.r) + (color1.g * (1.0 - control.r));
				o.b = (color0.b * control.r) + (color1.b * (1.0 - control.r));
				o.a = 1.0;

				return o;
			}

			ENDCG
		}
	}

	// This is a dummy shader; used to pass the required texture references through the build pipeline.
	// The actual texture combiner is overriden in the 3DS player, because it uses features not available in Unity's texture combiners.
	SubShader {
		Pass{
			SetTexture [_Splat0] {
				combine texture
			}
			SetTexture [_Splat1] {
				combine texture
			}
			SetTexture [_Control] {
				combine texture
			}
		}
	}

	Fallback "Diffuse"
}
