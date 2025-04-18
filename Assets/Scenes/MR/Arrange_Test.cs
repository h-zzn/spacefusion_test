using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

public class Arrange_Test : MonoBehaviour
{
    public List<GameObject> houses;
    public List<string> roomNums = new List<string>();
    public int chooseHouseNum;

    // users to populate
    public List<GameObject> users;

    // check replace mat done
    public GameObject swapMatCheck;
    public List<Vector3> outPoints = new List<Vector3>();
    public SharedSpacePoints sharedspacePoints = new SharedSpacePoints();

    // get voronoi region from Regions
    public GameObject regionsObj;

    // check anchor polygon points fed
    public bool characterInitialized = false;


    public bool objectsAugmented = false;
    // propagate copyed object
    public List<GameObject> copyedObjectList = new List<GameObject>();


    void Start()
    {

        List<HouseUsing> houseUsing = new List<HouseUsing>();
        for (int i = 0; i < houses.Count; i++)
        {
            HouseUsing house = new HouseUsing(houses[i].name.ToString());
            house.setRoomNames(roomNums[i].ToString());
            houseUsing.Add(house);
        }

        // python에서 받아오는 모든 값
        string filename = Application.dataPath + "/Tayoptoutput/dataForUnity.json";
        string jsonToRead = File.ReadAllText(filename);

        DataFromPython dataFromPython = JsonConvert.DeserializeObject<DataFromPython>(jsonToRead);

        // --------------------------------------- shared space --------------------------------------- //
        DataFromPython.SharedSpace sharedSpace = dataFromPython.savesharedspace();
        List<List<float>> coordinates = sharedSpace.coordinates;
        Vector3[] points = new Vector3[coordinates.Count];
        for (int currentPoint = 0; currentPoint < coordinates.Count; currentPoint++)
        {
            
            Vector3 tempPoint = new Vector3(Mathf.Round(coordinates[currentPoint][0] * 1000.0f) * 0.001f, 0.02f, Mathf.Round(coordinates[currentPoint][1] * 1000.0f) * 0.001f);
            points[currentPoint] = tempPoint;
        }


        // chooseHouseNum에 해당하는 회차만 돌아야함
        for (int housesCount = 0; housesCount < houses.Count; housesCount++)
        {

            
            if (housesCount == chooseHouseNum)
            {
                var script = houses[housesCount].AddComponent<Anchor>();
                script.otherHouses = new List<GameObject>();


                foreach (GameObject go in houses)
                {
                    if (go.name != houses[housesCount].name)
                    {
                        script.otherHouses.Add(go);
                    }
                    else
                    {
                        script.whichHouse = go;
                    }
                }

                
                // ------------------------------ shared space attachTo --------------------------------- //
                GameObject allPolygonToDraw = new GameObject("allPolygonToDraw");




                // --------------------------------------- arrange house ---------------------------------- //
                // 뭔가 python이랑 unity 사이의 거리가 처음부터 조금 다르게 들어오는 느낌도 들음
                if (housesCount == 0)
                {
                    Vector3 originalCentroid = new Vector3(dataFromPython.house_0[0], 0, dataFromPython.house_0[1]);

                    // 본인 centroid 만큼 역으로 이동시켜 원점으로 가져다놓고
                    script.otherHouses[0].transform.position += new Vector3(-dataFromPython.house_1[0], 0, -dataFromPython.house_1[1]);
                    script.otherHouses[1].transform.position += new Vector3(-dataFromPython.house_2[0], 0, -dataFromPython.house_2[1]);

                    // 거기서 이동한 만큼 이동시키고 
                    script.otherHouses[0].transform.position += new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);
                    script.otherHouses[1].transform.position += new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);

                    // 해당 centroid 기준으로 돌리고
                    Vector3 rotateOrigin_house_1 = new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);
                    script.otherHouses[0].transform.RotateAround(rotateOrigin_house_1, Vector3.up, -dataFromPython.house_1[4]);
                    Vector3 rotateOrigin_house_2 = new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);
                    script.otherHouses[1].transform.RotateAround(rotateOrigin_house_2, Vector3.up, -dataFromPython.house_2[4]);

                    // 다시 지금 house 위치로 데리고오고
                    script.otherHouses[0].transform.position += new Vector3(dataFromPython.house_0[0], 0, dataFromPython.house_0[1]);
                    script.otherHouses[1].transform.position += new Vector3(dataFromPython.house_0[0], 0, dataFromPython.house_0[1]);

                    // only translation, shared space
                    Vector3 mymove = new Vector3(dataFromPython.house_0[0], 0, dataFromPython.house_0[1]);
                    for (int currentPoint = 0; currentPoint < coordinates.Count; currentPoint++)
                    {
                        points[currentPoint] = points[currentPoint] + mymove;
                    }

