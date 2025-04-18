Shader "Custom/Zones"
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
        int _BaseRegion;


        // zone 1 and count
        uniform float4 _Zone1[1024];
        int _Zone1Count;

        uniform float4 _Zone2[1024];
        int _Zone2Count;


        int _mainUser;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        // Checking point in polygon
        bool isInsideZone1(float3 vertexPoint)
        {
            int intersections = 0;
            for (int i = 0; i < _Zone1Count; i++)
            {
                float4 a = _Zone1[i];
                float4 b = _Zone1[(i + 1) % _Zone1Count];

                if ((a.z > vertexPoint.z) != (b.z > vertexPoint.z))
                {
                    float intersectX = (b.x - a.x) * (vertexPoint.z - a.z) / (b.z - a.z) + a.x;
                    if (vertexPoint.x < intersectX)
                    {
                        intersections++;
                    }
                }
            }
            return (intersections % 2) != 0;
        }
        
        // Checking point in polygon
        bool isInsideZone2(float3 vertexPoint)
        {
            int intersections = 0;
            for (int i = 0; i < _Zone2Count; i++)
            {
                float4 a = _Zone2[i];
                float4 b = _Zone2[(i + 1) % _Zone2Count];

                if ((a.z > vertexPoint.z) != (b.z > vertexPoint.z))
                {
                    float intersectX = (b.x - a.x) * (vertexPoint.z - a.z) / (b.z - a.z) + a.x;
                    if (vertexPoint.x < intersectX)
                    {
                        intersections++;
                    }
                }
            }
            return (intersections % 2) != 0;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
    
            float minDist = 1e8;
            int minI = 0;

            for (int i = 0; i < _Length; i++)
            {
                float dist = pow(pow(abs(IN.worldPos.x - _Users[i].x), _P) + pow(abs(IN.worldPos.z - _Users[i].z), _P), 1 / _P);

                if (dist < minDist)
                {
                    minDist = dist;
                    minI = i;
                }
            }
    
            // Check if point is inside either zone
            bool isInZone1 = isInsideZone1(IN.worldPos);
            bool isInZone2 = isInsideZone2(IN.worldPos);
            
            // If point is in either zone, clip it
            if (isInZone1 || isInZone2)
            {
                clip(-1);
        
            }
            else
            {
                // Otherwise, render normally
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Emission = _Emission;
                o.Alpha = c.a;
            }
            
        }

        ENDCG
    }

}
