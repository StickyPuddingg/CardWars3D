Shader "N3DS/Unlit/Transparent/Diffuse Detail"
{
	Properties
	{
        _Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
        _Detail("Detail (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			SetTexture[_MainTex]{
			constantColor [_Color]
                Combine constant * texture DOUBLE, constant
            }

			SetTexture[_Detail]{
				constantColor [_Color]
                Combine previous * texture DOUBLE, constant
            }
		}
	}
}
