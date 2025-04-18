using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

public class Regions : MonoBehaviour
{

    public List<GameObject> houses = new List<GameObject>();


    // track user pos
    private GameObject arrangeHousesObj;
    public bool initialCharactersIn = false;
    public List<GameObject> characters = new List<GameObject>();
    public Vector4[] userPosVec4;

    // zone shader
    public Material localZoneShader;
    public Material remoteZoneShader;

    private List<List<Vector3>> selectedZones = new List<List<Vector3>>();


    // which house as main user
    public int chooseHouseNum;

    public GameObject myHouse;
    public GameObject otherHouse1;
    public GameObject otherHouse2;

    // sending to python
    public Sender sender;
    public bool receiveFromPython = false;
    private string userPosString = "";



    void Start()
    {

        userPosVec4 = new Vector4[houses.Count];

        arrangeHousesObj = GameObject.Find("ArrangeMRHouse");


        // circle around user pos
        ChangeMaterial(localZoneShader, myHouse);
        ChangeMaterial(remoteZoneShader, otherHouse1);
        ChangeMaterial(remoteZoneShader, otherHouse2);




    }



    // Update is called once per frame
    void Update()
    {

        // character initial pos
        if (initialCharactersIn == false && arrangeHousesObj.GetComponent<Arrange_Walkin>().characterInitialized == true)
        {
            Transform level1 = arrangeHousesObj.transform.GetChild(0);
            foreach (Transform users in level1)
            {
                //Debug.Log(users.gameObject.name);
                characters.Add(users.gameObject);

            }

            initialCharactersIn = true;

        }


        // python communication 
        if (Input.GetKeyDown(KeyCode.X))
        {
            userPosString = "";
            for (int i = 0; i < characters.Count; i++)
            {
                Vector3 posAtOrigin = getTransAtOrigin(houses[i], characters[i]);
                //Debug.Log(posAtOrigin);

                if (i == 0)
                {
                    userPosString = userPosString + "X((" + posAtOrigin.x.ToString() + ", " + posAtOrigin.z.ToString() + "), ";
                }
                else if (i == characters.Count - 1)
                {
                    userPosString = userPosString + "(" + posAtOrigin.x.ToString() + ", " + posAtOrigin.z.ToString() + "))";
                }
                else
                {
                    userPosString = userPosString + "(" + posAtOrigin.x.ToString() + ", " + posAtOrigin.z.ToString() + "), ";

                }


            }

            Debug.Log(userPosString); // ((1.5, 0), (1.5, 0), (2.47, 3.26), (0.8, 1.5))

            receiveFromPython = false;
            sender.SendToPython(userPosString);

        }




        // visualize traverse zones

        selectedZones = arrangeHousesObj.GetComponent<Arrange_Walkin>().selectedZones;

        Vector4[] zone1Vec4;
        Vector4[] zone2Vec4;

        if (chooseHouseNum == 0)
        {
            zone1Vec4 = new Vector4[selectedZones[1].Count];
            for (int i = 0; i < selectedZones[1].Count; i++)
            {
                zone1Vec4[i] = new Vector4(selectedZones[1][i].x, selectedZones[1][i].y, selectedZones[1][i].z, 0);
            }

            zone2Vec4 = new Vector4[selectedZones[2].Count];
            for (int i = 0; i < selectedZones[2].Count; i++)
            {
                zone2Vec4[i] = new Vector4(selectedZones[2][i].x, selectedZones[2][i].y, selectedZones[2][i].z, 0);
            }

            feedZone(myHouse, userPosVec4, 0, 0);
            feedZone(otherHouse1, userPosVec4, 0, 1);
            feedZone(otherHouse2, userPosVec4, 0, 2);


        }
        else if (chooseHouseNum == 1)
        {
            zone1Vec4 = new Vector4[selectedZones[0].Count];
            for (int i = 0; i < selectedZones[0].Count; i++)
            {
                zone1Vec4[i] = new Vector4(selectedZones[0][i].x, selectedZones[0][i].y, selectedZones[0][i].z, 0);
            }

            zone2Vec4 = new Vector4[selectedZones[2].Count];
            for (int i = 0; i < selectedZones[2].Count; i++)
            {
                zone2Vec4[i] = new Vector4(selectedZones[2][i].x, selectedZones[2][i].y, selectedZones[2][i].z, 0);
            }

            //Debug.Log(userPosVec4[1]);


            feedZone(myHouse, userPosVec4, 1, 1);

            feedZone(otherHouse1, userPosVec4, 1, 0);
            feedZone(otherHouse2, userPosVec4, 1, 2);


        }
        else if (chooseHouseNum == 2)
        {
            
            zone1Vec4 = new Vector4[selectedZones[0].Count];
            for (int i = 0; i < selectedZones[0].Count; i++)
            {
                zone1Vec4[i] = new Vector4(selectedZones[0][i].x, selectedZones[0][i].y, selectedZones[0][i].z, 0);
            }


            zone2Vec4 = new Vector4[selectedZones[1].Count];
            for (int i = 0; i < selectedZones[1].Count; i++)
            {
                zone2Vec4[i] = new Vector4(selectedZones[1][i].x, selectedZones[1][i].y, selectedZones[1][i].z, 0);
            }

            feedZone(myHouse, userPosVec4, 2, 2);
            feedZone(otherHouse1, userPosVec4, 2, 0);
            feedZone(otherHouse2, userPosVec4, 2, 1);
        }



    }



    void ChangeMaterial(Material newMat, GameObject whichPrefab)
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
                        // 애초에 내가 여기 wall 빼놓은 이유가 있을텐데 
                        //modifiedMaterial = new Material(newMatWall);
                        modifiedMaterial = new Material(newMat);
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


    void feedZone(GameObject whichPrefab, Vector4[] userPosVec4, int baseRegion, int whichRegion)
    {

        Renderer[] renderers = whichPrefab.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {

            if (rend.name.StartsWith("Ceiling_"))
            {
                for (int i = 0; i < rend.sharedMaterials.Length; i++)
                {
                    var material = rend.sharedMaterials[i];
                }
            }
            else
            {
                for (int i = 0; i < rend.sharedMaterials.Length; i++)
                {
                    var material = rend.sharedMaterials[i];

                    // user information
                    material.SetVectorArray("_Users", userPosVec4);
                    material.SetInt("_Length", userPosVec4.Length);
                    material.SetInt("_BaseRegion", baseRegion);
                    material.SetInt("_WhichRegion", whichRegion);
                }
            }


        }
    }

    Vector3 getTransAtOrigin(GameObject parent, GameObject child)
    {
        // Get A's current transformation matrix
        Matrix4x4 worldToLocalMatrix = parent.transform.worldToLocalMatrix;

        // Calculate B's position relative to A's original position
        Vector3 posAtOrigin = worldToLocalMatrix.MultiplyPoint3x4(child.transform.position);


        return posAtOrigin;
    }


}
