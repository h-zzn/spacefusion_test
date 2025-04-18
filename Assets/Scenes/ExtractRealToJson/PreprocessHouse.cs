using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using System;
using JetBrains.Annotations;
using GLTFast.Schema;
using System.Linq;
using Unity.Burst.CompilerServices;

public class PreprocessHouse : MonoBehaviour
{
    public GameObject[] houses;

    public string getFromFolder;


    private bool saveReady = true;
    private bool activateReady = false;
    private bool bboxReady = false;
    private GameObject Origin;

    private Renderer[] checkMesh;

    // AABB bounds
    private Renderer[] renderers;
    private Bounds bounds;
    private GameObject AABBCollider;

    // OBB bounds
    private Renderer[] OBBrenderers;
    private Bounds OBBbounds;
    private GameObject OBBCollider;

    // floor 
    private UnityEngine.Mesh floorMesh;


    // all houses
    private List<House> allHouses;
    string SAVE_BASE_PATH = "/Tayjsons/Real/";



    void Start()
    {
        // enable read/write of all houses
        //string pathToHouses = Application.dataPath + "/VirtualHouses/";
        string pathToHouses = Application.dataPath + "/" + getFromFolder + "/";
        string[] files = Directory.GetFiles(pathToHouses, "*.fbx", SearchOption.TopDirectoryOnly);

        // problem is, you cannot load fbx in runtime
        /*
        foreach (string file in files)
        {
            string houseName = file.Split('.')[0];
            GameObject prefab = Resources.Load(houseName) as GameObject;
            Debug.Log(prefab);
        }
        */


        // object and structures to origin
        for (int i = 0; i < houses.Length; i++)
        {
            // create reference frame;
            Origin = new GameObject("Origin");
            Origin.transform.position = Vector3.zero;
            Origin.transform.rotation = Quaternion.identity;
            Origin.transform.parent = houses[i].transform;

            houses[i].transform.Find("Objects").transform.localPosition = Vector3.zero;
            houses[i].transform.Find("Structure").transform.localPosition = Vector3.zero;

        }

        // erase unnecessary
        for (int i = 0; i < houses.Length; i++)
        {
            GameObject temphouse = houses[i];
            foreach (Transform child in temphouse.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains("Colliders") || child.name.Contains("TriggerColliders") || child.name.Contains("VisibilityPoints") || child.name.Contains("BoundingBox"))
                {
                    Destroy(child.gameObject);
                }
                else if (child.name.Contains("TriggerBox") || child.name.Contains("Particle") || child.name.Contains("Triggers"))
                {
                    Destroy(child.gameObject);
                }
            }

            foreach (Transform child in temphouse.GetComponentInChildren<Transform>())
            {
                if (child.name.Contains("Structure"))
                {
                    foreach (Transform structure in child.GetComponentsInChildren<Transform>())
                    {
                        if (structure.name.Contains("Floor"))
                        {
                            foreach (Transform floor in structure.GetComponentInChildren<Transform>())
                            {
                                floor.gameObject.AddComponent<MeshCollider>();
                            }

                        }
                    }
                }
            }

        }


    }

    private void Update()
    {
        // export with origin
        int startingNum = int.Parse(houses[0].name.Split('_')[1]);
        if (saveReady)
        {
            //ExportGameObjectSingle(house);
            for (int count = 0; count < houses.Length; count++)
            {
                int houseNum = startingNum + count;
                // if have to export to fbx, use this
                //ExportGameObject(houses[count], houseNum);
            }

            saveReady = false;
            activateReady = true;
            //bboxReady = true;
        }


        if (activateReady)
        {
            for (int count = 0; count < houses.Length; count++)
            {
                houses[count].SetActive(false);
            }


            activateReady = false;
            bboxReady = true;
        }


        if (bboxReady)
        {
            for (int i = 0; i < houses.Length; i++)
            {
                // activate only current
                houses[i].SetActive(true);


                GameObject temphouse = houses[i];
                House eachHouse = new House(temphouse.name);

                foreach (Transform child in temphouse.GetComponentInChildren<Transform>())
                {
                    if (child.name.Contains("Objects"))
                    {
                        int objectCounter = 0;

                        foreach (Transform furniture in child.GetComponentInChildren<Transform>())
                        {

                            // check whether mesh exists
                            checkMesh = furniture.GetComponentsInChildren<Renderer>();

                            if (checkMesh.Length == 0)
                            {
                                continue;
                            }
                            else
                            {
                                // prepare saver
                                HouseObject tempobject = new HouseObject(furniture.name);

                                // AABB
                                renderers = furniture.GetComponentsInChildren<Renderer>();
                                bounds = renderers[0].bounds;
                                for (var meshNum = 1; meshNum < renderers.Length; ++meshNum)
                                    bounds.Encapsulate(renderers[meshNum].bounds);

                                AABBCollider = new GameObject("AABBCollider");
                                AABBCollider.transform.position = Vector3.zero;
                                AABBCollider.transform.rotation = Quaternion.identity;
                                AABBCollider.transform.parent = furniture.transform;

                                BoxCollider furnitureCollider = AABBCollider.AddComponent<BoxCollider>();
                                furnitureCollider.center = bounds.center;
                                furnitureCollider.size = bounds.size;

                                Vector3[] AABBList = GetColliderVertexPos(AABBCollider);
                                //DrawBBox(AABBList);

                                // OBB
                                Quaternion currentObjRot = furniture.transform.localRotation;

                                furniture.transform.localRotation = Quaternion.identity;
                                OBBrenderers = furniture.GetComponentsInChildren<Renderer>();
                                OBBbounds = OBBrenderers[0].bounds;
                                for (var meshNum = 1; meshNum < OBBrenderers.Length; ++meshNum)
                                {
                                    OBBbounds.Encapsulate(OBBrenderers[meshNum].bounds);
                                }

                                OBBCollider = new GameObject("OBBCollider");
                                OBBCollider.transform.position = Vector3.zero;
                                OBBCollider.transform.rotation = Quaternion.identity;
                                OBBCollider.transform.parent = furniture.transform;

                                BoxCollider furnitureOBBCollider = OBBCollider.AddComponent<BoxCollider>();
                                furnitureOBBCollider.center = OBBbounds.center;
                                furnitureOBBCollider.size = OBBbounds.size;

                                // rotate to the original direction
                                furniture.transform.localRotation = currentObjRot;

                                Vector3[] OBBList = GetColliderVertexPos(OBBCollider);
                                //DrawBBox(OBBList);


                                // save each furniture wrt origin
                                tempobject.objName = furniture.name;
                                tempobject.objIndex = objectCounter.ToString();

                                if (furniture.name.Contains("Chair") || furniture.name.Contains("Sofa") || furniture.name.Contains("Bed"))
                                {
                                    tempobject.objCategory = 0.ToString();
                                }
                                else if (furniture.name.Contains("Table"))
                                {
                                    tempobject.objCategory = 1.ToString();
                                }
                                else { tempobject.objCategory = 2.ToString(); }

                                // object in which room?
                                RaycastHit[] hits = Physics.RaycastAll(AABBCollider.GetComponent<BoxCollider>().center, -Vector3.up, Mathf.Infinity);
                                foreach (RaycastHit hit in hits)
                                {

                                    if (hit.collider.GetType() == typeof(MeshCollider))
                                    {
                                        tempobject.SetObjInWhichFloor(hit.collider.transform.name);
                                        //Debug.Log(hit.collider.transform.name);
                                    }
                                    else if (hit.collider.transform.name == "AABBCollider" || hit.collider.transform.name == "OBBCollider")
                                    {

                                    }
                                    else
                                    {

                                    }
                                }


                                //Debug.DrawRay(AABBCollider.GetComponent<BoxCollider>().center, furniture.transform.TransformDirection(-Vector3.up) * hit.distance, Color.yellow);

                                tempobject.SetObjPos(Origin.transform.InverseTransformPoint(furniture.transform.position)); // pos
                                tempobject.SetObjRot(Quaternion.Inverse(Origin.transform.rotation) * furniture.transform.rotation);


                                // check facing direction
                                //Vector3 forward = furniture.transform.TransformDirection(Vector3.forward) * 2;
                                //Debug.DrawRay(furniture.transform.position, forward, Color.green, 30);

                                Vector3[] AABBListWRTOrigin = new Vector3[8];
                                for (int boundsLength = 0; boundsLength < AABBList.Length; boundsLength++)
                                {
                                    AABBListWRTOrigin[boundsLength] = Origin.transform.InverseTransformPoint(AABBList[boundsLength]);
                                }


                                tempobject.SetAABB(Origin.transform.InverseTransformPoint(bounds.center), AABBListWRTOrigin, bounds.size);

                                Vector3[] OBBListWRTOrigin = new Vector3[8];
                                for (int boundsLength = 0; boundsLength < AABBList.Length; boundsLength++)
                                {
                                    OBBListWRTOrigin[boundsLength] = Origin.transform.InverseTransformPoint(OBBList[boundsLength]);
                                }
                                tempobject.SetOBB(OBBListWRTOrigin);


                                // increase object num
                                objectCounter += 1;
                                eachHouse.AddObjects(tempobject);



                            }




                        }
                    }
                    else if (child.name.Contains("Structure"))
                    {

                        foreach (Transform structure in child.GetComponentsInChildren<Transform>())
                        {
                            if (structure.name.Contains("Floor"))
                            {
                                int floorCounter = 0;
                                foreach (Transform floor in structure.GetComponentInChildren<Transform>())
                                {
                                    HouseFloor tempfloor = new HouseFloor(floor.gameObject.name);
                                    tempfloor.SetFloorIndex(floorCounter.ToString());

                                    tempfloor.SetFloorPos(Origin.transform.InverseTransformPoint(floor.transform.position));
                                    tempfloor.setFloorRot(Quaternion.Inverse(Origin.transform.rotation) * floor.transform.rotation);


                                    floorMesh = floor.GetComponent<MeshFilter>().mesh;

                                    List<Vector3> floorCornerListWRTOrigin = new List<Vector3>();
                                    for (int boundsLength = 0; boundsLength < floorMesh.vertices.Length; boundsLength++)
                                    {
                                        floorCornerListWRTOrigin.Add(Origin.transform.InverseTransformPoint(floorMesh.vertices[boundsLength]));
                                    }

                                    tempfloor.setFloorCornerPoints(floorCornerListWRTOrigin);

                                    // mesh triangle도 mesh 생성을 위해 저장해두자 


                                    floorCounter += 1;
                                    eachHouse.AddFloors(tempfloor);
                                }


                            }
                            else if (structure.name.Contains("Walls"))
                            {
                                int wallCounter = 0;
                                foreach (Transform wall in structure.GetComponentInChildren<Transform>())
                                {
                                    

                                    HouseWall tempwall = new HouseWall(wall.gameObject.name);

                                    Vector3[] wallCornerPoints = new Vector3[8];
                                    wallCornerPoints = GetWallVertexPos(wall.gameObject);
                                    //DrawBBoxLine(wallCornerPoints, wall.gameObject);

                                    tempwall.SetWallIndex(wallCounter.ToString());

                                    tempwall.SetWallPos(Origin.transform.InverseTransformPoint(wall.transform.position));
                                    tempwall.SetWallRot(Quaternion.Inverse(Origin.transform.rotation) * wall.transform.rotation);

                                    Vector3[] wallCornerListWRTOrigin = new Vector3[8];
                                    for (int boundsLength = 0; boundsLength < wallCornerPoints.Length; boundsLength++)
                                    {
                                        wallCornerListWRTOrigin[boundsLength] = Origin.transform.InverseTransformPoint(wallCornerPoints[boundsLength]);
                                    }

                                    tempwall.SetWallCornerPoints(wallCornerListWRTOrigin);

                                    wallCounter++;
                                    eachHouse.AddWalls(tempwall);
                                }


                            }
                            else
                            {



                            }

                        }

                    }


                }


                //houses[i].SetActive(false);
                //allHouses.Add(eachHouse);

                
                SaveHouseToJson(eachHouse);

            }



            bboxReady = false;
            saveReady = false;
        }



    }


    // multiple
    public static void ExportGameObject(GameObject house, int houseNum)
    {
        string fileName = "realhouse_" + houseNum.ToString() + ".fbx";
        string filePath = Path.Combine(Application.dataPath + "/Realprocessedhouses/", fileName);


        ExportModelOptions exportSettings = new ExportModelOptions();
        exportSettings.ExportFormat = ExportFormat.ASCII;
        exportSettings.KeepInstances = false;

        //ModelExporter.ExportObjects(filePath, objects, exportSettings);
        ModelExporter.ExportObject(filePath, house, exportSettings);

    }


    // single 
    public static void ExportGameObjectSingle(GameObject house)
    {
        string fileName = "house_sample.fbx";
        string filePath = Path.Combine(Application.dataPath, fileName);


        ExportModelOptions exportSettings = new ExportModelOptions();
        exportSettings.ExportFormat = ExportFormat.ASCII;
        exportSettings.KeepInstances = false;

        //ModelExporter.ExportObjects(filePath, objects, exportSettings);
        ModelExporter.ExportObject(filePath, house, exportSettings);

    }

    public Vector3[] GetColliderVertexPos(GameObject obj)
    {
        BoxCollider b = obj.GetComponent<BoxCollider>(); //retrieves the Box Collider of the GameObject called obj
        Vector3[] vertices = new Vector3[8];
        vertices[0] = obj.transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f);
        vertices[1] = obj.transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f);
        vertices[2] = obj.transform.TransformPoint(b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f);
        vertices[3] = obj.transform.TransformPoint(b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f);
        vertices[4] = obj.transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, -b.size.z) * 0.5f);
        vertices[5] = obj.transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, -b.size.z) * 0.5f);
        vertices[6] = obj.transform.TransformPoint(b.center + new Vector3(b.size.x, b.size.y, b.size.z) * 0.5f);
        vertices[7] = obj.transform.TransformPoint(b.center + new Vector3(-b.size.x, b.size.y, b.size.z) * 0.5f);

        return vertices;
    }

    public Vector3[] GetWallVertexPos(GameObject obj)
    {
        Vector3[] vertices = new Vector3[8];

        Renderer wallRenderer = obj.GetComponent<Renderer>();
        Bounds bounds = wallRenderer.bounds;

        // transfrom 내 origin 이랑 transform이 뭔가 좀 다른거같음
        vertices[0] = bounds.center + new Vector3(-bounds.size.x, -bounds.size.y, -bounds.size.z) * 0.5f;
        vertices[1] = bounds.center + new Vector3(bounds.size.x, -bounds.size.y, -bounds.size.z) * 0.5f;
        vertices[2] = bounds.center + new Vector3(bounds.size.x, -bounds.size.y, bounds.size.z) * 0.5f;
        vertices[3] = bounds.center + new Vector3(-bounds.size.x, -bounds.size.y, bounds.size.z) * 0.5f;
        vertices[4] = bounds.center + new Vector3(-bounds.size.x, bounds.size.y, -bounds.size.z) * 0.5f;
        vertices[5] = bounds.center + new Vector3(bounds.size.x, bounds.size.y, -bounds.size.z) * 0.5f;
        vertices[6] = bounds.center + new Vector3(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
        vertices[7] = bounds.center + new Vector3(-bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;


        return vertices;
    }

    void DrawBBox(Vector3[] cubeVertices)
    {

        GameObject CubeforCheck = new GameObject("Cube");
        CubeforCheck.transform.parent = transform;
        MeshFilter meshFilter = CubeforCheck.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = CubeforCheck.AddComponent<MeshRenderer>();

        // Create a new mesh
        UnityEngine.Mesh mesh = new UnityEngine.Mesh();
        mesh.vertices = cubeVertices;

        int[] triangles = new int[]
        {
            0, 1, 2, 2, 3, 0, // Front face
            4, 5, 6, 6, 7, 4, // Back face
            0, 4, 7, 7, 3, 0, // Left face
            1, 5, 6, 6, 2, 1, // Right face
            0, 1, 5, 5, 4, 0, // Bottom face
            2, 3, 7, 7, 6, 2  // Top face
        };
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;
    }

    void SaveHouseToJson(House eachHouse)
    {
        // eachhouse name을 00이 붙는 형식으로 바꾸어둠
        int houseNum = int.Parse(eachHouse.houseName.Split(char.Parse("_"))[1]);
        string path = Application.dataPath + SAVE_BASE_PATH + eachHouse.houseName.Split(char.Parse("_"))[0] + "_" + houseNum.ToString("000") + ".json";
        //string path = Application.dataPath + SAVE_BASE_PATH + eachHouse.houseName.ToString() + ".json";
        string eachHouseJson = JsonUtility.ToJson(eachHouse);

        StreamWriter sw = new StreamWriter(path);
        sw.AutoFlush = true;
        sw.Write(eachHouseJson);
        Debug.Log(eachHouse.houseName.ToString() + " is written to the json file");

    }



}
