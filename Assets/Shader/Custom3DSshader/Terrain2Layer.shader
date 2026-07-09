Shader "N3DS/Terrain/2-Layer" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_TerrainTex1 ("Base Map", 2D) = "white" {}
		_TerrainTex2 ("Base (Mask 1)", 2D) = "white" {}
		_MaskTex1("Mask 1 (A)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200		

		Pass {
			SetTexture [_MaskTex1] {
				Combine texture, texture
			}
			SetTexture [_TerrainTex1] {
				Combine previous lerp(previous) texture
			}
			SetTexture [_MaskTex1] {
				Combine previous, texture
			}
			SetTexture [_TerrainTex2] {
				Combine texture lerp(previous) previous
			}
			SetTexture [_TerrainTex2] {
				ConstantColor[_Color]
				Combine previous * constant DOUBLE
			}
		}
	}

	Fallback Off
}
