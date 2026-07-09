// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "N3DS/Color" {
    Properties {
		_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags {
            "Queue"="Geometry"
            
        }
		
		Color [_Color]
		Pass {}
    }
}
