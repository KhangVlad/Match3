//Shader "Custom/RainbowGradient"
//{
//    Properties
//    {
//        [HideInInspector] [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
//        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
//        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
//        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
//        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
//        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
//        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
//        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
//        [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
//
//        _DegOFfset("Deg Offset", Range(0, 360)) = 0
//        _FlashSpeed("Flash Speed", Range(0, 4)) = 1
//        _FlashInterval("Flash Interval", Float) = 4
//        _FlashLength("Flash Length", Range(0, 1)) = 0.1
//        _FlashDensity("Flash Density", Float) = 4
//    }
//
//    SubShader
//    {
//        Tags
//        {
//            "Queue"="Transparent"
//            "IgnoreProjector"="True"
//            "RenderType"="Transparent"
//            "PreviewType"="Plane"
//            "CanUseSpriteAtlas"="True"
//        }
//
//        Stencil
//        {
//            Ref [_Stencil]
//            Comp [_StencilComp]
//            Pass [_StencilOp]
//            ReadMask [_StencilReadMask]
//            WriteMask [_StencilWriteMask]
//        }
//
//        Cull Off
//        Lighting Off
//        ZWrite Off
//        ZTest [unity_GUIZTestMode]
//        Blend SrcAlpha OneMinusSrcAlpha
//        ColorMask [_ColorMask]
//
//        Pass
//        {
//            Name "Default"
//        CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma target 2.0
//
//            #include "UnityCG.cginc"
//
//            #pragma multi_compile __ UNITY_UI_CLIP_RECT
//            #pragma multi_compile __ UNITY_UI_ALPHACLIP
//
//            struct appdata_t
//            {
//                float4 vertex  : POSITION;
//                float4 color    : COLOR;
//                float2 texcoord : TEXCOORD0;
//                UNITY_VERTEX_INPUT_INSTANCE_ID
//            };
//
//            struct v2f
//            {
//                float4 vertex  : SV_POSITION;
//                fixed4 color    : COLOR;
//                float2 texcoord  : TEXCOORD0;
//                float4 worldPosition : TEXCOORD1;
//                UNITY_VERTEX_OUTPUT_STEREO
//            };
//
//            sampler2D _MainTex;
//            fixed4 _Color;
//            fixed4 _TextureSampleAdd;
//            float4 _ClipRect;
//            float4 _MainTex_ST;
//            float _DegOFfset;
//            float _FlashSpeed;
//            float _FlashInterval;
//            float _FlashLength;
//            float _FlashDensity;
//
//            v2f vert(appdata_t v)
//            {
//                v2f OUT;
//                UNITY_SETUP_INSTANCE_ID(v);
//                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
//                OUT.worldPosition = v.vertex;
//                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
//
//                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
//
//                OUT.color = v.color * _Color;
//                return OUT;
//            }
//            
//            
//
//            float3 HUEtoRGB(in float H)
//{
//    float R = abs(H * 6 - 3) - 1;
//    float G = 2 - abs(H * 6 - 2);
//    float B = 2 - abs(H * 6 - 4);
//    return saturate(float3(R, G, B));
//}
//
//fixed4 frag(v2f IN) : SV_Target
//{
//    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
//
//    #ifdef UNITY_UI_CLIP_RECT
//    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
//    #endif
//
//    #ifdef UNITY_UI_ALPHACLIP
//    clip (color.a - 0.001);
//    #endif
//
//    float time = _Time.y * _FlashSpeed;
//
//    // Smooth radial gradient calculation
//    float angle = atan2(IN.texcoord.y - 0.5, IN.texcoord.x - 0.5);
//    float rad = frac((angle / 6.283184) + (_DegOFfset / 360.0) + time); 
//
//    // color.xyz *= HUEtoRGB(rad); // Apply smooth hue transition
//
//    // Ripple Effect with smoothstep for a soft transition
//    float dist = distance(float2(0.5, 0.5), IN.texcoord);
//    float wave = abs(frac(dist - time) * _FlashInterval - _FlashLength);
//    float ripple = smoothstep(0.05, 0.0, wave) * _FlashDensity; 
//
//    color.xyz += ripple; // Apply smooth ripple effect
//
//    return color;
//}
//
//
//        ENDCG
//        }
//    }
//}

Shader "Custom/RainbowGradient"
{
    Properties
    {
        [HideInInspector] [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _DegOffset("Deg Offset", Range(0, 360)) = 0
        _FlashSpeed("Flash Speed", Range(0, 4)) = 1
        _FlashInterval("Flash Interval", Float) = 4
        _FlashLength("Flash Length", Range(0, 1)) = 0.1
        _FlashDensity("Flash Density", Float) = 4
        _ColorCount("Color Count", Range(1, 12)) = 5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex  : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex  : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _DegOffset;
            float _FlashSpeed;
            float _FlashInterval;
            float _FlashLength;
            float _FlashDensity;
            int _ColorCount;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }
            

fixed4 frag(v2f IN) : SV_Target
{
    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

    #ifdef UNITY_UI_CLIP_RECT
    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
    #endif

    #ifdef UNITY_UI_ALPHACLIP
    clip (color.a - 0.001);
    #endif

    float time = _Time.y * _FlashSpeed;

    // Smooth radial gradient calculation
    float angle = atan2(IN.texcoord.y - 0.5, IN.texcoord.x - 0.5);

    float dist = distance(float2(0.5, 0.5), IN.texcoord);
    float wave = abs(frac(dist - time) * _FlashInterval - _FlashLength);
    float ripple = smoothstep(0.05, 0.0, wave) * _FlashDensity;

    // Generate rainbow gradient based on _ColorCount
    float rainbow = sin(angle * _ColorCount + _DegOffset);
    color.rgb = lerp(color.rgb, float3(1, 0, 1), rainbow * 0.5 + 0.5);
    color.rgb += ripple; // Apply smooth ripple effect

    return color;
}


        ENDCG
        }
    }
}