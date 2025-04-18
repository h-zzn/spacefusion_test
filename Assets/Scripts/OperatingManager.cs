using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Netcode;
using Unity.Netcode.Components;
using System;
using Unity.VisualScripting;
using Unity.Services.Qos.V2.Models;

public class OperatingManager : MonoBehaviour
{


    [SerializeField] GameObject tower;
    TowerManager towerMGR;
    [SerializeField] AvatarCullingModule AvtCullingModule;
    [SerializeField] SceneSelection sceneSelection;
    bool isStartTask2 = false;
    bool isBoxcullinginTask2 = false;
    [SerializeField] GameObject avatarTarget;

    [SerializeField] string path = "Assets/Record/";

    [Tooltip("TASK2 시작 트리거")]
    [SerializeField] string key = "K";

    [Tooltip("Condition (1,3)")]
    [SerializeField] int condition = 1;
    //StreamWriter sw;
    float time = 0;
    float task2TimeScore = 0;

    //[SerializeField] bool isServer = false;
    // Start is called before the first frame update
    void Start()
    {
        towerMGR = GameObject.FindObjectOfType<TowerManager>();
        //{
        //    if (isServer)
        //    {
        //        NetworkManager.Singleton.StartServer();
        //    }
        if (condition == 1)
        {
            sceneSelection.setPssThroughAll();
            AvtCullingModule.isCondition1 = true;
        }

        // Create directory and file for recording
        Debug.Log("Find directory \"" + path + "\"");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log("The directory doesn't exist. Creating directory \"" + path + "\"");
        }
        string file = DateTime.Now.ToString("MM-dd-HH-mm") + ".txt";
        path += file;
        if (!File.Exists(path))
        {
            Debug.Log("Not Exists: " + path);
            File.CreateText(path).Close();
            Debug.Log("Created file \"" + file + "\"");
        }
    }

    void record()
    {
        if (File.Exists(path))
        {
            //Debug.Log("record!");
            StreamWriter sw = new StreamWriter(path, true);

            string taskState = isStartTask2 ? "Task2" : "Task1";
            sw.Write("Time : " + DateTime.Now.ToString("MM-dd-HH-mm-ss.f") + " //" + "TaskState : " + taskState + "//" + "User1 Position" + avatarTarget.transform.GetChild(0).GetChild(0).gameObject.transform.position.ToString() + "/"
                + "User2 Position" + avatarTarget.transform.GetChild(0).GetChild(1).gameObject.transform.position.ToString() + "/"
                + "User3 Position" + avatarTarget.transform.GetChild(0).GetChild(2).gameObject.transform.position.ToString() + "/"
                );
            if (task2TimeScore != 0)
            {
                sw.Write(" Task2 TimeScore :" + task2TimeScore);
            }
            sw.WriteLine("");

            sw.Flush();
            sw.Close();
        }
    }

    void setTowermanagerUserInfo()
    {
        if (sceneSelection.pivot1 != null && towerMGR.pivot1 == null)
            towerMGR.pivot1 = sceneSelection.pivot1;
        if (sceneSelection.pivot2 != null && towerMGR.pivot2 == null)
            towerMGR.pivot2 = sceneSelection.pivot2;
    }

    // Update is called once per frame
    void Update()
    {
        setTowermanagerUserInfo();
        time += Time.deltaTime;
        if (isStartTask2)
        {
            task2TimeScore += Time.deltaTime;
        }

        if (sceneSelection.isAllUserConnected() && time > 0.1f)
        {
            record();
            time = 0;
        }

        if (Input.GetKeyDown(KeyCode.K) && !isStartTask2)
        {


            if (condition == 1)
            {
                //towerMGR.GameStartWithNormalBOx();

                towerMGR.GameStart();


                foreach (var item in towerMGR.boxes)
                {
                    if (item.layer != sceneSelection.type + 11)
                    {
                        AvtCullingModule.setBoxes(item);
                    }
                }

            }
            else if (condition == 3)
            {
                //GameObject go = Instantiate(tower, tower.transform.position, tower.transform.rotation);
                //towerMGR.GetComponent<NetworkObject>().Spawn();
                //towerMGR = go.GetComponent<TowerManager>();
                towerMGR.gameObject.GetComponent<MeshRenderer>().enabled = true;
                towerMGR.GameStart();
            }

            isStartTask2 = true;
        }


        if (towerMGR.boxes.Count == 6 && !isBoxcullinginTask2)
        {
            foreach (var item in towerMGR.boxes)
            {
                AvtCullingModule.setBoxes(item);
            }
            isBoxcullinginTask2 = true;
        }
    }
}
