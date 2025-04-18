using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTopface : MonoBehaviour
{


    public GameObject targetCube;


    void Start()
    {
        if (targetCube == null)
        {
            Debug.LogError("Target cube object not assigned!");
            return;
        }

        MeshFilter meshFilter = targetCube.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found on the target cube object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Find the top face vertices
        float maxY = Mathf.NegativeInfinity;
        foreach (var vertex in vertices)
        {
            if (vertex.y > maxY)
                maxY = vertex.y;
        }

        // Get the top face vertices and triangles
        List<Vector3> topVertices = new List<Vector3>();
        List<int> topTriangles = new List<int>();
        Dictionary<int, int> vertexMap = new Dictionary<int, int>();
        int index = 0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            if (v0.y == maxY && v1.y == maxY && v2.y == maxY)
            {
                if (!vertexMap.ContainsKey(triangles[i]))
                {
                    vertexMap[triangles[i]] = index++;
                    topVertices.Add(v0);
                }
                if (!vertexMap.ContainsKey(triangles[i + 1]))
                {
                    vertexMap[triangles[i + 1]] = index++;
                    topVertices.Add(v1);
                }
                if (!vertexMap.ContainsKey(triangles[i + 2]))
                {
                    vertexMap[triangles[i + 2]] = index++;
                    topVertices.Add(v2);
                }

                topTriangles.Add(vertexMap[triangles[i]]);
                topTriangles.Add(vertexMap[triangles[i + 1]]);
                topTriangles.Add(vertexMap[triangles[i + 2]]);
            }
        }

        // Create the new top face mesh
        Mesh topFaceMesh = new Mesh
        {
            vertices = topVertices.ToArray(),
            triangles = topTriangles.ToArray()
        };
        topFaceMesh.RecalculateNormals();
        topFaceMesh.RecalculateBounds();

        GameObject topFaceObject = new GameObject("TopFaceMesh", typeof(MeshFilter), typeof(MeshRenderer));
        topFaceObject.GetComponent<MeshFilter>().mesh = topFaceMesh;
        topFaceObject.GetComponent<MeshRenderer>().material = targetCube.GetComponent<MeshRenderer>().material;
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }




}
