using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.UI;

public class TruthManager : MonoBehaviourPunCallbacks
{
    public static TruthManager Instance;

    [Header("Spawners")]
    public Transform[] ChurchSpawners;

    [Header("Text")]
    TMP_Text UpperText;
    TMP_Text MiddleText;

    [Header("Buttons")]
    public Button SkipButton;

    [Header("Game Info")]
    int maxPlayers;
    string currentState;
    string stateStatus;

    void Awake()
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

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        if (currentState != "Trust") { return; }
    }
}
