 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;
using Oculus.Avatar2;
using UnityEngine.SceneManagement;
using Meta.XR.MultiplayerBlocks.Shared;
using Meta.XR.MultiplayerBlocks.NGO;


public class SceneSelection : MonoBehaviour
{
    [SerializeField] public int type = 0;
    [SerializeField] int[] AvatarIndexForUser = new int[3];
    public GameObject target;
    [SerializeField] GameObject avt;
    public GameObject pivot1, pivot2;
    [SerializeField] bool done = false;
    [SerializeField] GameObject houses;
    [SerializeField] bool IsDemoModeAndThisIsServer = false;
    bool A, B, C = false;

    // check augmented
    public bool notaugmented = true;

    // check MR Headset on
    public GameObject cameraRig;
    bool onlyOnce = false;

    // stop voronoi
    public bool modeB = false;
    

    // to detect pivot
    public GameObject meshEdgeAligner;
    public GameObject towerManager;
    public GameObject arrangeMRHouse;

    public Vector3 pivotDiff = Vector3.zero;
    public Vector3 diffChange = Vector3.zero;

    // detect my movement
    private Vector3 previousCharacterPos = Vector3.zero;
    private Vector3 currentCharacterPos = Vector3.zero;

    // return to original alignment
    private List<Vector3> originalRoomPos = new List<Vector3>();

    // transfer data
    public GameObject transfer;

    private void Awake()
    {
        //GameObject.Find("[BuildingBlock] Networked Avatar").GetComponent<>
    }

