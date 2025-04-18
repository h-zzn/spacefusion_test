using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMat : MonoBehaviour
{
    public Material toWhatMat;
    public int userNum;


    void Start()
    {
        // start 할때 바로 바꿈 -> 얘는 말 그대로 shader 바꿔주는 기능만 하는거임
        ChangeMaterial(toWhatMat, this.gameObject);

    }


    void Update()
    {
        


    }

    void ChangeMaterial(Material newMat, GameObject whichPrefab)
    {

        Renderer[] renderers = whichPrefab.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock(); // Create a new material property block

        foreach (Renderer rend in renderers)
        {
            if (rend.name.StartsWith("wall_exterior_"))
            {
                Destroy(rend.gameObject);
            }

            else
            {
                // move slightly btw each other
                float randX = Random.Range(-0.01f, 0.01f);
                float randZ = Random.Range(-0.01f, 0.01f);

                rend.gameObject.transform.position = rend.gameObject.transform.position + new Vector3(randX, 0, randZ);

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




}
