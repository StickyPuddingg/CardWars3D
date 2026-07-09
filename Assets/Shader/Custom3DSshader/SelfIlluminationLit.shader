Shader "N3DS/Lit/SelfIllumination"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_IlluminColor ("Illumination Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			// Set up basic lighting
			Material {
				Diffuse [_Color]
				Ambient [_Color]
			}
			Lighting On

			SetTexture[_MainTex]{ combine texture * primary, texture}
			SetTexture[_MainTex] { ConstantColor[_IlluminColor] combine constant lerp(previous) previous}
		}
	}
}
