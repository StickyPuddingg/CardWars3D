Shader "N3DS/Unlit/SelfIllumination"
{
	Properties
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_IlluminColor ("Illumination Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			SetTexture[_MainTex]{ combine texture, texture}
			SetTexture[_MainTex] { ConstantColor[_IlluminColor] combine constant lerp(previous) previous}
		}
	}
}
