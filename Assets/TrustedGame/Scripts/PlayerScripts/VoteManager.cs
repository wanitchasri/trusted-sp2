using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;
using Photon.Realtime;

public class VoteManager : MonoBehaviourPunCallbacks
{
    public static VoteManager Instance;

    [Header("Game Info")]
    int dayCount;
    string currentState;
    string stateStatus;
    private int maxPlayers;

    [Header("Player Info")]
    int myPlayerNumInGame;
    string myRole;
    Dictionary<int, string> playerStatuses;
    string[] playerRoles;
    Dictionary<int, string> sinnerRoles;

    [Header("Clone Info")]
    GameObject[] clonePrefabs;

    [Header("Vote Info")]
    public TMP_Text VoteText;
    public GameObject ChosenIcon;
    [SerializeField] public int gottenVote;
    [SerializeField] private bool IGotVote;
    public int voteRecordCount;
    private Dictionary<int, int> getVotes;

    [Header("Vote Result")]
    string voteResult;
    int protectedPlayer;
    Dictionary<string, string> targetSoulInfo;
    int targetSoulNum;

    [Header("Flags")]
    bool initializedVariables;
    bool movedPlayers;
    bool foundTarget;
    bool gotVoteResult;
    bool gotProtectedPlayer;
    bool gotTargetSoulInfo;
    bool handledTarget;
    public bool killedTarget;

    string messageToDebug;

    private void Awake()
    {
        Instance = this;
        VoteText = VoteText.GetComponent<TMP_Text>();
    }

    #region Starting the State

    [PunRPC]
    public void InitVoteManager()
    {
        //if (!this.photonView.IsMine) { return; }
        dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        myPlayerNumInGame = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1;
        // Debug.Log("Initialize > VoteManager [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "]");

        // Voting Phase
        InitVotes();
        gottenVote = 0;
        IGotVote = false;
        voteRecordCount = 0;

        // Recording Phase
        voteResult = "";
        protectedPlayer = 0;
        targetSoulNum = 0;

        // Target Flags
        foundTarget = false;
        gotVoteResult = false;
        gotProtectedPlayer = false;
        gotTargetSoulInfo = false;
        handledTarget = false;
        killedTarget = false;

        //if (PhotonNetwork.IsMasterClient)
        //{
        //    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "InitializedVariables" } });
        //}
    }

