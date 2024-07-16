using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class RotateAroundObject : MonoBehaviourPunCallbacks
{
    public Transform targetObj;
    [SerializeField] float speed = 25f;

    public bool timerStarted = false;
    [SerializeField] public double totalTime = 10f;
    double startTime;
    double elapsedTime;
    double remainingTime;
    public int timer;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            Hashtable hashStartTime = new Hashtable() { { "StartTime", PhotonNetwork.Time } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashStartTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(targetObj.position, Vector3.up, speed * Time.deltaTime);
        if (!timerStarted) return;

        elapsedTime = PhotonNetwork.Time - startTime;
        remainingTime = totalTime - (elapsedTime % totalTime);
        timer = (int)remainingTime;

        transform.RotateAround(targetObj.position, Vector3.up, speed * Time.deltaTime);
        if (timer <= 5)
        {
            speed = 40f;
        }
        else if (timer <= 0)
        {
            timerStarted = false;
        }

        //transform.LookAt(targetObj);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        foreach (var prop in propertiesThatChanged)
        {
            switch (Convert.ToString(prop.Key))
            {
                case "StartTime":
                    startTime = (double)prop.Value;
                    timerStarted = true;
                    break;
            }
        }
    }
}
