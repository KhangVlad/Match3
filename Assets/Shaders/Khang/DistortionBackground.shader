Shader "UI/DistortionBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionTex ("Distortion Texture", 2D) = "white" {}
        _DistortionAmount ("Distortion Amount", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Vector) = (0.1, 0.1, 0, 0)
        _ColorTint ("Color Tint", Color) = (1, 1, 1, 1)
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.01
        _WaveFrequency ("Wave Frequency", Range(0, 20)) = 10
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 distortionUV : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            sampler2D _DistortionTex;
            float4 _MainTex_ST;
            float4 _DistortionTex_ST;
            float _DistortionAmount;
            float4 _DistortionSpeed;
            float4 _ColorTint;
            float _WaveAmplitude;
            float _WaveFrequency;
            float4 _ClipRect;
            
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // Apply sine wave animation to vertex position
                float timeFactor = _Time.y * _WaveFrequency;
                float waveX = sin(v.vertex.x + timeFactor) * _WaveAmplitude;
                float waveY = cos(v.vertex.y + timeFactor) * _WaveAmplitude;
                v.vertex.xy += float2(waveX, waveY);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.distortionUV = TRANSFORM_TEX(v.uv, _DistortionTex);
                
                // Animate distortion UV coordinates
                o.distortionUV += _Time.y * _DistortionSpeed.xy;
                
                o.color = v.color * _ColorTint;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the distortion texture
                float2 distortion = tex2D(_DistortionTex, i.distortionUV).rg;
                
                // Remap from [0,1] to [-1,1]
                distortion = distortion * 2 - 1;
                
                // Apply distortion to main texture coordinates
                float2 distortedUV = i.uv + distortion * _DistortionAmount;
                
                // Sample the main texture with distorted coordinates
                fixed4 col = tex2D(_MainTex, distortedUV) * i.color;
                
                // Apply time-based color pulsing (optional)
                float pulse = 0.5 + 0.5 * sin(_Time.y);
                col.rgb += float3(0.05, 0.05, 0.05) * pulse;
                
                return col;
            }
            ENDCG
        }
    }
}