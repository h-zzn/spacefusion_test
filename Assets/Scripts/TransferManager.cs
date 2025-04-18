using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Components;
using Meta.XR.MultiplayerBlocks.NGO;
using Newtonsoft.Json;
using UnityEngine.TextCore.Text;



public class TransferManager : NetworkBehaviour
{
    // this is for opt result sending
    public GameObject arrangeMR;
    public GameObject regionsObj;
    private List<PythonEachHouse> receivedInfo;
    private List<PythonEachHouse> pythonAllHouse;



    // this is for pivot change
    public GameObject sceneSelection;
    public bool startSendingVec = false;
    public Vector3 lastReceivedVector;
    public Vector3 receivedFromClient1;
    public Vector3 receivedFromClient2;


    // related to mode shifts
    private bool walkin = false;


    void Start()
    {


    }


    void Update()
    {


        // you can change the mode here
        // A: 공간 freeze mode
        // B: 위치유지 mode
        if (Input.GetKeyDown(KeyCode.A))
        {
            walkin = true;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {

            walkin = false;
        }

        // update user pos 
        if (arrangeMR.GetComponent<Arrange_Walkin>().characterInitialized == true)
        {

            for (int i = 0; i < regionsObj.GetComponent<Regions>().characters.Count; i++)
            {
                if (walkin == false)
                {
                    regionsObj.GetComponent<Regions>().userPosVec4[i] = new Vector4(regionsObj.GetComponent<Regions>().characters[i].transform.position.x, 0, 
                                                        regionsObj.GetComponent<Regions>().characters[i].transform.position.z, 0);
                }

            }
        }




        /*
        // 여기는 arragement를 보내주는것
        // x: pos 보내는건 region에 있고
        // y: python에서 받아오는건 arrange에 있음
        // z: propagate to clients
        if (Input.GetKeyDown(KeyCode.Z))
        {
            string sendingOptString = arrangeMR.GetComponent<Arrange_Walkin>().sendingOptString;

            if (IsServer && sendingOptString != "")
            {
                optResultToClientRpc(sendingOptString);
                arrangeMR.GetComponent<Arrange_Walkin>().drawTraverseZone(arrangeMR.GetComponent<Arrange_Walkin>().pythonAllHouse, regionsObj.GetComponent<Regions>().chooseHouseNum);
            }
            else
            {

            }

        }
        */


        // if pressed, start sending
        if (Input.GetKeyDown(KeyCode.B))
        {
            startSendingVec = true;
        }


        if (startSendingVec && sceneSelection.GetComponent<SceneSelection>().modeB == true)
        {
            if (IsServer)
            {
                Vector3 sendingVec = sceneSelection.GetComponent<SceneSelection>().diffChange;
                VecFromServerToClientRpc(sendingVec);
                //ServerSendVector();
            }

            else
            {
                Vector3 sendingVec = sceneSelection.GetComponent<SceneSelection>().diffChange;
                VecFromClientsToServerRpc(sendingVec);
                //ClientSendVector();
            }
        }


    }


    // ------------------------- sending vector ------------------------- //    
    public struct Vector3Data : INetworkSerializable
    {
        private float x, y, z;

        public Vector3Data(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref y);
            serializer.SerializeValue(ref z);
        }
    }

    // Add the NetworkVariable inside the class
    private NetworkVariable<Vector3Data> serverRandomVector = new NetworkVariable<Vector3Data>(
        new Vector3Data(Vector3.zero),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    // ------------------------- Rpc functions ------------------------- //

    // this is receiver
    // server to all clients
    [ClientRpc]
    public void VecFromServerToClientRpc(Vector3 fromServerVector)
    {
        if (!IsServer)
        {
            Debug.Log($"server -> client: {fromServerVector}");
            lastReceivedVector = fromServerVector;
            sceneSelection.GetComponent<SceneSelection>().ApplyRoomMovement(fromServerVector);
        }
    }


    // client to server
    [ServerRpc(RequireOwnership = false)]
    public void VecFromClientsToServerRpc(Vector3 fromClientVector, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"client -> server {serverRpcParams.Receive.SenderClientId}: {fromClientVector}");
        receivedFromClient1 = fromClientVector;
        sceneSelection.GetComponent<SceneSelection>().ApplyRoomMovement(fromClientVector);


        // Optionally broadcast it to all clients
        //VecFromServerToClientRpc(fromClientVector);
    }


