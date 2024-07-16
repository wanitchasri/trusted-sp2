using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ThreatManager : MonoBehaviourPunCallbacks
{
    public static ThreatManager Instance;

    [Header("Time Info")]
    int dayCount;
    int currentTimer;
    double totalTimer;
    double stateTime;

    double titleTime;
    [Header("Spawn Locations")]
    public Transform[] ReaperSpawners;
    public Transform[] CloneSpawnersForReapers;
    public Transform[] SinnerSpawners;
    public Transform[] CloneSpawnersForKnight;
    public Transform[] CloneSpawnersForShaman;
    public Transform[] CloneSpawnersForStalker;
    [SerializeField] public Transform MyThreatSpawner;

    [Header("Prefabs")]
    public GameObject[] ClonePrefabsForReapers;
    public GameObject[] ClonePrefabsForKnight;
    public GameObject[] ClonePrefabsForShaman;
    public GameObject[] ClonePrefabsForStalker;

    [Header("Text")]
    TMP_Text UpperText;
    TMP_Text MiddleText;
    string textToShow;

    [Header("Buttons")]
    public Button ReviveButton;

    [Header("Player Info")]
    [SerializeField] string myRole;
    [SerializeField] string myTeam;

    [Header("Game Info")]
    string currentState;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    { 

    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }

        //currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        //if (currentState != "Threat") { return; }

        //dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
        //myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        //myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

        //currentTimer = StatusManager.Instance.timer;
        //titleTime = StatusManager.Instance.titleTime;
        //stateTime = StatusManager.Instance.stateTime;
        //totalTimer = StatusManager.Instance.totalTime;

        //if (currentTimer <= (totalTimer - (titleTime + stateTime)))
        //{
        //    UpperText.text = "";
        //    UpperText.gameObject.SetActive(false);
        //}
        //else if (currentTimer <= (totalTimer - titleTime))
        //{
        //    ShowGuideText();
        //}
    }

    //void ShowGuideText()
    //{
    //    UpperText = StatusManager.Instance.UpperText;
    //    MiddleText = StatusManager.Instance.MiddleText;

    //    switch (myTeam)
    //    {
    //        case "Reaper":
    //            if (dayCount == 1) { textToShow = "Who's first?"; }
    //            else { textToShow = "Who's next?"; }
    //            break;

    //        case "Sinner":
    //            switch (myRole)
    //            {
    //                case "Butcher":
    //                    textToShow = "Cook your sinful dish!";
    //                    break;
    //                case "Druid":
    //                    textToShow = "Find your magic flower!";
    //                    break;
    //                case "Knight":
    //                    textToShow = "Protect someone tonight!";
    //                    break;
    //                case "Shaman":
    //                    textToShow = "Revive someone tonight!";
    //                    break;
    //                case "Stalker":
    //                    textToShow = "Who will you stalk tonight?";
    //                    break;
    //                case "Wizard":
    //                    textToShow = "Find your wand!";
    //                    break;
    //            }
    //            break;
    //    }
    //    UpperText.text = textToShow;
    //    UpperText.gameObject.SetActive(true);
    //}

}
