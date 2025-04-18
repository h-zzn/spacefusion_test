using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfacceExtractor : MonoBehaviour
{
    //new Material(Shader.Find("Sprites/Default"));

    public GameObject targetHouse;
    public GameObject targetPolygon;
    public float upwardThreshold = 0.9f;

    public float wallHeight = 2.5f;

    void Start()
    {

        // --------------------------------------- delete wall -------------------------------------- //
        foreach (Transform child in targetHouse.transform)
        {
            if (child.name == "Structure")
            {
                foreach (Transform secondchild in child.transform)
                {
                    if(secondchild.name == "Walls")
                    {
                        Destroy(secondchild.gameObject);
                        //Debug.Log(secondchild.name);
                    }
                }
                
            }

        }


        // --------------------------------------- extract top surface --------------------------------------- //
        Quaternion originalRot = targetPolygon.transform.localRotation;
        //targetPolygon.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = targetPolygon.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found on the target polygon object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // List to store the top face vertices and triangles
        List<Vector3> topVertices = new List<Vector3>();
        List<int> topTriangles = new List<int>();
        Dictionary<int, int> vertexMap = new Dictionary<int, int>();
        int index = 0;

        // Iterate through each triangle
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            // Calculate the normal of the triangle
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            /*
            // draw normal and mesh triangles in world space
            Vector3 worldV0 = targetPolygon.transform.TransformPoint(v0);
            Vector3 worldV1 = targetPolygon.transform.TransformPoint(v1);
            Vector3 worldV2 = targetPolygon.transform.TransformPoint(v2);
            Vector3 centroid = (worldV0 + worldV1 + worldV2) / 3;
            
            Debug.DrawLine(centroid, centroid + normal * 0.5f, Color.red, 100f);
            Debug.DrawLine(worldV0, worldV1, Color.green, 100f);
            Debug.DrawLine(worldV1, worldV2, Color.green, 100f);
            Debug.DrawLine(worldV2, worldV0, Color.green, 100f);
            */


            // Check if the normal is facing forward
            // initial 2nd floor room 들은 forward

            // up vector is generally correct 
            if (Vector3.Dot(normal, Vector3.up) > upwardThreshold)
            {
                // Add vertices to the map if they are not already added
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

                // Add triangle indices for the top face mesh
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

        GameObject topFaceObject = new GameObject("room_2", typeof(MeshFilter), typeof(MeshRenderer));
        topFaceObject.GetComponent<MeshFilter>().mesh = topFaceMesh;
        topFaceObject.GetComponent<MeshRenderer>().material = targetPolygon.GetComponent<MeshRenderer>().material;


        topFaceObject.transform.localScale = new Vector3(targetPolygon.transform.localScale.x, targetPolygon.transform.localScale.y, targetPolygon.transform.localScale.z);
        topFaceObject.transform.localScale = topFaceObject.transform.localScale * 1f;
        topFaceObject.transform.localRotation = targetPolygon.transform.localRotation;
        topFaceObject.transform.localPosition = targetPolygon.transform.localPosition;

        GameObject tempstructure = targetHouse.transform.Find("Structure").gameObject;
        
        topFaceObject.transform.parent = tempstructure.transform.Find("Floor");


        // --------------------------------------- walling -------------------------------------- //
        // extract boundary edges
        List<Edge> boundaryEdges = ExtractBoundaryEdges(topFaceMesh);

        // here you can visualize boundaries
        //VisualizeBoundaries(topFaceObject, boundaryEdges);

        // create planes 
        GameObject walls = new GameObject("Walls");
        walls.transform.parent = targetHouse.transform.Find("Structure");
        //Debug.Log(walls.transform.parent.name);


        CreateBoundaryPlanes(walls, topFaceObject, boundaryEdges, wallHeight);


        // --------------------------------------- delete floor -------------------------------------- //

        foreach (Transform child in targetHouse.transform)
        {
            if (child.name == "Structure")
            {
                foreach (Transform secondchild in child.transform)
                {
                    if (secondchild.name == "Floor")
                    {
                        Destroy(secondchild.GetChild(0).gameObject);
                        
                        //Debug.Log(secondchild.name);
                    }
                }
                
            }

        }

        // 여기서 얘를 prefab으로 한번 빼야겠다 



        /*
        // squash mesh 
        MeshFilter meshFilter = targetMeshObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].z = 0;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        */



    }


    private List<Edge> ExtractBoundaryEdges(Mesh mesh)
    {
        Dictionary<Edge, int> edgeDict = new Dictionary<Edge, int>();
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            AddEdge(edgeDict, triangles[i], triangles[i + 1]);
            AddEdge(edgeDict, triangles[i + 1], triangles[i + 2]);
            AddEdge(edgeDict, triangles[i + 2], triangles[i]);
        }

        List<Edge> boundaryEdges = new List<Edge>();
        foreach (var edge in edgeDict)
        {
            if (edge.Value == 1)
            {
                boundaryEdges.Add(edge.Key);
            }
        }

        return boundaryEdges;
    }

    
    private void VisualizeBoundaries(GameObject topFaceObject, List<Edge> boundaryEdges)
    {
        MeshFilter meshFilter = topFaceObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found on the top face object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        foreach (var edge in boundaryEdges)
        {
            Vector3 v1 = topFaceObject.transform.TransformPoint(vertices[edge.v1]);
            Vector3 v2 = topFaceObject.transform.TransformPoint(vertices[edge.v2]);

            Debug.DrawLine(v1, v2, Color.blue, 100f);
        }
    }


    private void AddEdge(Dictionary<Edge, int> edgeDict, int v1, int v2)
    {
        Edge edge = new Edge(v1, v2);
        if (edgeDict.ContainsKey(edge))
        {
            edgeDict[edge]++;
        }
        else
        {
            edgeDict[edge] = 1;
        }
    }

    
    private void CreateBoundaryPlanes(GameObject attachToWhich, GameObject topFaceObject, List<Edge> boundaryEdges, float height)
    {

        MeshFilter meshFilter = topFaceObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found on the top face object!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3 center = mesh.bounds.center;


        int counter = 0;
        foreach (var edge in boundaryEdges)
        {

            Vector3 v1 = topFaceObject.transform.TransformPoint(vertices[edge.v1]);
            Vector3 v2 = topFaceObject.transform.TransformPoint(vertices[edge.v2]);

            // Create vertices for the plane
            Vector3 v3 = v1 + Vector3.up * height;
            Vector3 v4 = v2 + Vector3.up * height;

            // Create a plane mesh
            Mesh planeMesh = new Mesh();
            planeMesh.vertices = new Vector3[] { v1, v2, v3, v4 };
            planeMesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            planeMesh.RecalculateNormals();
            planeMesh.RecalculateBounds();

            // Create a new GameObject for the plane

            //GameObject planeObject = new GameObject("BoundaryPlane", typeof(MeshFilter), typeof(MeshRenderer));
            string wallName = "wall_2_" + counter.ToString();
            GameObject planeObject = new GameObject(wallName, typeof(MeshFilter), typeof(MeshRenderer));
            planeObject.GetComponent<MeshFilter>().mesh = planeMesh;
            planeObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));

            planeObject.transform.parent = attachToWhich.transform;
            // 여기 빼니까 되는데? 
            // Rotate the plane to face the center
            //Vector3 directionToCenter = (center - ((v1 + v2) / 2)).normalized;
            //planeObject.transform.LookAt(planeObject.transform.position + directionToCenter);



            counter++;
        }
    }

    private struct Edge
    {
        public int v1, v2;

        public Edge(int v1, int v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge)
            {
                Edge other = (Edge)obj;
                return (v1 == other.v1 && v2 == other.v2) || (v1 == other.v2 && v2 == other.v1);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return v1 ^ v2;
        }
    }



}
