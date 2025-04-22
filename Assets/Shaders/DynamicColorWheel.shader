//Shader "Custom/ColorSegmentShader"
//{
//    Properties
//    {
//        _MainTex ("Texture", 2D) = "white" {}
//        _SegmentCount ("Segment Count", Range(2, 16)) = 8
//        [HideInInspector] _ColorCount ("Color Count", Float) = 8
//        [HDR] _Color0 ("Color 0", Color) = (1, 0, 0.5, 1)
//        [HDR] _Color1 ("Color 1", Color) = (1, 0.5, 0, 1)
//        [HDR] _Color2 ("Color 2", Color) = (1, 1, 0, 1)
//        [HDR] _Color3 ("Color 3", Color) = (0.5, 1, 0, 1)
//        [HDR] _Color4 ("Color 4", Color) = (0, 1, 1, 1)
//        [HDR] _Color5 ("Color 5", Color) = (0, 0.5, 1, 1)
//        [HDR] _Color6 ("Color 6", Color) = (0.5, 0, 1, 1)
//        [HDR] _Color7 ("Color 7", Color) = (1, 0, 1, 1)
//        [HDR] _Color8 ("Color 8", Color) = (1, 0, 0, 1)
//        [HDR] _Color9 ("Color 9", Color) = (0, 1, 0, 1)
//        [HDR] _Color10 ("Color 10", Color) = (0, 0, 1, 1)
//        [HDR] _Color11 ("Color 11", Color) = (1, 1, 1, 1)
//        [HDR] _Color12 ("Color 12", Color) = (0.5, 0.5, 0.5, 1)
//        [HDR] _Color13 ("Color 13", Color) = (1, 1, 0.5, 1)
//        [HDR] _Color14 ("Color 14", Color) = (0.5, 1, 1, 1)
//        [HDR] _Color15 ("Color 15", Color) = (1, 0.5, 1, 1)
//        _CenterX ("Center X", Range(0, 1)) = 0.5
//        _CenterY ("Center Y", Range(0, 1)) = 0.5
//        _Rotation ("Rotation", Range(0, 360)) = 0
//    }
//    
//    SubShader
//    {
//        Tags 
//        { 
//            "Queue" = "Transparent" 
//            "RenderType" = "Transparent" 
//            "PreviewType" = "Plane"
//        }
//        
//        Cull Off
//        Lighting Off
//        ZWrite Off
//        Blend SrcAlpha OneMinusSrcAlpha
//        
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #pragma multi_compile_instancing
//            
//            #include "UnityCG.cginc"
//            
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//                UNITY_VERTEX_INPUT_INSTANCE_ID
//            };
//            
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                float4 vertex : SV_POSITION;
//                UNITY_VERTEX_OUTPUT_STEREO
//            };
//            
//            sampler2D _MainTex;
//            float4 _MainTex_ST;
//            float _SegmentCount;
//            float _ColorCount;
//            float4 _Color0, _Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7;
//            float4 _Color8, _Color9, _Color10, _Color11, _Color12, _Color13, _Color14, _Color15;
//            float _CenterX;
//            float _CenterY;
//            float _Rotation;
//            
//            v2f vert (appdata v)
//            {
//                v2f o;
//                UNITY_SETUP_INSTANCE_ID(v);
//                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                return o;
//            }
//            
//            float4 GetColorByIndex(int index) {
//                if (index == 0) return _Color0;
//                else if (index == 1) return _Color1;
//                else if (index == 2) return _Color2;
//                else if (index == 3) return _Color3;
//                else if (index == 4) return _Color4;
//                else if (index == 5) return _Color5;
//                else if (index == 6) return _Color6;
//                else if (index == 7) return _Color7;
//                else if (index == 8) return _Color8;
//                else if (index == 9) return _Color9;
//                else if (index == 10) return _Color10;
//                else if (index == 11) return _Color11;
//                else if (index == 12) return _Color12;
//                else if (index == 13) return _Color13;
//                else if (index == 14) return _Color14;
//                else return _Color15;
//            }
//            
//            fixed4 frag (v2f i) : SV_Target
//            {
//                // Sample the texture
//                fixed4 texColor = tex2D(_MainTex, i.uv);
//                
//                // Calculate the angle from center
//                float2 centerUV = float2(_CenterX, _CenterY);
//                float2 dirVector = i.uv - centerUV;
//                
//                // Apply rotation
//                float rotationRadians = _Rotation * UNITY_PI / 180.0;
//                float2 rotatedDir = float2(
//                    dirVector.x * cos(rotationRadians) - dirVector.y * sin(rotationRadians),
//                    dirVector.x * sin(rotationRadians) + dirVector.y * cos(rotationRadians)
//                );
//                
//                // Calculate angle in degrees (0-360)
//                float angle = atan2(rotatedDir.y, rotatedDir.x) * 180 / UNITY_PI;
//                if (angle < 0) angle += 360;
//                
//                // Determine which segment this angle belongs to
//                int segmentIndex = (int)(angle / (360.0 / _SegmentCount)) % _SegmentCount;
//                
//                // Get the color for this segment
//                // If we have fewer colors than segments, cycle through the available colors
//                int colorIndex = segmentIndex % max(1, (int)_ColorCount);
//                float4 segmentColor = GetColorByIndex(colorIndex);
//                
//                // Combine the texture color with the segment color
//                fixed4 finalColor = texColor * segmentColor;
//                
//                return finalColor;
//            }
//            ENDCG
//        }
//    }
//    
//    FallBack "Sprites/Default"
//    CustomEditor "ColorSegmentShaderEditor"
//}

