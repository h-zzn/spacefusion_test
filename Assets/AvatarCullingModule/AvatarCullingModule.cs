using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CulledAvatar
{
    public GameObject avatar;
    public GameObject raypoint;
    public MeshRenderer meshRenderer;
    public CulledAvatar(GameObject avatar, GameObject raypoint)
    {
        this.avatar = avatar;
        this.raypoint = raypoint;
    }


}
public class AvatarCullingModule : MonoBehaviour
{
    [Header("Avatar Culling")]
    public bool debugRay = true;
    public GameObject raypointPrefab;
    [SerializeField] StartVor vor;
    [SerializeField] List<CulledAvatar> boxes = new List<CulledAvatar>();
    //public List<CulledAvatar> culledAvatars = new List<CulledAvatar>();

    GameObject remote1, remote2;
    [Header("Sharedspace mesh")]
    public bool debugMesh = true;
    private GameObject sharedSpaceMesh;
    public Material mtrl;
    public bool isCondition1 = false;
    // Start is called before the first frame update


    void Start()
    {

    }
    public void setBoxes(GameObject go)
    {
        CulledAvatar box;
        InitCulledAvatar(go, out box);
        boxes.Add(box);
    }

    

    // Update is called once per frame
    void Update()
    {
        if (isCondition1)
        {


            //Debug.Log(sharedSpaceMesh.GetComponent<MeshFilter>().mesh.bounds.center);

            // SceneSelection; pivot1

            if (remote1 != null)
            {
                // 일단 여기 들어가고
                //Debug.Log("culledAvatar1 != null");
                AvatarCulling(remote1);
            }
            else
            {
                try
                {
                    GameObject go = GameObject.Find("RemoteAvatar1");
                    remote1 = go;
                }
                catch { }
            }

            // SceneSelection; pivot2
            if (remote2 != null)
            {
                //Debug.Log("culledAvatar1 != null");
                AvatarCulling(remote2);
            }
            else
            {
                try
                {
                    GameObject go = GameObject.Find("RemoteAvatar");
                    remote2 = go;
                }
                catch { }
            }


        }
        //cullingBoxes();
    }

    public void InitCulledAvatar(GameObject sourceAVT, out CulledAvatar targetAVT)
    {
        GameObject raypointInstance = Instantiate(raypointPrefab, sourceAVT.transform);
        targetAVT = new CulledAvatar(sourceAVT, raypointInstance);
    }
    
    public void CreateSharedSpaceMesh(Vector3[] points)
    {
        sharedSpaceMesh = new GameObject("SharedSpaceMesh");

        MeshFilter meshFilter = sharedSpaceMesh.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = sharedSpaceMesh.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = points;

        int[] triangles = Triangulate(points);

        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        if (mtrl != null)
        {
            meshRenderer.material = mtrl;
            meshRenderer.enabled = debugMesh;
        }

        MeshCollider meshCollider = sharedSpaceMesh.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
    }

    /*
    public void CreateSharedSpaceMesh(Vector3[] points)
    {
        sharedSpaceMesh = new GameObject("SharedSpaceMesh");
        MeshFilter meshFilter = sharedSpaceMesh.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = sharedSpaceMesh.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        // Calculate the center of the mesh
        Vector3 center = Vector3.zero;
        for (int i = 0; i < points.Length; i++)
        {
            center += points[i];
        }
        center /= points.Length;

        // Adjust vertices to be centered around the origin
        Vector3[] centeredVertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            centeredVertices[i] = points[i] - center;
        }

        // Assign the centered vertices to the mesh
        mesh.vertices = centeredVertices;

        int[] triangles = Triangulate(centeredVertices);
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        if (mtrl != null)
        {
            meshRenderer.material = mtrl;
            meshRenderer.enabled = debugMesh;
        }

        MeshCollider meshCollider = sharedSpaceMesh.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;

        // Move the GameObject to the calculated center position
        sharedSpaceMesh.transform.position = center;
    }
    */

