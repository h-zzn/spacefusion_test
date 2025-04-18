using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartVoronoi : MonoBehaviour
{
    public List<GameObject> houses = new List<GameObject>();
    private List<int> whichRegions = new List<int>();

    // track user pos
    private GameObject arrangeHousesObj;
    public bool initialCharactersIn = false;
    public List<GameObject> characters = new List<GameObject>();
    private Vector4[] userPosVec4;

    // voronoi shader
    public Material vorShader;
    public Material vorShaderWall;

    // which house as main user
    public int chooseHouseNum;


    void Start()
    {

        arrangeHousesObj = GameObject.Find("ArrangeHouse");
        userPosVec4 = new Vector4[houses.Count];

        // change mat for all house
        for (int housesCount = 0; housesCount < houses.Count; housesCount++)
        {
            ChangeMaterial(vorShader, vorShaderWall, houses[housesCount]);

        }


    }


    void Update()
    {
        // character initial pos
        if (initialCharactersIn == false && arrangeHousesObj.GetComponent<ArrangeHouses>().characterInitialized == true)
        {
            Transform level1 = arrangeHousesObj.transform.GetChild(0);
            foreach (Transform users in level1)
            {
                //Debug.Log(users.gameObject.name);
                characters.Add(users.gameObject);
            }

            initialCharactersIn = true;

        }
        
        // sharedspacePoints
        SharedSpacePoints sharedspacePoints = arrangeHousesObj.GetComponent<ArrangeHouses>().sharedspacePoints;

        // exterior
        List<Vector3> exteriorPoints = sharedspacePoints.exteriorPoints;
        Debug.Log(exteriorPoints.Count);

        Vector4[] exteriorVec4 = new Vector4[exteriorPoints.Count];
        for (int i = 0; i < exteriorPoints.Count; i++)
        {
            exteriorVec4[i] = new Vector4(exteriorPoints[i].x, exteriorPoints[i].y, exteriorPoints[i].z, 0);
        }
        

        // interior
        List<List<Vector3>> interiorPoints = sharedspacePoints.interiorPoints;
        Vector4[][] interiorVec4 = new Vector4[interiorPoints.Count][];
        for (int i = 0; i < interiorPoints.Count; i++)
        {
            interiorVec4[i] = new Vector4[interiorPoints[i].Count];
            for (int j = 0; j < interiorPoints[i].Count; j++)
            {
                Vector3 point = interiorPoints[i][j];
                interiorVec4[i][j] = new Vector4(point.x, point.y, point.z, 0);  
            }
        }


        // update user pos 
        if (arrangeHousesObj.GetComponent<ArrangeHouses>().characterInitialized == true)
        {
            
            for (int i = 0; i < characters.Count; i++)
            {
                userPosVec4[i] = new Vector4(characters[i].transform.position.x, 0, characters[i].transform.position.z, 0);

            }
        }

        for (int housesCount = 0; housesCount < houses.Count; housesCount++)
        {
            int mainUser;
            
            if (chooseHouseNum == housesCount)
            {
                mainUser = 1;
                feedOutpoints(houses[housesCount], exteriorVec4, interiorVec4, userPosVec4, housesCount, mainUser);

            }
            else
            {
                mainUser = 0;
                feedOutpoints(houses[housesCount], exteriorVec4, interiorVec4, userPosVec4, housesCount, mainUser);
            }
        }

    }


    void ChangeMaterial(Material newMat, Material newMatWall, GameObject whichPrefab)
    {

        Renderer[] renderers = whichPrefab.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock(); // Create a new material property block

        foreach (Renderer rend in renderers)
        {
            if (rend.name.StartsWith("wall_exterior_"))
            //if (rend.name.StartsWith("taehei"))
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
                    Material modifiedMaterial;
                    if (rend.name.StartsWith("wall_"))
                    {
                        modifiedMaterial = new Material(newMatWall); 
                    }
                    else
                    {
                        modifiedMaterial = new Material(newMat); 
                    }

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


    // feed voronoi & anchor
    void feedOutpoints(GameObject whichPrefab, Vector4[] exteriorVec4, Vector4[][] interiorVec4, Vector4[] userPosVec4, int whichRegion, int mainUser)
    {

        Renderer[] renderers = whichPrefab.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {

            if (rend.name.StartsWith("Ceiling_"))
            {
                for (int i = 0; i < rend.sharedMaterials.Length; i++)
                {
                    var material = rend.sharedMaterials[i];

                    
                    material.SetVectorArray("_Users", userPosVec4);
                    material.SetInt("_Length", userPosVec4.Length);
                    material.SetInt("_WhichRegion", whichRegion);

                    material.SetInt("_mainUser", mainUser);

                }
            }
            else
            {
                for (int i = 0; i < rend.sharedMaterials.Length; i++)
                {
                    var material = rend.sharedMaterials[i];
                    //Debug.Log(exteriorVec4.Length);
                    material.SetVectorArray("_Points", exteriorVec4);
                    material.SetInt("_PointCount", exteriorVec4.Length);

                    
                    for (int interiorLength = 0; interiorLength < interiorVec4.Length; interiorLength++)
                    {
                        string name = "_interiorPoints_" + interiorLength.ToString();
                        material.SetVectorArray(name, interiorVec4[interiorLength]);

                        string count = "_interiorPointsCount_" + interiorLength.ToString();
                        material.SetInt(count, interiorVec4[interiorLength].Length);
                    }
                    

                    material.SetVectorArray("_Users", userPosVec4);
                    material.SetInt("_Length", userPosVec4.Length);
                    material.SetInt("_WhichRegion", whichRegion);

                    material.SetInt("_mainUser", mainUser);

                }
            }


        }
    }




}
