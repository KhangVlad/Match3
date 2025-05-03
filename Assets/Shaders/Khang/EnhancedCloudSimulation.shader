//Shader "Custom/MapWithEnhancedClouds"
//{
//    Properties
//    {
//        _MainTex ("Map Texture", 2D) = "white" {}
//        _CloudTex ("Cloud Base Texture", 2D) = "white" {}
//        _DetailNoise ("Detail Noise Texture", 2D) = "white" {}
//        _LargeNoise ("Large Scale Noise", 2D) = "white" {}
//        
//        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.7)
//        _ShadowColor ("Cloud Shadow Color", Color) = (0.7, 0.7, 0.8, 0.5)
//        
//        _WindDirection ("Wind Direction", Vector) = (1, 0.3, 0, 0)
//        _WindSpeed ("Wind Speed", Range(0.001, 0.1)) = 0.02
//        _DetailSpeed ("Detail Speed", Range(0.001, 0.1)) = 0.04
//        
//        _NoiseScale1 ("Noise Scale 1", Range(0.1, 10)) = 1.0
//        _NoiseScale2 ("Noise Scale 2", Range(0.1, 10)) = 2.0
//        
//        _Density ("Cloud Density", Range(0, 2)) = 0.5
//        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.2
//        _Brightness ("Cloud Brightness", Range(0.5, 2)) = 1.2
//        _CloudOpacity ("Overall Cloud Opacity", Range(0, 1)) = 0.7
//        
//        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.03
//        _ShadowStrength ("Map Shadow Strength", Range(0, 1)) = 0.3
//    }
//    SubShader
//    {
//        Tags
//        {
//            "Queue"="Transparent" 
//            "RenderType"="Transparent"
//            "IgnoreProjector"="True"
//        }
//        LOD 100
//        
//        Blend SrcAlpha OneMinusSrcAlpha
//        ZWrite Off
//        Cull Back
//        
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma multi_compile_fog
//            
//            #include "UnityCG.cginc"
//
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//                float4 color : COLOR;
//            };
//
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                float2 baseNoiseUV : TEXCOORD1;
//                float2 detailNoiseUV : TEXCOORD2;
//                float4 vertex : SV_POSITION;
//                float4 color : COLOR;
//                UNITY_FOG_COORDS(3)
//            };
//
//            sampler2D _MainTex; // Map texture
//            float4 _MainTex_ST;
//            sampler2D _CloudTex; // Cloud base texture
//            sampler2D _DetailNoise;
//            sampler2D _LargeNoise;
//            
//            float4 _CloudColor;
//            float4 _ShadowColor;
//            
//            float4 _WindDirection;
//            float _WindSpeed;
//            float _DetailSpeed;
//            
//            float _NoiseScale1;
//            float _NoiseScale2;
//            
//            float _Density;
//            float _EdgeSoftness;
//            float _Brightness;
//            float _CloudOpacity;
//            float _DistortionStrength;
//            float _ShadowStrength;
//
//            // Improved noise function for smoother results
//            float2 hash22(float2 p)
//            {
//                p = float2(dot(p, float2(127.1, 311.7)),
//                           dot(p, float2(269.5, 183.3)));
//                
//                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
//            }
//
//            float perlinNoise(float2 p)
//            {
//                float2 pi = floor(p);
//                float2 pf = frac(p);
//                
//                float2 w = pf * pf * (3.0 - 2.0 * pf); // Smoother interpolation
//                
//                float n00 = dot(hash22(pi), pf);
//                float n01 = dot(hash22(pi + float2(0, 1)), pf - float2(0, 1));
//                float n10 = dot(hash22(pi + float2(1, 0)), pf - float2(1, 0));
//                float n11 = dot(hash22(pi + float2(1, 1)), pf - float2(1, 1));
//                
//                float n0 = lerp(n00, n01, w.y);
//                float n1 = lerp(n10, n11, w.y);
//                
//                return 0.5 + 0.5 * lerp(n0, n1, w.x); // Map to [0, 1]
//            }
//
//            v2f vert(appdata v)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                
//                // Different movement speeds for different scales of noise
//                float2 baseOffset = _Time.y * _WindSpeed * _WindDirection.xy;
//                float2 detailOffset = _Time.y * _DetailSpeed * (_WindDirection.xy + float2(0.2, -0.3));
//                
//                o.baseNoiseUV = o.uv * _NoiseScale1 + baseOffset;
//                o.detailNoiseUV = o.uv * _NoiseScale2 + detailOffset;
//                
//                o.color = v.color;
//                UNITY_TRANSFER_FOG(o, o.vertex);
//                
//                return o;
//            }
//
//            fixed4 frag(v2f i) : SV_Target
//            {
//                // Sample the map texture (this is our background)
//                fixed4 mapTex = tex2D(_MainTex, i.uv);
//                
//                // Sample cloud base texture (optional base for cloud shape)
//                fixed4 cloudBaseTex = tex2D(_CloudTex, i.uv);
//                
//                // Sample large scale noise for overall cloud shape
//                float largeNoise = tex2D(_LargeNoise, i.baseNoiseUV * 0.5).r;
//                
//                // Apply distortion to detail noise UV using the large scale noise
//                float2 distortion = largeNoise * _DistortionStrength;
//                float2 distortedUV = i.detailNoiseUV + distortion;
//                
//                // Sample detail noise for fine details
//                float detailNoise = tex2D(_DetailNoise, distortedUV).r;
//                
//                // Generate additional procedural noise for more variation
//                float proceduralNoise = perlinNoise(i.uv * 8.0 + _Time.y * 0.1);
//                
//                // Combine different noise layers with the cloud base texture
//                float combinedNoise = (largeNoise * 0.6 + detailNoise * 0.3 + proceduralNoise * 0.1) * cloudBaseTex.a;
//                
//                // Apply cloud density adjustment
//                float cloudMask = smoothstep(0.1, 0.1 + _EdgeSoftness, combinedNoise * _Density);
//                
//                // Create depth and lighting variation in clouds
//                float lightVariation = smoothstep(0.2, 0.8, largeNoise * detailNoise);
//                fixed4 cloudColor = lerp(_ShadowColor, _CloudColor, lightVariation) * _Brightness;
//                
//                // Combine with vertex color to allow for additional control
//                cloudColor *= i.color;
//                
//                // Calculate shadow on the map
//                float shadowMask = cloudMask * _ShadowStrength;
//                fixed4 shadowedMap = lerp(mapTex, mapTex * 0.7, shadowMask); // Darken the map where clouds cast shadows
//                
//                // Apply the clouds over the map
//                // First apply cloud shadows to the map, then blend clouds on top
//                fixed4 finalColor = lerp(shadowedMap, cloudColor, cloudMask * _CloudOpacity);
//                
//                // Preserve the original map's alpha
//                finalColor.a = mapTex.a;
//                
//                // Apply fog
//                UNITY_APPLY_FOG(i.fogCoord, finalColor);
//                
//                return finalColor;
//            }
//            ENDCG
//        }
//    }
//    Fallback "Unlit/Transparent"
//}
//


