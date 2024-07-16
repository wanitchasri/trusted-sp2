using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using TMPro;

public class TrustManager : MonoBehaviourPunCallbacks
{
    public static TrustManager Instance;

    [Header("Spawners")]
    public Transform[] SinnerSpawners;
    public Transform TargetSpawner;
    public Transform[] ReaperSpawners;

    [Header("Ability Buttons")]
    public GameObject AbilityPanel;
    public Button AttackButton;
    public Button AbilityButton;

    [Header("Text")]
    public TMP_Text AttackCoolDownText;
    public TMP_Text AbilityCoolDownText;

    [Header("Time Info")]
    public float AttackCoolDownTime;
    public float AbilityCoolDownTime;

    [Header("Game Info")]
    string currentState;

    [Header("Flags")]
    bool sentMessage;
    bool timeIsUp;
    bool endedHunt;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        AttackCoolDownText = AttackCoolDownText.GetComponent<TMP_Text>();
        AbilityCoolDownText = AbilityCoolDownText.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        if (currentState != "Trust") { return; }

    }

    public void OnAttackButtonClicked()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }
        PlayerAnimatorManagerSa.Instance.attackButtonClicked = true;
    }

    public void OnAbilityButtonClicked()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }
        PlayerAnimatorManagerSa.Instance.abilityButtonClicked = true;
    }
}
