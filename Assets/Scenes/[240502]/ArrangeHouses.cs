using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArrangeHouses : MonoBehaviour
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

    void Start()
    {
        
        List<HouseUsing> houseUsing = new List<HouseUsing>();
        for (int i = 0; i < houses.Count; i++)
        {
            HouseUsing house = new HouseUsing(houses[i].name.ToString());
            house.setRoomNames(roomNums[i].ToString());
            houseUsing.Add(house);
        }
        
        /*
        // get house information
        HouseUsing house23 = new HouseUsing("house_23");
        house23.setRoomNames("room_2");
        HouseUsing house31 = new HouseUsing("house_31");
        house31.setRoomNames("room_6");
        HouseUsing house48 = new HouseUsing("house_48");
        house48.setRoomNames("room_6");

        List<HouseUsing> houseUsing = new List<HouseUsing>();
        houseUsing.Add(house23);
        houseUsing.Add(house31);
        houseUsing.Add(house48);
        */


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


                // --------------------------------------- move house ---------------------------------- //
                // General houseTrans: [pos wrt centroid at origin, rot wrt centroid at origin, pos wrt house floorplan]
                string transfilename = Application.dataPath + "/Tayoptoutput/houseTrans.json";
                Dictionary<string, List<float>> transDict = script.alignedBaseline.getHouseTrans(transfilename);
                List<float> currentHouseTrans = transDict[script.whichHouse.name];

                string groundtransfilename = Application.dataPath + "/Tayoptoutput/groundTrans_" + houses[housesCount].name.ToString() + ".json";
                Dictionary<string, List<float>> groundtransDict = script.alignedBaseline.getGroundTrans(groundtransfilename);
                List<float> groundTrans = groundtransDict[script.whichHouse.name];


                if (housesCount == 0)
                {

                    for (int otherHousesCount = 0; otherHousesCount < script.otherHouses.Count; otherHousesCount++)
                    {
                        // bring to origin
                        List<float> otherHouseTrans = transDict[script.otherHouses[otherHousesCount].name];
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(-otherHouseTrans[3], 0, -otherHouseTrans[4]);

                        // move to centroid
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(otherHouseTrans[0], 0, otherHouseTrans[1]);

                        // rotate
                        Vector3 rotateOrigin = new Vector3(otherHouseTrans[0], 0, otherHouseTrans[1]);
                        script.otherHouses[otherHousesCount].transform.RotateAround(rotateOrigin, Vector3.up, -otherHouseTrans[2]);

                        // move to origin
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(-otherHouseTrans[0], 0, -otherHouseTrans[1]);

                        // put to the global pos of current room
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(groundTrans[0], 0, groundTrans[1]);


                    }
                }


                else
                {

                    List<float> myHouseTrans = transDict[script.whichHouse.name];

                    // 다른애들이 내가 돌아간 만큼 더 돌아가야 하는거
                    for (int otherHousesCount = 0; otherHousesCount < script.otherHouses.Count; otherHousesCount++)
                    {
                        // bring to origin
                        List<float> otherHouseTrans = transDict[script.otherHouses[otherHousesCount].name];
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(-otherHouseTrans[3], 0, -otherHouseTrans[4]);

                        // move to centroid
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(otherHouseTrans[0], 0, otherHouseTrans[1]);


                        // 여기서 내가 돌아간 방향 만큼 다른 애들도 돌려주고
                        Vector3 rotateOriginOfMyHouse = new Vector3(myHouseTrans[0], 0, myHouseTrans[1]);
                        script.otherHouses[otherHousesCount].transform.RotateAround(rotateOriginOfMyHouse, Vector3.up, myHouseTrans[2]);

                        // 걔들 원래 돌아간 만큼 돌리고 
                        Vector3 rotateOrigin = new Vector3(otherHouseTrans[0], 0, otherHouseTrans[1]);
                        script.otherHouses[otherHousesCount].transform.RotateAround(rotateOrigin, Vector3.up, -otherHouseTrans[2]);

                        // move to origin
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(-otherHouseTrans[0], 0, -otherHouseTrans[1]);

                        // put to the global pos of current room
                        script.otherHouses[otherHousesCount].transform.position += new Vector3(groundTrans[0], 0, groundTrans[1]);


                    }
                }


                // ------------------------------ Augment Object --------------------------------- //
                string byHouse = Application.dataPath + "/Tayoptoutput/objectBy_" + houses[housesCount].name + ".json";
                Dictionary<string, List<float>> objByDict = script.alignedBaseline.getHouseTrans(byHouse);
                GameObject augmentedObjs = new GameObject("augmentedObjs");
                augmentedObjs.transform.parent = houses[housesCount].transform;


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

                        // make sample pivot and rotate 
                        List<float> belongHouseTrans = transDict[houseName];

                        //copyedObject.transform.RotateAround(new Vector3(currentHouseTrans[0], 0, currentHouseTrans[1]), Vector3.up, currentHouseTrans[2]);
                        copyedObject.transform.RotateAround(new Vector3(currentHouseTrans[0], 0, currentHouseTrans[1]), Vector3.up, -belongHouseTrans[2]);
                        copyedObject.transform.position = new Vector3(pair.Value[0], 0, pair.Value[1]);


                        // trans position
                        copyedObject.transform.position = copyedObject.transform.position + houses[housesCount].transform.position;

                        copyedObject.transform.parent = augmentedObjs.transform;
                    }

                }


                // only to current user's room 
                // ------------------------ extract my current room ------------------------ //
                List<string> roomCompare = houseUsing[housesCount].roomNames;
                List<string> roomCompareNum = new List<string>();
                for (int roomCompareCount = 0; roomCompareCount < roomCompare.Count; roomCompareCount++)
                {
                    //Debug.Log(roomCompare[roomCompareCount].Split(char.Parse("_"))[1]);
                    roomCompareNum.Add(roomCompare[roomCompareCount].Split(char.Parse("_"))[1]);
                }

                // check which rooms are in
                /*
                for (int roomCompareCount = 0;roomCompareCount < roomCompareNum.Count; roomCompareCount++)
                {
                    Debug.Log(roomCompareNum[roomCompareCount]);
                }
                */

                foreach (Transform child in houses[housesCount].transform)
                {
                    if (child.name == "Objects")
                    {

                        for (int objectCount = 0; objectCount < child.childCount; objectCount++)
                        {
                            // 먼저 확인할것: 만약 augobject에 포함이 되어 있는 애면 지우면 안됨

                            List<string> notdistroyed = new List<string>();
                            foreach (KeyValuePair<string, List<float>> pair in objByDict)
                            {
                                var ind1 = pair.Key.IndexOf('_');
                                var ind2 = pair.Key.IndexOf('_', ind1 + 1);
                                string houseName = pair.Key.Substring(0, ind2);
                                string objectName = pair.Key.Substring(ind2 + 1);
                                
                                if(houseName == houses[housesCount].name && child.GetChild(objectCount).name == objectName)
                                {
                                    notdistroyed.Add(objectName);
                                }

                            }


                            string checker = child.GetChild(objectCount).name.Split(char.Parse("_"))[1];
                            if (!roomCompareNum.Contains(checker))
                            {
                                
                                if(notdistroyed.Contains(child.GetChild(objectCount).name) == true)
                                {
                                    Debug.Log(child.GetChild(objectCount).name);
                                }
                                else
                                {
                                    Destroy(child.GetChild(objectCount).gameObject);
                                }
                                
                                //Destroy(child.GetChild(objectCount).gameObject);
                            }


                            // only if boundary하면 더 나을거 같음 (일단은 보류)
                            // delete objects on wall
                            /*
                            if (child.GetChild(objectCount).name.StartsWith("Painting_") || child.GetChild(objectCount).name.StartsWith("window_"))
                            {
                                Destroy(child.GetChild(objectCount).gameObject);
                            }
                            */
                            
                        }

                    }
                    else if (child.name == "Structure")
                    {
                        foreach (Transform structure in child.transform)
                        {
                            if (structure.name == "Floor")
                            {
                                for (int i = 0; i < structure.childCount; i++)
                                {
                                    if (!roomCompare.Contains(structure.GetChild(i).name))
                                    {
                                        Destroy(structure.GetChild(i).gameObject);
                                    }
                                }
                            }
                            else if (structure.name == "Walls")
                            {
                                for (int i = 0; i < structure.childCount; i++)
                                {
                                    string checker = structure.GetChild(i).name.Split(char.Parse("_"))[1];
                                    if (!roomCompareNum.Contains(checker))
                                    {
                                        Destroy(structure.GetChild(i).gameObject); 
                                    }

                                }
                            }
                            else if (structure.name == "Ceiling")
                            {
                                for (int i = 0; i < structure.childCount; i++)
                                {
                                    string checker = structure.GetChild(i).name.Split(char.Parse("_"))[2];
                                    if (!roomCompareNum.Contains(checker))
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
                //allPolygonToDraw.transform.parent = houses[housesCount].transform;
                string filename;
                if (housesCount == 0)
                {
                    filename = Application.dataPath + "/Tayoptoutput/mergedPolygon_" + housesCount.ToString() + ".json";
                    //sharedspacePoints = script.alignedBaseline.GetCleanedSpacepoints(allPolygonToDraw, filename, houses[housesCount].transform.position);
                    sharedspacePoints = script.alignedBaseline.GetAnchorSpacepoints(allPolygonToDraw, filename, houses[housesCount].transform.position);

                }
                else
                {
                    //filename = Application.dataPath + "/Tayoptoutput/mergedPolygon_" + housesCount.ToString() + ".json";
                    filename = Application.dataPath + "/Tayoptoutput/mergedPolygon_" + housesCount.ToString() + ".json";
                    sharedspacePoints = script.alignedBaseline.GetAnchorSpacepoints(allPolygonToDraw, filename, houses[housesCount].transform.position);

                }


                

                // ------------------------------ character --------------------------------- //

                GameObject Characters = new GameObject("Characters");
                Characters.transform.position = houses[housesCount].transform.position;
                Characters.transform.parent = this.transform;



                for (int i = 0; i < users.Count; i++)
                {
                    
                    // 일단 모든 애들을 한번 크게 옮겨줘야함
                    GameObject copyedCharacter = GameObject.Instantiate(users[i].gameObject);
                    copyedCharacter.transform.position = houses[housesCount].transform.position + new Vector3(0, 0f, 0);
                    copyedCharacter.transform.position += new Vector3(currentHouseTrans[3], 0, currentHouseTrans[4]);
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





    void Update()
    {




    }


    
}
