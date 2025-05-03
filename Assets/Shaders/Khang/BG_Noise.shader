Shader "UI/ProceduralPattern"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _UVScale("UV Scale", Float) = 15.0
        _GradientColor1("Gradient Color 1", Color) = (1, 1, 0, 1)  // Yellow
        _GradientColor2("Gradient Color 2", Color) = (1, 0, 0, 1)  // Red
        _GradientColor3("Gradient Color 3", Color) = (0, 1, 0, 1)  // Green
        
        // UI Properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
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
            
            float _UVScale;
            float4 _GradientColor1;
            float4 _GradientColor2;
            float4 _GradientColor3;
            
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
            
            // Voronoi noise function
            float2 voronoi(float2 x)
            {
                float2 n = floor(x);
                float2 f = frac(x);
                
                float md = 8.0;
                float2 m = float2(8.0, 8.0);
                
                for(int j = -1; j <= 1; j++)
                {
                    for(int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(float(i), float(j));
                        float2 o = n + g;
                        float2 r = g - f + (0.5 + 0.5 * sin(fmod(o.x + o.y * 3.0, 7.0)));
                        
                        float d = dot(r, r);
                        
                        if(d < md)
                        {
                            md = d;
                            m = r;
                        }
                    }
                }
                
                return float2(md, m.x + m.y);
            }
            
            // Simple hash for randomness
            float hash(float2 p)
            {
                p = frac(p * 0.3183099 + .1);
                p *= 17.0;
                return frac(p.x * p.y * (p.x + p.y));
            }
            
           float4 frag(v2f IN) : SV_Target 
            {
                // Scale UV
                float2 uv = IN.texcoord * _UVScale;
                
                // Generate Voronoi patterns
                float2 vor1 = voronoi(uv);
                float2 vor2 = voronoi(uv + float2(0.5, 0.5));
                
                // Create pattern 1 (yellow-red gradient)
                float pattern1 = vor1.x;
                float4 color1 = lerp(_GradientColor1, _GradientColor2, pattern1);
                
                // Create pattern 2 (green with voronoi)
                float pattern2 = vor2.y;
                float4 color2 = _GradientColor3 * (1.0 - pattern2);
                
                // Combine patterns
                float mask = smoothstep(0.0, 1.0, vor1.y);
                float4 finalColor = lerp(color2, color1, mask);
                
                // Add some noise variation
                float noise = hash(floor(uv));
                finalColor *= (0.8 + 0.2 * noise);
                
                // Apply color tint
                finalColor *= IN.color;
                
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif
                
                return finalColor;
            }
            ENDCG
        }
    }
}