
Shader "Unlit/Cloud2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CloudTex ("Cloud Texture", 2D) = "white" {}
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.5)
        _CloudScale ("Cloud Scale", Float) = 1
        _AnimatedXY ("Animated XY", Vector) = (0, 0, 0, 0)
        _CloudSpeed ("Cloud Speed", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
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
            sampler2D _CloudTex;
            float4 _CloudColor;
            float _CloudScale;
            float4 _AnimatedXY;
            float _CloudSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Cloud texture loops over the main texture (1:1 mapping)
                float2 animatedOffset = frac(_Time.y * _CloudSpeed) * _AnimatedXY.xy;
                o.cloudUV = (o.uv + animatedOffset) * _CloudScale; // Scale affects pattern, not loop

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mapCol = tex2D(_MainTex, i.uv);
                fixed4 cloudCol = tex2D(_CloudTex, i.cloudUV) * _CloudColor;
                fixed4 col = lerp(mapCol, cloudCol, cloudCol.a);
                return col;
            }
            ENDCG
        }
    }
}
