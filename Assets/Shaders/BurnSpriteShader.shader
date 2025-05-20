Shader "Unlit/BurnSpriteShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _BurnMap ("Burn Noise Texture", 2D) = "white" {}
        _Threshold ("Burn Threshold", Range(0,1)) = 0.5
        [HDR] _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeWidth ("Edge Width", Range(0.01, 0.2)) = 0.05
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BurnMap;
            float4 _MainTex_ST;
            float _Threshold;
            float4 _EdgeColor;
            float _EdgeWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float burnValue = tex2D(_BurnMap, i.uv).r;

                // Get alpha mask using threshold
                float alpha = step(_Threshold, burnValue);

                // Edge detection
                float edge = smoothstep(_Threshold - _EdgeWidth, _Threshold, burnValue) -
                             smoothstep(_Threshold, _Threshold + _EdgeWidth, burnValue);

                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 edgeCol = _EdgeColor;

                return lerp(edgeCol, col, alpha) * col.a;
            }
            ENDCG
        }
    }
}
