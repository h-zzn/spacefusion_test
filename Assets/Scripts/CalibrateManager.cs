using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus;
using Oculus.Interaction;

public class CalibrateManager : MonoBehaviour
{

    [SerializeField] GameObject RoomA, RoomB, RoomC;
    // Start is called before the first frame update
    GameObject selectedModel;
    //[SerializeField] GameObject pivotcube;
    [SerializeField] GameObject canvas2;
    [SerializeField] TextMeshProUGUI posx, posy, posz, roty;
    GameObject handlex, handley, handlez, handleq, handles;
    public GameObject rooms;
    Transform cubeorigin, worldorigin;
    public Transform getTransform() { return selectedModel.transform; }
    public void setTransform(Transform input)
    {
        selectedModel.transform.position = input.position;
        selectedModel.transform.rotation = input.rotation;
    }
    void Start()
    {
        //manager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        //cubeorigin = pivotcube.transform;
        worldorigin = rooms.transform;
        handlex = canvas2.transform.Find("adjustBar_X").GetChild(2).gameObject;
        handley = canvas2.transform.Find("adjustBar_Y").GetChild(2).gameObject;
        handlez = canvas2.transform.Find("adjustBar_Z").GetChild(2).gameObject;
        handles = canvas2.transform.Find("adjustBar_S").GetChild(2).gameObject;
        handleq = canvas2.transform.Find("adjustBar_Q").GetChild(2).GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {


        Vector3 pos = new Vector3(handlex.transform.localPosition.y * 30, handley.transform.localPosition.y * 5, handlez.transform.localPosition.y * 30);

        //Quaternion rot = pivotcube.transform.rotation;
        rooms.transform.position = pos;
        float angle = handleq.transform.localRotation.eulerAngles.x;
        //float scale = 1 + handles.transform.localPosition.y * 2;
        //rooms.transform.localScale = new Vector3(scale, scale, scale);


        rooms.transform.rotation = Quaternion.Euler(0, angle, 0);

        //rooms.transform.rotation = rot;
        //scale_f.text = "scale \n" + scale.ToString("F3");
        posx.text = pos.x.ToString("F3"); posy.text = pos.y.ToString("F3"); posz.text = pos.z.ToString("F3");
        roty.text = "angle \n" + angle.ToString("F3");

    }
    public void selectRoomA()
    {
       // manager.RoomType = "A";
        selectedModel = RoomA;

        RoomA.SetActive(true);
        RoomB.SetActive(false);
        RoomC.SetActive(false);
    }
    public void selectRoomB()
    {
       // manager.RoomType = "B";
        selectedModel = RoomB;

        RoomA.SetActive(false);
        RoomB.SetActive(true);
        RoomC.SetActive(false);
    }
    public void selectRoomC()
    {
       // manager.RoomType = "C";
        selectedModel = RoomC;

        RoomA.SetActive(false);
        RoomB.SetActive(false);
        RoomC.SetActive(true);
    }
}
