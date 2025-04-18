using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutVor : MonoBehaviour
{
    public int whichRegion;

    // vor related
    public List<GameObject> points;
    private Vector4[] pointsvec4;

    public Material toWhatMat;


    void Start()
    {

        pointsvec4 = new Vector4[10];


        // 여기서 material 바꿔주자
        ChangeMaterial(toWhatMat, this.gameObject);

    }


    void Update()
    {

        for (int i = 0; i < points.Count; i++)
        {
            pointsvec4[i] = new Vector4(points[i].transform.position.x, 0, points[i].transform.position.z, 0);
        }

        Renderer renderer = this.GetComponent<Renderer>();
        Material mat = renderer.sharedMaterial;

        mat.SetVectorArray("_Users", pointsvec4);
        mat.SetInt("_Length", pointsvec4.Length);
        mat.SetInt("_WhichRegion", whichRegion);

    }

    void ChangeMaterial(Material newMat, GameObject whichPrefab)
    {

        Renderer[] renderers = whichPrefab.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock(); // Create a new material property block

        foreach (Renderer rend in renderers)
        {

            Material[] modifiedMaterials = new Material[rend.sharedMaterials.Length];

            for (int i = 0; i < rend.sharedMaterials.Length; i++)
            {
                // Duplicate the original material
                Material originalMaterial = rend.sharedMaterials[i];
                Material modifiedMaterial = new Material(newMat); // Zone1

                modifiedMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry + 100; // originally Geometry


                //modifiedMaterials[i] = modifiedMaterial; // 뒤쪽에 하나 더 있음

                // preserve color
                if (originalMaterial.HasProperty("_Color"))
                {
                    Color originalColor = originalMaterial.color;
                    block.SetColor("_Color", originalColor);
                    rend.SetPropertyBlock(block, i);
                }

                // Copy main texture (albedo)
                if (originalMaterial.HasProperty("_MainTex"))
                {
                    modifiedMaterial.SetTexture("_MainTex", originalMaterial.GetTexture("_MainTex"));
                }

                // Copy other textures like normal map
                if (originalMaterial.HasProperty("_BumpMap") && modifiedMaterial.HasProperty("_BumpMap"))
                {
                    modifiedMaterial.SetTexture("_BumpMap", originalMaterial.GetTexture("_BumpMap"));
                }

                // Copy Smoothness 
                if (originalMaterial.HasProperty("_Glossiness") && modifiedMaterial.HasProperty("_Glossiness"))
                {
                    float smoothness = originalMaterial.GetFloat("_Glossiness");
                    modifiedMaterial.SetFloat("_Glossiness", smoothness);
                }


                modifiedMaterials[i] = modifiedMaterial;

            }

            rend.sharedMaterials = modifiedMaterials; // Apply the modified materials




        }

    }

}
