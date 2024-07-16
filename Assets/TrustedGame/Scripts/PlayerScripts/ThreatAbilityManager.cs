using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

public class ThreatAbilityManager : MonoBehaviourPunCallbacks
{
    public static ThreatAbilityManager Instance;

    [Header("Game Info")]
    int dayCount;
    string currentState;
    string stateStatus;
    private string[] roleAssignment;
    Dictionary<int, string> playerStatuses;
    string[] ThreatAbilitySinners = { "Knight", "Shaman", "Stalker" };
    string reaperAbility;

    [Header("Time Info")]
    bool timeIsUp;
    double totalTime;
    double titleTime;
    double stolenAbilityTime;
    double stolenShowTime;
    int timer;

    [Header("Player Info")]
    int myPlayerNumInGame;
    string myTeam;
    string myRole;
    bool IAmAlive;
    bool IAmClone;
    string myCloneRole;

    [Header("Local Vote Info")]
    string nameChecker;
    int playerNumberToVote;
    [SerializeField] private int givenVote;
    [SerializeField] private int gottenVote;
    int currentVote;
    string votedCloneName;
    public string votedPlayerName;

    [Header("Room Vote Info")]
    int voteRecordCount;
    Dictionary<int, int> getVotes;
    string voteResult;

    [Header("Vote Flags")]
    bool spawnedClones;
    [SerializeField] bool initializedVariables = false;
    [SerializeField] public bool IGaveVote;
    bool recordedMyVote;
    bool activatedAbility;
    bool abilityStolen;

    [Header("Message Flags")]
    bool alerted;
    bool sentMessage;
    bool alertedClones;
    bool sentVoteResult;
    string messageToDebug;

    [Header("UI")]
    TMP_Text AlertText;
    Button ButtonToShow;


    private void Awake()
    {
        Instance = this;
    }

    [PunRPC]
    public void InitThreatAbilityManager()
    {
        //if (!this.photonView.IsMine) { return; }
        dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        myPlayerNumInGame = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1;
        // Debug.Log("Initialize > AbilityManager [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "]");

        playerNumberToVote = 0;
        givenVote = 0;
        gottenVote = 0;
        currentVote = 0;
        votedCloneName = "";
        votedPlayerName = "";
        voteRecordCount = 0;

        IGaveVote = false;
        recordedMyVote = false;
        activatedAbility = false;
        sentVoteResult = false;

        reaperAbility = "";
        abilityStolen = false;

        alerted = false;
        sentMessage = false;
        alertedClones = false;
    }

    void CheckForClone()
    {
        if (!this.photonView.IsMine) { return; }
        if (this.gameObject.name.Contains("CloneFor"))
        {
            IAmClone = true;
            myCloneRole = this.gameObject.name.Substring(0, this.gameObject.name.Length - 2);
            myCloneRole = myCloneRole.Replace("CloneFor", "");
            nameChecker = "CloneFor" + myCloneRole;

            myPlayerNumInGame = GetCloneNumber(this.gameObject.name);
        }
        else if (!this.gameObject.name.Contains("CloneFor"))
        {
            IAmClone = false;
            myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
            nameChecker = "CloneFor" + myRole;
            myPlayerNumInGame = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1;
        }
    }

    int GetCloneNumber(string cloneName)
    {
        string getPlayerNum = cloneName.Replace(nameChecker + " ", "");
        int convertedNum = 0;
        if (int.TryParse(getPlayerNum, out convertedNum))
        {
            myPlayerNumInGame = convertedNum;
        }
        return convertedNum;
    }

