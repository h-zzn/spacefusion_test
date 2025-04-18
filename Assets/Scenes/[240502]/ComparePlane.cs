using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ComparePlane : MonoBehaviour
{
    public List<GameObject> characters;
    private Vector4[] pointsvec4;
    public int whichRegion;

    // color related
    private Color[] colorarray;
    private Vector4[] colorvec4;

    public Transform targetPlane;

    private GameObject arrangeHousesObj;
    private GameObject startVoronoiObj;
    private bool initialized = false;



    void Start()
    {
        arrangeHousesObj = GameObject.Find("ArrangeHouse");
        startVoronoiObj = GameObject.Find("StartVoronoi");


        // ------------- color --------------- //
        colorarray = new Color[10];
        colorarray[0] = new Color(1f, 0.82f, 0.965f, 1);
        colorarray[1] = new Color(1, 0.647f, 0.929f, 1);
        colorarray[2] = new Color(0.812f, 0.663f, 0.941f, 1);
        colorarray[3] = new Color(0.694f, 0.345f, 1f, 1);

        colorvec4 = new Vector4[10];
        pointsvec4 = new Vector4[3]; // house 갯수 혹은 User 명수로 initialize 해야함


        

    }


    void Update()
    {

        // pointsvec4에 userpoint를 먹여보자
        
        // character initial pos
        if(initialized == false)
        {
            if (startVoronoiObj.GetComponent<StartVoronoi>().initialCharactersIn == true && arrangeHousesObj.GetComponent<ArrangeHouses>().characterInitialized == true)
            {
                Transform level1 = arrangeHousesObj.transform.GetChild(0);
                foreach (Transform users in level1)
                {
                    characters.Add(users.gameObject);
                }

                initialized = true;
            }
        }
        



        for (int i = 0; i < characters.Count; i++)
        {
            pointsvec4[i] = new Vector4(characters[i].transform.position.x, 0, characters[i].transform.position.z, 0);
            colorvec4[i] = new Vector4(colorarray[i].r, colorarray[i].g, colorarray[i].b, 1);
        }


        Renderer renderer = this.GetComponent<Renderer>();
        Material mat = renderer.sharedMaterial;

        mat.SetVectorArray("_Users", pointsvec4);
        mat.SetInt("_Length", pointsvec4.Length);
        mat.SetVectorArray("_Colors", colorvec4);

        mat.SetInt("_WhichRegion", whichRegion);




    }



}
