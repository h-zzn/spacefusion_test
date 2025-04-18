Shader "Custom/Con1Box"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metalness", Range(0, 1)) = 0

        [HDR]_Emission ("Emission", Color) = (0,0,0)
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

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

        // Polygon points and count
        uniform float4 _Points[1024];
        int _PointCount;


        int _mainUser;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        // Checking point in polygon
        bool isInsidePolygon(float3 vertexPoint)
        {
            int intersections = 0;
            for (int i = 0; i < _PointCount; i++)
            {
                float4 a = _Points[i];
                float4 b = _Points[(i + 1) % _PointCount];

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
            if (_mainUser == 1)
            {
        
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Alpha = c.a;
            }
    
            else
            {
                bool isInside = isInsidePolygon(IN.worldPos);
                
                if (isInside)
                {
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    o.Albedo = c.rgb;
                    o.Metallic = _Metallic;
                    o.Alpha = c.a;
                }
                else
                {
                    clip(-1);
                }
            }
        }

        ENDCG
    }
}
