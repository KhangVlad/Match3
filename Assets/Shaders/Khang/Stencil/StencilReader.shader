
Shader "Examples/StencilReader"
{
    Properties
    {
        _Color ("Overlay Color (NotEqual)", Color) = (0,0.2,1,0.5) // Blue color for NotEqual case
        _EqualColor ("Equal Color", Color) = (1,0.2,0,0.5) // New orange color for Equal case
        _MainTex ("Texture", 2D) = "white" {}
        [IntRange]_StencilID ("Stencil ID", Range(0,255)) = 1
        [Enum(Equal,3,NotEqual,6)] _StencilComp ("Stencil Comparison", Float) = 6
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Stencil
        {
            Ref [_StencilID]
            Comp [_StencilComp] // Compare with stencil buffer
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // Transparent blending

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float stencilComp : TEXCOORD1; // Store stencil comparison mode
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _EqualColor; // Added Equal color
                float _StencilComp; // Added to access stencil comp value
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                OUT.stencilComp = _StencilComp; // Pass stencil comparison mode to fragment shader
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // Choose color based on stencil comparison mode
                float4 overlayColor = (IN.stencilComp == 3) ? _EqualColor : _Color;
                
                // Blend color: texture * overlay color
                float4 finalColor = texColor * overlayColor;
                finalColor.a *= 0.8; // Slight alpha reduction for smoother blending
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}