Shader "Custom/ColorSegmentShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SegmentCount ("Segment Count", Range(2, 16)) = 8
        [HideInInspector] _ColorCount ("Color Count", Float) = 8
        [HDR] _Color0 ("Color 0", Color) = (1, 0, 0.5, 1)
        [HDR] _Color1 ("Color 1", Color) = (1, 0.5, 0, 1)
        [HDR] _Color2 ("Color 2", Color) = (1, 1, 0, 1)
        [HDR] _Color3 ("Color 3", Color) = (0.5, 1, 0, 1)
        [HDR] _Color4 ("Color 4", Color) = (0, 1, 1, 1)
        [HDR] _Color5 ("Color 5", Color) = (0, 0.5, 1, 1)
        [HDR] _Color6 ("Color 6", Color) = (0.5, 0, 1, 1)
        [HDR] _Color7 ("Color 7", Color) = (1, 0, 1, 1)
        [HDR] _Color8 ("Color 8", Color) = (1, 0, 0, 1)
        [HDR] _Color9 ("Color 9", Color) = (0, 1, 0, 1)
        [HDR] _Color10 ("Color 10", Color) = (0, 0, 1, 1)
        [HDR] _Color11 ("Color 11", Color) = (1, 1, 1, 1)
        [HDR] _Color12 ("Color 12", Color) = (0.5, 0.5, 0.5, 1)
        [HDR] _Color13 ("Color 13", Color) = (1, 1, 0.5, 1)
        [HDR] _Color14 ("Color 14", Color) = (0.5, 1, 1, 1)
        [HDR] _Color15 ("Color 15", Color) = (1, 0.5, 1, 1)
        _CenterX ("Center X", Range(0, 1)) = 0.5
        _CenterY ("Center Y", Range(0, 1)) = 0.5
        _Rotation ("Rotation", Range(0, 360)) = 0
        _StartAngleOffset ("Start Angle Offset", Range(0, 360)) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "PreviewType" = "Plane"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SegmentCount;
            float _ColorCount;
            float4 _Color0, _Color1, _Color2, _Color3, _Color4, _Color5, _Color6, _Color7;
            float4 _Color8, _Color9, _Color10, _Color11, _Color12, _Color13, _Color14, _Color15;
            float _CenterX;
            float _CenterY;
            float _Rotation;
            float _StartAngleOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 GetColorByIndex(int index) {
                if (index == 0) return _Color0;
                else if (index == 1) return _Color1;
                else if (index == 2) return _Color2;
                else if (index == 3) return _Color3;
                else if (index == 4) return _Color4;
                else if (index == 5) return _Color5;
                else if (index == 6) return _Color6;
                else if (index == 7) return _Color7;
                else if (index == 8) return _Color8;
                else if (index == 9) return _Color9;
                else if (index == 10) return _Color10;
                else if (index == 11) return _Color11;
                else if (index == 12) return _Color12;
                else if (index == 13) return _Color13;
                else if (index == 14) return _Color14;
                else return _Color15;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Calculate the angle from center
                float2 centerUV = float2(_CenterX, _CenterY);
                float2 dirVector = i.uv - centerUV;
                
                // Apply rotation
                float rotationRadians = _Rotation * UNITY_PI / 180.0;
                float2 rotatedDir = float2(
                    dirVector.x * cos(rotationRadians) - dirVector.y * sin(rotationRadians),
                    dirVector.x * sin(rotationRadians) + dirVector.y * cos(rotationRadians)
                );
                
                // Calculate angle in degrees (0-360)
                float angle = atan2(rotatedDir.y, rotatedDir.x) * 180 / UNITY_PI;
                if (angle < 0) angle += 360;
                
                // Apply start angle offset
                angle = (angle + _StartAngleOffset) % 360;
                
                // Determine which segment this angle belongs to
                int segmentIndex = (int)(angle / (360.0 / _SegmentCount)) % _SegmentCount;
                
                // Get the color for this segment
                // If we have fewer colors than segments, cycle through the available colors
                int colorIndex = segmentIndex % max(1, (int)_ColorCount);
                float4 segmentColor = GetColorByIndex(colorIndex);
                
                // Combine the texture color with the segment color
                fixed4 finalColor = texColor * segmentColor;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
    CustomEditor "ColorSegmentShaderEditor"
}