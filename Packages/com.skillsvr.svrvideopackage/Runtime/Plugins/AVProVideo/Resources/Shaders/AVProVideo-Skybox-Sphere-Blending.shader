Shader "AVProVideo/Skybox/SphereBlending"
{
	Properties
	{
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		_Rotation ("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _MainTex ("MainTex (HDR)", 2D) = "black" { }
		[NoScaleOffset] _ChromaTex ("Chroma", 2D) = "black" { }
		[NoScaleOffset] _AlternateTex("Alternate Texture", 2D) = "black" {}
		_AlternateTexWeight("Alternate Texture Weight", Range(0, 1)) = 0
		[KeywordEnum(None, Top_Bottom, Left_Right, Custom_UV)] Stereo ("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug ("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
		[Toggle(USE_YPCBCR)] _UseYpCbCr("Use YpCbCr", Float) = 0
	}

	SubShader
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

		CGINCLUDE
		#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT STEREO_CUSTOM_UV
		#pragma multi_compile STEREO_DEBUG_OFF STEREO_DEBUG
		#pragma multi_compile FORCEEYE_NONE FORCEEYE_LEFT FORCEEYE_RIGHT
		#pragma multi_compile APPLY_GAMMA_OFF APPLY_GAMMA
		#pragma multi_compile USE_YPCBCR_OFF USE_YPCBCR
		#include "UnityCG.cginc"
		#include "AVProVideo.cginc"

		half4 _Tint;
		half _Exposure;
		float _Rotation;

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		half4 _MainTex_HDR;
		float4 _MainTex_ST;
		sampler2D _AlternateTex;
		uniform float _AlternateTexWeight;
		uniform float4 _AlternateTex_ST;
#if USE_YPCBCR
		sampler2D _ChromaTex;
		float4x4 _YpCbCrTransform;
#endif
		uniform float3 _cameraPosition;

		float3 RotateAroundYInDegrees (float3 vertex, float degrees)
		{
			const float CONST_PI = 3.14159265359f;
			float alpha = degrees * CONST_PI / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float3(mul(m, vertex.xz), vertex.y).xzy;
		}

		inline float2 ToRadialCoords(float3 coords)
		{
			const float CONST_PI = 3.14159265359f;
			float3 normalizedCoords = normalize(coords);
			float latitude = acos(normalizedCoords.y);
			float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
			float2 sphereCoords = float2(longitude, latitude) * float2(0.5/CONST_PI, 1.0/CONST_PI);
			float2 radial = float2(0.5,1.0) - sphereCoords;
			radial.x += 0.25;
			radial.x = fmod(radial.x, 1.0);
			return radial;
		}

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
#ifdef UNITY_STEREO_INSTANCING_ENABLED
			UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
		};
		
		struct v2f {
			float4 vertex : SV_POSITION;
			float3 texcoord : TEXCOORD0;
			float2 altTexcoord : TEXCOORD1; // New texture coordinate
#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
			float4 scaleOffset : TEXCOORD2;
	#if STEREO_DEBUG
			float4 tint : COLOR;
	#endif
#endif
#ifdef UNITY_STEREO_INSTANCING_ENABLED
			UNITY_VERTEX_OUTPUT_STEREO
#endif
		};

		v2f sb_vert(appdata_t v)
		{
			v2f o;
#ifdef UNITY_STEREO_INSTANCING_ENABLED
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
#endif
			float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
			o.vertex = XFormObjectToClip(float4(rotated, 0.0));
			o.texcoord = v.vertex.xyz;

			// Calculate the alternate texture coordinate
			o.altTexcoord = TRANSFORM_TEX(v.texcoord, _AlternateTex);

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
			o.scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(_cameraPosition, UNITY_MATRIX_V[0].xyz), _MainTex_ST.y < 0.0);

			#if STEREO_DEBUG
			o.tint = GetStereoDebugTint(IsStereoEyeLeft(_cameraPosition, UNITY_MATRIX_V[0].xyz));
			#endif
#endif
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			float2 tc = ToRadialCoords(i.texcoord);
			#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
			tc.xy *= i.scaleOffset.xy;
			tc.xy += i.scaleOffset.zw;
			#endif

			tc = TRANSFORM_TEX(tc, _MainTex);

			half4 mainTexColor;
#if USE_YPCBCR
			mainTexColor = SampleYpCbCr(_MainTex, _ChromaTex, tc, _YpCbCrTransform);
#else
			mainTexColor = SampleRGBA(_MainTex, tc);
#endif
			half4 tex = SampleRGBA(_MainTex, tc);
			half4 tex2 = SampleRGBA(_AlternateTex, tc);

			// Blend the main texture and alternate texture based on weight
			half4 finalColor = lerp(tex, tex2, _AlternateTexWeight); //lerp(mainTexColor, _AlternateTex, _AlternateTexWeight);

			// Blend the main texture and alternate texture based on weight
			//half4 finalColor = lerp(mainTexColor, altTexColor, _AlternateTexWeight);

			half3 c = finalColor.rgb;//DecodeHDR(tex, _MainTex_HDR);
			//c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
			c *= _Exposure;

			//half3 c = finalColor.rgb;
			//c = DecodeHDR(tex, _MainTex_HDR);
			//c = c * _Tint.rgb;
			///c = c * unity_ColorSpaceDouble.rgb;
			//c *= _Exposure;
#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
	#if STEREO_DEBUG
			c *= i.tint;
	#endif
#endif
			return half4(c, 1.0);
		}
		ENDCG

		Pass	// Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			v2f vert(appdata_t v)
			{
				return sb_vert(v);
			}
			ENDCG
		}
		
	}
}
