Shader "Custom/EmissiveSpriteMasked"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1
        [HideInInspector]_RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector]_Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData]_AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData]_EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        // Add mask interaction property
        // [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Mask Interaction", Float) = 8 // Equal = 8
        // Replace the existing _StencilComp property with this:
        [MaterialEnum(Off,0,Visible Inside Mask,3,Visible Outside Mask,6)] _StencilComp ("Mask Interaction", Float) = 3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp [_StencilComp]  // Use the property to control comparison
            Pass Keep
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _RendererColor;
            float4 _EmissionColor;
            float _EmissionStrength;
            float4 _Flip;
            float _EnableExternalAlpha;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                float2 flipSign = _Flip.xy;
                OUT.texcoord = IN.texcoord * flipSign;
                OUT.color = IN.color * _Color * _RendererColor;
                return OUT;
            }

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 c = tex2D(_MainTex, uv);
                #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
                if (_EnableExternalAlpha > 0.5)
                {
                    c.a = tex2D(_AlphaTex, uv).r;
                }
                #endif
                return c;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 e = tex2D(_MainTex, i.texcoord);
                fixed4 c = SampleSpriteTexture(i.texcoord) * i.color;
                fixed4 emission = _EmissionColor * _EmissionStrength;
                c.rgb += emission.rgb;
                return c;
            }
            ENDCG
        }
    }
}