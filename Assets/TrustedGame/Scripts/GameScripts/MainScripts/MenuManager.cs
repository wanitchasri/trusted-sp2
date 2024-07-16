using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SENKOJI.Trusted
{
    public class MenuManager : MonoBehaviourPunCallbacks
    {
        public GameObject InRoomSettingParentPanel;
        public GameObject InRoomSettingChildPanel;

        public GameObject QuitPanel;
        public GameObject BackToLobbyPanel;

        public bool isInRoom;

    // Start is called before the first frame update
    void Start()
    {
            if (PhotonNetwork.InRoom)
            {
                isInRoom = true;
            }
            if (SceneManager.GetActiveScene().name != "LobbyScene")
            {
                if (((string)PhotonNetwork.CurrentRoom.CustomProperties["SetupStatus"] == "FinishedSetup"))
                {
                    isInRoom = true;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isInRoom)
                {
                    SetActivePanelOnTop(BackToLobbyPanel);
                } 
                else
                {
                    SetActivePanelOnTop(QuitPanel);
                }
            }
        }

        public void OnCloseButtonClicked(GameObject thisPanel)
        {
            thisPanel.SetActive(false);
        }

        public void SetActivePanelOnTop(GameObject thisPanel)
        {
            thisPanel.SetActive(true);
        }

        public void OnBackToLobbyButtonClicked()
        {
            Dictionary<int, string> playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
            playerStatuses[PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1] = "LeftRoom";
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Playerstatuses",  playerStatuses } });

            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LeaveRoom();
            // PhotonNetwork.Disconnect();

            OnCloseButtonClicked(BackToLobbyPanel);
            OnCloseButtonClicked(InRoomSettingChildPanel);
            OnCloseButtonClicked(InRoomSettingParentPanel);

            if (LobbyPanelManager.Instance != null)
            {
                LobbyPanelManager.Instance.SetActivePanelOnTop(LobbyPanelManager.Instance.LobbySettingPanel);
            }
        }

        public void OnQuitGameButtonClicked()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif

            Application.Quit();
        }
    }

}
