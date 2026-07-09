Shader "N3DS/Ice" {
    Properties {
        _Rim_color ("Rim_color", Color) = (0.8299633,0.9289047,0.9485294,1)
        _Ambient_color ("Ambient_color", Color) = (0.5294118,0.5294118,0.5294118,1)
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Diffuse_color_copy ("Diffuse_color_copy", Color) = (0.5294118,0.5294118,0.5294118,1)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
		Pass {
		
		Material {
				Diffuse [_Diffuse_color_copy]
				Specular[_Rim_color]
				Ambient [_Ambient_color]
			}
			Lighting On

			SetTexture[_Diffuse] {
			
			ConstantColor[_Rim_color]
			combine texture * constant
			}

		}
    }
}
