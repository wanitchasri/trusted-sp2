using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;
using System.Linq;

namespace SENKOJI.Trusted
{
    public class LobbyPanelManager : MonoBehaviourPunCallbacks
    {
        static public LobbyPanelManager Instance;
        public bool isInRoom;

        [Header("Login Panel")]
        public GameObject LoginPanel;
        public TMP_InputField PlayerNameInput;

        [Header("Selection Panel")]
        public GameObject SelectionPanel;

        [Header("Room Type Panel")]
        public GameObject RoomTypePanel;
        public bool isPrivateRoom;

        [Header("Create Private Room Panel")]
        public GameObject CreatePrivateRoomPanel;
        public TMP_InputField PrivateRoomNameInputField;
        public TMP_InputField PasswordInputField;
        // public InputField MaxPlayersInputField;

        [Header("Create Public Room Panel")]
        public GameObject CreatePublicRoomPanel;
        public TMP_InputField PublicRoomNameInputField;

        [Header("Join Random Room Panel")]
        public GameObject JoinRandomRoomPanel;

        [Header("Room List Panel")]
        public GameObject RoomListPanel;
        public GameObject RoomListContent;
        public GameObject RoomListEntryPrefab;

        [Header("Inside Room Panel")]
        public GameObject InsideRoomPanel;
        public TMP_Text InsideRoomNameText;
        public TMP_Text InsideRoomNameTextBack;

        public Button StartGameButton;
        public GameObject PlayerListEntryPrefab;
        private Dictionary<string, RoomInfo> cachedRoomList;
        private Dictionary<string, GameObject> roomListEntries;
        private Dictionary<int, GameObject> playerListEntries;

        [Header("Setting Panel")]
        public GameObject LobbySettingPanel;
        public GameObject InRoomSettingPanel;

        [Header("Sounds")]
        AudioSource audioSource;
        public AudioClip LobbyMusic;

        #region UNITY

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            cachedRoomList = new Dictionary<string, RoomInfo>();
            roomListEntries = new Dictionary<string, GameObject>();

            PlayerNameInput = PlayerNameInput.GetComponent<TMP_InputField>();
            PrivateRoomNameInputField = PrivateRoomNameInputField.GetComponent<TMP_InputField>();
            PasswordInputField = PasswordInputField.GetComponent<TMP_InputField>();

            PlayerNameInput.text = "Player " + UnityEngine.Random.Range(1000, 10000);
        }

        public void Start()
        {   
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = 0.1f;
            audioSource.clip = LobbyMusic;
            audioSource.loop = true; // set loop to true
            audioSource.Play();
        }


        //private void Update()
        //{
        //}

        #endregion

       #region PUN CALLBACKS

        public override void OnConnectedToMaster()
        {
            this.SetActivePanel(SelectionPanel.name);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();

            UpdateCachedRoomList(roomList);
            UpdateRoomListView();
        }

        public override void OnJoinedLobby()
        {
            // whenever this joins a new lobby, clear any previous room lists
            cachedRoomList.Clear();
            ClearRoomListView();
        }

        // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
        public override void OnLeftLobby()
        {
            cachedRoomList.Clear();
            ClearRoomListView();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Room " + UnityEngine.Random.Range(1000, 10000);

            RoomOptions options = new RoomOptions {MaxPlayers = 8};

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public override void OnJoinedRoom()
        {
            // joining (or entering) a room invalidates any cached lobby room list (even if LeaveLobby was not called due to just joining a room)
            cachedRoomList.Clear();

            SetActivePanel(InsideRoomPanel.name);
            // SetActivePanelOnTop(LobbySettingPanel);
            SetActivePanelOnTop(InRoomSettingPanel);

            isInRoom = true;
            InsideRoomNameText.text = PhotonNetwork.CurrentRoom.Name;
            InsideRoomNameTextBack.text = InsideRoomNameText.text;

            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerListEntryPrefab);
                entry.transform.SetParent(InsideRoomPanel.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<PlayerListEntrySa>().Initialize(p.ActorNumber, p.NickName);

                object isPlayerReady;
                if (p.CustomProperties.TryGetValue(TrustedGameProperties.PLAYER_READY, out isPlayerReady))
                {
                    entry.GetComponent<PlayerListEntrySa>().SetPlayerReady((bool)isPlayerReady);
                }

                playerListEntries.Add(p.ActorNumber, entry);
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());

            Hashtable props = new Hashtable
            {
                {TrustedGameProperties.PLAYER_LOADED_LEVEL, false}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public override void OnLeftRoom()
        {
            SetActivePanelOnTop(LobbySettingPanel);
            SetActivePanel(SelectionPanel.name);

            isInRoom = false;

            foreach (GameObject entry in playerListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            playerListEntries.Clear();
            playerListEntries = null;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(InsideRoomPanel.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListEntrySa>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

            playerListEntries.Add(newPlayer.ActorNumber, entry);

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                StartGameButton.gameObject.SetActive(CheckPlayersReady());
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            GameObject entry;
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                object isPlayerReady;
                if (changedProps.TryGetValue(TrustedGameProperties.PLAYER_READY, out isPlayerReady))
                {
                    entry.GetComponent<PlayerListEntrySa>().SetPlayerReady((bool)isPlayerReady);
                }
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }


        public void LocalPlayerPropertiesUpdated()
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        #endregion

        #region UI CALLBACKS

        public void OnBackButtonClicked()
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            SetActivePanel(SelectionPanel.name);
        }

        public void OnCreateRoomButtonClicked()
        {
            string roomName = PublicRoomNameInputField.text;
            
            if (isPrivateRoom)
            {
                string password = PasswordInputField.text;
            }

            roomName = (roomName.Equals(string.Empty)) ? "Room " + UnityEngine.Random.Range(1000, 10000) : roomName;
               
            byte maxPlayers;
            //byte.TryParse(MaxPlayersInputField.text, out maxPlayers);
            //maxPlayers = (byte) Mathf.Clamp(maxPlayers, 2, 8);
            maxPlayers = 8;

            RoomOptions options = new RoomOptions {MaxPlayers = maxPlayers, PlayerTtl = 10000 };

            PhotonNetwork.CreateRoom(roomName, options, null);
            SetActivePanelOnTop(InRoomSettingPanel);
        }

        public void OnJoinRandomRoomButtonClicked()
        {
            SetActivePanel(JoinRandomRoomPanel.name);
            OnCloseButtonClicked(LobbySettingPanel);

            PhotonNetwork.JoinRandomRoom();
        }

        public void OnLeaveGameButtonClicked()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void OnGoButtonClicked()
        {
            string playerName = PlayerNameInput.text.ToString();

            if (!playerName.Equals(""))
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                Debug.LogError("Player Name is invalid.");
            }
        }

        public void OnStartGameButtonClicked()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

                int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
                int[] characterNumbers = new int[maxPlayers];
                for (int i=0; i<maxPlayers; i++)
                {
                    characterNumbers[i] = i;
                    //Debug.Log("characterNumber=" + i);
                }

                System.Random random = new System.Random();
                characterNumbers = characterNumbers.OrderBy(x => random.Next()).ToArray();
                //foreach (var i in characterNumbers)
                //{
                //    Debug.Log("characterMeshOrder=" + i);
                //}

                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "CharacterMeshOrder", characterNumbers } });
            }
        }

        public void OnRoomListButtonClicked()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            SetActivePanel(RoomListPanel.name);
        }

        private void ClearRoomListView()
        {
            foreach (GameObject entry in roomListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            roomListEntries.Clear();
        }

        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }

        private void UpdateRoomListView()
        {
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomListEntryPrefab);
                entry.transform.SetParent(RoomListContent.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<RoomListEntrySa>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

                roomListEntries.Add(info.Name, entry);
            }
        }

        #endregion

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }


            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object isPlayerReady;
                if (p.CustomProperties.TryGetValue(TrustedGameProperties.PLAYER_READY, out isPlayerReady))
                {
                    if (!(bool)isPlayerReady)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
            {
                return true;
            }

            return false;
        }

        public void SetActivePanel(string activePanel)
        {
            // Debug.Log("activePanel"+ activePanel);

            LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
            SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));

            RoomTypePanel.SetActive(activePanel.Equals(RoomTypePanel.name));
            CreatePublicRoomPanel.SetActive(activePanel.Equals(CreatePublicRoomPanel.name));
            CreatePrivateRoomPanel.SetActive(activePanel.Equals(CreatePrivateRoomPanel.name));

            JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));

            RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
        }

        public void OnCloseButtonClicked(GameObject thisPanel)
        {
            thisPanel.SetActive(false);
        }

        public void SetActivePanelOnTop(GameObject thisPanel)
        {
            thisPanel.SetActive(true);
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);

            foreach (var prop in propertiesThatChanged)
            {
                if (Convert.ToString(prop.Key) == "CharacterMeshOrder")
                {
                    //if (!PhotonNetwork.CurrentRoom.AddPlayer(PhotonNetwork.LocalPlayer))
                    //{
                    //    PhotonNetwork.CurrentRoom.StorePlayer(PhotonNetwork.LocalPlayer);
                    //}
                    if (audioSource != null)
                    {
                        audioSource.Stop();
                    }

                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.LoadLevel("TrustedGameScene");
                    }
                }
            }
        }
    }
}