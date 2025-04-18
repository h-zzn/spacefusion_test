Shader "Custom/VorAndAnchorWall"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic("Metalness", Range(0, 1)) = 0

        [HDR]_Emission("Emission", Color) = (0,0,0)
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        // voronoi
        _P("P", Range(1,2)) = 2
        _isBox("Box",int) = 0
    }

        SubShader
        {
            Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

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

            int _isBox = 0;
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


            // check point close to the polygon
            bool isCloseToBoundary(float3 vertexPoint, float threshold)
            {
                int intersections = 0;
                bool closeTo = false;

                for (int i = 0; i < _PointCount; i++)
                {
                    float4 a = _Points[i];
                    float4 b = _Points[(i + 1) % _PointCount];

                    if ((a.z > vertexPoint.z) != (b.z > vertexPoint.z))
                    {
                        float intersectX = (b.x - a.x) * (vertexPoint.z - a.z) / (b.z - a.z) + a.x;

                        if (abs(vertexPoint.x - intersectX) <= threshold)
                        {
                            closeTo = true;
                        }

                        if (vertexPoint.x < intersectX)
                        {
                            intersections++;
                        }
                    }
                }

                bool isInside = (intersections % 2) != 0;
                return isInside && closeTo;
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

                if (_isBox == 0) {

                    if (_mainUser == 1)
                    {
                        //bool isInside = isInsidePolygon(IN.worldPos);

                        if (minI != _WhichRegion)
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
                        //bool isInsideAndClose = isCloseToBoundary(IN.worldPos, 0.1f);


                        //if (minI == _WhichRegion && isInside) // 이줄이 다르구나? -> 이게 아마 원래 벽 뚫는거때문에 그런거같은데?
                        if (minI == _WhichRegion || isInside) //which region 안에 들어가면 보여주는데, 단 anchor선보다 훨씬 안에 있으면 없애라
                        {
                            /*
                            if (isInsideAndClose == false)
                            {
                                o.Albedo = fixed4(0, 1, 0, 1);
                            }
                            */

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

                else {

                    // dealing with my region
                    if (_mainUser == 1)
                    {
                        bool isInside = isInsidePolygon(IN.worldPos);
                        if (isInside == true)
                        {
                            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                            o.Albedo = c.rgb;
                            o.Metallic = _Metallic;
                            o.Alpha = c.a;
                        }
                        else
                        {
                            if (minI != _WhichRegion)
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

                    }
                    else
                    {
                        bool isInside = isInsidePolygon(IN.worldPos);
                        //bool isInsideAndClose = isCloseToBoundary(IN.worldPos, 0.1f);

                        if (isInside)
                        {
                            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                            o.Albedo = c.rgb;
                            o.Metallic = _Metallic;
                            o.Alpha = c.a;
                        }
                        else
                        {
                            if (minI == _WhichRegion)
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
                }


            }

            ENDCG
        }

}
