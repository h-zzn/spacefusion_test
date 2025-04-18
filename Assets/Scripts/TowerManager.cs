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


public class TowerManager : NetworkBehaviour
{
    // Start is called before the first frame update
    [SerializeField] List<GameObject> spawnPoints = new List<GameObject>();
    [SerializeField] List<GameObject> Inside = new List<GameObject>();
    Arrange arrange;
    [SerializeField] GameObject table;
    [SerializeField] public List<GameObject> boxes;
    [Range(0, 1)]
    public float offset = 0.1f;
    float initialOffset;
    [SerializeField] List<GameObject> boxPrefab = new List<GameObject>();
    public float[] rules;

    [SerializeField] float testVelocity = 0;

    private NetworkVariable<bool> isUpdate = new NetworkVariable<bool>(false);
    private NetworkVariable<float> networkOffset = new NetworkVariable<float>(0.005f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] List<GameObject> SharedSpaceInside = new List<GameObject>();

    public GameObject pivot1;
    public GameObject pivot2;




    float timer = 0;

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

    // ------------------------- sending vector ------------------------- //


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InitializeSpawnPoints();
        //GameStart();


        networkOffset.OnValueChanged += OnOffsetChanged;
        serverRandomVector.OnValueChanged += OnServerVectorChanged;
    }

    private void InitializeSpawnPoints()
    {
        if (spawnPoints.Count == 0 || spawnPoints[0] == null)
        {
            GameObject[] gg = GameObject.FindGameObjectsWithTag("spawnpoint");
            spawnPoints.Clear();
            SharedSpaceInside.Clear();
            foreach (var item in gg)
            {
                spawnPoints.Add(item);
                if (item.name.Contains("2") || item.name.Contains("4") || item.name.Contains("6"))
                {
                    SharedSpaceInside.Add(item);
                }
            }
        }
    }

    [ServerRpc]
    public void GameStartServerRpc()
    {
        if (!IsServer) return;
        GameStart();
    }

    public void GameStart()
    {
        rules = new float[spawnPoints.Count];
        boxes = new List<GameObject>();
        //initialOffset = offset;
        float height = table.transform.position.y;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Vector3 spawnPosition = spawnPoints[i].transform.position + new Vector3(0, (boxPrefab[i].transform.localScale.y / 2), 0);
            GameObject go = Instantiate(boxPrefab[i], spawnPosition, Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();


            SetupBox(go, i);

            //float f = (boxPrefab.transform.localScale.y) + (offset * i);
            //go.transform.localScale = new Vector3(f, f, f);
            boxes.Add(go);

            height += (i == 0) ? boxPrefab[i].transform.localScale.y / 2 : ((boxPrefab[i - 1].transform.localScale.y / 2) + (boxPrefab[i].transform.localScale.y / 2));
            rules[i] = height;
        }

        isUpdate.Value = true;
        setTableVisibleClientRpc();
    }

    private void SetupBox(GameObject go, int index)
    {
        if (SharedSpaceInside.Contains(spawnPoints[index]))
        {
            go.layer = 0;
            go.tag = "Untagged";
            go.GetComponent<MeshRenderer>().material.color = Color.white;
            SetTagAndLayerClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId, "Untagged", 0, Color.white);
        }
        else
        {
            switch (spawnPoints[index].transform.parent.name)
            {
                case "house_0":
                    go.layer = 11;
                    go.tag = "house0";
                    go.GetComponent<MeshRenderer>().material.color = Color.blue;
                    SetTagAndLayerClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId, "house0", 11, Color.blue);
                    break;
                case "house_1":
                    go.layer = 12;
                    go.tag = "house1";
                    go.GetComponent<MeshRenderer>().material.color = Color.red;
                    if (pivot1 != null)
                    {
                        go.GetComponent<NetworkObject>().ChangeOwnership(pivot1.GetComponent<NetworkObject>().OwnerClientId);
                        //Debug.Log("Ownership is transfered to " + pivot1.GetComponent<NetworkObject>().OwnerClientId);
                    }
                    SetTagAndLayerClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId, "house1", 12, Color.red);
                    break;
                case "house_2":
                    go.layer = 13;
                    go.tag = "house2";
                    go.GetComponent<MeshRenderer>().material.color = Color.green;
                    if (pivot2 != null)
                    {
                        go.GetComponent<NetworkObject>().ChangeOwnership(pivot2.GetComponent<NetworkObject>().OwnerClientId);
                    }
                    SetTagAndLayerClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId, "house2", 13, Color.green);
                    break;
            }
        }
    }
    [ClientRpc]
    void setTableVisibleClientRpc()
    {
        if (IsClient)
        {
            if (gameObject.GetComponent<MeshRenderer>().enabled == false)
            {
                gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }


    [ClientRpc]
    void SetTagAndLayerClientRpc(ulong networkObjectId, string tag, int layer, Color c)
    {
        if (IsServer) return;
        NetworkObject networkObject;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject))
        {
            GameObject obj = networkObject.gameObject;
            obj.tag = tag;
            obj.layer = layer;
            obj.GetComponent<MeshRenderer>().material.color = c;
            boxes.Add(obj);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void checktowerRpc()
    {
        if (!CheckRule())
        {
            ReArrange();
        }
    }

    void Update()
    {

       

        //if (IsServer)
        //{
        //    timer += Time.deltaTime;
        //    if (timer >= 3 && Inside.Count > 0)
        //    {
        //        checktowerRpc();
        //        timer = 0;
        //    }
        //}
        //if (!IsServer) return;

        //if (boxes.Count == 6)
        //{
        //    //if (offset != initialOffset)
        //    //{
        //    //    networkOffset.Value = offset;
        //    //    initialOffset = offset;
        //    //}
        //    //UpdateScale();
        //    //if (!IsServer)
        //    //{
        //    //    CheckAndReArrange();
        //    //}
        //    {

        //        if (Inside.Count > 0)
        //        {
        //            float velocitysum = 0;
        //            foreach (var item in Inside)
        //            {
        //                if (item.GetComponent<Rigidbody>().isKinematic == true)
        //                {
        //                    velocitysum += 1;
        //                }
        //                velocitysum += item.GetComponent<Rigidbody>().velocity.magnitude;
        //            }
        //            testVelocity = velocitysum;
        //            if (velocitysum <= 0.01f)
        //            {
        //                if (!CheckRule())
        //                {
        //                    ReArrange();
        //                }
        //            }
        //        }

        //        foreach (var item in boxes)
        //        {
        //            //if ((item.GetComponentInChildren<TouchHandGrabInteractable>().State != InteractableState.Select) && (!Inside.Contains(item)) && (item.GetComponent<Rigidbody>().velocity.magnitude >= 0.01f))
        //            //{
        //                if ((item.GetComponentInChildren<GrabInteractable>().State != InteractableState.Select) && (!Inside.Contains(item)) && (item.GetComponent<Rigidbody>().velocity.magnitude >= 0.01f))
        //            {
        //                //if (!IsServer)
        //                //{
        //                //    item.GetComponent<NetworkObject>().RemoveOwnership();
        //                //}
        //                ReArrange(item);
        //            }
        //        }
        //    }

        //}
    }
    private void ReArrange()
    {
        foreach (var item in Inside)
        {
            int num = boxes.IndexOf(item);
            //Vector3 dir = (item.transform.position - spawnPoints[num].transform.position).normalized * 5;
            //dir.y = 0.5f;
            try { item.transform.position = spawnPoints[num].transform.position + new Vector3(0, (item.transform.localScale.y / 2), 0); }
            catch { }

        }
    }
    private void ReArrange(GameObject box)
    {
        int num = boxes.IndexOf(box);
        Vector3 goal = spawnPoints[num].transform.position;
        Vector3 dir = goal - box.transform.position;
        // box.GetComponent<Rigidbody>().AddForce(dir * 10);
        // Debug.Log("RE obj Mag:" + dir.magnitude.ToString());
    }

    //private void CheckAndReArrange()
    //{
    //    if (Inside.Count > 0 && IsAllBoxesStatic())
    //    {
    //        if (!CheckRule())
    //        {
    //            ReArrangeServerRpc();
    //        }
    //    }

    //    foreach (var item in boxes)
    //    {
    //        if ((item.GetComponentInChildren<TouchHandGrabInteractable>().State != InteractableState.Select) &&
    //            (!Inside.Contains(item)) &&
    //            (item.GetComponent<Rigidbody>().velocity.magnitude >= 0.01f))
    //        {
    //            ReArrangeServerRpc(item.GetComponent<NetworkObject>().NetworkObjectId);
    //        }
    //    }

    //    if (IsServer)
    //    {
    //        if (Inside.Count == boxes.Count)
    //        {
    //            isUpdate.Value = false;
    //        }
    //    }
    //}

    private bool IsAllBoxesStatic()
    {
        foreach (var item in Inside)
        {
            if (!item.GetComponent<Rigidbody>().isKinematic && item.GetComponent<Rigidbody>().velocity.magnitude > 0.01f)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckRule()
    {
        foreach (var item in Inside)
        {
            int num = boxes.IndexOf(item);
            if (Mathf.Abs(item.transform.position.y - rules[num]) > 0.01f)
                return false;
        }
        return true;
    }

    //[Rpc(SendTo.Server)]
    //private void ReArrangeServerRpc()
    //{
    //    foreach (var item in Inside)
    //    {
    //        int num = boxes.IndexOf(item);
    //        Vector3 dir = (item.transform.position - spawnPoints[num].transform.position).normalized * 5;
    //        dir.y = 0.5f;
    //        item.GetComponent<Rigidbody>().AddForce(dir * 30);
    //    }
    //}

    //[Rpc(SendTo.Server)]
    //private void ReArrangeServerRpc(ulong networkObjectId)
    //{
    //    NetworkObject networkObject;
    //    if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject))
    //    {
    //        GameObject box = networkObject.gameObject;
    //        int num = boxes.IndexOf(box);
    //        Vector3 goal = spawnPoints[num].transform.position;
    //        Vector3 dir = goal - box.transform.position;
    //        box.GetComponent<Rigidbody>().AddForce(dir * 10);
    //    }
    //}

    private void OnOffsetChanged(float previousValue, float newValue)
    {
        UpdateScale();
    }

    private void UpdateScale()
    {
        //for (int i = 0; i < boxes.Count; i++)
        //{
        //    float f = boxPrefab.transform.localScale.y + (networkOffset.Value * i);
        //    boxes[i].transform.localScale = new Vector3(f, f, f);
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Inside.Contains(other.gameObject))
        {
            if (other.CompareTag("house0") || other.CompareTag("house1") || other.CompareTag("house2"))
                Inside.Add(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!Inside.Contains(other.gameObject))
        {
            if (other.CompareTag("house0") || other.CompareTag("house1") || other.CompareTag("house2"))
                Inside.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Inside.Contains(other.gameObject))
        {
            Inside.Remove(other.gameObject);
        }
    }





    // callback method
    private void OnServerVectorChanged(Vector3Data previousValue, Vector3Data newValue)
    {
        Debug.Log($"NetworkVariable vector updated: {newValue.ToVector3()}");
        // Process the new vector value here
    }
    

}




