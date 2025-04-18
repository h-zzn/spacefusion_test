
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SeedPoints : MonoBehaviour
{
    public List<GameObject> points;

    private Vector4[] pointsvec4;

    // color related
    private Color[] colorarray;
    private Vector4[] colorvec4;

    // texture saving related

    public Transform targetPlane;
    public Material vorMat; // Assign your Voronoi shader
    public RenderTexture renderTexture;
    private Camera renderCamera;
    private int textureWidth = 1024;
    private int textureHeight = 1024;


    // get plane coordinate
    private Transform planeTransform;
    //public Vector3 pointOnPlane;
    public GameObject wherePoint;



    void Start()
    {
        // ------------- color --------------- //
        colorarray = new Color[10];
        colorarray[0] = new Color(1f, 0.82f, 0.965f, 1);
        colorarray[1] = new Color(1, 0.647f, 0.929f, 1);
        colorarray[2] = new Color(0.812f, 0.663f, 0.941f, 1);
        colorarray[3] = new Color(0.694f, 0.345f, 1f, 1);

        colorvec4 = new Vector4[10];
        pointsvec4 = new Vector4[10];


        // ------------- texture --------------- //
        // Setup a camera
        GameObject camObj = new GameObject("RenderCamera");
        renderCamera = camObj.AddComponent<Camera>();
        renderCamera.backgroundColor = Color.black;
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = 10;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;


        // camera facing plane
        if (targetPlane != null)
        {
            renderCamera.transform.position = targetPlane.position + Vector3.up * 10; 
            renderCamera.transform.LookAt(targetPlane.position);
        }

        // rendertexture 만들고
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
            renderCamera.targetTexture = renderTexture;
        }
        renderCamera.cullingMask = LayerMask.GetMask("PlaneForVor");




        planeTransform = this.gameObject.transform;


    }


    void Update()
    {

        for (int i = 0; i < points.Count; i++)
        {
            pointsvec4[i] = new Vector4(points[i].transform.position.x, 0, points[i].transform.position.z, 0);
            colorvec4[i] = new Vector4(colorarray[i].r, colorarray[i].g, colorarray[i].b, 1);
        }
        //Debug.Log(pointsvec4[0].x);

        Renderer renderer = this.GetComponent<Renderer>();
        Material mat = renderer.sharedMaterial;

        mat.SetVectorArray("_Users", pointsvec4);
        mat.SetVectorArray("_Colors", colorvec4);
        mat.SetInt("_Length", pointsvec4.Length); 


        // ------------- texture --------------- //
        Texture2D voronoiTexture = CaptureTexture();
        ApplyTextureToTargetPlane(voronoiTexture);

        Renderer planeRenderer = targetPlane.GetComponent<Renderer>();

        // -------------------------- get color from texture -------------------------- //

        // Assuming the plane uses a standard mesh with normalized UVs that match its scale
        Vector3 localPoint = planeTransform.InverseTransformPoint(wherePoint.transform.position);
        MeshRenderer meshRenderer = planeTransform.GetComponent<MeshRenderer>();
        Texture2D texture = meshRenderer.material.mainTexture as Texture2D;

        if (texture != null)
        {
            
            //float u = Mathf.Clamp01(localPoint.x / planeTransform.localScale.x + 0.5f);
            float u = (localPoint.x + 10) / 20;
            u = Mathf.Clamp01(u);
            //Debug.Log(u);


            //float v = Mathf.Clamp01(localPoint.z / planeTransform.localScale.z + 0.5f);
            float v = (localPoint.z + 10) / 20;
            v = Mathf.Clamp01(v);

            // Get pixel color
            Color color = texture.GetPixelBilinear(u, v);
            //Debug.Log($"Color at point {wherePoint.transform.position} is {color}");
        }




    }

    // -------------------------- custom functions ---------------------------- //
    Texture2D CaptureTexture()
    {
        // Render the shader to the RenderTexture
        renderCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        renderCamera.Render();

        // Read pixels from the RenderTexture and save them as a new Texture2D
        Texture2D outputTexture = new Texture2D(renderTexture.width, renderTexture.height);
        outputTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        outputTexture.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = null;

        // 이걸로 저장 가능
        //SaveTextureToFile(outputTexture, "SavedTexture.png");
        return outputTexture;
    }

    void SaveTextureToFile(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/" + fileName, bytes);
        Debug.Log("Saved Texture to " + fileName);
    }


    void ApplyTextureToTargetPlane(Texture2D texture)
    {
        if (targetPlane != null)
        {
            Renderer planeRenderer = targetPlane.GetComponent<Renderer>();
            if (planeRenderer != null)
            {
                // Ensure the material can use the texture
                if (planeRenderer.material.HasProperty("_MainTex"))
                {
                    planeRenderer.material.SetTexture("_MainTex", texture);
                }
                else
                {
                    Debug.LogWarning("Material does not have a '_MainTex' property.");
                }
            }
        }
    }


}
