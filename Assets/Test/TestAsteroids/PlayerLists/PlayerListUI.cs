using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Ken.Asteroids;

public class PlayerListUI : MonoBehaviourPunCallbacks
{ 
    public static PlayerListUI Instance;
    [SerializeField] private TMPro.TextMeshProUGUI _playerListText;
    //public string playerListString;

    void Start()
    {
        Instance = this;
        // Clear the text before displaying the player names
        _playerListText.SetText("");
    }

    public void UpdatePlayerList()
    {
        string playerListString = "Player List: \n";

        for (int i = 0; i<PhotonNetwork.PlayerList.Length; i++)
        {
            string playerName = PhotonNetwork.PlayerList[i].NickName;
            playerListString += playerName + "\n";
        }

        Debug.Log("Updateeeee the Player Name");
        _playerListText.text = playerListString;
    }

    //public override void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    UpdatePlayerList();
    //}

    //public override void OnPlayerLeftRoom(Player otherPlayer)
    //{
    //    UpdatePlayerList();
    //}
}
