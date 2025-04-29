Shader "Unlit/CloudWithNoise"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PerlinNoise ("Noise Texture", 2D) = "white" {}
        _AnimatedXY ("Animated XY", Vector) = (0, 0, 0, 0)
        _NoiseSpeed ("Noise Speed", Float) = 0.1
        _NoiseColor ("Blend Color", Color) = (1, 1, 1, 0.5)
      
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            ZTest LEqual
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 cloudUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _PerlinNoise;
            float4 _AnimatedXY;
            float _NoiseSpeed;
            float4 _NoiseColor;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float2 animatedOffset = frac(_Time.y * _NoiseSpeed) * _AnimatedXY.xy;
                o.cloudUV = (o.uv + animatedOffset);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mapCol = tex2D(_MainTex, i.uv);
                float noiseValue = tex2D(_PerlinNoise, i.cloudUV).r;
                fixed4 noiseCol = lerp(mapCol, _NoiseColor, noiseValue);

                fixed4 col = lerp(mapCol, noiseCol, _NoiseColor.a);
                return col;
            }
            ENDCG
        }
    }
}