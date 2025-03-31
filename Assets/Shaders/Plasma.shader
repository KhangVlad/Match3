Shader "UI/PlasmaHueShift"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Plasma Scale", Float) = 1
        _TimeScale ("Plasma Speed", Float) = 1
        _HueSpeed ("Hue Shift Speed", Float) = 1
        _BlendColor1 ("Blend Color 1", Color) = (1,1,1,1)
        _BlendColor2 ("Blend Color 2", Color) = (1,1,1,1)
        _BlendColor3 ("Blend Color 3", Color) = (1,1,1,1)
        _Saturation ("Saturation", Float) = 1
        _Brightness ("Brightness", Float) = 1

    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        LOD 100
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog   

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // UI Image color support
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Scale;
            float _TimeScale;
            float _HueSpeed;
            float _Saturation;
            float _Brightness;
            float4 _BlendColor1;
            float4 _BlendColor2;
            float4 _BlendColor3;


            // Hue Shift Function
            float3 HueShift(float3 color, float hueShift)
            {
                float3x3 RGB_YIQ = float3x3(0.299, 0.587, 0.114, 0.595716, -0.274453, -0.321263, 0.211456, -0.522591,
                                            0.311135);
                float3x3 YIQ_RGB = float3x3(1, 0.9563, 0.6210, 1, -0.2721, -0.6474, 1, -1.1070, 1.7046);
                float3 YIQ = mul(RGB_YIQ, color);
                float hue = atan2(YIQ.z, YIQ.x) + hueShift;
                float chroma = length(float2(YIQ.x, YIQ.z)) * _Saturation;
                float Y = YIQ.x + _Brightness;
                float I = chroma * cos(hue);
                float Q = chroma * sin(hue);

                float3 shiftYIQ = float3(Y, I, Q);
                float3 shiftRGB = mul(YIQ_RGB, shiftYIQ);
                return shiftRGB;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; // UI Image Tint
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y;

                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Blend the colors
                float3 blendColor = lerp(_BlendColor1.rgb, _BlendColor2.rgb, sin(time * _TimeScale) * 0.5 + 0.5);
                blendColor = lerp(blendColor, _BlendColor3.rgb, sin(time * _TimeScale + 3.14159) * 0.5 + 0.5);

                // Apply the blend color to the texture color
                col.rgb = col.rgb * blendColor;

                // Apply hue shift
                col.rgb = HueShift(col.rgb, time * _HueSpeed);

                // Apply UI Image tint
                col.rgb *= i.color.rgb;
                return col;
            }
            ENDCG
        }
    }
}