//Shader "Custom/HexagonTransition" {
//    Properties {
//        _MainTex ("Texture", 2D) = "white" {}
//        _SecondTex ("Second Texture", 2D) = "white" {}
//        _TransitionTex ("Transition Texture", 2D) = "white" {}
//        _Progress ("Transition Progress", Range(0, 1)) = 0
//        _HexSize ("Hexagon Size", Range(0.01, 0.5)) = 0.1
//        _EdgeSmoothing ("Edge Smoothing", Range(0.001, 0.1)) = 0.01
//        _ColorIntensity ("Color Intensity", Range(0, 2)) = 1.0
//        [HDR] _GlowColor ("Glow Color", Color) = (0.5, 0.2, 1.0, 1)
//    }
//
//    SubShader {
//        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
//        LOD 100
//
//        Pass {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #include "UnityCG.cginc"
//
//            struct appdata {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//            };
//
//            struct v2f {
//                float2 uv : TEXCOORD0;
//                float4 vertex : SV_POSITION;
//            };
//
//            sampler2D _MainTex;
//            sampler2D _SecondTex;
//            sampler2D _TransitionTex;
//            float4 _MainTex_ST;
//            float _Progress;
//            float _HexSize;
//            float _EdgeSmoothing;
//            float _ColorIntensity;
//            float4 _GlowColor;
//
//            v2f vert (appdata v) {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                return o;
//            }
//
//            // Function to generate a hexagonal grid pattern
//            float2 hexCoord(float2 uv) {
//                float2 r = float2(1, 1.73);
//                float2 h = r * 0.5;
//                float2 a = fmod(uv, r) - h;
//                float2 b = fmod(uv + h, r) - h;
//                return (dot(a, a) < dot(b, b)) ? a : b;
//            }
//
//            fixed4 frag (v2f i) : SV_Target {
//                // Calculate hexagon grid
//                float2 hexUV = i.uv / _HexSize;
//                float2 gridPos = hexCoord(hexUV);
//                float dist = length(gridPos) / (_HexSize * 0.5);
//                
//                // Use transition texture to control progress
//                float2 transUV = i.uv;
//                float transValue = tex2D(_TransitionTex, transUV).r;
//                
//                // Calculate transition threshold with smoothing
//                float threshold = 1.0 - _Progress;
//                float mask = smoothstep(threshold - _EdgeSmoothing, threshold + _EdgeSmoothing, transValue * (1.0 - dist * 0.3));
//                
//                // Sample both textures
//                fixed4 col1 = tex2D(_MainTex, i.uv);
//                fixed4 col2 = tex2D(_SecondTex, i.uv);
//                
//                // Create glowing edge effect
//                float edgeGlow = smoothstep(_EdgeSmoothing * 2.0, 0.0, abs(mask - 0.5)) * _ColorIntensity;
//                fixed4 glowEffect = _GlowColor * edgeGlow;
//                
//                // Blend between textures using the mask
//                fixed4 finalColor = lerp(col1, col2, mask) + glowEffect;
//                
//                return finalColor;
//            }
//            ENDCG
//        }
//    }
//    
//    FallBack "Diffuse"
//}
Shader "Custom/WorldMapTransition" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionTex ("Transition Texture", 2D) = "white" {}
        _Progress ("Transition Progress", Range(0, 1)) = 0
        _HexSize ("Hexagon Size", Range(0.01, 0.5)) = 0.1
        _EdgeSmoothing ("Edge Smoothing", Range(0.001, 0.1)) = 0.01
        _ColorIntensity ("Color Intensity", Range(0, 2)) = 1.0
        [HDR] _GlowColor ("Glow Color", Color) = (0.5, 0.2, 1.0, 1)
        _Transparency ("Overall Transparency", Range(0, 1)) = 0.7
    }

    SubShader {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True"
        }
        LOD 100
        
        // Enable blending for transparency
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _TransitionTex;
            float4 _MainTex_ST;
            float _Progress;
            float _HexSize;
            float _EdgeSmoothing;
            float _ColorIntensity;
            float4 _GlowColor;
            float _Transparency;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Function to generate a hexagonal grid pattern
            float2 hexCoord(float2 uv) {
                float2 r = float2(1, 1.73);
                float2 h = r * 0.5;
                float2 a = fmod(uv, r) - h;
                float2 b = fmod(uv + h, r) - h;
                return (dot(a, a) < dot(b, b)) ? a : b;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Calculate hexagon grid
                float2 hexUV = i.uv / _HexSize;
                float2 gridPos = hexCoord(hexUV);
                float dist = length(gridPos) / (_HexSize * 0.5);
                
                // Use transition texture to control progress
                float2 transUV = i.uv;
                float transValue = tex2D(_TransitionTex, transUV).r;
                
                // Calculate transition threshold with smoothing
                float threshold = 1.0 - _Progress;
                float mask = smoothstep(threshold - _EdgeSmoothing, threshold + _EdgeSmoothing, transValue * (1.0 - dist * 0.3));
                
                // Sample main texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Create alpha based on transition
                float alpha = mask * col.a * _Transparency;
                
                // Create glowing edge effect
                float edgeGlow = smoothstep(_EdgeSmoothing * 2.0, 0.0, abs(mask - 0.5)) * _ColorIntensity;
                fixed4 glowEffect = _GlowColor * edgeGlow;
                
                // Add glow and adjust alpha
                fixed4 finalColor = col + glowEffect;
                finalColor.a = lerp(alpha, 1.0, edgeGlow * 0.7);
                
                // Fade out as progress approaches 1
                finalColor.a *= 1.0 - smoothstep(0.8, 1.0, _Progress);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
}