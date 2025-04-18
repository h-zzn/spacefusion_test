Shader "Custom/Voronoi"
{
    Properties
    {
        _HeatTex("Texture", 2D) = "white" {}
        _P("P", Range(1,2)) = 2
        
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"}
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:blend

        // voronoi region 
        sampler2D _HeatTex;
        float _P;
        float _D;
        int _Length;
        float4 _Users[10];
        fixed4 _Colors[10];

        // anchor points
        uniform float4 _Points[1024];
        int _PointCount;



        struct Input {
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


        void surf (Input IN, inout SurfaceOutputStandard o) {
            float minDist = 1e8; // Large initial value
            int minI = 0; // Index of closest point

            for (int i = 0; i < _Length; i++) {
                float dist = pow(pow(abs(IN.worldPos.x - _Users[i].x), _P) + pow(abs(IN.worldPos.z - _Users[i].z), _P), 1/_P);

                if (dist < minDist) {
                    minDist = dist;
                    minI = i;
                }
            }
    
            if (_D == 0) {
                o.Albedo = _Colors[minI].rgb;
                o.Alpha = _Colors[minI].a;
            } else {
                float c = minDist;
                float4 color = tex2D(_HeatTex, float2(c, 0.5)); // 앞 자리 texture, 뒤가 coordinate
                o.Albedo = color.rgb;
                o.Alpha = color.a;
            }
            
    
        }
        ENDCG
    }
    Fallback "Diffuse"
}
