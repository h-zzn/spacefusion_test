using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Meta.XR.MultiplayerBlocks.NGO;

public class Arrange_Walkin : MonoBehaviour
{

    public List<GameObject> houses;
    public List<string> roomNums = new List<string>();
    public int chooseHouseNum;
    public int forObjAug;

    // users to populate
    public List<GameObject> users;

    // get voronoi region from Regions
    public GameObject regionsObj;

    // check anchor polygon points fed
    public bool characterInitialized = false;

    // AvatarCullingModule
    public AvatarCullingModule avatarCullingModule;

    public GameObject sceneSelection;

    // ====================== added for walkin ======================
    private List<PythonEachHouse> pythonAllHouse;
    public List<List<Vector3>> selectedZones = new List<List<Vector3>>();
    public bool receivedZones = false;
    private List<PythonEachHouse> receivedInfo;

    public Sender sender;
    private string defaultPath = "C:/Users/hayley/Desktop/WalkIn_Opt/";
    public bool visUpdated = false;
    public string sendingOptString = "";


    // added for circle
    public float radius = 0.9f;
    public int segments = 360;
    public float lineWidth = 0.01f;
    public Color lineColor = Color.green;
    private LineRenderer lineRenderer;

    private bool circleDrawn = false;

    public GameObject transfer;
    public bool isServer = false;

