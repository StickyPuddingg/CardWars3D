Shader "N3DS/Unlit/Dungeon Diffuse"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGBA)", 2D) = "white" {}
	}
	SubShader
	{
		//Tags { "LightMode" = "Vertex" "RenderType"="Opaque" }
		Tags { "RenderType"="Opaque" }
		LOD 200

		/*Material {
			Diffuse [_Color]
			Ambient [_Color]
		} 
		Lighting On*/
		
		Pass
		{
			//SetTexture[_MainTex]{ ConstantColor[_Color] combine texture * primary DOUBLE, texture}
			SetTexture[_MainTex]{ ConstantColor[_Color] combine texture * constant DOUBLE, texture}
			//SetTexture[_MainTex]{ ConstantColor[_Color] combine previous * constant, previous}
		}
	}
}