    void Update()
    {
        if (!this.photonView.IsMine) { return; }
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"] != "Alive") { IAmAlive = false; } else { IAmAlive = true; }

        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"] != null)
        {
            dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
            currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
            stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
            playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

            switch (currentState)
            {
                case "Threat":
                    CheckForClone();
                    break;
                case "Trust":
                    return;
                case "Truth":
                    nameChecker = "Player";
                    break;
            }
            gottenVote = this.gameObject.GetComponent<VoteManager>().gottenVote;
        }

        reaperAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperAbility"];
        //Debug.Log("IAmAlive="+IAmAlive);
        switch (stateStatus)
        {
            case "MovedPlayersToTheirPosition":
            case "MovedPlayersToChurch":
                // I fix this!!
                //if (currentState == "Threat"
                //&& (reaperAbility != null)
                //&& ThreatAbilitySinners.Contains(reaperAbility)
                //&& reaperAbility != "Knight")
                if (currentState == "Threat"
                && reaperAbility == "NotNow!")
                {
                    if (!alertedClones)
                    {
                        alertedClones = true;
                        if (PhotonNetwork.IsMasterClient)
                        {
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperAbility", reaperAbility } });
                        }
                    }
                    abilityStolen = true;

                    if (myRole == "Reaper"
                    || myCloneRole == "Reaper")
                    {
                        switch (reaperAbility)
                        {
                            case "Shaman":
                                if (myRole == "Reaper" && !alerted)
                                {
                                    alerted = true;
                                    StartCoroutine(PlayerIconManager.Instance.Alert("Middle", "Shaman Ability Activated!", 3f));
                                }
                                // Case : no one to revive
                                roleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
                                playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
                                bool canRevive = false;
                                for (int i = 0; i < roleAssignment.Length; i++)
                                {
                                    if (roleAssignment[i] == "Reaper")
                                    {
                                        if (playerStatuses[i + 1] != "Alive")
                                        {
                                            canRevive = true;
                                        }
                                    }
                                }
                                if (canRevive)
                                {
                                    StartCoroutine(PlayerIconManager.Instance.Alert("Upper", "You've got Shaman's ability! \nRevive your friend to help with the hunt!", 5f));
                                    ButtonToShow = ThreatManager.Instance.ReviveButton;
                                    ButtonToShow.transform.Find("ButtonText").GetComponent<TMP_Text>().text = "Click to Revive";
                                    ButtonToShow.gameObject.SetActive(true);
                                }
                                else
                                {
                                    StartCoroutine(PlayerIconManager.Instance.Alert("Upper", "You've got Shaman's ability! \nToo bad your friend is alive...", 3f));
                                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "ActivatedReaperAbility" } });
                                }
                                break;

                            case "Stalker":
                                ManageVote(nameChecker);
                                break;
                        }
                    }
                    else if (myRole != "Reaper")
                    {
                        StatusManager.Instance.TitlePanel.gameObject.SetActive(true);
                        StatusManager.Instance.TitleMessage.text = "Tonight is going to be a long....long...night";
                        StatusManager.Instance.TitleMessage.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (ThreatAbilitySinners.Contains(myRole)
                    || ThreatAbilitySinners.Contains(myCloneRole)
                    || myRole == "Reaper"
                    || myCloneRole == "Reaper"
                    || currentState == "Truth")
                    {
                        ManageVote(nameChecker);
                        if (myRole != "Reaper" && !alerted)
                        {
                            alerted = true;
                            StartCoroutine(PlayerIconManager.Instance.Alert("Middle", myRole + " Ability Activated!", 3f));
                        }
                    }

                    if (currentState == "Truth"
                    && reaperAbility == "Knight")
                    {
                        ButtonToShow = TruthManager.Instance.SkipButton;
                        ButtonToShow.transform.Find("ButtonText").GetComponent<TMP_Text>().text = "Click to Skip";
                        ButtonToShow.gameObject.SetActive(true);
                    }
                }
                break;

