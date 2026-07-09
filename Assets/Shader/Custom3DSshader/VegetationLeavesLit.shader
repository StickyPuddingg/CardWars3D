Shader "N3DS/Vegetation/Leaves"
{
	Properties {
		_Color ("Main Color", Color) = (.5, .5, .5, .5)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0.000000,1.000000)) = 0.1
    }
    SubShader {

		Pass {
		
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		
		// Set up basic lighting
		Material {
			Diffuse [_Color]
			Ambient [_Color]
		}
		Lighting On


		Cull Off

		AlphaTest Greater [_Cutoff]

		 SetTexture [_MainTex] {
                combine texture * primary, texture
            }
		}

		/*Pass {

		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		// Set up basic lighting
		Material {
			Diffuse [_Color]
			Ambient [_Color]
		}
		Lighting On

		 Cull Off
		 ZWrite Off
		 ZTest Less
         
		 AlphaTest LEqual [_Cutoff]

		 Blend SrcAlpha OneMinusSrcAlpha
			
            SetTexture [_MainTex] {
                combine texture * primary, texture
            }
		}*/
    }
}
