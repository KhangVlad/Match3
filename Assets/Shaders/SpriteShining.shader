Shader "Custom/SpriteShiny"
{
   Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlintColor ("Glint Color", Color) = (1,0.98,0.9,1)  // Warm white default
        _GlintLength ("Glint Length", Range(0.01,0.5)) = 0.18
        _GlintSharpness ("Glint Sharpness", Range(1,50)) = 18.0
        _GlintSpeed ("Glint Speed", Float) = 2.3
        _GlintIntensity ("Glint Intensity", Range(0,15)) = 4.0
        _GlintAngle ("Glint Angle", Range(0,360)) = 30.0    // Added back
        _GlintNoise ("Glint Noise", Range(0,0.3)) = 0.08    // Subtle imperfections
        _GlintFrequency ("Glint Frequency", Float) = 1.0
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
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Convert angle to direction vector
                float angleRad = _GlintAngle * PI / 180.0;
                float2 direction = normalize(float2(cos(angleRad), sin(angleRad)));
                
                // Project texture coordinates onto glint direction
                float projectedPos = dot(IN.texcoord - 0.5, direction) + 0.5;
                
                // Animate with time and add noise
                float noise = (frac(IN.texcoord.y * 37.0) - 0.5) * _GlintNoise;
                float glintPos = sin(_Time.y * _GlintSpeed * _GlintFrequency + noise);
                
                // Calculate distance with sharp falloff
                float dist = abs(projectedPos - glintPos);
                float glint = pow(saturate(1.0 - dist/_GlintLength), _GlintSharpness);
                
                // Apply intensity and mask with sprite alpha
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
