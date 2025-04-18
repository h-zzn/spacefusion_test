using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class HouseUsing
{
    public string houseName = "";
    public List<string> roomNames = new List<string>();

    public HouseUsing(string houseName)
    {
        this.houseName = houseName;
    }

    public void setRoomNames(string roomNamesString)
    {
        string[] splitted = roomNamesString.Split(char.Parse(","));
        if(splitted.Length > 0 )
        {
            for (int i = 0; i < splitted.Length; i++)
            {
                roomNames.Add(splitted[i]);
            }
        }
        else
        {
            roomNames.Add(roomNamesString);
            
        }
    }
}


public class VisualizeAnchor : MonoBehaviour
{

    public List<GameObject> houses;
    public List<GameObject> characters;

    public int houseOnLive;
    public GameObject originalCamera;

    public Material anchorWallMat;
    public Material anchorFloorMat;



    private List<GameObject> houseCentroids = new List<GameObject>();
    private List<GameObject> houseVisList = new List<GameObject>();
    private GameObject visCamera = null;
    private Vector3 initialOffset = Vector3.zero;

    void Start()
    {

        // get house information
        HouseUsing house1 = new HouseUsing("house_1");
        house1.setRoomNames("room_6");
        HouseUsing house5 = new HouseUsing("house_5");
        house5.setRoomNames("room_2,room_3");
        HouseUsing house12 = new HouseUsing("house_12");
        house12.setRoomNames("room_2");
        HouseUsing house15 = new HouseUsing("house_15");
        house15.setRoomNames("room_4");

        List<HouseUsing> houseUsing = new List<HouseUsing>();
        houseUsing.Add(house1);
        houseUsing.Add(house5);
        houseUsing.Add(house12);
        houseUsing.Add(house15);

        

        for (int housesCount = 0; housesCount < houses.Count; housesCount++)
        {

            // ------------------------------ add script to each ------------------------------ //
            List<GameObject> housesCopy = houses.ToList();
            string anchorName = "vis_" + houses[housesCount].name;
            GameObject houseAnchor = new GameObject(anchorName);
            houseAnchor.transform.parent = this.transform;


            var script = houseAnchor.AddComponent<Anchor>();
            script.otherHouses = new List<GameObject>();


            for (int housesCopyCount = 0; housesCopyCount < housesCopy.Count; housesCopyCount++)
            {
                if (housesCopy[housesCopyCount].name == houses[housesCount].name)
                {
                    script.whichHouse = houses[housesCount];
                }
                else
                {
                    script.otherHouses.Add(housesCopy[housesCopyCount]);
                }
            }

            

            // ------------------------------ copy rooms in use --------------------------------- //
            GameObject housesVis = Instantiate(houses[housesCount]);
            housesVis.name = houses[housesCount].name + "_vis";
            housesVis.transform.position = houses[housesCount].transform.position + new Vector3(0, 0, 30);

            houseVisList.Add(housesVis);


            List<string> roomCompare = houseUsing[housesCount].roomNames;
            List<string> roomCompareNum = new List<string>();
            for (int roomCompareCount = 0;  roomCompareCount < roomCompare.Count; roomCompareCount++)
            {
                //Debug.Log(roomCompare[roomCompareCount].Split(char.Parse("_"))[1]);
                roomCompareNum.Add(roomCompare[roomCompareCount].Split(char.Parse("_"))[1]);
            }

            foreach (Transform child in housesVis.transform)
            {
                if(child.name == "Objects")
                {
                    
                    for (int objectCount = 0; objectCount < child.childCount; objectCount++)
                    {

                        string checker = child.GetChild(objectCount).name.Split(char.Parse("_"))[1];
                        if (!roomCompareNum.Contains(checker))
                        {
                            Destroy(child.GetChild(objectCount).gameObject);
                        }
                    }

                }else if (child.name == "Structure")
                {
                    foreach (Transform structure in child.transform)
                    {
                        if(structure.name == "Floor")
                        {
                            for (int i = 0; i < structure.childCount; i++)
                            {
                                if (!roomCompare.Contains(structure.GetChild(i).name))
                                {
                                    Destroy(structure.GetChild(i).gameObject);
                                }
                            }
                        }
                        else
                        {
                            Destroy(structure.gameObject);
                        }
                    }
                }
            }


            // ------------------------------ visualize anchor --------------------------------- //
            GameObject allPolygonToDraw = new GameObject("allPolygonToDraw");
            //allPolygonToDraw.transform.parent = script.whichHouse.transform;
            allPolygonToDraw.transform.parent = housesVis.transform;

            string filename = Application.dataPath + "/Tayoptoutput/mergedPolygon_" + housesCount.ToString() + ".json";
            script.alignedBaseline.DrawLinePolygon(allPolygonToDraw, filename, houseVisList[housesCount].transform.position);

            // augment object
            string transfilename = Application.dataPath + "/Tayoptoutput/houseTrans.json";
            Dictionary<string, List<float>> transDict = script.alignedBaseline.getHouseTrans(transfilename);
            List<float> currentHouseTrans = transDict[script.whichHouse.name];

            string byHouse = Application.dataPath + "/Tayoptoutput/objectBy_" + houses[housesCount].name + ".json";
            Dictionary<string, List<float>> objByDict = script.alignedBaseline.getHouseTrans(byHouse);


            foreach (KeyValuePair<string, List<float>> pair in objByDict)
            {
                var ind1 = pair.Key.IndexOf('_');
                var ind2 = pair.Key.IndexOf('_', ind1 + 1);
                string houseName = pair.Key.Substring(0, ind2);
                string objectName = pair.Key.Substring(ind2 + 1);


                if (houseName != script.whichHouse.name)
                {
                    GameObject currentHouseToSearch = null;
                    for (int otherHousesCount = 0; otherHousesCount < script.otherHouses.Count; otherHousesCount++)
                    {
                        if (script.otherHouses[otherHousesCount].name == houseName)
                        {
                            currentHouseToSearch = script.otherHouses[otherHousesCount];
                        }
                    }


                    GameObject copyingObject = currentHouseToSearch.gameObject.transform.Find("Objects/" + objectName.ToString()).gameObject;

                    // 얘의 origin에서부터의 local transformation을 알아
                    GameObject copyedObject = Instantiate(copyingObject);


                    // make sample pivot and rotate around
                    copyedObject.transform.RotateAround(new Vector3(currentHouseTrans[0], 0, currentHouseTrans[1]), Vector3.up, currentHouseTrans[2]);
                    copyedObject.transform.position = new Vector3(pair.Value[0], 0, pair.Value[1]);
                    
                    // 얘가 이동을 담당하고 있음
                    copyedObject.transform.position = copyedObject.transform.position + houseVisList[housesCount].transform.position;

                    copyedObject.transform.parent = houseAnchor.transform;
                }

            }



            // ------------------------------ start walling --------------------------------- //
            Transform biggest = houses[1].transform.Find("Structure/Walls");
            GameObject centroid = new GameObject(houses[housesCount].name + "_centroid");
            centroid.transform.position = new Vector3(currentHouseTrans[3], 0, currentHouseTrans[4]); // origin을 기준으로 할때 centroid
            //centroid.transform.position = centroid.transform.position + houses[housesCount].transform.position;
            houseCentroids.Add(centroid);



            List<string> wallCompare = houseUsing[1].roomNames;
            List<string> wallCompareNum = new List<string>();
            for (int wallCompareCount = 0; wallCompareCount < wallCompare.Count; wallCompareCount++)
            {
                wallCompareNum.Add(wallCompare[wallCompareCount].Split(char.Parse("_"))[1]);
            }
            
            List<float> bigHouseTrans = transDict[houseUsing[1].houseName];
            GameObject bigHouseGameobject = new GameObject(houseUsing[1].houseName + "_Trans");
            bigHouseGameobject.transform.position = new Vector3(bigHouseTrans[3], 0, bigHouseTrans[4]);


            GameObject copyedWall = new GameObject("Walls");
            copyedWall.transform.position = copyedWall.transform.position + houseVisList[housesCount].transform.position;
            copyedWall.transform.parent = housesVis.transform;


            foreach (Transform wall in biggest)
            {
                string checker = wall.name.Split(char.Parse("_"))[1];
                if (wallCompareNum.Contains(checker))
                {
                    GameObject eachWall = Instantiate(wall.gameObject);
                    Vector3 relativePosition = bigHouseGameobject.transform.InverseTransformPoint(eachWall.transform.position);
                    eachWall.transform.position = centroid.transform.TransformPoint(relativePosition);
                    eachWall.transform.position = eachWall.transform.position + houseVisList[housesCount].transform.position;


                    eachWall.transform.parent = copyedWall.transform;

                    // wall material
                    eachWall.GetComponent<MeshRenderer>().material = anchorWallMat;
                    

                }
            }

            //centroid.transform.position = centroid.transform.position + houses[housesCount].transform.position;



            // ------------------------------ temp floor --------------------------------- //

            // 지금 상대하고 있는 방의 원점으로 옮겨야겠다 
            GameObject copyedFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            copyedFloor.transform.localScale = new Vector3(1.1f , 1.1f, 1.1f);
            copyedFloor.transform.position = houseVisList[housesCount].transform.position + new Vector3(0, -0.1f, 0);
            copyedFloor.transform.position = copyedFloor.transform.position + centroid.transform.position;
            copyedFloor.GetComponent<MeshRenderer>().material = anchorFloorMat;



            // ------------------------------ temp character --------------------------------- //

            GameObject Characters = new GameObject("Characters");
            Characters.transform.position = houseVisList[housesCount].transform.position;
            Characters.transform.parent = housesVis.transform;



            for (int i = 0; i < characters.Count; i++)
            {
                
                GameObject copyedCharacter = GameObject.Instantiate(characters[i].gameObject);
                copyedCharacter.transform.position = houseVisList[housesCount].transform.position + new Vector3(0, -0.1f, 0);
                copyedCharacter.transform.position = copyedCharacter.transform.position + centroid.transform.position;
                copyedCharacter.transform.parent = Characters.transform;

                if(i == 0)
                {
                    copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(1.5f, 0, 0);
                }
                else if(i == 1)
                {
                    copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(-2f, 0, -0.5f);
                    copyedCharacter.transform.eulerAngles = new Vector3(0, 80, 0);
                }
                else if(i==2)
                {
                    copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(-2.3f, 0, 3);
                    copyedCharacter.transform.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    copyedCharacter.transform.position = copyedCharacter.transform.position + new Vector3(2.8f, 0, 2.5f);
                    copyedCharacter.transform.eulerAngles = new Vector3(0, 200, 0);
                }


            }




        }


        
        // camera
        originalCamera.transform.position = houses[houseOnLive].transform.position;
        originalCamera.transform.position = originalCamera.transform.position + houseCentroids[houseOnLive].transform.position + new Vector3(0, 1.2f, 0);


        GameObject copyedCamera = new GameObject("copyedCamera");
        Camera cameraComponent = copyedCamera.AddComponent<Camera>();
        //cameraComponent.depth = -1;

        cameraComponent.targetDisplay = 1;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(41/255, 40/255, 40/255);
        copyedCamera.transform.position = houseVisList[houseOnLive].transform.position;
        copyedCamera.transform.position = copyedCamera.transform.position + houseCentroids[houseOnLive].transform.position + new Vector3(0, 1.2f, 0);
        


        visCamera = copyedCamera;
        initialOffset = originalCamera.transform.position - visCamera.transform.position; // 얘네가 가지고 있는 원래 차이
    }

    bool created = false;

    private void Update()
    {

        // original camera가 이동하는 만큼
        Vector3 desiredPosition = originalCamera.transform.position - initialOffset;
        visCamera.transform.position = desiredPosition;
        visCamera.transform.rotation = originalCamera.transform.rotation;
    }
}
