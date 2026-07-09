Shader "N3DS/Effects/Color Dissolve"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		[PerRendererData]_DissolveColor ("Dissolve Color", Color) = (1,1,1,0)
		_MainTex ("Noise (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Geometry+2" }
		LOD 200
		Blend DstAlpha SrcAlpha

		Pass
		{
			SetTexture[_MainTex] { ConstantColor[_Color] combine constant }
			SetTexture[_MainTex] { ConstantColor[_DissolveColor] combine previous DOUBLE, texture - constant }
			SetTexture[_MainTex] { ConstantColor[_DissolveColor] combine constant lerp(previous) previous}
		}
	}
}
