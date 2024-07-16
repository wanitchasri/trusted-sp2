using System;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using TMPro;
using System.Collections;

public class PlayerIconManager : MonoBehaviourPunCallbacks
{
    public static PlayerIconManager Instance;

    [Header("Playground Models")]
    [SerializeField] public GameObject ReaperModel;
    [SerializeField] public GameObject[] SinnerModels;

    [Header("Weapons")]
    [SerializeField] public GameObject AttackWeapon;
    [SerializeField] public GameObject[] SinnerAbilities;

    [Header("Decorations")]
    [SerializeField] public GameObject[] RoleDecorations;
    [SerializeField] public GameObject[] TargetDecorations;
    [SerializeField] public GameObject[] DeadDecorations;

    [Header("Text")]
    public TMP_Text UpperTextIcon;
    public TMP_Text LowerTextIcon;
    TMP_Text AlertText;
    public TMP_Text WinnerText;
    public TMP_Text WinnerTextBack;

    [Header("Game Status")]
    int dayCount;
    string setupStatus;
    string currentState;
    string stateStatus;

    [Header("Game Info")]
    Dictionary<int, string> sinnerRoles;
    string[] roleAssignment;
    Dictionary<int, string> playerStatuses;
    string voteResult;
    Dictionary<string, string> targetSoulInfo;
    string reaperAbility;

    [Header("Message")]
    string reaperMessage;
    string priestMessage;
    string messageToShow;

    [Header("Player Info")]
    int myPlayerNumInGame;
    string myTeam;
    string myRole;
    string myColor;
    int roleIndex = -1;
    string playerStatus;

    [Header("Flags")]
    bool showedMessage;
    bool showedAbility;
    bool showedResult;
    bool showedTarget;

