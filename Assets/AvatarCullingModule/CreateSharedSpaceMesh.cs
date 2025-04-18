using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSharedSpaceMesh : MonoBehaviour
{
    private Vector3[] vertices;
    public Material mtrl;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Create(Vector3[] points)
    {
        vertices = points;

        // Ensure MeshFilter is attached to the GameObject
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Ensure MeshRenderer is attached to the GameObject
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh; // Assign newly created mesh to the mesh filter

        int[] triangles = new int[(vertices.Length - 2) * 3];

        for (int i = 0; i < vertices.Length - 2; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // This helps with proper lighting

        // Apply material
        if (mtrl != null)
        {
            meshRenderer.material = mtrl;
        }

        // Add and configure MeshCollider
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh; // Assign the mesh to the collider
        meshCollider.convex = false; // Set to true if you need physics interactions like collisions
    }
}
