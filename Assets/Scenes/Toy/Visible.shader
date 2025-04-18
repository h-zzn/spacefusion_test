Shader "Custom/Visible"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metalness", Range(0, 1)) = 0

        [HDR]_Emission ("Emission", Color) = (0,0,0)
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

        // voronoi
        _P("P", Range(1,2)) = 2

    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows keepalpha

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half3 _Emission;

        // voronoi region
        float _P;
        int _Length;
        float4 _Users[10];
        fixed4 _Colors[10];
        int _WhichRegion;

        // Polygon points and count
        uniform float4 _Points[1024];
        int _PointCount;

        int _mainUser;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };


        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            
            float minDist = 1e8; // Large initial value
            int minI = 0; // Index of closest point

            for (int i = 0; i < _Length; i++)
            {
                float dist = pow(pow(abs(IN.worldPos.x - _Users[i].x), _P) + pow(abs(IN.worldPos.z - _Users[i].z), _P), 1 / _P);

                if (dist < minDist)
                {
                    minDist = dist;
                    minI = i;
                }
            }
    
            o.Albedo = _Colors[minI].rgb;
            o.Alpha = _Colors[minI].a;
    
        
            // 만약 같은 구역안에 있으면 
            if (minI == _WhichRegion)
            {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
            }
            else
            {
                clip(-1);
            }
    
    
    
    
        }

        ENDCG
    }
}
