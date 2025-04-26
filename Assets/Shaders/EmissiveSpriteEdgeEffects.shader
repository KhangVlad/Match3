Shader "Custom/SpriteShinyEdge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlintColor ("Glint Color", Color) = (1,0.98,0.9,1)
        _GlintLength ("Glint Length", Range(0.01,0.5)) = 0.18
        _GlintSharpness ("Glint Sharpness", Range(1,50)) = 18.0
        _GlintSpeed ("Glint Speed", Float) = 2.3
        _GlintIntensity ("Glint Intensity", Range(0,15)) = 4.0
        _GlintAngle ("Glint Angle", Range(0,360)) = 30.0
        _GlintNoise ("Glint Noise", Range(0,0.3)) = 0.08
        _GlintFrequency ("Glint Frequency", Float) = 1.0
        
        // Edge effects properties
        _EdgeBlurSize ("Edge Blur Size", Range(0, 0.1)) = 0.02
        _EdgeNoiseStrength ("Edge Noise Strength", Range(0, 0.5)) = 0.1
        _EdgeNoiseScale ("Edge Noise Scale", Float) = 10.0
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.5
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define PI 3.14159265359

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GlintColor;
            float _GlintLength;
            float _GlintSharpness;
            float _GlintSpeed;
            float _GlintIntensity;
            float _GlintAngle;
            float _GlintNoise;
            float _GlintFrequency;
            
            // Edge effects parameters
            float _EdgeBlurSize;
            float _EdgeNoiseStrength;
            float _EdgeNoiseScale;
            float _EdgeThreshold;

            // Simple noise function
            float rand(float2 co) {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample original texture
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Calculate edge detection (using alpha channel)
                float edge = 1.0 - smoothstep(_EdgeThreshold, _EdgeThreshold + 0.1, c.a);
                
                // Apply edge blur if needed
                if (edge > 0 && _EdgeBlurSize > 0)
                {
                    float4 blurred = float4(0, 0, 0, 0);
                    float blurPixels = 4.0;
                    float2 texelSize = _EdgeBlurSize / _ScreenParams.xy;
                    
                    for (float x = -blurPixels; x <= blurPixels; x++)
                    {
                        for (float y = -blurPixels; y <= blurPixels; y++)
                        {
                            float2 offset = float2(x, y) * texelSize;
                            blurred += tex2D(_MainTex, IN.texcoord + offset) * IN.color;
                        }
                    }
                    
                    blurred /= pow(blurPixels * 2 + 1, 2);
                    c = lerp(c, blurred, edge * _EdgeBlurSize * 10);
                }
                
                // Add edge noise
                if (edge > 0 && _EdgeNoiseStrength > 0)
                {
                    float noise = rand(IN.texcoord * _EdgeNoiseScale + _Time.xx);
                    noise = (noise - 0.5) * 2.0 * _EdgeNoiseStrength * edge;
                    c.rgb += noise;
                }
                
                // Glint effect (original code)
                float angleRad = _GlintAngle * PI / 180.0;
                float2 direction = normalize(float2(cos(angleRad), sin(angleRad)));
                
                float projectedPos = dot(IN.texcoord - 0.5, direction) + 0.5;
                
                float noiseGlint = (frac(IN.texcoord.y * 37.0) - 0.5) * _GlintNoise;
                float glintPos = sin(_Time.y * _GlintSpeed * _GlintFrequency + noiseGlint);
                
                float dist = abs(projectedPos - glintPos);
                float glint = pow(saturate(1.0 - dist/_GlintLength), _GlintSharpness);
                
                glint *= _GlintIntensity * c.a;
                
                // Additive blending (preserves original colors)
                c.rgb += _GlintColor.rgb * glint;
                c.rgb *= c.a;
                
                return c;
            }
            ENDCG
        }
    }
}