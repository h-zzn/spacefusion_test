Shader "Custom/VorAndAnchor"
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

        // Polygon interior
        float4 _interiorPoints_0[100];
        int _interiorPointsCount_0;

        float4 _interiorPoints_1[100];
        int _interiorPointsCount_1;

        float4 _interiorPoints_2[100];
        int _interiorPointsCount_2;

        float4 _interiorPoints_3[100];
        int _interiorPointsCount_3;


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
        
        // check inside polygons as well
        bool isInterior(float3 vertexPoint, float4 pointsarray[100], int pointCount)
        {
            int intersections = 0;
            for (int i = 0; i < pointCount; i++)
            {
                float4 a = pointsarray[i];
                float4 b = pointsarray[(i + 1) % pointCount];

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
    
            
            if (_mainUser == 1)
            {
        
                bool isInside = isInsidePolygon(IN.worldPos);
        
                // 이중에 하나라도 해당하면 clip 해야함
                bool counting0 = isInterior(IN.worldPos, _interiorPoints_0, _interiorPointsCount_0);
                bool counting1 = isInterior(IN.worldPos, _interiorPoints_1, _interiorPointsCount_1);
                bool counting2 = isInterior(IN.worldPos, _interiorPoints_2, _interiorPointsCount_2);
                bool counting3 = isInterior(IN.worldPos, _interiorPoints_3, _interiorPointsCount_3);
        
        
                //if (minI != _WhichRegion && !isInside && counting0 && counting1 && counting2 && counting3)
                if (minI != _WhichRegion && !isInside) // && counting0
                {
                    clip(-1);
                }
        
                else if (minI != _WhichRegion && (counting0 || counting1 || counting2 || counting3))
                {
                    clip(-1);
                }
                else
                {
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    o.Albedo = c.rgb;
                    o.Metallic = _Metallic;
                    o.Alpha = c.a;
                }
            }
            else
            {
                bool isInside = isInsidePolygon(IN.worldPos);
        
                bool counting0 = isInterior(IN.worldPos, _interiorPoints_0, _interiorPointsCount_0);
                bool counting1 = isInterior(IN.worldPos, _interiorPoints_1, _interiorPointsCount_1);
                bool counting2 = isInterior(IN.worldPos, _interiorPoints_2, _interiorPointsCount_2);
                bool counting3 = isInterior(IN.worldPos, _interiorPoints_3, _interiorPointsCount_3);
        
        
                if (minI == _WhichRegion && !isInside)
                {
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    o.Albedo = c.rgb;
                    o.Metallic = _Metallic;
                    o.Alpha = c.a;
                }
                else if (minI == _WhichRegion && (counting0 || counting1 || counting2 || counting3))
                {
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    o.Albedo = c.rgb;
                    o.Metallic = _Metallic;
                    o.Alpha = c.a;
                }
                else
                {
                    //o.Albedo = fixed4(0, 1, 0, 1);
                    clip(-1);
                }
            }
        }

        ENDCG
    }

}
