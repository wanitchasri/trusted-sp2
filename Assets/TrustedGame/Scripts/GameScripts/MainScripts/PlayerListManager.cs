using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;

public class PlayerListManager : MonoBehaviourPunCallbacks
{ 
    public static PlayerListManager Instance;

    [SerializeField] private TMPro.TextMeshProUGUI _playerListText;
    public GameObject[] Lines;

    Dictionary<int, string> playerStatuses;

    void Start()
    {
        Instance = this;
        // Clear the text before displaying the player names
        _playerListText.SetText("");
    }

    public void UpdatePlayerList()
    {
        playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
        string playerListString = ""; // "Player List: \n";

        for (int i = 0; i<PhotonNetwork.PlayerList.Length; i++)
        {
            string playerName = PhotonNetwork.PlayerList[i].NickName;
            playerListString += playerName + "\n";

            if (playerStatuses != null)
            {
                if (playerStatuses[i + 1].Contains("Soul") || playerStatuses[i + 1] == "Revived")
                {
                    Lines[i].gameObject.SetActive(true);
                }

                if (playerStatuses[i + 1] == "LeftRoom")
                {
                    Lines[i].gameObject.SetActive(false);
                }
            }
        }

        _playerListText.text = playerListString;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        foreach (var prop in propertiesThatChanged)
        {
            switch (Convert.ToString(prop.Key))
            {
                case "PlayerStatuses":
                    if (!PhotonNetwork.InRoom) { UpdatePlayerList(); }
                    break;
            }
        }
    }

}
