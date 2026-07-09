Shader "N3DS/Effects/Texture Dissolve"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		[PerRendererData]_DissolveColor ("Dissolve Color", Color) = (1,1,1,1)
		_BaseTex ("Base (RGB)", 2D) = "white" {}
		_MainTex ("Noise (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Geometry+2" }
		LOD 200
		Blend DstAlpha SrcAlpha

		Pass
		{
			SetTexture[_BaseTex] { ConstantColor[_Color] combine texture * constant }
			SetTexture[_MainTex] { ConstantColor[_DissolveColor] combine previous DOUBLE, texture - constant }
			SetTexture[_MainTex] { ConstantColor[_DissolveColor] combine constant lerp(previous) previous}
		}
	}
}