    void Start()
    {
        GameObject oveCamrig = GameObject.FindFirstObjectByType<OVRCameraRig>().gameObject;
        switch (type)
        {
            // 여기서 player layer change
            case 0:
                A = true;
                ChangeLayerRecursively(oveCamrig, 14);
                break;
            case 1: B = true; ChangeLayerRecursively(oveCamrig, 15); break;
            case 2: C = true; ChangeLayerRecursively(oveCamrig, 16); break;
        }
        if (!IsDemoModeAndThisIsServer)
        {

            //setPassthrogh(type);
        }

    }
    private void ChangeLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, layer);
        }
    }


    // Update is called once per frame
    void Update()
    {

        if(notaugmented == true)
        {
            //activateAugObjs();

            notaugmented = false;
        }


        if (target.GetComponent<Arrange_Walkin>().characterInitialized && !done)
        {

            avt = target.transform.GetChild(0).GetChild(type).gameObject;
            //canvas.SetActive(true);

            // save original house location 
            for (int i = 0; i < arrangeMRHouse.GetComponent<Arrange_Walkin>().houses.Count; i++)
            {
                originalRoomPos.Add(arrangeMRHouse.GetComponent<Arrange_Walkin>().houses[i].transform.position);
                
            }


            done = true;
        }

        if (avt != null)
        {
            avt.transform.position = Camera.main.transform.position;
            avt.transform.rotation = Camera.main.transform.rotation;



            if (IsHeadsetOff())
            {
                // sync camera and character
                if(GameObject.Find("LocalAvatar") != null)
                {
                    GameObject me = GameObject.Find("LocalAvatar");
                    Camera.main.transform.parent.parent.SetParent(me.transform);

                    // locate avatar pos
                    if (onlyOnce == false)
                    {
                        //me.transform.position = new Vector3(2.47f, 0f, 3.26f);
                        me.transform.position = new Vector3(4.1f, 0, 3.26f);
                        previousCharacterPos = me.transform.position;
                        currentCharacterPos = me.transform.position;
                        onlyOnce = true;
                    }

                    previousCharacterPos = currentCharacterPos;    
                    currentCharacterPos = me.transform.position;   

                    // Calculate the movement that occurred this frame
                    Vector3 positionChange = currentCharacterPos - previousCharacterPos;
                    // save to send
                    diffChange = positionChange;

                }
            }

            // deactivate when other user comes in
            
            if (GameObject.Find("RemoteAvatar1") != null)
            {
                GameObject remote1 = GameObject.Find("RemoteAvatar1");

                if(modeB == true)
                {
                    if (transfer.GetComponent<TransferManager>().startSendingVec == true)
                    {
                        remote1.GetComponent<ClientNetworkTransform>().SyncPositionX = false;
                        remote1.GetComponent<ClientNetworkTransform>().SyncPositionY = false;
                        remote1.GetComponent<ClientNetworkTransform>().SyncPositionZ = false;
                    }

                    // move other rooms and avatars
                    remote1.transform.position += diffChange;


                    for (int i = 0; i < arrangeMRHouse.GetComponent<Arrange_Walkin>().houses.Count; i++)
                    {
                        if (i != type)
                        {
                            arrangeMRHouse.GetComponent<Arrange_Walkin>().houses[i].transform.position += diffChange;
                        }

                    }

                    // Update Voronoi regions


                }
                else
                {

                    remote1.GetComponent<ClientNetworkTransform>().SyncPositionX = true;
                    remote1.GetComponent<ClientNetworkTransform>().SyncPositionY = true;
                    remote1.GetComponent<ClientNetworkTransform>().SyncPositionZ = true;

                }

            }

            if (GameObject.Find("RemoteAvatar") != null)
            {
                GameObject remote = GameObject.Find("RemoteAvatar");

                if (modeB == true)
                {
                    if (transfer.GetComponent<TransferManager>().startSendingVec == true)
                    {
                        remote.GetComponent<ClientNetworkTransform>().SyncPositionX = false;
                        remote.GetComponent<ClientNetworkTransform>().SyncPositionY = false;
                        remote.GetComponent<ClientNetworkTransform>().SyncPositionZ = false;
                    }

                    // move other rooms and avatars
                    remote.transform.position += diffChange;


                    for (int i = 0; i < arrangeMRHouse.GetComponent<Arrange_Walkin>().houses.Count; i++)
                    {
                        if (i != type)
                        {
                            arrangeMRHouse.GetComponent<Arrange_Walkin>().houses[i].transform.position += diffChange;
                        }

                    }


                }
                else
                {

                    remote.GetComponent<ClientNetworkTransform>().SyncPositionX = true;
                    remote.GetComponent<ClientNetworkTransform>().SyncPositionY = true;
                    remote.GetComponent<ClientNetworkTransform>().SyncPositionZ = true;

                }

            }


            // 거리유지 mode
            if (pivot1 != null && Input.GetKeyDown(KeyCode.B))
            {
                modeB = true;
            }


            // 공간 freeze mode
            if (pivot1 != null && Input.GetKeyDown(KeyCode.A))
            {
                modeB = false;

                /*
                // return to original location
                for (int i = 0; i < arrangeMRHouse.GetComponent<Arrange_Walkin>().houses.Count; i++)
                {
                    arrangeMRHouse.GetComponent<Arrange_Walkin>().houses[i].transform.position = originalRoomPos[i];
                }
                */

            }


            // if headset is not on
            if (IsHeadsetOff())
            {
                // this is to sync me and my real physical space
                if (pivot1 != null && A)
                {
                    if (pivot1.transform.childCount > 0)
                    {
                        try 
                        {
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.position = pivot1.transform.position;
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.rotation = pivot1.transform.rotation;
                        }
                        catch { }

                    }
                }
                if (pivot2 != null && A)
                {

                    if (pivot2.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.position;
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.GetChild(1).rotation;
                    }
                }

                if (pivot1 != null && B)
                {
                    if (pivot1.transform.childCount > 0)
                    {
                        try
                        {

                            // 여기서 상대 character 위치를 조정해주는구나? 
                            //Debug.Log(pivot1.transform.GetChild(2).position);
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.position;
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.GetChild(1).rotation;
                    }
                }
                if (pivot2 != null && B)
                {
                    if (pivot2.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.position;
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.GetChild(1).rotation;
                    }
                }

                if (pivot1 != null && C)
                {
                    if (pivot1.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.position;
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.GetChild(1).rotation;
                    }
                }
                if (pivot2 != null && C)
                {
                    if (pivot2.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.position = pivot2.transform.position;
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.rotation = pivot2.transform.rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(1).gameObject.transform.position = pivot2.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(1).gameObject.transform.rotation = pivot2.transform.GetChild(1).rotation;
                    }
                }



            }
            else
            {
                // this is to sync me and my real physical space
                if (pivot1 != null && A)
                {
                    if (pivot1.transform.childCount > 0)
                    {
                        //try
                        //{
                        //    target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot1.transform.GetChild(2).position;
                        //    target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot1.transform.GetChild(2).rotation;

                        //}
                        //catch { }
                        try //  DEMO를 위해 수정; 아래는 원본 코드
                        {
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.position = pivot1.transform.GetChild(2).position;
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.rotation = pivot1.transform.GetChild(2).rotation;
                        }
                        catch { }

                    }
                }
                if (pivot2 != null && A)
                {

                    if (pivot2.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.GetChild(2).position;
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.GetChild(2).rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.GetChild(1).rotation;
                    }
                }

                if (pivot1 != null && B)
                {
                    if (pivot1.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.GetChild(2).position;
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.GetChild(2).rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.GetChild(1).rotation;
                    }
                }
                if (pivot2 != null && B)
                {
                    if (pivot2.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.GetChild(2).position;
                            target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.GetChild(2).rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.position = pivot2.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(2).gameObject.transform.rotation = pivot2.transform.GetChild(1).rotation;
                    }
                }

                if (pivot1 != null && C)
                {
                    if (pivot1.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.GetChild(2).position;
                            target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.GetChild(2).rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.position = pivot1.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(0).gameObject.transform.rotation = pivot1.transform.GetChild(1).rotation;
                    }
                }
                if (pivot2 != null && C)
                {
                    if (pivot2.transform.childCount > 0)
                    {
                        try
                        {
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.position = pivot2.transform.GetChild(2).position;
                            target.transform.GetChild(0).GetChild(1).gameObject.transform.rotation = pivot2.transform.GetChild(2).rotation;
                        }
                        catch { }
                        //target.transform.GetChild(0).GetChild(1).gameObject.transform.position = pivot2.transform.GetChild(1).position;
                        //target.transform.GetChild(0).GetChild(1).gameObject.transform.rotation = pivot2.transform.GetChild(1).rotation;
                    }
                }
            }
            

            updateavatar();
            checkAvatartandUpdate();

        }

    }



    void checkAvatartandUpdate()
    {
        if (type == 0)
        {
            GameObject go = null;
            try
            {
                go = GameObject.Find("LocalAvatar");
                if(go.GetComponent<AvatarBehaviourNGO>().LocalAvatarIndex != AvatarIndexForUser[type])
                    go.GetComponent<AvatarBehaviourNGO>().LocalAvatarIndex = AvatarIndexForUser[type];

                if (pivot1 != null)
                {
                    if(pivot1.GetComponent<AvatarBehaviourNGO>().LocalAvatarIndex != AvatarIndexForUser[1])
                    pivot1.GetComponent<AvatarBehaviourNGO>().LocalAvatarIndex = AvatarIndexForUser[1];
                }

                if (pivot2 != null)
                {
                    if (pivot2.GetComponent<AvatarBehaviourNGO>().LocalAvatarIndex != AvatarIndexForUser[2])
                        pivot2.GetComponent<AvatarBehaviourNGO>().LocalAvatarIndex = AvatarIndexForUser[2];
                }
            }
            catch { }
        }
    }


    void updateavatar()
    {
        GameObject go = null;
        try
        {
            go = GameObject.Find("RemoteAvatar");
            if (pivot1 == null)
            {
                pivot1 = go;
                go.name += "1";
            }
            else if (pivot2 == null)
            {
                pivot2 = go;
            }
        }
        catch { }


    }

    public void setPssThroughAll()
    {
        setPassthrogh(0);
        setPassthrogh(1);
        setPassthrogh(2);
    }
    void setPassthrogh(int type)
    {
        // aug object들은 visualize 시키자


        GameObject go = houses.transform.GetChild(type).gameObject;
        var objects = go.transform.FindChildRecursive("Object");
        foreach (var renderer in objects.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }
        var walls = go.transform.FindChildRecursive("Walls");
        foreach (var renderer in walls.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }

        
    }

    public bool isAllUserConnected()
    {
        if (pivot1 != null && pivot2 != null)
        {
            return true;
        }
        else
            return false;
    }

    public void activateAugObjs()
    {
        GameObject continueRender = GameObject.Find("augmentedObjs");
        //Debug.Log(continueRender.name);

        
        var augobjs = continueRender.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in augobjs)
        {
            renderer.enabled = true;
        }

    }


    public bool IsHeadsetOff()
    {
        return cameraRig.transform.GetChild(0).transform.localPosition == Vector3.zero;
    }


    // Add this method to SceneSelection
    public void ApplyRoomMovement(Vector3 movement)
    {
        if (IsDemoModeAndThisIsServer == true)
        {
            GameObject room = arrangeMRHouse.GetComponent<Arrange_Walkin>().houses[1];
            room.transform.position -= movement;
        }
        else
        {
            GameObject room = arrangeMRHouse.GetComponent<Arrange_Walkin>().houses[0];
            //Debug.Log("-------------------------" + movement.ToString());
            room.transform.position -= movement;
        }
    }



}