    void Start()
    {

        string filename = Application.dataPath + "/Tayoptoutput/polygon_transformations.json";

        
        if (File.Exists(filename))
        {
            string jsonToRead = File.ReadAllText(filename);
            //List<PythonEachHouse> receivedInfo = JsonConvert.DeserializeObject<List<PythonEachHouse>>(jsonToRead);
            receivedInfo = JsonConvert.DeserializeObject<List<PythonEachHouse>>(jsonToRead);
            pythonAllHouse = receivedInfo;


            
            // arrange houses
            for (int i = 0; i < pythonAllHouse.Count; i++)
            {
                /*
                // rotationn
                Vector3 centroid = new Vector3(pythonAllHouse[i].polygon.centroid.x, 0, pythonAllHouse[i].polygon.centroid.y);
                houses[i].transform.RotateAround(centroid, Vector3.up, -pythonAllHouse[i].polygon.rotation);

                // translation
                Vector3 transVec = new Vector3(pythonAllHouse[i].polygon.trans.x, 0, pythonAllHouse[i].polygon.trans.y);
                houses[i].transform.position = houses[i].transform.position + transVec;
                */


                /*
                if(chooseHouseNum == i)
                {

                    // draw traverse zone
                    if (pythonAllHouse[i].traverse.IsNested)
                    {
                        for (int j = 0; j < pythonAllHouse[i].traverse.NestedCoords.Count; j++)
                        {
                            // traverse zone object
                            GameObject traverseZone = new GameObject($"traverseZone_{j}");
                            Vector3[] points = ConvertToVector3Array(pythonAllHouse[i].traverse.NestedCoords[j]);
                            DrawZone(traverseZone, points);
                        }
                        

                    }
                    else
                    {
                        // connected zone
                        GameObject traverseZone = new GameObject($"traverseZone");
                        Vector3[] points = ConvertToVector3Array(pythonAllHouse[i].traverse.SimpleCoords);
                        DrawZone(traverseZone, points);
                    }


                }
                */


            }


            // 여기서 알수는 없는데 뭔가 initialize 해주는게 있나본데?
            // 여기서 이제 boundary information 읽으면서 
            for (int i = 0; i < pythonAllHouse.Count; i++)
            {
                List<Vector3> eachSelectedZones = new List<Vector3>();
                for (int j = 0; j < pythonAllHouse[i].boundary.coords.Length; j++)
                {
                    float x = pythonAllHouse[i].boundary.coords[j][0];
                    float z = pythonAllHouse[i].boundary.coords[j][1];
                    Vector3 zonePoints = new Vector3(x, 0, z);

                    eachSelectedZones.Add(zonePoints);

                }
                selectedZones.Add(eachSelectedZones);
            }
            
            
        }

        receivedZones = true;


        GameObject Characters = new GameObject("Characters");
        Characters.transform.position = houses[0].transform.position;
        Characters.transform.parent = this.transform;

        // this is needed to attach joining users
        Vector3 moveForCharacter = new Vector3(0, 0, 0);
        for (int i = 0; i < users.Count; i++)
        {

            // 일단 모든 애들을 한번 크게 옮겨줘야함
            GameObject copyedCharacter = GameObject.Instantiate(users[i].gameObject);
            copyedCharacter.transform.position = houses[0].transform.position + new Vector3(0, 0f, 0);

            // check here
            copyedCharacter.transform.position += moveForCharacter;
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

    }


    // Update is called once per frame
    void Update()
    {

        GameObject Characters = GameObject.Find("Characters");
        GameObject myCharacter = GameObject.Find("LocalAvatar");

        /*
        // 여기서 mycharacter 금방 언제나 line draw
        if (myCharacter != null && circleDrawn == false)
        {
            
            lineRenderer = myCharacter.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = myCharacter.AddComponent<LineRenderer>();
            }

            // Configure LineRenderer
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;


            DrawCircle();
            circleDrawn = true;
        }
        */


        
        if (Input.GetKeyDown(KeyCode.Y) && visUpdated == false)
        {


            circleDrawn = false;
            regionsObj.GetComponent<Regions>().receiveFromPython = true;
            sender.SendToPython("Y");

            string optResultFile = "polygon_transformations.json";

            string jsonContent = File.ReadAllText(defaultPath + optResultFile);
            sendingOptString = jsonContent;
            //Debug.Log(sendingOptString); // 잘 들어오고


            
            // houses to default pos + rot 

            for (int i = 0; i < pythonAllHouse.Count; i++)
            {
                houses[i].transform.position = Vector3.zero;
                houses[i].transform.rotation = Quaternion.identity;
            }
            

            
            // delete previous traverze zone line
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("traverseZone"))
                {
                    Destroy(obj);
                }
            }

            // deactivate sync
            GameObject remote1 = GameObject.Find("RemoteAvatar1");
            if (remote1 != null)
            {
                remote1.GetComponent<ClientNetworkTransform>().SyncPositionX = false;
                remote1.GetComponent<ClientNetworkTransform>().SyncPositionY = false;
                remote1.GetComponent<ClientNetworkTransform>().SyncPositionZ = false;
            }

            GameObject remote = GameObject.Find("RemoteAvatar");
            if (remote != null)
            {
                remote.GetComponent<ClientNetworkTransform>().SyncPositionX = false;
                remote.GetComponent<ClientNetworkTransform>().SyncPositionY = false;
                remote.GetComponent<ClientNetworkTransform>().SyncPositionZ = false;
            }
            


            // ======================================= reflect opt result ======================================= //
            receivedInfo = JsonConvert.DeserializeObject<List<PythonEachHouse>>(jsonContent);
            pythonAllHouse = receivedInfo;


            // arrange houses
            for (int i = 0; i < pythonAllHouse.Count; i++)
            {
                
                // rotation
                Vector3 centroid = new Vector3(pythonAllHouse[i].polygon.centroid.x, 0, pythonAllHouse[i].polygon.centroid.y);
                houses[i].transform.RotateAround(centroid, Vector3.up, -pythonAllHouse[i].polygon.rotation);

                // translation
                Vector3 transVec = new Vector3(pythonAllHouse[i].polygon.trans.x, 0, pythonAllHouse[i].polygon.trans.y);
                houses[i].transform.position = houses[i].transform.position + transVec;
                


                if (chooseHouseNum == i)
                {

                    // 나도 옮겨줘야하네 -> 얘는 나중에 실공간 기준으로 옮겨주는 식으로 구현한다

                    myCharacter.transform.position = new Vector3(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[i].boundary.final_centroid.y);

                    regionsObj.GetComponent<Regions>().userPosVec4[i] = new Vector4(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[i].boundary.final_centroid.y, 0);


                }
                else
                {
                    if (chooseHouseNum == 0)
                    {
                        if(remote1 != null)
                        {
                            remote1.transform.position = new Vector3(pythonAllHouse[1].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[1].boundary.final_centroid.y);
                        }
                        if(remote != null)
                        {
                            remote.transform.position = new Vector3(pythonAllHouse[2].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[2].boundary.final_centroid.y);
                        }
                       
                    }
                    else if (chooseHouseNum == 1)
                    {
                        if (remote1 != null)
                        {
                            remote1.transform.position = new Vector3(pythonAllHouse[0].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[0].boundary.final_centroid.y);
                        }
                        if (remote != null)
                        {
                            remote.transform.position = new Vector3(pythonAllHouse[2].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[2].boundary.final_centroid.y);
                        }
                    }
                    else if (chooseHouseNum == 2)
                    {
                        if (remote1 != null)
                        {
                            remote1.transform.position = new Vector3(pythonAllHouse[1].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[0].boundary.final_centroid.y);
                        }
                        if (remote != null)
                        {
                            remote.transform.position = new Vector3(pythonAllHouse[2].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[2].boundary.final_centroid.y);
                        }

                    }

                    
                    // this is working if the remote is empty
                    Characters.transform.GetChild(i).transform.position = new Vector3(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                       pythonAllHouse[i].boundary.final_centroid.y);

                    regionsObj.GetComponent<Regions>().userPosVec4[i] = new Vector4(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[i].boundary.final_centroid.y, 0);

                }



                drawTraverseZone(pythonAllHouse, chooseHouseNum);



            }


            // propagate to clients
            if (isServer == true)
            {

                transfer.GetComponent<TransferManager>().optResultToClientRpc(sendingOptString);
                drawTraverseZone(pythonAllHouse, regionsObj.GetComponent<Regions>().chooseHouseNum);
            }
            else
            {

            }





            // 이거때문에 두 번째 부터는 안들어오는거 같은데
            //visUpdated = true;
        }




