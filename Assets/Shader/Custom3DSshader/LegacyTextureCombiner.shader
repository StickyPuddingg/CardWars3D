Shader "N3DS/LegacyTextureCombiner" {
    Properties {
        _ColourTex ("Base Colour Texture", 2D) = "white" {}
        _DetailTex1 ("Detail Texture 1", 2D) = "white" {}
        _DetailTex2 ("Detail Texture 2", 2D) = "white" {}
    }

    SubShader {
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha

            SetTexture[_ColourTex] {
                combine texture
            }

            SetTexture[_DetailTex1] {
                combine previous + texture
            }

            SetTexture[_DetailTex2] {
                combine previous + texture
            }

            SetTexture[null] {
                constantColor (0.4, 0.4, 0.4, 1)
                combine previous * constant
            }
        }
    }
    //Fallback "Legacy Shaders/VertexLit"
}
