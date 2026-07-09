// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// N3DS Lightmap shader sample.
Shader "N3DS/Terrain/Lightmap/Simple" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_MainTex("Main Texture", 2D) = "white" {}
		_TerrainTex ("Base (RGB)", 2D) = "white" {}
	}


	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass {
			Tags { "LightMode" = "Vertex" }

			CGPROGRAM

			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma target 2.0

			#include "HLSLSupport.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			#define SUPPORT_MAIN_COLOR
			//#define SUPPORT_VERTEX_COLOR
			//#define SUPPORT_SPECULAR
			#define SUPPORT_TILING

			sampler2D _TerrainTex;
			#ifdef SUPPORT_TILING
			float4 _TerrainTex_ST;
			#endif

			#ifdef SUPPORT_MAIN_COLOR
			float4 _Color;
			#endif

			#ifdef SUPPORT_SPECULAR
			#define SPECULAR_BIAS (0.75)
			//float4 _SpecColor;
			float _Shininess;
			#define SPECULAR_INDEX (0)
			#endif

			#define N3DS_FIXOVERFLOW_R_SCALE (10.0)
			#ifdef N3DS_FIXOVERFLOW_R_SCALE
			#define N3DS_FIXOVERFLOW_SCALE (1.0 / N3DS_FIXOVERFLOW_R_SCALE)
			#endif

			struct Input {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				#ifdef SUPPORT_VERTEX_COLOR
				float4 color : COLOR;
				#endif
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord0 : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef SUPPORT_SPECULAR
			inline float ShadeVertexSpecularGlossy(float3 viewpos, float3 viewN, float3 norm_toLight)
			{
				#ifdef N3DS_FIXOVERFLOW_R_SCALE
				float viewposLenSq = dot(viewpos, viewpos);
				float3 norm_viewpos = -viewpos * (rsqrt(viewposLenSq) * N3DS_FIXOVERFLOW_SCALE);
				float3 tempdir = norm_viewpos + norm_toLight * 0.1;
				float tempdirLenSq = dot(tempdir, tempdir);
				float3 norm_tempdir = tempdir * (rsqrt(tempdirLenSq) * N3DS_FIXOVERFLOW_SCALE);
				float d = max(0.0, dot(viewN * N3DS_FIXOVERFLOW_SCALE, norm_tempdir));
				float s = pow(d * (N3DS_FIXOVERFLOW_R_SCALE * N3DS_FIXOVERFLOW_R_SCALE), _Shininess * 128.0); // As like UnityBlinnPhongLight().
				return s * (SPECULAR_BIAS * 0.5); // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#else // N3DS_FIXOVERFLOW_R_SCALE
				float d = max(0.0, dot(viewN, normalize(normalize(-viewpos) + norm_toLight)));
				float s = pow(d, _Shininess * 128.0); // As like UnityBlinnPhongLight().
				return s * (SPECULAR_BIAS * 0.5); // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#endif // N3DS_FIXOVERFLOW_R_SCALE
			}
			#endif

			inline float4 ShadeVertexLightsFull (float3 viewpos, float3 viewN)
			{
				float4 lightAtten = float4(unity_LightAtten[0].z, unity_LightAtten[1].z, unity_LightAtten[2].z, unity_LightAtten[3].z);

				#ifdef SUPPORT_SPECULAR
				float3 norm_toLight;
				#endif // SUPPORT_SPECULAR

				float3 toLight;
				float4 lightLengthSq;
				float4 lightDiff;

				toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
				lightLengthSq.x = dot(toLight, toLight);
				toLight *= rsqrt(lightLengthSq.x);
				lightDiff.x = dot (viewN, toLight);
				#ifdef SUPPORT_SPECULAR
				#if SPECULAR_INDEX == 0
				norm_toLight = toLight;
				#endif // SPECULAR_INDEX
				#endif // SUPPORT_SPECULAR

				toLight = unity_LightPosition[1].xyz - viewpos.xyz * unity_LightPosition[1].w;
				lightLengthSq.y = dot(toLight, toLight);
				toLight *= rsqrt(lightLengthSq.y);
				lightDiff.y = dot (viewN, toLight);
				#ifdef SUPPORT_SPECULAR
				#if SPECULAR_INDEX == 1
				norm_toLight = toLight;
				#endif // SPECULAR_INDEX
				#endif // SUPPORT_SPECULAR

				toLight = unity_LightPosition[2].xyz - viewpos.xyz * unity_LightPosition[2].w;
				lightLengthSq.z = dot(toLight, toLight);
				toLight *= rsqrt(lightLengthSq.z);
				lightDiff.z = dot (viewN, toLight);
				#ifdef SUPPORT_SPECULAR
				#if SPECULAR_INDEX == 2
				norm_toLight = toLight;
				#endif // SPECULAR_INDEX
				#endif // SUPPORT_SPECULAR

				toLight = unity_LightPosition[3].xyz - viewpos.xyz * unity_LightPosition[3].w;
				lightLengthSq.w = dot(toLight, toLight);
				toLight *= rsqrt(lightLengthSq.w);
				lightDiff.w = dot (viewN, toLight);
				#ifdef SUPPORT_SPECULAR
				#if SPECULAR_INDEX == 3
				norm_toLight = toLight;
				#endif // SPECULAR_INDEX
				#endif // SUPPORT_SPECULAR

				lightAtten = 1.0 / (1.0 + lightLengthSq * lightAtten);

				float4 temp = lightAtten * max(0.0, lightDiff);
				#ifdef N3DS_FIXOVERFLOW_SCALE
				temp *= N3DS_FIXOVERFLOW_SCALE; // Fix for overflow in N3DS.
				#endif

				float4 lightColor;
				lightColor.rgb = unity_LightColor[0].rgb * temp.x;
				lightColor.rgb += unity_LightColor[1].rgb * temp.y;
				lightColor.rgb += unity_LightColor[2].rgb * temp.z;
				lightColor.rgb += unity_LightColor[3].rgb * temp.w;

				#ifdef N3DS_FIXOVERFLOW_SCALE
				lightColor.rgb += (UNITY_LIGHTMODEL_AMBIENT * N3DS_FIXOVERFLOW_SCALE).rgb; // Fix for overflow in N3DS.
				#else
				lightColor.rgb += UNITY_LIGHTMODEL_AMBIENT.rgb;
				#endif

				#ifdef SUPPORT_SPECULAR
				lightColor.a = ShadeVertexSpecularGlossy(viewpos, viewN, norm_toLight);
				#else // SUPPORT_SPECULAR
				lightColor.a = 0.0;
				#endif // SUPPORT_SPECULAR

				return lightColor;
			}

			v2f vert_surf (Input IN)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(IN.vertex);
				
				// prefetch.
				float3 viewpos = mul (UNITY_MATRIX_MV, IN.vertex).xyz;
				float3 viewN = normalize (mul ((float3x3)UNITY_MATRIX_IT_MV, IN.normal));

				float4 light = ShadeVertexLightsFull(viewpos, viewN);
				
				#if defined(SUPPORT_VERTEX_COLOR) && defined(SUPPORT_MAIN_COLOR)
				o.color = IN.color * _Color;
				#elif defined(SUPPORT_VERTEX_COLOR)
				o.color = IN.color;
				#elif defined(SUPPORT_MAIN_COLOR)
				o.color = _Color;
				#endif

				#ifdef SUPPORT_SPECULAR
				#if defined(SUPPORT_VERTEX_COLOR) || defined(SUPPORT_MAIN_COLOR)
				o.color *= light;
				#else // defined(SUPPORT_VERTEX_COLOR) || defined(SUPPORT_MAIN_COLOR)
				o.color = light;
				#endif // defined(SUPPORT_VERTEX_COLOR) || defined(SUPPORT_MAIN_COLOR)
				#else // SUPPORT_SPECULAR
				#if defined(SUPPORT_VERTEX_COLOR) || defined(SUPPORT_MAIN_COLOR)
				o.color.rgb *= light.rgb;
				#else // defined(SUPPORT_VERTEX_COLOR) || defined(SUPPORT_MAIN_COLOR)
				o.color = float4(light.rgb, 1.0);
				#endif // defined(SUPPORT_VERTEX_COLOR) || defined(SUPPORT_MAIN_COLOR)
				#endif // SUPPORT_SPECULAR
				
				#ifdef N3DS_FIXOVERFLOW_R_SCALE
				// Fix for overflow in N3DS.
				o.color.rgb *= N3DS_FIXOVERFLOW_R_SCALE * (1.0 / 2.0); // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#else
				o.color.rgb *= (1.0 / 2.0); // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#endif

				#ifdef SUPPORT_TILING
				o.texcoord0 = TRANSFORM_TEX(IN.texcoord.xy, _TerrainTex);
				#else
				o.texcoord0 = IN.texcoord.xy;
				#endif
				return o;
			}

			fixed4 frag_surf (v2f IN) : SV_Target
			{
				fixed4 col = tex2D(_TerrainTex, IN.texcoord0) * IN.color;
				#ifdef SUPPORT_SPECULAR
				col.rgb = lerp(col.rgb, _SpecColor.rgb, col.a * 2.0) * 2.0;
				#else
				col.rgb *= 2.0;
				#endif
				return col;
			}

			ENDCG
			
			/*
			// for Diffuse
			SetTexture [_TerrainTex] {
				Combine texture * primary DOUBLE, texture * primary
			}*/
			// for Specular
			/*
			SetTexture [_TerrainTex] {
				Combine texture * primary DOUBLE, texture * primary DOUBLE
			}
			SetTexture [_TerrainTex] {
				constantColor [_SpecColor]
				Combine constant lerp (previous) previous, previous
			}
			*/
		}

		Pass {
			Tags { "LightMode" = "VertexLM" }

			CGPROGRAM

			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma target 2.0

			#include "HLSLSupport.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			#define SUPPORT_MAIN_COLOR
			//#define SUPPORT_VERTEX_COLOR
			#define SUPPORT_TILING

			sampler2D _TerrainTex;
			#ifdef SUPPORT_TILING
			float4 _TerrainTex_ST;
			#endif

			#ifdef SUPPORT_MAIN_COLOR
			float4 _Color;
			#endif

			struct Input {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				#if defined(SUPPORT_VERTEX_COLOR)
				float4 color : COLOR;
				#endif
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				#if defined(SUPPORT_MAIN_COLOR) || defined(SUPPORT_VERTEX_COLOR)
				float4 color : COLOR;
				#endif
				float2 lmap : TEXCOORD0;
				float2 texcoord0 : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert_surf (Input IN)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(IN.vertex);

				#ifdef SUPPORT_TILING
				o.texcoord0 = TRANSFORM_TEX(IN.texcoord.xy, _TerrainTex);
				#else
				o.texcoord0 = IN.texcoord.xy;
				#endif

				#if defined(SUPPORT_VERTEX_COLOR) && defined(SUPPORT_MAIN_COLOR)
				o.color = IN.color * _Color * 0.5; // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#elif defined(SUPPORT_VERTEX_COLOR)
				o.color = IN.color * 0.5; // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#elif defined(SUPPORT_MAIN_COLOR)
				o.color = _Color * 0.5; // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#else
				o.color = 0.5;
				#endif

				o.lmap.xy = IN.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				return o;
			}

			fixed4 frag_surf (v2f IN) : SV_Target
			{
				fixed4 col = tex2D(_TerrainTex, IN.texcoord0);
				#if defined(SUPPORT_MAIN_COLOR) || defined(SUPPORT_VERTEX_COLOR)
				col *= IN.color * 2.0;
				#endif
				col.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap));
				return col;
			}

			ENDCG

			// for Lightmap(Note: Counts as VertexLMRGBM in N3DS.)
			/*SetTexture [unity_Lightmap] {
				matrix [unity_LightmapMatrix]
				combine texture * texture alpha DOUBLE
			}
			SetTexture [_TerrainTex] {
				Combine previous * texture DOUBLE, texture // Memo: Not QUAD.
			}
			SetTexture [_TerrainTex] {
				Combine previous * primary DOUBLE, previous * primary DOUBLE
			}
			SetTexture [_TerrainTex] {
				constantColor (0.6,0.6,0.6,1.0)
				Combine previous * constant DOUBLE, previous
			}*/
		}

		// for Editor only.
		Pass {
			Tags { "LightMode" = "VertexLMRGBM" }

			CGPROGRAM

			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma target 2.0

			#include "HLSLSupport.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			#define SUPPORT_MAIN_COLOR
			//#define SUPPORT_VERTEX_COLOR
			#define SUPPORT_TILING

			sampler2D _TerrainTex;
			#ifdef SUPPORT_TILING
			float4 _TerrainTex_ST;
			#endif

			#ifdef SUPPORT_MAIN_COLOR
			float4 _Color;
			#endif

			struct Input {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				#if defined(SUPPORT_VERTEX_COLOR)
				float4 color : COLOR;
				#endif
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				#if defined(SUPPORT_MAIN_COLOR) || defined(SUPPORT_VERTEX_COLOR)
				float4 color : COLOR;
				#endif
				float2 lmap : TEXCOORD0;
				float2 texcoord0 : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert_surf (Input IN)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(IN.vertex);

				#ifdef SUPPORT_TILING
				o.texcoord0 = TRANSFORM_TEX(IN.texcoord.xy, _TerrainTex);
				#else
				o.texcoord0 = IN.texcoord.xy;
				#endif

				#if defined(SUPPORT_VERTEX_COLOR) && defined(SUPPORT_MAIN_COLOR)
				o.color = IN.color * _Color * 0.5; // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#elif defined(SUPPORT_VERTEX_COLOR)
				o.color = IN.color * 0.5; // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#elif defined(SUPPORT_MAIN_COLOR)
				o.color = _Color * 0.5; // because of limited range with u8 RGB - compensated in the combiner/pixel shader
				#else
				o.color = 0.5;
				#endif

				o.lmap.xy = IN.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				return o;
			}

			fixed4 frag_surf (v2f IN) : SV_Target
			{
				fixed4 col = tex2D(_TerrainTex, IN.texcoord0);
				#if defined(SUPPORT_MAIN_COLOR) || defined(SUPPORT_VERTEX_COLOR)
				col *= IN.color * 2.0;
				#endif
				col.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap));
				return col;
			}

			ENDCG
		}
	}

	Fallback Off
}
