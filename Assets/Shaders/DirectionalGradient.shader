Shader "Custom/FloatingGradientSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorTop ("Top Color", Color) = (1,0,0,0.5)
        _ColorBottom ("Bottom Color", Color) = (0,0,1,0.5)
        _ColorLeft ("Left Color", Color) = (0,1,0,0.5)
        _ColorRight ("Right Color", Color) = (1,1,0,0.5)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [Enum(Vertical,0, Horizontal,1, FourColor,2)] _GradientType ("Gradient Type", Float) = 0
        _GradientIntensity ("Gradient Intensity", Range(0,1)) = 0.3
        _GlowIntensity ("Glow Intensity", Range(0,2)) = 0.5
        _PulseSpeed ("Pulse Speed", Range(0,10)) = 0
        _FloatSpeed ("Float Speed", Range(0,10)) = 1
        _FloatDistance ("Float Distance", Range(0,1)) = 0.2
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
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
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
                float2 worldPos : TEXCOORD1;
            };
            
            fixed4 _Color;
            fixed4 _ColorTop;
            fixed4 _ColorBottom;
            fixed4 _ColorLeft;
            fixed4 _ColorRight;
            float _GradientType;
            float _GradientIntensity;
            float _GlowIntensity;
            float _PulseSpeed;
            float _FloatSpeed;
            float _FloatDistance;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xy;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample original sprite texture
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                
                // Calculate modified UVs for floating effect
                float2 floatUV = IN.texcoord;
                
                // Apply floating motion
                floatUV.y += sin(_Time.y * _FloatSpeed + IN.worldPos.x * 2) * _FloatDistance;
                floatUV.x += cos(_Time.y * _FloatSpeed * 0.7 + IN.worldPos.y * 2) * _FloatDistance * 0.5;
                
                // Calculate gradient color using the floating UVs
                fixed4 gradientColor;
                
                if (_GradientType == 0) // Vertical
                {
                    gradientColor = lerp(_ColorBottom, _ColorTop, floatUV.y);
                }
                else if (_GradientType == 1) // Horizontal
                {
                    gradientColor = lerp(_ColorLeft, _ColorRight, floatUV.x);
                }
                else // Four Color
                {
                    fixed4 top = lerp(_ColorLeft, _ColorRight, floatUV.x);
                    fixed4 bottom = lerp(_ColorBottom, _ColorTop, floatUV.x);
                    gradientColor = lerp(bottom, top, floatUV.y);
                }
                
                // Apply pulse if enabled
                if (_PulseSpeed > 0)
                {
                    float pulse = 1.0 + 0.2 * sin(_Time.y * _PulseSpeed);
                    gradientColor *= pulse;
                }
                
                // Apply glow effect
                gradientColor *= (1.0 + _GlowIntensity);
                
                // Blend gradient with original sprite color
                // This is the key change - we're blending instead of replacing
                fixed4 final = lerp(c, c * gradientColor, _GradientIntensity);
                
                // Maintain original alpha
                final.a = c.a * IN.color.a;
                
                // Premultiply alpha
                final.rgb *= final.a;
                
                return final;
            }
        ENDCG
        }
    }
}