    /*
    // this is sender
    // server -> client
    public void ServerSendVector()
    {
        if (!IsServer) return;
        Vector3 sendingVec = sceneSelection.GetComponent<SceneSelection>().diffChange;

        // this one is usually for backup
        //serverRandomVector.Value = new Vector3Data(sendingVec);
        VecFromServerToClientRpc(sendingVec);
    }

    // client -> server
    public void ClientSendVector()
    {
        if (IsServer) return;
        Vector3 sendingVec = sceneSelection.GetComponent<SceneSelection>().diffChange;

        VecFromClientsToServerRpc(sendingVec);
    }
    */


    // callback method
    private void OnServerVectorChanged(Vector3Data previousValue, Vector3Data newValue)
    {
        Debug.Log($"NetworkVariable vector updated: {newValue.ToVector3()}");
        // Process the new vector value here
    }



    // ------------------------- propagate opt result ------------------------- //

    [ClientRpc]
    public void optResultToClientRpc(string sendingOptString)
    {

        GameObject Characters = GameObject.Find("Characters");
        GameObject myCharacter = GameObject.Find("LocalAvatar");


        if (!IsServer)
        {

            // return to original position
            for (int i = 0; i < arrangeMR.GetComponent<Arrange_Walkin>().houses.Count; i++)
            {
                arrangeMR.GetComponent<Arrange_Walkin>().houses[i].transform.position = Vector3.zero;
                arrangeMR.GetComponent<Arrange_Walkin>().houses[i].transform.rotation = Quaternion.identity;
            }

            // delete previous traverse zones
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


            Debug.Log($"server -> client: string input");
            receivedInfo = JsonConvert.DeserializeObject<List<PythonEachHouse>>(sendingOptString);
            pythonAllHouse = receivedInfo;


            // arrange houses
            for (int i = 0; i < pythonAllHouse.Count; i++)
            {

                // rotationn
                Vector3 centroid = new Vector3(pythonAllHouse[i].polygon.centroid.x, 0, pythonAllHouse[i].polygon.centroid.y);
                arrangeMR.GetComponent<Arrange_Walkin>().houses[i].transform.RotateAround(centroid, Vector3.up, -pythonAllHouse[i].polygon.rotation);

                // translation
                Vector3 transVec = new Vector3(pythonAllHouse[i].polygon.trans.x, 0, pythonAllHouse[i].polygon.trans.y);
                arrangeMR.GetComponent<Arrange_Walkin>().houses[i].transform.position = arrangeMR.GetComponent<Arrange_Walkin>().houses[i].transform.position + transVec;



                if (arrangeMR.GetComponent<Arrange_Walkin>().chooseHouseNum == i)
                {

                    // 나도 옮겨줘야하네 -> 얘는 나중에 실공간 기준으로 옮겨주는 식으로 구현한다


                    myCharacter.transform.position = new Vector3(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[i].boundary.final_centroid.y);

                    regionsObj.GetComponent<Regions>().userPosVec4[i] = new Vector4(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[i].boundary.final_centroid.y, 0);



                }
                else
                {
                    if (arrangeMR.GetComponent<Arrange_Walkin>().chooseHouseNum == 0)
                    {
                        if (remote1 != null)
                        {
                            remote1.transform.position = new Vector3(pythonAllHouse[1].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[1].boundary.final_centroid.y);
                        }
                        if (remote != null)
                        {
                            remote.transform.position = new Vector3(pythonAllHouse[2].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[2].boundary.final_centroid.y);
                        }
                    }
                    else if (arrangeMR.GetComponent<Arrange_Walkin>().chooseHouseNum == 1)
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
                    else if (arrangeMR.GetComponent<Arrange_Walkin>().chooseHouseNum == 2)
                    {
                        if (remote1 != null)
                        {
                            remote1.transform.position = new Vector3(pythonAllHouse[1].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[1].boundary.final_centroid.y);
                        }
                        if (remote != null)
                        {
                            remote.transform.position = new Vector3(pythonAllHouse[2].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[2].boundary.final_centroid.y);
                        }
                    }

                    
                    Characters.transform.GetChild(i).transform.position = new Vector3(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                       pythonAllHouse[i].boundary.final_centroid.y);

                    regionsObj.GetComponent<Regions>().userPosVec4[i] = new Vector4(pythonAllHouse[i].boundary.final_centroid.x, 0,
                                                                                        pythonAllHouse[i].boundary.final_centroid.y, 0);





                }


                // draw traverse zone
                arrangeMR.GetComponent<Arrange_Walkin>().drawTraverseZone(pythonAllHouse, arrangeMR.GetComponent<Arrange_Walkin>().chooseHouseNum);

                

            }


        }
    }


}
