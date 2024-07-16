using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;

public class StatusManager : MonoBehaviourPunCallbacks
{
    static public StatusManager Instance;
    GameObject playerInstance;

    [Header("Panel")]
    public GameObject TitlePanel;
    public TMP_Text TitleTextFront;
    public TMP_Text TitleTextBack;
    public TMP_Text TitleMessage;

    [Header("Status Text")]
    public TMP_Text DayTextFront;
    public TMP_Text DayTextBack;
    public TMP_Text TimerText;

    [Header("Message Text")]
    public TMP_Text UpperText;
    public TMP_Text MiddleText;

    [Header("Player Text")]
    public GameObject PlayerInfo;
    public TMP_Text PlayerNameTextUI;
    public TMP_Text PlayerRoleTextUI;

    [Header("Time Flags")]
    public bool timerStarted = false;
    public bool timeIsUp = false;
    bool timerReset;

    [Header("Time Info")]
    [SerializeField] public double totalTime;
    double startTime;
    double elapsedTime;
    double remainingTime;
    public int timer;

    [Header("Game Time Breakdown")]
    public double titleTime = 5f;
    public double stolenAbilityTime = 10f;
    public double stolenShowTime = 5f;
    public double stateTime;
    public double endTime = 5f;
    public double resetTime = 5f;

    [Header("Setup Time Breakdown")]
    [SerializeField] double teamTime = 5f;
    [SerializeField] double spawnTime = 5f;
    [SerializeField] double humanTime = 5f;
    [SerializeField] double roleTime = 5f;
    // [SerializeField] double prepareTime = 10f;

    [Header("Game Info")]
    int dayCount;
    string setupStatus;
    string currentState;
    string stateStatus;
    string reaperAbility;
    string[] ThreatAbilitySinners = { "Knight", "Shaman", "Stalker" };
    string[] TrustAbilitySinners = { "Butcher", "Druid", "Wizard" };

    [Header("Status Flags")]
    int messageSent;
    bool skippingTrust;
    bool abilityStolen;

    [Header("Camera Pos")]
    public Transform PlaygroundCamera;
    public Transform ThreatCameraForReapers;
    public Transform ThreatCameraForKnight;
    public Transform ThreatCameraForShaman;
    public Transform ThreatCameraForStalker;
    public Transform TrustCamera;
    public Transform TruthCamera;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        TitleTextFront = TitleTextFront.GetComponent<TMP_Text>();
        TitleTextBack = TitleTextBack.GetComponent<TMP_Text>();
        TitleMessage = TitleMessage.GetComponent<TMP_Text>();

        DayTextFront = DayTextFront.GetComponent<TMP_Text>();
        DayTextBack = DayTextBack.GetComponent<TMP_Text>();
        TimerText = TimerText.GetComponent<TMP_Text>();

        PlayerNameTextUI = PlayerNameTextUI.GetComponent<TMP_Text>();
        PlayerRoleTextUI = PlayerRoleTextUI.GetComponent<TMP_Text>();

