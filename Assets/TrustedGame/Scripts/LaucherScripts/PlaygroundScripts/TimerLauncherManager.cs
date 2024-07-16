using UnityEngine;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class TimerLauncherManager : MonoBehaviourPunCallbacks
{

    static public TimerLauncherManager Instance;

    [Header("Game Info")]
    string setupStatus;

    [Header("Timer Info")]
    bool timerStarted = false;
    double totalTime; //30
    [SerializeField] double teamTime = 5f;
    [SerializeField] double spawnTime = 5f;
    [SerializeField] double humanTime = 5f;
    [SerializeField] double roleTime = 5f;
    [SerializeField] double prepareTime = 10f;
    double startTime;
    double elapsedTime;
    double remainingTime;
    int timer;

    [Header("UI")]
    public TMP_Text TimerText;

    [Header("Flags")]
    bool resetTimer;
    int messageSent;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        TimerText = TimerText.GetComponent<TMP_Text>();
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StartTime", PhotonNetwork.Time } });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerStarted || !PhotonNetwork.LocalPlayer.IsLocal) return;

        elapsedTime = PhotonNetwork.Time - startTime;
        remainingTime = totalTime - (elapsedTime % totalTime);
        
        timer = (int)remainingTime;
        TimerText.text = timer.ToString();

        if (PhotonNetwork.IsMasterClient)
        {
            setupStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["SetupStatus"];
            //Debug.Log(setupStatus);
            switch (setupStatus)
            {
                case "ShowingTeams":
                    if (timer <= (totalTime - (teamTime))
                        && messageSent == 0) // when timer <= (30 - 5) 
                    {
                        messageSent += 1;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "SpawnTime" } });
                    }
                    //else if (timer <= (totalTime - (teamTime))
                    //         && messageSent == 0) // when timer <= (30 - 5) 
                    //{
                    //    messageSent += 1;
                    //}
                    break;

                case "ShowingHumanBody":
                    if (timer <= (totalTime - (teamTime + spawnTime + humanTime))
                        && messageSent == 1) // when timer <= (30 - 15) 
                    {
                        messageSent += 1;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "ShowedHumanBody" } });
                    }
                    break;

                case "ShowingRoles":
                    if (timer <= (totalTime - (teamTime + spawnTime + humanTime + roleTime))
                        && messageSent == 2) // when timer <= (30 - 20) 
                    {
                        messageSent += 1;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "ShowedRoles" } });
                    }
                    break;

                case "ShowedRoles":
                    if (timer <= 0
                        && messageSent == 3)
                    {
                        messageSent += 1;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "FinishedSetup" } });
                    }
                    break;
            }
        }
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
                    totalTime = teamTime + spawnTime + humanTime + roleTime + prepareTime;
                    timerStarted = true;
                    break;
                case "SetupStatus":
                    if (Convert.ToString(prop.Value) == "FinishedSetup") 
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Alive" } });
                        timerStarted = false;
                        if (PhotonNetwork.IsMasterClient)
                        {
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "StartGame" } });
                            PhotonNetwork.LoadLevel("ThreatScene");
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "Threat" } });
                            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "StateStatus", "BegunThreat" } });
                        }
                    }
                    break;
            }
        }
    }
}