    [Header("GameObject")]
    public GameObject MiddleBanner;
    public GameObject UpperBanner;
    public GameObject EndingPopup;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        UpperTextIcon = UpperTextIcon.GetComponent<TMP_Text>();
        LowerTextIcon = LowerTextIcon.GetComponent<TMP_Text>();
        WinnerText = WinnerText.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Dictionary<int, string> basicSinnerRoles = new Dictionary<int, string>();
        basicSinnerRoles.Add(0, "Butcher");
        basicSinnerRoles.Add(1, "Druid");
        basicSinnerRoles.Add(2, "Knight");
        basicSinnerRoles.Add(3, "Shaman");
        basicSinnerRoles.Add(4, "Stalker");
        basicSinnerRoles.Add(5, "Wizard");
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SinnerRoles", basicSinnerRoles } });
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }

        setupStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["SetupStatus"];
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];

        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"] != null)
        {
            stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
            playerStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
            myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

            switch (stateStatus)
            {
                case "BegunThreat":
                    if (!showedMessage)
                    {
                        showedMessage = true;
                        ShowGuideText();
                    }
                    break;

                case "BegunTrust":
                    if (!showedAbility)
                    {
                        ShowTrustAbilityIcon(!showedAbility);
                        showedAbility = !showedAbility;

                        if (playerStatus == "Revived")
                        {
                            StartCoroutine(Alert("Middle", "Shaman revived you!", 3f));
                        }
                        else
                        {
                            string message = "Hunt all souls";
                            if (myRole != "Reaper") { message = "Protect all souls!"; }
                            StartCoroutine(Alert("Upper", message, 3f));
                        }
                    }
                    break;

                case "EndedTrust":
                    if (showedAbility)
                    {
                        ShowTrustAbilityIcon(!showedAbility);
                        showedAbility = !showedAbility;
                    }
                    break;

                case "GotVoteResult":
                    if (!showedResult)
                    {
                        showedResult = true;
                        ShowVoteResult(true);
                    }
                    break;

                case "KilledTarget":
                case "ShowingTarget":
                    if (currentState == "Truth"
                        && !showedTarget)
                    {
                        showedTarget = true;
                        ShowVoteResult(false);
                        ShowTarget(true);
                    }
                    break;
            }
            
            switch (currentState)
            {
                case "Trust":
                    if (PlayerAnimatorManagerSa.Instance.usedAbility)
                    {
                        TrustManager.Instance.AbilityButton.gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }


    //void ApplyModelColor(string targetTeam, GameObject model, string colorString)
    //{
    //    Renderer modelRenderer = model.gameObject.GetComponentInChildren<Renderer>();
    //    Material materialToApply;

    //    if (targetTeam == "Reaper")
    //    {
    //        materialToApply = modelRenderer.material;
    //    }
    //    else
    //    {
    //        Material[] modelMaterials = modelRenderer.materials;
    //        materialToApply = modelMaterials[0];
    //    }

    //    Color colorToApply = Color.clear;
    //    colorToApply = ConvertRGBAToColor(colorString);
    //    colorToApply.a = 1f;

    //    if (materialToApply != null)
    //    {
    //        materialToApply.SetColor("_TintColor", colorToApply);
    //    }
    //}
    //Color ConvertRGBAToColor(string rgbaString)
    //{
    //    string[] rgba = rgbaString.Substring(5, rgbaString.Length - 6).Split(", ");
    //    Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
    //    return color;
    //}

    void ShowGuideText()
    {
        switch (myTeam)
        {
            case "Reaper":
                if (dayCount == 1) { messageToShow = "Who's first?"; }
                else { messageToShow = "Who's next?"; }
                break;

            case "Sinner":
                switch (myRole)
                {
                    case "Butcher":
                        messageToShow = "Cook your sinful dish!";
                        break;
                    case "Druid":
                        messageToShow = "Find your magic flower!";
                        break;
                    case "Knight":
                        messageToShow = "Protect someone tonight!";
                        break;
                    case "Shaman":
                        messageToShow = "Revive someone tonight!";
                        break;
                    case "Stalker":
                        messageToShow = "Who will you stalk tonight?";
                        break;
                    case "Wizard":
                        messageToShow = "Find your wand!";
                        break;
                }
                break;
        }
        StartCoroutine(Alert("Upper", messageToShow, 3f));
    }

    public void ShowRole(bool isActive)
    {
        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        switch (myTeam)
        {
            case "Reaper":
                ReaperModel.SetActive(isActive);
                break;

            case "Sinner":
                myColor = (string)PhotonNetwork.LocalPlayer.CustomProperties["Color"];
                sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];

                for (int i = 0; i < sinnerRoles.Count; i++)
                {
                    if (sinnerRoles[i] == myRole)
                    {
                        roleIndex = i;
                    }
                }
                //ApplyModelColor(myTeam, SinnerModels[roleIndex], myColor);
                SinnerModels[roleIndex].SetActive(isActive);
                break;
        }

        UpperTextIcon.text = PhotonNetwork.LocalPlayer.NickName;
        LowerTextIcon.text = myRole;
        UpperTextIcon.gameObject.SetActive(isActive);
        LowerTextIcon.gameObject.SetActive(isActive);

        PlaygroundManager.Instance.showedRole = true;
    }

    void ShowVoteResult(bool isActive)
    {
        voteResult = (string)PhotonNetwork.CurrentRoom.CustomProperties["VoteResult"];

        Dictionary<int, string> messageOptions = new Dictionary<int, string>();
        switch (currentState)
        {
            case "Threat":
                switch (voteResult)
                {
                    case "FoundTarget":
                        messageOptions.Add(0, "Beware! \nYou might be next...");
                        messageOptions.Add(1, "Don't forget to lock your house! \nWe're coming...");
                        break;

                    case "Draw":
                        messageOptions.Add(0, "You all are so sinful...\nthat we cannot decide...");
                        messageOptions.Add(1, "We wanted to kill all of you! \nToo bad we can't...");
                        StartCoroutine(Alert("Middle", "Draw!", 3f));
                        break;

                    case "NoVoteFound":
                        messageOptions.Add(0, "Lucky you! \nWe're lazy today...");
                        messageOptions.Add(1, "You are given a chance! \nSee you soon...");
                        StartCoroutine(Alert("Middle", "No Vote!", 3f));
                        break;
                }
                reaperMessage = messageOptions[Random.Range(0, messageOptions.Count)];
                StartCoroutine(Alert("Upper", reaperMessage, 3f));
                break;

            case "Truth":
                switch (voteResult)
                {
                    case "FoundTarget":
                        messageOptions.Add(0, "I hope you are sure... \nLight it up!");
                        messageOptions.Add(1, "Let them burn!!!");
                        StartCoroutine(Alert("Middle", "Burned!", 3f));
                        break;

                    case "Draw":
                        messageOptions.Add(0, "Seems like there's a conflict here...");
                        messageOptions.Add(1, "Do you not trust each other?");
                        StartCoroutine(Alert("Middle", "Draw!", 3f));
                        break;

                    case "NoVoteFound":
                        messageOptions.Add(0, "Haven't you learnt anything last night?");
                        messageOptions.Add(1, "Good choice. \nLet your friend die tonight then!");
                        StartCoroutine(Alert("Middle", "No Vote!", 3f));
                        break;

                }
                priestMessage = messageOptions[Random.Range(0, messageOptions.Count)];
                StartCoroutine(Alert("Upper", priestMessage, 3f));
                break;
        }

        if (voteResult != "FoundTarget")
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "IconStatus", "ShowingVoteResult" } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "StateStatus", "TargetProtected" } });
        }
    }

    public IEnumerator Alert(string type, string message, float duration)
    {
        switch (type)
        {
            case "Upper":
                StatusManager.Instance.UpperText.text = message;
                UpperBanner.SetActive(true);
                StatusManager.Instance.UpperText.gameObject.SetActive(true);
                break;

            case "Middle":
                StatusManager.Instance.MiddleText.text = message;
                MiddleBanner.SetActive(true);
                StatusManager.Instance. MiddleText.gameObject.SetActive(true);
                break;
        }
        yield return new WaitForSeconds(duration);
        switch (type)
        {
            case "Upper":
                StatusManager.Instance.UpperText.text = "";
                UpperBanner.SetActive(false);
                StatusManager.Instance.UpperText.gameObject.SetActive(false);
                break;

            case "Middle":
                StatusManager.Instance.MiddleText.text = "";
                MiddleBanner.SetActive(false);
                StatusManager.Instance.MiddleText.gameObject.SetActive(false);
                break;
        }
    }

    void ShowTarget(bool isActive)
    {
        targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
        string targetName = targetSoulInfo["NickName"];
        string targetTeam = targetSoulInfo["Team"];
        string targetRole = targetSoulInfo["Role"];
        string targetColor = targetSoulInfo["Color"];

        //Debug.Log("targetTeam"+targetTeam);
        //Debug.Log("targetColor" + targetColor);
        switch (targetTeam)
        {
            case "Reaper":
                //ApplyModelColor(targetTeam, ReaperModel, targetColor);
                ReaperModel.SetActive(isActive);
                break;

            case "Sinner":
                Debug.Log("targetRole" + targetRole);
                sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
                for (int i = 0; i < sinnerRoles.Count; i++)
                {
                    if (sinnerRoles[i] == targetRole)
                    {
                        roleIndex = i;
                    }
                }
                //Debug.Log("roleIndex" + roleIndex);
                //ApplyModelColor(targetTeam, SinnerModels[roleIndex], targetColor);
                SinnerModels[roleIndex].SetActive(isActive);
                break;
        }

        UpperTextIcon.text = targetName;
        LowerTextIcon.text = targetRole;
        UpperTextIcon.gameObject.SetActive(isActive);
        LowerTextIcon.gameObject.SetActive(isActive);

        if (PhotonNetwork.IsMasterClient) { PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "IconStatus", "ShowingTarget" } }); }
    }

    public void ShowRoleForStalker(string roleToShow, bool isActive)
    {
        string playerTeam = "Reaper";
        if (roleToShow != playerTeam) { playerTeam = "Sinner"; }

        switch (playerTeam)
        {
            case "Reaper":
                ReaperModel.SetActive(isActive);
                break;

            case "Sinner":
                myColor = (string)PhotonNetwork.LocalPlayer.CustomProperties["Color"];
                sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];

                for (int i = 0; i < sinnerRoles.Count; i++)
                {
                    if (sinnerRoles[i] == roleToShow)
                    {
                        roleIndex = i;
                    }
                }
                //ApplyModelColor(myTeam, SinnerModels[roleIndex], myColor);
                SinnerModels[roleIndex].SetActive(isActive);
                break;
        }

        //PlayerNameText.text = NickName;
        LowerTextIcon.text = roleToShow;
        //PlayerNameText.gameObject.SetActive(isActive);
        LowerTextIcon.gameObject.SetActive(isActive);

        //if (isActive == true)
        //{
        //    RoleText.gameObject.SetActive(isActive);
        //}
    }

    public void ShowTrustAbilityIcon(bool isActive)
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }
        playerStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
        if (playerStatus.Contains("Soul")) { return; }

        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        reaperAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperAbility"];

        TrustManager.Instance.AbilityPanel.SetActive(isActive);
        TrustManager.Instance.AttackButton.gameObject.SetActive(isActive);
        AttackWeapon.SetActive(isActive);

        int TrustAbilityID = -1;
        switch (myRole)
        {
            case "Reaper":
                switch (reaperAbility)
                {
                    case "Butcher":
                        TrustAbilityID = 0;
                        break;

                    case "Druid":
                        TrustAbilityID = 1;
                        break;

                    case "Wizard":
                        TrustAbilityID = 2;
                        break;
                }
                break;

            case "Butcher":
                TrustAbilityID = 0;
                break;

            case "Druid":
                TrustAbilityID = 1;
                break;

            case "Wizard":
                TrustAbilityID = 2;
                break;
        }

        if (TrustAbilityID >= 0)
        {
            SinnerAbilities[TrustAbilityID].SetActive(isActive);
            TrustManager.Instance.AbilityButton.gameObject.SetActive(isActive);
            TrustManager.Instance.AbilityButton.enabled = true;
        }
    }

    public void OnReviveButtonClicked()
    {
        roleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
        playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

        for (int i = 0; i < roleAssignment.Length; i++)
        {
            if (roleAssignment[i] == "Reaper")
            {
                if (playerStatuses[i+1] != "Alive")
                {
                    playerStatuses[i+1] = "Revived";
                    break;
                }
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerStatuses", playerStatuses } });
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "ActivatedReaperAbility" } });
        ThreatManager.Instance.ReviveButton.gameObject.SetActive(false);
    }

    void OnSkipButtonClicked()
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "SkipVote" } });
    }

    void ShowTrustResult()
    {

    }

    public void EndGame()
    {
        EndingPopup.SetActive(true);
        string winnerTeam = (string)PhotonNetwork.CurrentRoom.CustomProperties["WinnerTeam"];
        string endMessage = "Defeated!";
        if (myTeam == winnerTeam)
        {
            endMessage = "Victory!";
        }
        WinnerText.text = endMessage;
        WinnerTextBack.text = WinnerText.text;
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        foreach (var prop in propertiesThatChanged)
        {
            switch (Convert.ToString(prop.Key))
            {
                case "StateStatus":
                    if (Convert.ToString(prop.Value).Contains("Ended"))
                    {
                        showedMessage = false;
                        showedResult = false;
                        showedTarget = false;
                    }
                    
                    switch (Convert.ToString(prop.Value))
                    {
                        case "TargetProtected":
                            StartCoroutine(Alert("Middle", "Target Protected!", 3f));
                            break;
                    }
                    break;
            }
        }
    }
}
