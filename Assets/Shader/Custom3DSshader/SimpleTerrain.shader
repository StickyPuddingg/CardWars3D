Shader "N3DS/Terrain/Simple" {
    Properties{
        _MainTex("Main Texture", 2D) = "white" {}
        _TerrainTex("Terrain Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
    }


    SubShader{
        Pass{
            Material{
                Diffuse[_Color]
            }
            Lighting On
            SetTexture[_TerrainTex]{
                Combine texture * primary DOUBLE, texture * constant
            }
        }
    }
    Fallback "Diffuse"
}