                    sharedspacePoints = script.alignedBaseline.DrawSingleSharedSpace(allPolygonToDraw, points);
                    // 이동한 point 자체를 sharedspacepoints로 바꿔놔야 할거같은데? 


                }
                else if (housesCount == 1)
                {

                    // -------------------------- replicate python arrange -------------------------- //

                    // bring to origin then move wrt origin
                    // house_0
                    script.otherHouses[0].transform.position += new Vector3(-dataFromPython.house_0[0], 0, -dataFromPython.house_0[1]);

                    // house_1
                    script.whichHouse.transform.position += new Vector3(-dataFromPython.house_1[0], 0, -dataFromPython.house_1[1]);
                    script.whichHouse.transform.position += new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);
                    Vector3 house_1_originWRTCenter = new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);
                    script.whichHouse.transform.RotateAround(house_1_originWRTCenter, Vector3.up, -dataFromPython.house_1[4]);
                    

                    // house_2
                    script.otherHouses[1].transform.position += new Vector3(-dataFromPython.house_2[0], 0, -dataFromPython.house_2[1]);
                    script.otherHouses[1].transform.position += new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);
                    Vector3 house_2_originWRTCenter = new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);
                    script.otherHouses[1].transform.RotateAround(house_2_originWRTCenter, Vector3.up, -dataFromPython.house_2[4]);



                    // -------------------------- 다시 제자리로 가져다두기 -------------------------- //

                    // 일단 전부다 내가 이동한 만큼 이동시켜서, 원점으로 가지고 오자
                    Vector3 mymove = new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);

                    script.otherHouses[0].transform.position -= mymove;
                    script.whichHouse.transform.position -= mymove;
                    script.otherHouses[1].transform.position -= mymove;

                    // shared space도 my move만큼 이동한다
                    for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
                    {
                        points[currentPoint] = points[currentPoint] + -mymove;
                    }


                    // checking
                    /*
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = Vector3.zero;
                    sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    Renderer sphereRenderer = sphere.GetComponent<Renderer>();
                    sphereRenderer.material.color = Color.red;
                    */


                    // 그 다음 0,0 기준으로 돌린다
                    script.otherHouses[0].transform.RotateAround(Vector3.zero, Vector3.up, dataFromPython.house_1[4]);
                    script.whichHouse.transform.RotateAround(Vector3.zero, Vector3.up, dataFromPython.house_1[4]);
                    script.otherHouses[1].transform.RotateAround(Vector3.zero, Vector3.up, dataFromPython.house_1[4]);

                    // shared space
                    for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
                    {
                        points[currentPoint] = Quaternion.AngleAxis(dataFromPython.house_1[4], Vector3.up) * points[currentPoint];
                    }


                    // 이제 다시 내자리로 가져다 놓는다
                    Vector3 mymove_original = new Vector3(dataFromPython.house_1[0], 0, dataFromPython.house_1[1]);
                    script.otherHouses[0].transform.position += mymove_original;
                    script.whichHouse.transform.position += mymove_original;
                    script.otherHouses[1].transform.position += mymove_original;

                    for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
                    {
                        points[currentPoint] += mymove_original;
                    }

                    // draw shared space
                    sharedspacePoints = script.alignedBaseline.DrawSingleSharedSpace(allPolygonToDraw, points);

                }
                else
                {

                    // -------------------------- replicate python arrange -------------------------- //

                    // bring to origin then move wrt origin
                    // house_0
                    script.otherHouses[0].transform.position += new Vector3(-dataFromPython.house_0[0], 0, -dataFromPython.house_0[1]);

                    // house_1
                    script.otherHouses[1].transform.position += new Vector3(-dataFromPython.house_1[0], 0, -dataFromPython.house_1[1]);
                    script.otherHouses[1].transform.position += new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);
                    Vector3 house_1_originWRTCenter = new Vector3(dataFromPython.house_1[2], 0, dataFromPython.house_1[3]);
                    script.otherHouses[1].transform.RotateAround(house_1_originWRTCenter, Vector3.up, -dataFromPython.house_1[4]);


                    // house_2 
                    script.whichHouse.transform.position += new Vector3(-dataFromPython.house_2[0], 0, -dataFromPython.house_2[1]);
                    script.whichHouse.transform.position += new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);
                    Vector3 house_2_originWRTCenter = new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);
                    script.whichHouse.transform.RotateAround(house_1_originWRTCenter, Vector3.up, -dataFromPython.house_2[4]);


                    // 일단 전부다 내가 이동한 만큼 이동시켜서, 원점으로 가지고 오자
                    Vector3 mymove = new Vector3(dataFromPython.house_2[2], 0, dataFromPython.house_2[3]);

                    script.otherHouses[0].transform.position -= mymove;
                    script.otherHouses[1].transform.position -= mymove;
                    script.whichHouse.transform.position -= mymove;

                    // shared space도 my move만큼 이동한다
                    for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
                    {
                        points[currentPoint] = points[currentPoint] + -mymove;
                    }


                    // rotate wrt 0,0
                    script.otherHouses[0].transform.RotateAround(Vector3.zero, Vector3.up, dataFromPython.house_2[4]);
                    script.otherHouses[1].transform.RotateAround(Vector3.zero, Vector3.up, dataFromPython.house_2[4]);
                    script.whichHouse.transform.RotateAround(Vector3.zero, Vector3.up, dataFromPython.house_2[4]);

                    for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
                    {
                        points[currentPoint] = Quaternion.AngleAxis(dataFromPython.house_2[4], Vector3.up) * points[currentPoint];
                    }

                    // place back to original
                    Vector3 mymove_original = new Vector3(dataFromPython.house_2[0], 0, dataFromPython.house_2[1]);
                    script.otherHouses[0].transform.position += mymove_original;
                    script.otherHouses[1].transform.position += mymove_original;
                    script.whichHouse.transform.position += mymove_original;

                    for (int currentPoint = 0; currentPoint < points.Length; currentPoint++)
                    {
                        points[currentPoint] = points[currentPoint] + mymove_original;
                    }

                    // draw shared space
                    sharedspacePoints = script.alignedBaseline.DrawSingleSharedSpace(allPolygonToDraw, points);

                }


                
                // --------------------------------------- augment object ---------------------------------- //
                string augfilename = File.ReadAllText(Application.dataPath + "/Tayoptoutput/augObjects.json");
                Dictionary<string, List<string>> augObjects = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(augfilename);


                //List<GameObject> copyedObjectList = new List<GameObject>();
                foreach (var item in augObjects)
                {
                    Debug.Log($"{item.Key}: {string.Join(", ", item.Value)}");

                    string[] parts = item.Key.Split('_');
                    if (int.Parse(parts[1]) != housesCount)
                    {
                        // from which house
                        GameObject objectInThisHouse = GameObject.Find(item.Key);

                        // iter object
                        for (int i = 0; i < item.Value.Count; i++)
                        {
                            string objectName = item.Value[i].ToString();
                            GameObject copyingObject = objectInThisHouse.gameObject.transform.Find("Objects/" + objectName.ToString()).gameObject;
                            GameObject copyedObject = Instantiate(copyingObject);

                            copyedObject.transform.position = copyingObject.transform.position;
                            copyedObject.transform.rotation = objectInThisHouse.transform.rotation;

                            copyedObjectList.Add(copyedObject);

                        }
                        

                    }
                }
                
                // 이 copyed object에 대해서 feedpoint를 다시 해줘야 할거같음



                GameObject augmentedObjs = new GameObject("augmentedObjs");
                //augmentedObjs.transform.parent = houses[housesCount].transform;
                
                
                for (int i = 0; i < copyedObjectList.Count; i++)
                {
                    copyedObjectList[i].transform.parent = augmentedObjs.transform;
                }

                objectsAugmented = true;


                // ------------------------------ character --------------------------------- //

                GameObject Characters = new GameObject("Characters");
                Characters.transform.position = houses[housesCount].transform.position;
                Characters.transform.parent = this.transform;


                
                for (int i = 0; i < users.Count; i++)
                {

                    // 일단 모든 애들을 한번 크게 옮겨줘야함
                    GameObject copyedCharacter = GameObject.Instantiate(users[i].gameObject);
                    copyedCharacter.transform.position = houses[housesCount].transform.position + new Vector3(0, 0f, 0);
                    // here check!!
                    copyedCharacter.transform.position += new Vector3(houses[housesCount].transform.position.x, 0, houses[housesCount].transform.position.z);
                    copyedCharacter.transform.parent = Characters.transform;

                    if (i == 0)
                    {
                        copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(1.5f, 0, 0);
                    }
                    else if (i == 1)
                    {
                        copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(-1.4f, 0, 0.5f);
                        copyedCharacter.transform.eulerAngles = new Vector3(0, 130, 0);
                    }
                    else if (i == 2)
                    {
                        copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(0.8f, 0, 1.5f);
                        copyedCharacter.transform.eulerAngles = new Vector3(0, 180, 0);
                    }
                    else
                    {
                        copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(-2f, 0, -0.5f);
                        copyedCharacter.transform.eulerAngles = new Vector3(0, 90, 0);
                    }


                }
                

                characterInitialized = true;



                // -------------------------- only within here!! -------------------------- //
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