            // I add this!!
            case "ActivatingReaperAbility":
                if (myRole == "Reaper"
                && !activatedAbility)
                {
                    switch (reaperAbility)
                    {
                        //case "Knight":
                        //    activatedAbility = true;
                        //    break;

                        //case "Shaman":
                        //    activatedAbility = true;
                        //    RevivePlayer();
                        //    break;

                        case "Stalker":
                            if (myRole == "Reaper" && !alerted)
                            {
                                alerted = true;
                                StartCoroutine(PlayerIconManager.Instance.Alert("Middle", "Stalker Ability Activated!", 3f));
                            }
                            activatedAbility = true;
                            StalkPlayer(true);
                            break;

                        default:
                            Debug.Log("This ability cannot be used in this state!");
                            break;
                    }
                    sentMessage = false;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    timer = StatusManager.Instance.timer;
                    totalTime = StatusManager.Instance.totalTime;
                    titleTime = StatusManager.Instance.titleTime;
                    stolenAbilityTime = StatusManager.Instance.stolenAbilityTime;
                    stolenShowTime = StatusManager.Instance.stolenShowTime;
                    if (timer <= (totalTime - titleTime - stolenAbilityTime - stolenShowTime))
                    {
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "ActivatedReaperAbility" } });
                    }
                }
                break;

            case "ActivatedReaperAbility":
                if (abilityStolen && myRole == "Reaper"
                && !sentMessage) 
                { 
                    if (reaperAbility == "Stalker") { StalkPlayer(false); } 
                    
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperAbility", "" } });

                    ButtonToShow = ThreatManager.Instance.ReviveButton;
                    ButtonToShow.transform.Find("ButtonText").GetComponent<TMP_Text>().text = "";
                    ButtonToShow.gameObject.SetActive(false);
                }

                abilityStolen = false;
                if (ThreatAbilitySinners.Contains(myRole)
                    || ThreatAbilitySinners.Contains(myCloneRole)
                    || myRole == "Reaper"
                    || myCloneRole == "Reaper"
                    || currentState == "Truth")
                {
                    if (myRole != "Reaper" && !sentMessage)
                    {
                        sentMessage = true;
                        StatusManager.Instance.TitlePanel.gameObject.SetActive(false);
                        StatusManager.Instance.TitleMessage.text = "";
                        StatusManager.Instance.TitleMessage.gameObject.SetActive(false);
                    }

                    ManageVote(nameChecker);
                }
                break;

            case "FinishedVotes":
                if (IAmClone) { return; }

                //voteRecordCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["VoteRecordCount"];
                //if ((playerNumberToVote == 0)
                //&& (voteRecordCount == (myPlayerNumInGame - 1))
                //&& (!recordedMyVote))
                //{
                //    recordedMyVote = true;
                //    RecordVote();
                //}

                if (currentState == "Threat"
                && ThreatAbilitySinners.Contains(myRole)
                && !activatedAbility)
                {
                    //this.photonView.RPC("LetsDebug", RpcTarget.All, this.gameObject.name + " votedPlayerName:" + votedPlayerName + " activatedAbility:" + activatedAbility + " myRole:" + myRole + " ");
                    StartCoroutine(PlayerIconManager.Instance.Alert("Middle", myRole + " Ability Activated!", 3f));
                    switch (myRole)
                    {
                        case "Knight":
                            activatedAbility = true;
                            ProtectPlayer();
                            break;

                        case "Shaman":
                            activatedAbility = true;
                            RevivePlayer();
                            break;

                        case "Stalker":
                            activatedAbility = true;
                            StalkPlayer(true);
                            break;
                    }
                }
                break;
        }
    }
    void ManageVote(string nameChecker)
    {
        //messageToDebug = ("ManageVote() : [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "]" + " timeIsUp =" + timeIsUp + " ,myTeam=" + myTeam + " ,givenVote=" + givenVote + " ,gottenVote=" + gottenVote + ", IAmAlive" + IAmAlive + "=" + (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"] + " ,IAmClone" + IAmClone);
        //thisPhotonView.RPC("LetsDebug", RpcTarget.All, messageToDebug);

        // I add this!!
        if (!abilityStolen) { timeIsUp = StatusManager.Instance.timeIsUp; Debug.Log("timeIsUp"+timeIsUp); }
        else if (abilityStolen)
        {
            timer = StatusManager.Instance.timer;
            totalTime = StatusManager.Instance.totalTime;
            titleTime = StatusManager.Instance.titleTime;
            stolenAbilityTime = StatusManager.Instance.stolenAbilityTime;
            if (timer <= (totalTime - titleTime - stolenAbilityTime))  // if timer <= (50 - 5 - 10)
            {
                timeIsUp = true;
            }
            else
            {
                timeIsUp = false;
            }
        }

        if (!timeIsUp)
        {
            if (!IAmAlive) { return; }

            if (!IAmClone)
            {
                //this.photonView.RPC("LetsDebug", RpcTarget.All, "!IAmClone " + " Name:" + this.gameObject.name + " NameChecker:" + nameChecker + " PlayerNum:" + myPlayerNumInGame + " Role:" + myRole);
                CheckForVote(nameChecker);
                if (givenVote != 0)
                {
                    int voteToUpdate = givenVote;
                    givenVote = 0;
                    this.photonView.RPC("UpdateVoteToPlayer", RpcTarget.AllViaServer, votedCloneName, nameChecker, voteToUpdate);
                    votedPlayerName = votedCloneName.Replace(nameChecker, "Player");
                    if (currentState == "Threat" 
                    && nameChecker.Contains("Reaper")
                    && !abilityStolen)
                    {
                        this.photonView.RPC("UpdateVoteToPlayer", RpcTarget.AllViaServer, votedPlayerName, "Player", voteToUpdate);
                    }
                }
            }

            if (IAmClone || currentState == "Truth")
            {
                //this.photonView.RPC("LetsDebug", RpcTarget.All, "IAmClone " + " Name:" + this.gameObject.name + " NameChecker:" + nameChecker + " PlayerNum:" + myPlayerNumInGame + " CloneRole:" + myCloneRole);
                if (gottenVote != currentVote)
                {
                    //this.photonView.RPC("LetsDebug", RpcTarget.All, nameChecker + myPlayerNumInGame + " I got a vote.");
                    currentVote = gottenVote;
                    this.photonView.RPC("ShowVote", RpcTarget.AllViaServer, nameChecker);
                }
            }
        }
        else if (timeIsUp)
        {
            if (IAmClone) { return; }
            this.photonView.RPC("LetsDebug", RpcTarget.All, PhotonNetwork.IsMasterClient + " Time's up " + sentMessage + " " + abilityStolen);
            if (!sentMessage)
            {
                sentMessage = true;
                if (!abilityStolen)
                {
                    if (stateStatus != "FinishedVotes")
                    {
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "FinishedVotes" } });
                    }
                }
                else
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "ActivatingReaperAbility" } });
                }
            }
        }
    }

    void CheckForVote(string nameChecker)
    {
        if (!IAmAlive) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit)) 
            {
                if (currentState == "Truth" && Hit.collider.gameObject.name == this.gameObject.name) { StartCoroutine(PlayerIconManager.Instance.Alert("Upper", "Are you going to kill yourself?", 2f)); }

                if (Hit.collider.gameObject.name.Contains(nameChecker)
                && Hit.collider.gameObject.name != this.gameObject.name)
                {
                    string playerStatus = VoteManager.Instance.GetPlayerStatus(Hit.collider.gameObject.name, nameChecker);
                    bool canVote = false;
                    if (currentState == "Threat")
                    {
                        // if (myRole.Contains("Shaman"))
                        if ((myRole == "Shaman")
                        || ((myRole == "Reaper" && reaperAbility == "Shaman")))
                        {
                            if (playerStatus.Contains("Soul"))
                            {
                                canVote = true;
                            }
                        }
                        else
                        {
                            if (playerStatus == "Alive") { canVote = true; }
                        }
                    }
                    else
                    {
                        if (playerStatus == "Alive") { canVote = true; }
                        else if (playerStatus != "Alive" && currentState == "Truth") { StartCoroutine(PlayerIconManager.Instance.Alert("Upper", "Souls cannot be chosen..", 2f)); }
                    }

                    if (canVote)
                    {
                        string votingCloneName = Hit.collider.gameObject.name;
                        // Debug.Log("BEFORE - votedCloneName" + votedCloneName + " votingCloneName" + votingCloneName + " IGaveVote" + IGaveVote + "givenVote" + givenVote);

                        if (votingCloneName == votedCloneName
                        || votedCloneName == "")
                        {
                            switch (IGaveVote)
                            {
                                case (true):
                                    givenVote = -1;
                                    votedCloneName = "";
                                    break;
                                case (false):
                                    givenVote = 1;
                                    break;
                            }
                            votedCloneName = votingCloneName;
                            IGaveVote = !IGaveVote;
                        }
                        else if (votingCloneName != votedCloneName)
                        {
                            if (!IGaveVote)
                            {
                                givenVote = 1;
                                votedCloneName = votingCloneName;
                                IGaveVote = !IGaveVote;
                            }
                        }

                        messageToDebug = "(1) CheckForVote [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "] : IGaveVote=" + IGaveVote + ", givenVote=" + givenVote + ", votedCloneName=" + votedCloneName;
                        //Debug.Log(messageToDebug);
                        this.photonView.RPC("LetsDebug", RpcTarget.All, messageToDebug);
                    }

                    if (!canVote && myRole == "Shaman") { Debug.Log("Shaman cannot vote a human!"); }
                }
            }
        }

    }

    void RecordVote()
    {
        if (this.gameObject.name.Contains("CloneFor")) { return; }

        int voteToRecord = 0;
        // Check alive in case for disconnected players too
        if (IAmAlive)
        {
            voteToRecord = gottenVote;
            //playerNumberToVote = myPlayerNumInGame;
            //getVotes[playerNumberToVote] = gottenVote;
            //PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "Votes", getVotes } });
            messageToDebug = "(4) RecordVote [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "] : playerNumberToVote=" + playerNumberToVote + ", gottenVote=" + gottenVote;
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "VoteRecord" + myPlayerNumInGame, voteToRecord } });
        
        //this.photonView.RPC("AnnounceRecordVote", RpcTarget.AllViaServer);
        this.photonView.RPC("LetsDebug", RpcTarget.All, messageToDebug);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (!this.photonView.IsMine || this.gameObject.name.Contains("CloneFor")) { return; }

        myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        //this.photonView.RPC("LetsDebug", RpcTarget.All, "myRole=" + myRole + " contains=" + ThreatAbilitySinners.Contains(myRole) + " spawned=" + !spawnedClones);

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        foreach (var prop in propertiesThatChanged)
        {
            switch (Convert.ToString(prop.Key))
            {
                //case "CloneStatus":
                //    string cloneStatus = Convert.ToString(prop.Value);
                //    switch (cloneStatus)
                //    {
                //        case "SpawnedClonesForReaper":
                //            switch (myRole)
                //            {
                //                case "Knight":
                //                case "Shaman":
                //                case "Stalker":
                //                    if (!spawnedClones)
                //                    {
                //                        spawnedClones = true;
                //                        this.photonView.RPC("SpawnClonesForVote", RpcTarget.AllViaServer, myRole);
                //                    }
                //                    break;
                //            }
                //            break;
                //    }
                //    break;

                case "StateStatus":
                    stateStatus = Convert.ToString(prop.Value);
                    if (stateStatus == "BegunThreat" || stateStatus == "BegunTruth")
                    {
                        if (!initializedVariables)
                        {
                            initializedVariables = true;
                            this.photonView.RPC("InitThreatAbilityManager", RpcTarget.AllViaServer);
                        }
                    }
                    else if (stateStatus.Contains("Ended"))
                    {
                        spawnedClones = false;
                        initializedVariables = false;
                    }

                    switch (stateStatus)
                    {
                        case "BegunThreat":
                            abilityStolen = false;
                            break;

                        case "EndedThreat":
                            if (myRole == "Stalker") { StalkPlayer(false); }
                            break;

                        case "FinishedVotes":
                            if (!recordedMyVote)
                            {
                                recordedMyVote = true;
                                RecordVote();
                                if (PhotonNetwork.IsMasterClient)
                                {
                                    playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
                                    for (int i = 0; i<playerStatuses.Count; i++) 
                                    { 
                                        if (playerStatuses[i + 1] != null)
                                        {
                                            if (playerStatuses[i + 1] == "LeftRoom")
                                            {
                                                VoteManager.Instance.voteRecordCount++;
                                            }
                                        }
                                    }
                                }
                            }
                            break;

                        case "GotVoteResult":
                            if (PhotonNetwork.IsMasterClient
                            && !sentVoteResult)
                            {
                                voteResult = (string)PhotonNetwork.CurrentRoom.CustomProperties["VoteResult"];
                                if (voteResult == "NoVoteFound")
                                {
                                    sentVoteResult = true;
                                    // Show Panel "NoVoteFound"
                                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "NoVoteFound" } });
                                }
                                else if (voteResult == "Draw")
                                {
                                    sentVoteResult = true;
                                    // Show Panel "Draw"
                                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "Draw" } });
                                }
                            }
                            break;

                        case "ActivatedReaperAbility":
                            this.photonView.RPC("InitThreatAbilityManager", RpcTarget.AllViaServer);
                            break;
                    }
                    break;

                case "Votes":
                    getVotes = (Dictionary<int, int>)prop.Value;
                    break;

                case "PlayerStatuses":
                    playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
                    if (playerStatuses[PhotonNetwork.LocalPlayer.GetPlayerNumber()+1] == "Revived")
                    {
                        string myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Revived" + myStatus } });
                    }
                    break;
            }
        }
    }

    void StalkPlayer(bool isActive)
    {
        if (votedCloneName != "")
        {
            int cloneNumber = GetCloneNumber(votedCloneName);
            if (cloneNumber == 0) { cloneNumber = -1; }
            string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];

            //string votedPlayerName = votedCloneName.Replace("nameChecker", "Player");
            //GameObject playerObj = GameObject.Find(votedPlayerName);
            string roleToShow = getRoleAssignment[cloneNumber - 1];
            PlayerIconManager.Instance.ShowRoleForStalker(roleToShow, isActive);
        }
        else
        {
            // Alert something..
        }
    }

    void ProtectPlayer()
    {
        int cloneNumber = -1;
        if (IGaveVote)
        {
            cloneNumber = GetCloneNumber(votedCloneName);
            this.photonView.RPC("LetsDebug", RpcTarget.All, "Player " + cloneNumber + " is protected!");
        }
        else
        {
            this.photonView.RPC("LetsDebug", RpcTarget.All, "No one is protected!");
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ProtectedPlayer", cloneNumber } });
        // Also Alert to local player
    }

    void RevivePlayer()
    {
        int cloneNumber = -1;
        playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
        if (IGaveVote)
        {
            cloneNumber = GetCloneNumber(votedCloneName);
            // If this is target, cannot revive
            playerStatuses[cloneNumber] = "Revived";
            this.photonView.RPC("LetsDebug", RpcTarget.All, "Player" + cloneNumber + " is revived!");
        }
        else
        {
            this.photonView.RPC("LetsDebug", RpcTarget.All, "No one is revived!");
        }
        // Also change for local player
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerStatuses", playerStatuses } });
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (!targetPlayer.IsLocal) return;

        foreach (var prop in changedProps)
        {
            switch (Convert.ToString(prop.Value))
            {
                case "MovedPlayersToTheirPosition":
                    if (!spawnedClones)
                    {
                        spawnedClones = true;
                        if (PhotonNetwork.IsMasterClient)
                        {
                            this.photonView.RPC("SpawnClonesForVote", RpcTarget.AllViaServer, "Reaper");
                        }
                        switch (myRole)
                        {
                            case "Knight":
                            case "Shaman":
                            case "Stalker":
                                string playerStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
                                if (playerStatus == "Alive")
                                {
                                    this.photonView.RPC("SpawnClonesForVote", RpcTarget.AllViaServer, myRole);
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.givenVote);
            stream.SendNext(this.IGaveVote);
            stream.SendNext(this.votedCloneName);
        }
        else
        {
            // Network player, receive data
            this.givenVote = (int)stream.ReceiveNext();
            this.IGaveVote = (bool)stream.ReceiveNext();
            this.votedCloneName = (string)stream.ReceiveNext();
        }
    }
}