        // 다시 신호가 가면 모두의 싱크를 맞춰준다



    }


    public Vector3[] ConvertToVector3Array(List<float[]> coords)
    {
        Vector3[] points = new Vector3[coords.Count];
        for (int i = 0; i < coords.Count; i++)
        {
            // Assuming coords[i] contains [x, y]. Using y for z to convert 2D to 3D
            points[i] = new Vector3(coords[i][0], 0.05f, coords[i][1]);
        }
        return points;
    }


    public SharedSpacePoints DrawZone(GameObject addToWhichGameobj, Vector3[] points)
    {
        SharedSpacePoints sharedspacePoints = new SharedSpacePoints();
        LineRenderer lineRenderer = new LineRenderer();
        lineRenderer = addToWhichGameobj.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.widthMultiplier = 0.015f;
        lineRenderer.positionCount = points.Length;

        lineRenderer.SetPositions(points);
        sharedspacePoints.exteriorPoints = points.ToList();

        return sharedspacePoints;
    }


    public void DrawCircle()
    {
        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        for (int i = 0; i <= segments; i++)
        {
            // Changed to XZ plane (x and z coordinates instead of x and y)
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);

            lineRenderer.SetPosition(i, new Vector3(x, 0f, z));  // Y is now 0, Z is used instead
            theta += deltaTheta;
        }
    }


    public void drawTraverseZone(List<PythonEachHouse> pythonAllHouse, int chooseHouseNum)
    {

        
        // draw traverse zone
        if (pythonAllHouse[chooseHouseNum].traverse.IsNested)
        {
            for (int j = 0; j < pythonAllHouse[chooseHouseNum].traverse.NestedCoords.Count; j++)
            {
                // traverse zone object
                GameObject traverseZone = new GameObject($"traverseZone_{j}");
                Vector3[] points = ConvertToVector3Array(pythonAllHouse[chooseHouseNum].traverse.NestedCoords[j]);
                DrawZone(traverseZone, points);
            }


        }
        else
        {
            // connected zone
            GameObject traverseZone = new GameObject($"traverseZone");
            Vector3[] points = ConvertToVector3Array(pythonAllHouse[chooseHouseNum].traverse.SimpleCoords);
            DrawZone(traverseZone, points);
        }
    }



}