        UpperText = UpperText.GetComponent<TMP_Text>();
        MiddleText = MiddleText.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];

        if (!timerStarted) { TimerText.gameObject.SetActive(false); return; }

        // Debug.Log("State = " + currentState + " timerStarted" + timerStarted + " timeisUp" + timeIsUp + " totalTime" + totalTime);

        elapsedTime = PhotonNetwork.Time - startTime;
        remainingTime = totalTime - (elapsedTime % totalTime);
        timer = (int)remainingTime;

        if (currentState == "SetupGame")
        {
            TimerText.gameObject.SetActive(false);
            if (timer <= 0) { timeIsUp = true; }

            if (PhotonNetwork.IsMasterClient)
            {
                ManageSetupStatus();
            }
        }
        else
        {
            TimerText.text = (timer - endTime).ToString();
            if (timer <= endTime) { timeIsUp = true; }

            ManageGameUI();
            ManageGameStates();
            ManageGameProperties();
        }
    }

    void ManageGameUI()
    {
        PlayerInfo.SetActive(true);
        PlayerNameTextUI.text = PhotonNetwork.LocalPlayer.NickName;
        PlayerRoleTextUI.text = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

        dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"];
        DayTextFront.text = "Day " + dayCount.ToString();
        DayTextBack.text = DayTextFront.text;
        DayTextFront.gameObject.SetActive(true);
        DayTextBack.gameObject.SetActive(true);

        if (timeIsUp) { TimerText.gameObject.SetActive(false); }
        else { if (currentState != "RevealTargetSoul") { TimerText.gameObject.SetActive(true); } }
    }

    void ManageSetupStatus()
    {
        // I add this!!
        setupStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["SetupStatus"];

        // Debug.Log("State=" + currentState + " Setup=" + setupStatus);
        switch (setupStatus)
        {
            case "Spawned":
                TitlePanel.gameObject.SetActive(false);
                // TimerText.gameObject.SetActive(true);
                break;

            case "ShowingTeams":
                if (timer <= (totalTime - (teamTime))
                    && PhotonNetwork.LocalPlayer.CustomProperties["Role"] != null
                    && messageSent == 0) // when timer <= (30 - 5)  == 25
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "SpawnTime" } });
                    messageSent++;
                }
                break;

            case "ShowingHumanBody":
                if (timer <= (totalTime - (teamTime + spawnTime))
                    && messageSent == 1) // when timer <= (30 - 15) 
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "ShowedHumanBody" } });
                    messageSent++;
                }
                break;

            case "ShowingRoles":
                if (timer <= (totalTime - (teamTime + spawnTime + humanTime))
                    && messageSent == 2) // when timer <= (30 - 20) 
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "ShowedRoles" } });
                    messageSent++;
                }
                break;

            case "ShowedRoles":
                if (timeIsUp
                    && messageSent == 3)
                {

                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "FinishedSetup" } });
                    messageSent++;
                }
                break;
        }
    }

    void ManageGameStates()
    {
        if (timer <= 0)
        {
            timerStarted = false;
            Debug.Log("timer<=0 :  stateStatus=" + stateStatus + "currentState=" + currentState + " " + PhotonNetwork.IsMasterClient + " " + !WinnerIsFound());

            if (PhotonNetwork.IsMasterClient)
            {
                if ((stateStatus.Contains("Ended") && !WinnerIsFound()))
                {
                    switch (currentState)
                    {
                        case "Threat":
                            if (!skippingTrust)
                            {
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "RevealTargetSoul" } });
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "BegunRevealTargetSoul" } });
                            }
                            else
                            {
                                // PhotonNetwork.LoadLevel("TruthScene");
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "Truth" } });
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "BegunTruth" } });
                                // StateStatus = {"BegunTruth", "InitializedVariables", "FinishedVotes", "RecordedVotes", "GotVoteResult", "KilledTarget", "EndedTruth"}
                            }
                            break;

                        case "RevealTargetSoul":
                            // PhotonNetwork.LoadLevel("TrustScene");
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "Trust" } });
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "BegunTrust" } });
                            // StateStatus = {"BegunTrust", "Hunting" "SoulIsFree", "SoulTaken", "EndedTrust"}
                            break;

                        case "Trust":
                            // PhotonNetwork.LoadLevel("TruthScene");
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "Truth" } });
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "BegunTruth" } });
                            break;

                        case "Truth":
                            // PhotonNetwork.LoadLevel("ThreatScene");
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "Day", dayCount + 1 } });
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "Threat" } });
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "BegunThreat" } });
                            // StateStatus = {"BegunThreat", "SpawnedClonesFor"+responsibleRole, "InitializedVariables", "FinishedVotes", "RecordedVotes", "GotVoteResult", "KilledTarget", "EndedThreat"}
                            break;
                    }
                }
            }
        }
        //else if ((timer <= endTime) && !timerReset)
        //{
        //    timeIsUp = true;
        //    // TimerText.gameObject.SetActive(false);
        //}
        //else if ((timer <= (totalTime - titleTime)) && !timerReset)
        //{
        //    TitlePanel.SetActive(false);
        //    //if (currentState != "RevealTargetSoul")
        //    //{
        //    // TimerText.gameObject.SetActive(true);
        //    //}
        //}
    }


    void ManageGameProperties()
    {
        MiddleText.gameObject.SetActive(stateStatus == "CountDown");
        switch (currentState)
        {
            case "RevealTargetSoul":
                if (timerReset) { return; }
                if (timer <= 1 && messageSent == 2)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "CountDown" } });
                    messageSent++;
                }
                else if (timer >= (titleTime + stateTime) && messageSent == 1)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "RevealingTargetName" } });
                    messageSent++;
                }
                else if (timer >= (titleTime) && messageSent == 0)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "RevealingTargetRole" } });
                    messageSent++;
                }
                break;
        }
    }

    bool WinnerIsFound()
    {
        string winnerTeam = "";
        int aliveReapers = (int)PhotonNetwork.CurrentRoom.CustomProperties["AliveReapers"];
        int aliveSinners = (int)PhotonNetwork.CurrentRoom.CustomProperties["AliveSinners"];

        Debug.Log("Alive Reapers = " + aliveReapers);
        Debug.Log("Alive Sinners = " + aliveSinners);
        if (aliveSinners <= aliveReapers)
        {
            winnerTeam = "Reaper";
        }
        else if (aliveReapers == 0)
        {
            winnerTeam = "Sinner";
        }

        if (winnerTeam != "")
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "WinnerTeam", winnerTeam } });
            return true;
        }
        return false;
    }


    IEnumerator ShowTitlePanel()
    {
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        if (currentState != "RevealTargetSoul") { TitleTextFront.text = currentState; } 
        else { TitleTextFront.text = ""; }
        TitleTextBack.text = TitleTextFront.text;

        TitlePanel.SetActive(true);
        yield return new WaitForSeconds(5f);
        TitlePanel.SetActive(false);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null) { currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"]; }
        if (PhotonNetwork.CurrentRoom.CustomProperties["Day"] != null) { dayCount = (int)PhotonNetwork.CurrentRoom.CustomProperties["Day"]; }

        foreach (var prop in propertiesThatChanged)
        {
            
            Debug.Log("Day=" + dayCount + " currentState=" + currentState + "Key=" + Convert.ToString(prop.Key) + " Value=" + Convert.ToString(prop.Value));

            if (Convert.ToString(prop.Value).Contains("Begun")) { StartCoroutine(ShowTitlePanel()); }
            switch (Convert.ToString(prop.Key))
            {
                case "StartTime":
                    startTime = (double)prop.Value;
                    timerStarted = true;
                    timeIsUp = false;
                    timerReset = false;
                    messageSent = 0;
                    break;

                case "ResetTime":
                    timerReset = true;
                    totalTime = resetTime;
                    startTime = (double)prop.Value;
                    timerStarted = true;
                    break;

                // I add this!!
                case "SetupStatus":
                    switch (Convert.ToString(prop.Value))
                    {
                        case "AssignedRoles":
                            totalTime = teamTime + spawnTime + humanTime + roleTime; // + prepareTime
                            if (PhotonNetwork.IsMasterClient)
                            {
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StartTime", PhotonNetwork.Time } });
                            }
                            break;

                        case "FinishedSetup":
                            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Alive" } });
                            if (PhotonNetwork.IsMasterClient)
                            {
                                // PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "StartGame" } });
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "Threat" } });
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "StateStatus", "BegunThreat" } });
                            }
                            break;
                    }
                    break;

                case "GameStatus":
                    currentState = Convert.ToString(prop.Value);
                    titleTime = 5f;
                    if (currentState != "EndGame")
                    {
                        switch (currentState)
                        {
                            case "Threat":
                                reaperAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperAbility"];
                                stateTime = 30f;  //30
                                if (reaperAbility != "" && reaperAbility != null)
                                {
                                    stateTime = stateTime + stolenAbilityTime + stolenShowTime;
                                }
                                break;

                            case "RevealTargetSoul":
                                titleTime = 2f;
                                stateTime = 5f;
                                break;

                            case "Trust":
                                stateTime = 150f;  //90
                                break;

                            case "Truth":
                                stateTime = 30f;  //30
                                break;
                        }
                        totalTime = titleTime + stateTime + endTime;
                    }
                    break;

                case "StateStatus":
                    stateStatus = Convert.ToString(prop.Value);

                    if (stateStatus.Contains("Ended"))
                    {
                        timeIsUp = false;
                    }

                    switch (stateStatus)
                    {
                        case "SoulTaken":
                            StartCoroutine(PlayerIconManager.Instance.Alert("Middle", "Soul is taken!", 5f));
                            break;

                        case "CountDown":
                            MiddleText.text = timer.ToString();
                            break;
                    }

                    if (PhotonNetwork.IsMasterClient)
                    {
                        Hashtable hashStartTime = new Hashtable() { { "StartTime", PhotonNetwork.Time } };
                        switch (stateStatus)
                        {
                            case "BegunThreat":
                                skippingTrust = false;
                                PhotonNetwork.CurrentRoom.SetCustomProperties(hashStartTime);
                                break;

                            case "BegunTrust":
                            case "BegunTruth":
                            case "BegunRevealTargetSoul":
                                PhotonNetwork.CurrentRoom.SetCustomProperties(hashStartTime);
                                break;

                            case "TargetProtected":
                                skippingTrust = true;
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ResetTime", PhotonNetwork.Time } });
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "Ended" + currentState } });
                                break;

                            //case "ShowingTarget":
                            //case "ShowingVoteResult":
                            case "KilledTarget":
                            case "BurnedTarget":
                            case "SoulTaken":
                            case "SkipVote":
                            case "CountDown":
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ResetTime", PhotonNetwork.Time } });
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "Ended" + currentState } });
                                break;
                        }
                    }

                    break;

                case "WinnerTeam":
                    PlayerIconManager.Instance.EndGame();
                    break;
            }
        }
    }

}
