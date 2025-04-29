Shader "Custom/MapWithEnhancedClouds"
{
    Properties
    {
        _MainTex ("Map Texture", 2D) = "white" {}
        _CloudTex ("Cloud Base Texture", 2D) = "white" {}
        _DetailNoise ("Detail Noise Texture", 2D) = "white" {}
        _LargeNoise ("Large Scale Noise", 2D) = "white" {}
        
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
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 baseNoiseUV : TEXCOORD1;
                float2 detailNoiseUV : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_FOG_COORDS(3)
            };

            sampler2D _MainTex; // Map texture
            float4 _MainTex_ST;
            sampler2D _CloudTex; // Cloud base texture
            sampler2D _DetailNoise;
            sampler2D _LargeNoise;
            
            float4 _CloudColor;
            float4 _ShadowColor;
            
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Different movement speeds for different scales of noise
                float2 baseOffset = _Time.y * _WindSpeed * _WindDirection.xy;
                float2 detailOffset = _Time.y * _DetailSpeed * (_WindDirection.xy + float2(0.2, -0.3));
                
                o.baseNoiseUV = o.uv * _NoiseScale1 + baseOffset;
                o.detailNoiseUV = o.uv * _NoiseScale2 + detailOffset;
                
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the map texture (this is our background)
                fixed4 mapTex = tex2D(_MainTex, i.uv);
                
                // Sample cloud base texture (optional base for cloud shape)
                fixed4 cloudBaseTex = tex2D(_CloudTex, i.uv);
                
                // Sample large scale noise for overall cloud shape
                float largeNoise = tex2D(_LargeNoise, i.baseNoiseUV * 0.5).r;
                
                // Apply distortion to detail noise UV using the large scale noise
                float2 distortion = largeNoise * _DistortionStrength;
                float2 distortedUV = i.detailNoiseUV + distortion;
                
                // Sample detail noise for fine details
                float detailNoise = tex2D(_DetailNoise, distortedUV).r;
                
                // Generate additional procedural noise for more variation
                float proceduralNoise = perlinNoise(i.uv * 8.0 + _Time.y * 0.1);
                
                // Combine different noise layers with the cloud base texture
                float combinedNoise = (largeNoise * 0.6 + detailNoise * 0.3 + proceduralNoise * 0.1) * cloudBaseTex.a;
                
                // Apply cloud density adjustment
                float cloudMask = smoothstep(0.1, 0.1 + _EdgeSoftness, combinedNoise * _Density);
                
                // Create depth and lighting variation in clouds
                float lightVariation = smoothstep(0.2, 0.8, largeNoise * detailNoise);
                fixed4 cloudColor = lerp(_ShadowColor, _CloudColor, lightVariation) * _Brightness;
                
                // Combine with vertex color to allow for additional control
                cloudColor *= i.color;
                
                // Calculate shadow on the map
                float shadowMask = cloudMask * _ShadowStrength;
                fixed4 shadowedMap = lerp(mapTex, mapTex * 0.7, shadowMask); // Darken the map where clouds cast shadows
                
                // Apply the clouds over the map
                // First apply cloud shadows to the map, then blend clouds on top
                fixed4 finalColor = lerp(shadowedMap, cloudColor, cloudMask * _CloudOpacity);
                
                // Preserve the original map's alpha
                finalColor.a = mapTex.a;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent"
}