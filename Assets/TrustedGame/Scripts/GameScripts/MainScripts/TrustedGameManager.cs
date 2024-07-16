using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;
using System.Collections.Generic;
using TMPro;

using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class TrustedGameManager : MonoBehaviourPunCallbacks
{

	static public TrustedGameManager Instance;
    private GameObject instance;

	[Header("Map")]
	public GameObject ReaperSide;
	public GameObject SinnerSide;
	public GameObject Bridge;
	public GameObject RoleRevealBoxes;
	public GameObject TargetRevealBox;

    [Header("Sounds")]
    AudioSource audioSource;
    public AudioClip SetUpSound;
    public AudioClip ThreatSound;
    public AudioClip RevealTargetSoud;
    public AudioClip TrustSound;
    public AudioClip TruthSound;

    bool updatedPlayerList;
	Dictionary<int, string> playerStatuses;

	#region MonoBehaviour CallBacks

	void Start()
	{
		Instance = this;

        audioSource = GetComponent<AudioSource>();
        // in case we started this demo with the wrong scene being active, simply load the menu scene
        //if (!PhotonNetwork.IsConnected)
        //{
        //	SceneManager.LoadScene("LobbyScene");
        //	return;
        //}
    }


	void Update()
	{
		// "back" button of phone equals "Escape". quit app if that's pressed
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			QuitApplication();
		}

		if (PlayerListManager.Instance != null
		&& !updatedPlayerList)
        {
			updatedPlayerList = true;
			PlayerListManager.Instance.UpdatePlayerList();
		}

	}

    #endregion

    #region Photon Callbacks

    public override void OnCreatedRoom()
	{
		base.OnCreatedRoom();
	}

    /// <summary>
    /// Called when a Photon Player got connected.
    /// </summary>
    /// <param name="other">Other.</param>
    //public override void OnPlayerEnteredRoom(Player other)
    //{
    //	Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting

    //	if (PhotonNetwork.IsMasterClient)
    //	{
    //		Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
    //	}

    //}

    void TurnSoundOn(AudioClip audioClip)
    {
		audioSource.Stop();
		audioSource.volume = 0.1f;
        audioSource.clip = audioClip;
        audioSource.loop = true; // set loop to true
        audioSource.Play();
    }

    /// <summary>
    /// Called when a Photon Player got disconnected.
    /// </summary>
    /// <param name="other">Other.</param>
    public override void OnPlayerLeftRoom(Player other)
	{
		Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects

		playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
		playerStatuses[other.GetPlayerNumber() + 1] = "LeftRoom";
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerStatuses", playerStatuses } }); 

		PlayerListManager.Instance.UpdatePlayerList();

		if (other.IsMasterClient)
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
        }

        if (PhotonNetwork.IsMasterClient)
		{
			Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
		}
	}

	public override void OnLeftRoom()
	{
		SceneManager.LoadScene("LobbyScene");
	}

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }

		base.OnRoomPropertiesUpdate(propertiesThatChanged);
		foreach (var prop in propertiesThatChanged)
		{
			switch (Convert.ToString(prop.Key))
            {
				case "GameStatus":
					switch (Convert.ToString(prop.Value))
                    {
						case "SetupGame":
							RoleRevealBoxes.SetActive(true);
							if (SetUpSound != null)
                            {
								TurnSoundOn(SetUpSound);
							}
                            break;

						case "Threat":
							ReaperSide.SetActive(true);
							SinnerSide.SetActive(true);
							if (ThreatSound != null)
							{
								TurnSoundOn(ThreatSound);
							}
                            break;

						case "RevealTargetSoul":
							TargetRevealBox.SetActive(true);
							ReaperSide.SetActive(false);
							SinnerSide.SetActive(false);
							if (RevealTargetSoud != null)
							{
								TurnSoundOn(RevealTargetSoud);
							}
							break;

						case "Trust":
							ReaperSide.SetActive(true);
							SinnerSide.SetActive(true);
							Bridge.SetActive(true);
							if (TrustSound != null)
							{
								TurnSoundOn(TrustSound);
							}
							break;

						case "Truth":
							Bridge.SetActive(false);
							SinnerSide.SetActive(true);
							if (TruthSound != null)
							{
								TurnSoundOn(TruthSound);
							}
                            break;
					}
					break;

            }
		}
	}

    #endregion


    #region Public Methods 

    public bool LeaveRoom()
	{
		return PhotonNetwork.LeaveRoom();
	}

	public void QuitApplication()
	{
		Application.Quit();
	}

	#endregion


}