Shader "Custom/MapWithEnhancedClouds_URP"
{
    Properties
    {
        _MainTex ("Map Texture", 2D) = "white" {}
        _CloudTex ("Cloud Base Texture", 2D) = "white" {}
        _DetailNoise ("Detail Noise Texture", 2D) = "white" {}
        _LargeNoise ("Large Scale Noise", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}

        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.7)
        _ShadowColor ("Cloud Shadow Color", Color) = (0.7, 0.7, 0.8, 0.5)

        _WindDirection ("Wind Direction", Vector) = (1, 0.3, 0, 0)
        _WindSpeed ("Wind Speed", Range(0.001, 0.1)) = 0.02
        _DetailSpeed ("Detail Speed", Range(0.001, 0.1)) = 0.04

        _NoiseScale1 ("Noise Scale 1", Range(0.1, 10)) = 1.0
        _NoiseScale2 ("Noise Scale 2", Range(0.1, 10)) = 2.0

        _Density ("Cloud Density", Range(0, 2)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.2
        _Brightness ("Cloud Brightness", Range(0.5, 2)) = 1.2
        _CloudOpacity ("Overall Cloud Opacity", Range(0, 1)) = 0.7

        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.03
        _ShadowStrength ("Map Shadow Strength", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags
            {
                "LightMode" = "Universal2D"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 baseNoiseUV : TEXCOORD2;
                float2 detailNoiseUV : TEXCOORD3;
                half2 lightingUV : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex);
            TEXTURE2D(_DetailNoise);
            SAMPLER(sampler_DetailNoise);
            TEXTURE2D(_LargeNoise);
            SAMPLER(sampler_LargeNoise);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _CloudColor;
                half4 _ShadowColor;

                float4 _WindDirection;
                float _WindSpeed;
                float _DetailSpeed;

                float _NoiseScale1;
                float _NoiseScale2;

                float _Density;
                float _EdgeSoftness;
                float _Brightness;
                float _CloudOpacity;
                float _DistortionStrength;
                float _ShadowStrength;
                float4 _MainTex_ST;
            CBUFFER_END

            // Define shape lights
            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            // Improved noise function for smoother results
            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                                            dot(p, float2(269.5, 183.3)));

                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float perlinNoise(float2 p)
            {
                float2 pi = floor(p);
                float2 pf = frac(p);

                float2 w = pf * pf * (3.0 - 2.0 * pf); // Smoother interpolation

                float n00 = dot(hash22(pi), pf);
                float n01 = dot(hash22(pi + float2(0, 1)), pf - float2(0, 1));
                float n10 = dot(hash22(pi + float2(1, 0)), pf - float2(1, 0));
                float n11 = dot(hash22(pi + float2(1, 1)), pf - float2(1, 1));

                float n0 = lerp(n00, n01, w.y);
                float n1 = lerp(n10, n11, w.y);

                return 0.5 + 0.5 * lerp(n0, n1, w.x); // Map to [0, 1]
            }

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                // Different movement speeds for different scales of noise
                float2 baseOffset = _Time.y * _WindSpeed * _WindDirection.xy;
                float2 detailOffset = _Time.y * _DetailSpeed * (_WindDirection.xy + float2(0.2, -0.3));

                o.baseNoiseUV = o.uv * _NoiseScale1 + baseOffset;
                o.detailNoiseUV = o.uv * _NoiseScale2 + detailOffset;

                o.color = v.color;

                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                // Sample the map texture (this is our background)
                half4 mapTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Sample cloud base texture (optional base for cloud shape)
                half4 cloudBaseTex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, i.uv);

                // Sample large scale noise for overall cloud shape
                float largeNoise = SAMPLE_TEXTURE2D(_LargeNoise, sampler_LargeNoise, i.baseNoiseUV * 0.5).r;

                // Apply distortion to detail noise UV using the large scale noise
                float2 distortion = largeNoise * _DistortionStrength;
                float2 distortedUV = i.detailNoiseUV + distortion;

                // Sample detail noise for fine details
                float detailNoise = SAMPLE_TEXTURE2D(_DetailNoise, sampler_DetailNoise, distortedUV).r;

                // Generate additional procedural noise for more variation
                float proceduralNoise = perlinNoise(i.uv * 8.0 + _Time.y * 0.1);

                // Combine different noise layers with the cloud base texture
                float combinedNoise = (largeNoise * 0.6 + detailNoise * 0.3 + proceduralNoise * 0.1) * cloudBaseTex.a;

                // Apply cloud density adjustment
                float cloudMask = smoothstep(0.1, 0.1 + _EdgeSoftness, combinedNoise * _Density);

                // Create depth and lighting variation in clouds
                float lightVariation = smoothstep(0.2, 0.8, largeNoise * detailNoise);
                half4 cloudColor = lerp(_ShadowColor, _CloudColor, lightVariation) * _Brightness;

                // Ensure consistent color space handling
                #ifdef UNITY_COLORSPACE_GAMMA
        cloudColor.rgb = LinearToSRGB(cloudColor.rgb);
                #endif

                // Combine with vertex color to allow for additional control
                cloudColor *= i.color;

                // Calculate shadow on the map
                float shadowMask = cloudMask * _ShadowStrength;
                half4 shadowedMap = lerp(mapTex, mapTex * 0.7, shadowMask); // Darken the map where clouds cast shadows

                // Apply the clouds over the map
                // First apply cloud shadows to the map, then blend clouds on top
                half4 finalColor = lerp(shadowedMap, cloudColor, cloudMask * _CloudOpacity);

                // Make sure final color has correct alpha
                finalColor.a = mapTex.a;

                // Sample the mask texture to determine which parts will be affected by lighting
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                // Setup surface data for lighting
                SurfaceData2D surfaceData;
                InitializeSurfaceData(finalColor.rgb, finalColor.a, mask, surfaceData);
                InputData2D inputData;
                InitializeInputData(i.uv, i.lightingUV, inputData);

                // Apply 2D lighting
                half4 litColor = CombinedShapeLightShared(surfaceData, inputData);

                // Final color space correction
                #if defined(UNITY_EDITOR) && !defined(UNITY_COLORSPACE_GAMMA)
        litColor.rgb = SRGBToLinear(litColor.rgb);
                #endif

                return litColor;
            }
            ENDHLSL
        }

        // Add Normal Map Pass for 2D lighting
        Pass
        {
            Tags
            {
                "LightMode" = "NormalsRendering"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 tangentWS : TEXCOORD2;
                half3 bitangentWS : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex);
            TEXTURE2D(_DetailNoise);
            SAMPLER(sampler_DetailNoise);
            TEXTURE2D(_LargeNoise);
            SAMPLER(sampler_LargeNoise);

            CBUFFER_START(UnityPerMaterial)
                half4 _CloudColor;
                half4 _ShadowColor;
                float4 _WindDirection;
                float _WindSpeed;
                float _DetailSpeed;
                float _NoiseScale1;
                float _NoiseScale2;
                float _Density;
                float _EdgeSoftness;
                float _Brightness;
                float _CloudOpacity;
                float _DistortionStrength;
                float _ShadowStrength;
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.color = attributes.color;
                o.normalWS = -GetViewForwardDir();
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                    dot(p, float2(269.5, 183.3)));

                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float perlinNoise(float2 p)
            {
                float2 pi = floor(p);
                float2 pf = frac(p);

                float2 w = pf * pf * (3.0 - 2.0 * pf);

                float n00 = dot(hash22(pi), pf);
                float n01 = dot(hash22(pi + float2(0, 1)), pf - float2(0, 1));
                float n10 = dot(hash22(pi + float2(1, 0)), pf - float2(1, 0));
                float n11 = dot(hash22(pi + float2(1, 1)), pf - float2(1, 1));

                float n0 = lerp(n00, n01, w.y);
                float n1 = lerp(n10, n11, w.y);

                return 0.5 + 0.5 * lerp(n0, n1, w.x);
            }

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                // Sample map texture and cloud textures for main appearance
                half4 mapTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 cloudBaseTex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, i.uv);

                // Calculate animated UVs for noise textures
                float2 baseOffset = _Time.y * _WindSpeed * _WindDirection.xy;
                float2 detailOffset = _Time.y * _DetailSpeed * (_WindDirection.xy + float2(0.2, -0.3));
                float2 baseNoiseUV = i.uv * _NoiseScale1 + baseOffset;
                float2 detailNoiseUV = i.uv * _NoiseScale2 + detailOffset;

                // Sample noise for cloud shapes
                float largeNoise = SAMPLE_TEXTURE2D(_LargeNoise, sampler_LargeNoise, baseNoiseUV * 0.5).r;
                float2 distortion = largeNoise * _DistortionStrength;
                float detailNoise = SAMPLE_TEXTURE2D(_DetailNoise, sampler_DetailNoise, detailNoiseUV + distortion).r;

                // Calculate final cloud color and mask
                float combinedNoise = (largeNoise * 0.6 + detailNoise * 0.3 + perlinNoise(i.uv * 8.0 + _Time.y * 0.1) *
                    0.1) * cloudBaseTex.a;
                float cloudMask = smoothstep(0.1, 0.1 + _EdgeSoftness, combinedNoise * _Density);
                float lightVariation = smoothstep(0.2, 0.8, largeNoise * detailNoise);
                half4 cloudColor = lerp(_ShadowColor, _CloudColor, lightVariation) * _Brightness;
                cloudColor *= i.color;

                // Apply shadows and clouds to map
                float shadowMask = cloudMask * _ShadowStrength;
                half4 shadowedMap = lerp(mapTex, mapTex * 0.7, shadowMask);
                half4 finalColor = lerp(shadowedMap, cloudColor, cloudMask * _CloudOpacity);

                // Sample normal map for lighting details
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                // Apply normals rendering for 2D lighting
                return NormalsRenderingShared(finalColor, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }

        // Universal Forward pass
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 baseNoiseUV : TEXCOORD1;
                float2 detailNoiseUV : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex);
            TEXTURE2D(_DetailNoise);
            SAMPLER(sampler_DetailNoise);
            TEXTURE2D(_LargeNoise);
            SAMPLER(sampler_LargeNoise);

            CBUFFER_START(UnityPerMaterial)
                half4 _CloudColor;
                half4 _ShadowColor;
                float4 _WindDirection;
                float _WindSpeed;
                float _DetailSpeed;
                float _NoiseScale1;
                float _NoiseScale2;
                float _Density;
                float _EdgeSoftness;
                float _Brightness;
                float _CloudOpacity;
                float _DistortionStrength;
                float _ShadowStrength;
                float4 _MainTex_ST;
            CBUFFER_END

            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
       dot(p, float2(269.5, 183.3)));

                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            float perlinNoise(float2 p)
            {
                float2 pi = floor(p);
                float2 pf = frac(p);

                float2 w = pf * pf * (3.0 - 2.0 * pf);

                float n00 = dot(hash22(pi), pf);
                float n01 = dot(hash22(pi + float2(0, 1)), pf - float2(0, 1));
                float n10 = dot(hash22(pi + float2(1, 0)), pf - float2(1, 0));
                float n11 = dot(hash22(pi + float2(1, 1)), pf - float2(1, 1));

                float n0 = lerp(n00, n01, w.y);
                float n1 = lerp(n10, n11, w.y);

                return 0.5 + 0.5 * lerp(n0, n1, w.x);
            }

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.color = attributes.color;

                // Different movement speeds for different scales of noise
                float2 baseOffset = _Time.y * _WindSpeed * _WindDirection.xy;
                float2 detailOffset = _Time.y * _DetailSpeed * (_WindDirection.xy + float2(0.2, -0.3));

                o.baseNoiseUV = o.uv * _NoiseScale1 + baseOffset;
                o.detailNoiseUV = o.uv * _NoiseScale2 + detailOffset;

                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                // Sample the map texture (this is our background)
                half4 mapTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Sample cloud base texture (optional base for cloud shape)
                half4 cloudBaseTex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, i.uv);

                // Sample large scale noise for overall cloud shape
                float largeNoise = SAMPLE_TEXTURE2D(_LargeNoise, sampler_LargeNoise, i.baseNoiseUV * 0.5).r;

                // Apply distortion to detail noise UV using the large scale noise
                float2 distortion = largeNoise * _DistortionStrength;
                float2 distortedUV = i.detailNoiseUV + distortion;

                // Sample detail noise for fine details
                float detailNoise = SAMPLE_TEXTURE2D(_DetailNoise, sampler_DetailNoise, distortedUV).r;

                // Generate additional procedural noise for more variation
                float proceduralNoise = perlinNoise(i.uv * 8.0 + _Time.y * 0.1);

                // Combine different noise layers with the cloud base texture
                float combinedNoise = (largeNoise * 0.6 + detailNoise * 0.3 + proceduralNoise * 0.1) * cloudBaseTex.a;

                // Apply cloud density adjustment
                float cloudMask = smoothstep(0.1, 0.1 + _EdgeSoftness, combinedNoise * _Density);

                // Create depth and lighting variation in clouds
                float lightVariation = smoothstep(0.2, 0.8, largeNoise * detailNoise);
                half4 cloudColor = lerp(_ShadowColor, _CloudColor, lightVariation) * _Brightness;

                // Combine with vertex color to allow for additional control
                cloudColor *= i.color;

                // Calculate shadow on the map
                float shadowMask = cloudMask * _ShadowStrength;
                half4 shadowedMap = lerp(mapTex, mapTex * 0.7, shadowMask); // Darken the map where clouds cast shadows

                // Apply the clouds over the map
                half4 finalColor = lerp(shadowedMap, cloudColor, cloudMask * _CloudOpacity);

                return finalColor;
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}