    private int[] Triangulate(Vector3[] vertices)
    {
        List<int> indices = new List<int>();
        int n = vertices.Length;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area(vertices) > 0)
            for (int v = 0; v < n; v++) V[v] = v;
        else
            for (int v = 0; v < n; v++) V[v] = (n - 1) - v;

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                break;

            int u = v;
            if (nv <= u) u = 0;
            v = u + 1;
            if (nv <= v) v = 0;
            int w = v + 1;
            if (nv <= w) w = 0;

            if (Snip(vertices, u, v, w, nv, V))
            {
                int a, b, c;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Area(Vector3[] vertices)
    {
        float A = 0;
        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            Vector3 vi = vertices[i];
            Vector3 vj = vertices[j];
            A += vj.x * vi.z - vi.x * vj.z;
        }
        return A * 0.5f;
    }

    private bool Snip(Vector3[] vertices, int u, int v, int w, int n, int[] V)
    {
        Vector3 A = vertices[V[u]];
        Vector3 B = vertices[V[v]];
        Vector3 C = vertices[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * (C.z - A.z)) - ((B.z - A.z) * (C.x - A.x))))
            return false;
        for (int p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector3 P = vertices[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        // The area approach
        float areaABC = Mathf.Abs((A.x - P.x) * (B.z - P.z) - (A.z - P.z) * (B.x - P.x)) +
                        Mathf.Abs((B.x - P.x) * (C.z - P.z) - (B.z - P.z) * (C.x - P.x)) +
                        Mathf.Abs((C.x - P.x) * (A.z - P.z) - (C.z - P.z) * (A.x - P.x));
        float areaOrig = Mathf.Abs((A.x - B.x) * (C.z - B.z) - (A.z - B.z) * (C.x - B.x));
        return Mathf.Abs(areaOrig - areaABC) < 0.01f;
    }

    void AvatarCulling(GameObject culledAvatar)
    {

        //RaycastHit hit;
        Vector3 rayDirection = Vector3.down;

        if (culledAvatar.transform.childCount == 0) return;
        // Draw the ray in the editor for debugging purposes


        if (debugRay)
        {
            Debug.DrawLine(culledAvatar.transform.GetChild(2).position, culledAvatar.transform.GetChild(2).position + rayDirection * 10, Color.red);
        }

        // only active meshrenderer
        MeshRenderer meshRenderer = culledAvatar.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null) return;

        RaycastHit[] hits;
        int layerMask = Physics.AllLayers;
        bool hitSharedSpaceMesh = false;

        // originally 2 -> 1 is head
        //hits = Physics.RaycastAll(culledAvatar.transform.GetChild(2).position, culledAvatar.transform.GetChild(2).position + rayDirection * 10, 10, layerMask);
        hits = Physics.RaycastAll(culledAvatar.transform.GetChild(1).position, rayDirection * 10, 10, layerMask);


        for (int i = 0; i < hits.Length; i++)
        {
            //RaycastHit hit = hits[i];

            // debugger
            /*
            if (hit.collider.gameObject.name.Contains("GameObject"))
            {
                continue;
            }
            else
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} at distance: {hit.distance}");
            }
            */

            foreach (RaycastHit hit in hits)
            {
                //Debug.Log($"Hit object: {hit.collider.gameObject.name}");

                if (hit.collider.gameObject == sharedSpaceMesh)
                {
                    hitSharedSpaceMesh = true;
                    break;  
                }
            }

            meshRenderer.enabled = hitSharedSpaceMesh;
        }

        /*
        // Shoot the ray
        if (Physics.Raycast(culledAvatar.transform.GetChild(2).position, culledAvatar.transform.GetChild(2).position + rayDirection, out hit, 100))
        {
            // 여기서 들어가는게 문제있는거 같은데
            Debug.Log(hit.collider.gameObject);

            if (hit.collider.gameObject == sharedSpaceMesh) //&& culledAvatar.meshRenderer.enabled == false)
            {
                meshRenderer.enabled = true;
            }
            else
            {
                meshRenderer.enabled = false;
            }
        }
        else
        {
            meshRenderer.enabled = false;
        }*/

    }


}
