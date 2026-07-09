Shader "N3DS/Unlit/Diffuse Detail"
{
	Properties
	{
        _Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
        _Detail("Detail (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			SetTexture[_MainTex]{
			constantColor [_Color]
                Combine constant * texture DOUBLE, texture * constant
            }

			SetTexture[_Detail]{
                Combine previous * texture DOUBLE, texture
            }
		}
	}
}
