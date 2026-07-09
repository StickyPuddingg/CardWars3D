Shader "N3DS/Unlit/Vegetation/Leaves (No Vertex)"
{
	Properties {
		_Color ("Main Color", Color) = (.5, .5, .5, .5)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0.000000,1.000000)) = 0.1
    }
    SubShader {

		Pass {
		
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }

		Cull Off

		AlphaTest Greater [_Cutoff]

		 SetTexture [_MainTex] {
				ConstantColor[_Color]
                combine texture * constant, texture
            }
		}

		/*Pass {

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		 Cull Off
		 ZWrite Off
		 ZTest Less
         
		 AlphaTest LEqual [_Cutoff]

		 Blend SrcAlpha OneMinusSrcAlpha
			
            SetTexture [_MainTex] {
				ConstantColor[_Color]
                combine texture * constant, texture
            }
		}*/
    }
}