    void InitVotes()
    {
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        //Debug.Log("CURRENTSTATE" + currentState);
        if (currentState != "Threat" && currentState != "Truth") { return; }

        if (PhotonNetwork.IsMasterClient)
        {
            // Debug.Log("MASTER");
            maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];

            Dictionary<int, int> voteDictionary = new Dictionary<int, int>();
            for (int i = 1; i <= maxPlayers; i++)
            {
                voteDictionary.Add(i, 0);
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "Votes", voteDictionary } });
            // PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "VoteRecordCount", 0 } });

            Dictionary<string, string> targetSoulInfo = new Dictionary<string, string>();
            targetSoulInfo.Add("PlayerNumber", "");
            targetSoulInfo.Add("NickName", "");
            targetSoulInfo.Add("Color", "");
            targetSoulInfo.Add("Role", "");
            targetSoulInfo.Add("Team", "");
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "TargetSoulInfo", targetSoulInfo } });
        }
    }

    [PunRPC]
    void ResetVoteText()
    {
        // Debug.Log("ResetVoteText");
        gottenVote = 0;
        VoteText.text = "";
        VoteText.gameObject.SetActive(false);
    }
    //bool IsLastSpawnedClone()
    //{
    //    int maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];
    //    string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
    //    if (getRoleAssignment[maxPlayers - 1] == "Stalker")
    //    {
    //        maxPlayers -= 1;
    //    }
    //    if (this.gameObject.name.Contains("Stalker")
    //        && this.gameObject.name.Contains(Convert.ToString(maxPlayers)))
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    [PunRPC]
    public void MovePlayersToTheirPosition()
    {
        if (!this.photonView.IsMine) { return; }
        if (this.gameObject.name.Contains("CloneFor")) { return; }

        //string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
        Transform[] ReaperSpawners = ThreatManager.Instance.ReaperSpawners;
        Transform[] SinnerSpawners = ThreatManager.Instance.SinnerSpawners;

        this.gameObject.GetComponent<CharacterController>().enabled = false;

        myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        Transform MyThreatSpawner = myRole switch
        {
            "Reaper" => ReaperSpawners[GetReaperID(PhotonNetwork.LocalPlayer.GetPlayerNumber())],
            _ => SinnerSpawners[GetSinnerID(myRole)],
        };

        // this.photonView.RPC("LetsDebug", RpcTarget.All, this.gameObject.name + "MySpawner=" + MyThreatSpawner.name + this.gameObject.GetComponent<CharacterController>().enabled);
        this.gameObject.transform.position = MyThreatSpawner.position;
        this.gameObject.transform.rotation = MyThreatSpawner.rotation;

        this.gameObject.GetComponent<CharacterController>().enabled = true;

        //for (int roleIndex = 0; roleIndex < getRoleAssignment.Length; roleIndex++)
        //{
        //    if (PhotonNetwork.LocalPlayer.GetPlayerNumber() == (roleIndex))
        //    {
        //        this.gameObject.GetComponent<CharacterController>().enabled = false;

        //        string role = getRoleAssignment[roleIndex];


        //        if (role == "Reaper")
        //        {
        //            List<int> reaperIndex = new List<int>();
        //            int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
        //            for (int i = 0; i < getRoleAssignment.Length; i++)
        //            {
        //                if (getRoleAssignment[i] == role)
        //                {
        //                    reaperIndex.Add(i);
        //                }
        //            }

        //            for (int j = 0; j < reaperIndex.Count; j++)
        //            {
        //                if (playerNumber == reaperIndex[j])
        //                {
        //                    this.gameObject.transform.position = ReaperSpawners[j].position;
        //                    this.gameObject.transform.rotation = ReaperSpawners[j].rotation;
        //                }
        //            }
        //            this.gameObject.GetComponent<CharacterController>().enabled = false;
        //        }
        //        else
        //        {
        //            int sinnerIndex = 0;
        //            switch (role)
        //            {

        //                case "Butcher":
        //                    sinnerIndex = 0;
        //                    break;
        //                case "Druid":
        //                    sinnerIndex = 1;
        //                    break;
        //                case "Knight":
        //                    sinnerIndex = 2;
        //                    break;
        //                case "Shaman":
        //                    sinnerIndex = 3;
        //                    break;
        //                case "Stalker":
        //                    sinnerIndex = 4;
        //                    break;
        //                case "Wizard":
        //                    sinnerIndex = 5;
        //                    break;
        //            }

        //            this.gameObject.transform.position = SinnerSpawners[sinnerIndex].position;
        //            this.gameObject.transform.rotation = SinnerSpawners[sinnerIndex].rotation;
        //            this.gameObject.GetComponent<CharacterController>().enabled = true;
        //        }
        //    }
        //}
        if (PhotonNetwork.IsMasterClient) { PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "MovedPlayersToTheirPosition" } }); }
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "StateStatus", "MovedPlayersToTheirPosition" } });
    }

    [PunRPC]
    void MovePlayersToBridge()
    {
        if (!this.photonView.IsMine) { return; }
        if (this.gameObject.name.Contains("CloneFor")) { return; }

        Transform[] SinnerSpawners = TrustManager.Instance.SinnerSpawners;
        Transform[] ReaperSpawners = TrustManager.Instance.ReaperSpawners;
        Transform TargetSpawner = TrustManager.Instance.TargetSpawner;

        string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
        string myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];

        for (int roleIndex = 0; roleIndex < getRoleAssignment.Length; roleIndex++)
        {
            if (PhotonNetwork.LocalPlayer.GetPlayerNumber() == (roleIndex))
            {
                this.gameObject.GetComponent<CharacterController>().enabled = false;

                if (myStatus == "TargetSoul")
                {
                    this.gameObject.transform.position = TargetSpawner.position;
                    this.gameObject.transform.rotation = TargetSpawner.rotation;
                }
                else
                {
                    string role = getRoleAssignment[roleIndex];
                    if (role == "Reaper")
                    {
                        List<int> reaperIndex = new List<int>();
                        int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
                        for (int i = 0; i < getRoleAssignment.Length; i++)
                        {
                            if (getRoleAssignment[i] == role)
                            {
                                reaperIndex.Add(i);
                            }
                        }

                        for (int j = 0; j < reaperIndex.Count; j++)
                        {
                            if (playerNumber == reaperIndex[j])
                            {
                                this.gameObject.transform.position = ReaperSpawners[j].position;
                                this.gameObject.transform.rotation = ReaperSpawners[j].rotation;
                            }
                        }
                    }
                    else
                    {

                        List<int> sinnerIndex = new List<int>();
                        int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
                        for (int i = 0; i < getRoleAssignment.Length; i++)
                        {
                            if (getRoleAssignment[i] != "Reaper")
                            {
                                sinnerIndex.Add(i);
                            }
                        }

                        if (sinnerIndex[sinnerIndex.Count - 1] == playerNumber)
                        {
                            targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
                            targetSoulNum = Convert.ToInt32(targetSoulInfo["PlayerNumber"]);
                            playerNumber = targetSoulNum - 1;
                        }
                        for (int j = 0; j < sinnerIndex.Count; j++)
                        {
                            if (playerNumber == sinnerIndex[j])
                            {
                                this.gameObject.transform.position = SinnerSpawners[j].position;
                                this.gameObject.transform.rotation = SinnerSpawners[j].rotation;
                            }
                        }
                    }
                }

            }
        }

        this.gameObject.GetComponent<CharacterController>().enabled = true;

        if (PhotonNetwork.IsMasterClient) { PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "MovedPlayersToBridge" } }); }
    }

    [PunRPC]
    void MovePlayersToMirror()
    {
        if (!this.photonView.IsMine) { return; }
        if (this.gameObject.name.Contains("CloneFor")) { return; }

        Transform MySpawner = TargetRevealer.Instance.MySpawner;
        GameObject[] HumanMeshes = TargetRevealer.Instance.HumanMeshes;
        TMP_Text TargetNameText = TargetRevealer.Instance.NameText;
        TMP_Text RoleText = TargetRevealer.Instance.RoleText;

        this.gameObject.GetComponent<CharacterController>().enabled = false;

        targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
        int convertedNum = -1;
        if (int.TryParse(targetSoulInfo["PlayerNumber"], out convertedNum))
        {
            targetSoulNum = convertedNum;
        }

        string myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        if ((string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"] == "TargetSoul")
        {
            Transform TargetSoulSpawner = TargetRevealer.Instance.TargetSoulSpawner;
            MySpawner = TargetSoulSpawner;
        }
        else
        {
            Transform[] ReaperSpawners = TargetRevealer.Instance.ReaperSpawners;
            Transform[] SinnerSpawners = TargetRevealer.Instance.SinnerSpawners;

            switch (myRole)
            {
                case "Reaper":
                    MySpawner = ReaperSpawners[GetReaperID(PhotonNetwork.LocalPlayer.GetPlayerNumber())];
                    break;

                default:
                    sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
                    int mySinnerID = GetSinnerID(myRole);
                    if (mySinnerID == sinnerRoles.Count - 1)
                    {
                        mySinnerID = GetSinnerID(targetSoulInfo["Role"]);
                    }
                    MySpawner = SinnerSpawners[mySinnerID];
                    break;
            }
        }

        transform.position = MySpawner.position;
        transform.rotation = MySpawner.rotation;


        int[] meshOrder = (int[])PhotonNetwork.CurrentRoom.CustomProperties["CharacterMeshOrder"];
        int myBodyID = meshOrder[targetSoulNum - 1];
        HumanMeshes[myBodyID].gameObject.SetActive(true);

        TargetNameText.text = targetSoulInfo["NickName"];
        RoleText.text = targetSoulInfo["Role"];

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "MovedPlayersToMirror" } });
        }
    }
    int GetReaperID(int playerNumber)
    {
        playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
        int reaperCount = 0;
        for (int i = 0; i < playerRoles.Length; i++)
        {
            if (playerRoles[i] == "Reaper")
            {
                if (i == playerNumber)
                {
                    return reaperCount;
                }
                reaperCount += 1;
            }
        }
        return -1;
    }
    int GetSinnerID(string role)
    {
        sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
        for (int i = 0; i < sinnerRoles.Count; i++)
        {
            if (role == sinnerRoles[i])
            {
                return i;
            }
        }
        return -1;
    }

    [PunRPC]
    void MovePlayersToChurch()
    {
        if (!this.photonView.IsMine) { return; }
        if (this.gameObject.name.Contains("CloneFor")) { return; }

        Transform[] ChurchSpawners = TruthManager.Instance.ChurchSpawners;
        this.gameObject.GetComponent<CharacterController>().enabled = false;

        for (int i = 0; i <= ChurchSpawners.Length; i++)
        {
            if (PhotonNetwork.LocalPlayer.GetPlayerNumber() == i)
            {
                this.gameObject.transform.position = ChurchSpawners[i].position;
                //Debug.Log(ChurchSpawners[i].rotation);
                this.gameObject.transform.rotation = ChurchSpawners[i].rotation;
            }
        }
        //this.gameObject.GetComponent<CharacterController>().enabled = false;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "MovedPlayersToChurch" } });
        }
    }

    [PunRPC]
    void SpawnClonesForVote(string responsibleRole)
    {
        if (this.gameObject.name.Contains("CloneFor")) { return; }
        int maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];

        clonePrefabs = new GameObject[maxPlayers];
        Transform[] cloneSpawners = new Transform[maxPlayers];
        switch (responsibleRole)
        {
            case "Reaper":
                clonePrefabs = ThreatManager.Instance.ClonePrefabsForReapers;   // Don't forget to set Length(=maxPlayers) in Inspector!!!!
                cloneSpawners = ThreatManager.Instance.CloneSpawnersForReapers;
                break;
            case "Knight":
                clonePrefabs = ThreatManager.Instance.ClonePrefabsForKnight;    // Don't forget to set Length(=maxPlayers) in Inspector!!!!
                cloneSpawners = ThreatManager.Instance.CloneSpawnersForKnight;
                break;
            case "Shaman":
                clonePrefabs = ThreatManager.Instance.ClonePrefabsForShaman;    // Don't forget to set Length(=maxPlayers) in Inspector!!!!
                cloneSpawners = ThreatManager.Instance.CloneSpawnersForShaman;
                break;
            case "Stalker":
                clonePrefabs = ThreatManager.Instance.ClonePrefabsForStalker;   // Don't forget to set Length(=maxPlayers) in Inspector!!!!
                cloneSpawners = ThreatManager.Instance.CloneSpawnersForStalker;
                break;
        }

        // The pool is individual (every player needs to add prefab to pool)
        string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
        playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

        int cloneCount = -1;
        for (int i = 0; i < maxPlayers; i++)
        {
            if (getRoleAssignment[i] != responsibleRole)
            {
                // I add this!!
                string condition = "Alive";
                if (responsibleRole == "Shaman") { condition = "Soul"; }

                if (playerStatuses[i + 1].Contains(condition))
                {
                    cloneCount++;
                    GameObject prefabToSpawn = GameObject.Find("Player " + (i + 1));
                    string cloneName = "CloneFor" + responsibleRole + " " + (i + 1);

                    // Debug.Log("SpawnClonesForVote() : cloneCount + " Role=" + responsibleRole + "prefabToSpawn.name" + prefabToSpawn.name);
                    if (!PrefabIsInPool(prefabToSpawn.name))
                    {
                        if (prefabToSpawn != null)
                        {
                            AddToPrefabPool(prefabToSpawn);
                        }
                    }

                    // this.photonView.RPC("LetsDebug", RpcTarget.All, this.gameObject.name + " " + PhotonNetwork.LocalPlayer.CustomProperties["Role"] + " " + responsibleRole + " " + ((responsibleRole != "Reaper" && (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == responsibleRole)));
                    if (prefabToSpawn != null
                    && ((responsibleRole == "Reaper" && PhotonNetwork.IsMasterClient)
                        || (responsibleRole != "Reaper" && (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == responsibleRole)))
                    {
                        clonePrefabs[cloneCount] = PhotonNetwork.Instantiate(prefabToSpawn.name, cloneSpawners[cloneCount].transform.position, cloneSpawners[cloneCount].transform.rotation) as GameObject;

                        Dictionary<int, string> cloneInfo = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties[responsibleRole + "CloneInfo"];
                        if (!isInDictionary(cloneInfo, clonePrefabs[cloneCount].GetPhotonView().ViewID))
                        {
                            // Check if it is already in cloneInfo
                            cloneInfo.Add(clonePrefabs[cloneCount].GetPhotonView().ViewID, responsibleRole);
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { responsibleRole + "CloneInfo", cloneInfo } });
                        }
                    }
                }
            }
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == responsibleRole)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "CloneStatus", "SpawnedClonesFor" + responsibleRole } });
            }
        }
    }
    public bool PrefabIsInPool(string cloneName)
    {
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        foreach (KeyValuePair<string, GameObject> prefabInPool in pool.ResourceCache)
        {
            if (prefabInPool.Key == cloneName)
            {
                return true;
            }
        }
        return false;
    }
    public void AddToPrefabPool(GameObject prefabToSpawn)
    {
        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        //Debug.Log(pool.ResourceCache.Keys);

        if (pool != null && prefabToSpawn != null)
        {
            pool.ResourceCache.Add(prefabToSpawn.name, prefabToSpawn);
        }
    }
    bool isInDictionary(Dictionary<int, string> dic, int keyToCheck)
    {
        foreach (KeyValuePair<int, string> info in dic)
        {
            if (info.Key == keyToCheck)
            {
                return true;
            }
        }
        return false;
    }

    void GetPlayerNumber()
    {
        myPlayerNumInGame = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1;

        if (this.gameObject.name.Contains("CloneFor"))
        {
            string myCloneRole = this.gameObject.name.Substring(0, this.gameObject.name.Length - 2);
            string getPlayerNum = this.gameObject.name.Replace("CloneFor" + myCloneRole + " ", "");
            int convertedNum = 0;
            if (int.TryParse(getPlayerNum, out convertedNum))
            {
                myPlayerNumInGame = convertedNum;
            }
        }
    }

    #endregion

    #region Manage Votes
    void Update()
    {
        GetPlayerNumber();

        if (PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null)
        {
            dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
            currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
            stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];

            switch (currentState)
            {
                case "Trust":
                    return;
                case "Truth":
                    gotProtectedPlayer = true;
                    break;
            }

            targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];

            if (gotVoteResult && gotProtectedPlayer && gotTargetSoulInfo
                &&!handledTarget)
            {
                handledTarget = true;

                //targetSoulNum = Convert.ToInt32(targetSoulInfo["PlayerNumber"]);
                int convertedNum = -1;
                if (int.TryParse(targetSoulInfo["PlayerNumber"], out convertedNum))
                {
                    targetSoulNum = convertedNum;
                }
                voteResult = (string)PhotonNetwork.CurrentRoom.CustomProperties["VoteResult"];
                protectedPlayer = (int)PhotonNetwork.CurrentRoom.CustomProperties["ProtectedPlayer"];
                if (voteResult == "FoundTarget")
                {
                    if (myPlayerNumInGame == targetSoulNum
                        && !this.gameObject.name.Contains("CloneFor"))
                    {
                        if (targetSoulNum != protectedPlayer)
                        {
                            this.photonView.RPC("KillTarget", RpcTarget.AllViaServer, this.photonView.ViewID);
                        }
                        else
                        {
                            StartCoroutine(PlayerIconManager.Instance.Alert("Upper", "You are killed!", 3f));
                            this.photonView.RPC("LetsDebug", RpcTarget.All, "Target is protected!");
                        }
                    }
                }

                protectedPlayer = (int)PhotonNetwork.CurrentRoom.CustomProperties["ProtectedPlayer"];
                if (protectedPlayer == targetSoulNum
                    && PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "TargetProtected" } });
                }
            }
        }

    }

    [PunRPC]
    void UpdateVoteToPlayer(string cloneName, string namePrefix, int inputVoteAmount)
    {
        GameObject targetObj = GameObject.Find(cloneName);
        VoteManager VoteManagerInstance = targetObj.GetComponent<VoteManager>();
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];

        string playerStatus = GetPlayerStatus(cloneName, namePrefix);
        //bool canVote = false;
        //if (currentState == "Threat") 
        //{
        //    if (cloneName.Contains("Shaman"))
        //    {
        //        if (playerStatus.Contains("Soul"))
        //        {
        //            canVote = true;
        //        }
        //    }
        // else
            //{ 
            //if (playerStatus == "Alive") { canVote = true; } 
            //}
        //}
            //


            //if (canVote)
            //{
            VoteManagerInstance.gottenVote += inputVoteAmount;

            messageToDebug = "(2) UpdateVoteToPlayer [Day" + dayCount + "][State" + currentState + "][" + cloneName + "] : cloneName=" + cloneName + ", VoteManagerInstance.gottenVote=" + VoteManagerInstance.gottenVote;
            Debug.Log(messageToDebug);
        //}
    }

    public string GetPlayerStatus(string prefabNameToCheck, string namePrefix)
    {
        playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

        string getPlayerNum = prefabNameToCheck.Replace(namePrefix + " ", "");
        int convertedNum = 0;
        if (int.TryParse(getPlayerNum, out convertedNum))
        {
            return playerStatuses[convertedNum];
        }
        return playerStatuses[convertedNum];
    }

    [PunRPC]
    void ShowVote(string namePrefix)
    {
        if (this.gameObject.name.Contains(namePrefix))
        {
            VoteManager VoteManagerInstance = this.gameObject.GetComponent<VoteManager>();
            if (VoteManagerInstance.gottenVote > 0) { VoteManagerInstance.IGotVote = true; } else { VoteManagerInstance.IGotVote = false; }

            currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
            if (currentState == "Threat")
            {
                if (namePrefix.Contains("Reaper"))
                {
                    VoteManagerInstance.VoteText.text = VoteManagerInstance.gottenVote.ToString();
                    VoteManagerInstance.VoteText.gameObject.SetActive(VoteManagerInstance.IGotVote);
                }
                else
                {
                    VoteManagerInstance.ChosenIcon.gameObject.SetActive(VoteManagerInstance.IGotVote);
                }
            }
            else
            {
                VoteManagerInstance.VoteText.text = VoteManagerInstance.gottenVote.ToString();
                VoteManagerInstance.VoteText.gameObject.SetActive(VoteManagerInstance.IGotVote);
            }
            

            messageToDebug = "(3) ShowVote [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "] : VoteManagerInstance.IGotVote=" + VoteManagerInstance.IGotVote + ", VoteManagerInstance.gottenVote=" + VoteManagerInstance.gottenVote;
            Debug.Log(messageToDebug);
        }
    }

    #endregion

    [PunRPC]
    void AnnounceRecordVote()
    {
        if (!this.photonView.IsMine) { return; }

        voteRecordCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["VoteRecordCount"];
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "VoteRecordCount", voteRecordCount + 1 } });
    }

    #region Find & Kill the Target
    void FindTarget()
    {
        stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
        if (stateStatus == "SkipVote") { return; }

        Dictionary<int, int> votes = new Dictionary<int, int>();
        for (int i = 1 ; i <= PhotonNetwork.CurrentRoom.MaxPlayers ; i++)
        {
            int voteRecord = 0;
            if (PhotonNetwork.CurrentRoom.CustomProperties["VoteRecord" + i] != null)
            {
                voteRecord = (int)PhotonNetwork.CurrentRoom.CustomProperties["VoteRecord" + i];
            }
            votes.Add(i, voteRecord);
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "Votes", votes } });

        int maxVote = 0;
        for (int i = 1; i <= votes.Count; i++)
        {
            int thisVote = votes[i];
            if (thisVote == maxVote
                && thisVote != 0) 
            {
                voteResult = "Draw";
            }
            
            if (thisVote > maxVote)
            {
                maxVote = thisVote;
                voteResult = "FoundTarget";
                targetSoulInfo["PlayerNumber"] = Convert.ToString(i);
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "TargetSoulInfo", targetSoulInfo } });
            }
        }
        if (maxVote == 0 && voteResult != "Draw") 
        {
            voteResult = "NoVoteFound";
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "VoteResult", voteResult } });

        messageToDebug = "(5) FindTarget [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "] : voteResult=" + voteResult;
        Debug.Log(messageToDebug);
    }



    [PunRPC]
    void KillTarget(int targetViewID)
    {
        if (!this.photonView.IsMine) { return; }
        if (this.gameObject.name.Contains("CloneFor")) { return; }

        stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
        if (stateStatus == "SkipVote") { return; }

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        //Debug.Log("Killing Target");
        if (this.gameObject.GetComponent<PhotonView>().ViewID == targetViewID)
        {
            myPlayerNumInGame = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1;
            playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

            string soulIndicator = "Soul";
            switch (currentState)
            {
                case "Threat":
                    soulIndicator = "TargetSoul";
                    break;
                case "Truth":
                    soulIndicator = "BurnedSoul";
                    this.photonView.RPC("BurnSuspect", RpcTarget.AllViaServer, this.photonView.ViewID, true);
                    this.photonView.RPC("ChangeToSoulColor", RpcTarget.AllViaServer, this.photonView.ViewID, "Soul");
                    break;

            }
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", soulIndicator } });
            playerStatuses[myPlayerNumInGame] = soulIndicator;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerStatuses", playerStatuses } });

            // targetSoulInfo["PlayerNumber"] = Convert.ToString(PhotonNetwork.LocalPlayer.GetPlayerNumber()+1);
            targetSoulInfo["NickName"] = PhotonNetwork.LocalPlayer.NickName;
            targetSoulInfo["Color"] = (string)PhotonNetwork.LocalPlayer.CustomProperties["Color"];
            targetSoulInfo["Role"] = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
            targetSoulInfo["Team"] = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "TargetSoulInfo", targetSoulInfo } });

            messageToDebug = "(6) KillTarget [Day" + dayCount + "][State" + currentState + "][" + this.gameObject.name + "] : targetSoulNum=" + targetSoulNum + ", playerStatuses[targetSoulNum]=" + playerStatuses[targetSoulNum];
            this.photonView.RPC("LetsDebug", RpcTarget.All, messageToDebug + " ID:" + this.gameObject.GetComponent<PhotonView>().ViewID + " " + targetViewID);
            // Show Panel "Target Soul"
        }
    }

    [PunRPC]
    void ShowTheWinner()
    {
        if (!this.photonView.IsMine) { return; }

        string winnerTeam = (string)PhotonNetwork.CurrentRoom.CustomProperties["WinnerTeam"];
        Debug.Log("!!!!!CONGRATULATIONS!!!!! The Winner Is..." + winnerTeam);
    }
    #endregion


    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (!this.photonView.IsMine || this.gameObject.name.Contains("CloneFor")) { return; }

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        myPlayerNumInGame = PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1;
        myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

        foreach (var prop in propertiesThatChanged)
        {

            switch (Convert.ToString(prop.Key))
            {
                case "StateStatus":
                    stateStatus = Convert.ToString(prop.Value);
                    dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
                    // Debug.Log("Day=" + dayCount + " prop.Key=" + Convert.ToString(prop.Key) + " prop.Value=" + Convert.ToString(prop.Value));
                    // I fix this!!

                    if (stateStatus == "BegunThreat" || stateStatus == "BegunTruth")
                    {
                        if (!initializedVariables)
                        {
                            initializedVariables = true;
                            this.photonView.RPC("InitVoteManager", RpcTarget.AllViaServer);
                            this.photonView.RPC("ResetVoteText", RpcTarget.AllViaServer);
                        }
                    }
                    else if (stateStatus.Contains("Ended"))
                    {
                        movedPlayers = false;
                        initializedVariables = false;
                    }

                    switch (stateStatus)
                    {
                        // Threat
                        case "BegunThreat":
                            if (!movedPlayers)
                            { 
                                movedPlayers = true;
                                this.photonView.RPC("MovePlayersToTheirPosition", RpcTarget.AllViaServer);
                            }
                            break;

                        case "ActivatedReaperAbility":
                            this.photonView.RPC("InitVoteManager", RpcTarget.AllViaServer);
                            this.photonView.RPC("ResetVoteText", RpcTarget.AllViaServer);
                            break;

                        case "BegunRevealTargetSoul":
                            if (!movedPlayers)
                            {
                                movedPlayers = true;
                                this.photonView.RPC("MovePlayersToMirror", RpcTarget.AllViaServer);
                            }
                            break;

                        // Trust
                        case "BegunTrust":
                            if (!movedPlayers)
                            {
                                movedPlayers = true;
                                this.photonView.RPC("MovePlayersToBridge", RpcTarget.AllViaServer);
                            }
                            break;

                        // Truth
                        case "BegunTruth":
                            if (!movedPlayers)
                            {
                                movedPlayers = true;
                                this.photonView.RPC("MovePlayersToChurch", RpcTarget.AllViaServer);
                            }
                            break;

                        case "RecordedVotes":
                            if (PhotonNetwork.IsMasterClient
                                && !foundTarget)
                            {
                                foundTarget = true;
                                FindTarget();
                            }
                            break;
                    }
                    break;

                // Restart the variables
                case "Votes":
                    getVotes = (Dictionary<int, int>)prop.Value;
                    break;

                case "PlayerStatuses":
                    playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

                    if (targetSoulInfo["PlayerNumber"] != null)
                    {
                        int convertedNum = -1;
                        if (int.TryParse(targetSoulInfo["PlayerNumber"], out convertedNum))
                        {
                            targetSoulNum = convertedNum;
                        }
                    }

                    if (!killedTarget
                        && targetSoulNum > 0
                        && (playerStatuses[targetSoulNum] == "TargetSoul" || playerStatuses[targetSoulNum] == "BurnedSoul")
                        && PhotonNetwork.IsMasterClient)
                    {
                        if (playerStatuses[targetSoulNum] == "TargetSoul")
                        {
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "KilledTarget" } });
                        }
                        else if (playerStatuses[targetSoulNum] == "BurnedSoul")
                        {
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "BurnedTarget" } });
                        }
                        killedTarget = true;
                    }
                    break;

                // Update vote record count
                //case "VoteRecordCount":
                //    voteRecordCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["VoteRecordCount"];
                //    maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];
                //    Debug.Log("VoteRecordCount = " + voteRecordCount + " [Day" + dayCount + "][State" + currentState + "]");
                //    //int aliveSinners = (int)PhotonNetwork.CurrentRoom.CustomProperties["AliveSinners"];
                //    //int aliveReapers = (int)PhotonNetwork.CurrentRoom.CustomProperties["AliveReapers"];
                //    if (PhotonNetwork.IsMasterClient
                //    && voteRecordCount >= maxPlayers)
                //    {
                //        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "RecordedVotes" } });
                //    }
                //    break;

                case "ProtectedPlayer":
                    gotProtectedPlayer = true;
                    protectedPlayer = Convert.ToInt32(prop.Value);
                    break;

                // When the target is found, Kill that target
                case "VoteResult":
                    if (Convert.ToString(prop.Value) == "FoundTarget")
                    {
                        gotVoteResult = true;
                    }
                    if (PhotonNetwork.IsMasterClient) { PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "GotVoteResult" } }); }
                    voteResult = (string)PhotonNetwork.CurrentRoom.CustomProperties["VoteResult"];
                    break;

                case "TargetSoulInfo":
                    gotTargetSoulInfo = true;
                    targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
                    if (PhotonNetwork.IsMasterClient)
                    {
                        targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
                        string targetSoulTeam = targetSoulInfo["Team"];
                        if (targetSoulTeam != "")
                        {
                            switch (targetSoulTeam)
                            {
                                case "Reaper":
                                    int aliveReapers = (int)PhotonNetwork.CurrentRoom.CustomProperties["AliveReapers"];
                                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "AliveReapers", aliveReapers - 1 } });
                                    break;
                                case "Sinner":
                                    int aliveSinners = (int)PhotonNetwork.CurrentRoom.CustomProperties["AliveSinners"];
                                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "AliveSinners", aliveSinners - 1 } });
                                    break;
                            }
                        }
                    }
                    break;
            }

            if (Convert.ToString(prop.Key).Contains("VoteRecord"))
            {
                if (currentState == "SetupGame") { return; }
                string voteRecordKey = Convert.ToString(prop.Key);
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log(Convert.ToString(prop.Key) + " : " + Convert.ToString(prop.Value));
                    voteRecordCount += 1;

                    if (voteRecordCount == PhotonNetwork.CurrentRoom.PlayerCount)
                    {
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "RecordedVotes" } });
                    }
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.gottenVote);
            stream.SendNext(this.IGotVote);
        }
        else
        {
            // Network player, receive data
            this.gottenVote = (int)stream.ReceiveNext();
            this.IGotVote = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    void LetsDebug(string message)
    {
        Debug.Log(message);
    }

}
