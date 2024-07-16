using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System;
using TMPro;

#pragma warning disable 649

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManagerSa : MonoBehaviourPunCallbacks, IPunObservable
{
    public static PlayerManagerSa Instance;

    [Header("Player Info")]
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    string myStatus;

    [Header("Body Objects")]
    public GameObject[] HumanBodies;
    public GameObject[] SinnerBodies;
    public GameObject ReaperBody;
    public GameObject RevivedBody;
    [SerializeField] private GameObject myPreviousBody;
    [SerializeField] public GameObject myCurrentBody;
    public GameObject TargetSoulAura;

    [Header("Avatars")]
    public Avatar FantasyAvatar;
    public Avatar DungeonAvatar;
    public Avatar HorrorAvatar;

    [Header("Root")]
    [SerializeField] GameObject rootToEnable;
    [SerializeField] GameObject rootToDisable;
    public GameObject HumanRoot;
    public GameObject SinnerRoot;
    public GameObject ReaperRoot;

    [Header("Materials")]
    public Material[] DressMaterials;   // Red, Green, Purple, Silver, Gold
    public Material SoulMaterial;
    public Material TargetSoulMaterial;
    public Material TakenSoulMaterial;
    public Material WizardMaterial;

    [Header("Weapons")]
    public GameObject ReaperWeapon;
    public GameObject StolenReaperWeapon;
    public GameObject[] SinnerWeapons;
    public GameObject[] RevivedSinnerWeapons;
    [SerializeField] GameObject myWeapon;

    [Header("UI")]
    public TMP_Text PlayerNameText;
    //[SerializeField]
    //private GameObject playerUiPrefab;

    [Header("Game Info")]
    string currentState;
    string stateStatus;
    string cloneStatus;
    //Dictionary<int, string> sinnerRoles;
    //private bool leavingRoom;

    [Header("Flags")]
    bool checkedReaperClones;
    bool checkedKnightClones;
    bool checkedShamanClones;
    bool checkedStalkerClones;

    [Header("Audio")]
    //AudioSource audioSource;
    //public AudioClip BurnSound;
    public GameObject BurnEffect;

    public float Health = 1f;
    bool destroyedComponents;

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    public void Awake()
    {
        Instance = this;
        PlayerNameText = PlayerNameText.GetComponent<TMP_Text>();

        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
        switch (currentState)
        {
            case "BegunThreat":
            case "MovedPlayersToTheirPosition":
                //bool unspawnedClones = true;
                //do
                //{
                //    string cloneStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["CloneStatus"];
                //    if (cloneStatus != "Unspawned")
                //    {
                //        unspawnedClones = false;
                //        stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
                        
                //        string cloneRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
                //        if (cloneStatus == "SpawnedClonesForReaper") { cloneRole = "Reaper"; }
                //        this.photonView.RPC("NameCloneInstance", RpcTarget.All, this.photonView.ViewID, cloneRole);
                //    }
                //}
                //while (unspawnedClones);
                break;
        }

        switch ((string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"])
        {
            case "Threat":
                break;
            
            default:
                if (photonView.IsMine)
                {
                    LocalPlayerInstance = this.gameObject;
                    this.photonView.RPC("NamePlayerInstance", RpcTarget.All, this.photonView.ViewID, PhotonNetwork.LocalPlayer.GetPlayerNumber());
                    this.photonView.RPC("DressHuman", RpcTarget.All, this.photonView.ViewID, PhotonNetwork.LocalPlayer.GetPlayerNumber());
                }
                DontDestroyOnLoad(gameObject);
                break;
        }
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    public void Start()
    {
        //audioSource = GetComponent<AudioSource>();

        switch ((string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"])
        {
            case "Threat":
                if (this.gameObject.name.Contains("Clone"))
                {
                    return;
                }
                break;

            case "Trust":
                if (PhotonNetwork.IsMasterClient)
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
                break;

            default:
                InitializePlayer();
                break;
        }

#if UNITY_5_4_OR_NEWER
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif

    }

    [PunRPC]
    void NamePlayerInstance(int viewID, int playerNumber) //
    {
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            int thisPlayerNumber = playerNumber + 1;
            this.gameObject.name = "Player " + thisPlayerNumber;
            PlayerNameText.text = this.photonView.Owner.NickName;
        }
    }

    [PunRPC]
    void NameCloneInstance(int viewID, string responsibleRole)
    {
        //Debug.Log("responsibleRole" + responsibleRole);
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            string cloneName = this.gameObject.name;
            //cloneName = cloneName.Replace("Player", "VoteClone");

            cloneName = cloneName.Replace("Player", "CloneFor" + responsibleRole);
            this.gameObject.name = cloneName.Replace("(Clone)", "");

            // Debug.Log("NameCloneInstance() : cloneName" + cloneName + " responsibleRole" + responsibleRole);
            int cloneNumber = Convert.ToInt32(this.gameObject.name.Replace("CloneFor" + responsibleRole + " ", ""));

            // Debug.Log("GetLastCloneNumber() :" +  GetLastCloneNumber(responsibleRole));
            if (cloneNumber == GetLastCloneNumber(responsibleRole))
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "CloneStatus", "RenamedClonesFor" + responsibleRole } });
                //this.photonView.RPC("LetsDebug", RpcTarget.All, " >> cloneName:" + cloneName);
            }
        }
    }
    int GetLastCloneNumber(string responsibleRole)
    {
        int maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];
        string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
        //Debug.Log("GetLastCloneNumber() >> getRoleAssignment[maxPlayer - 1]"+getRoleAssignment[maxPlayers - 1]+ " responsibleRole" + responsibleRole + " maxPlayers"+maxPlayers);
        if (getRoleAssignment[maxPlayers - 1] == responsibleRole)
        {
            maxPlayers -= 1;
        }
        return maxPlayers;
    }

    [PunRPC]
    void DressHuman(int viewID, int playerNumber)
    {
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            int[] meshOrder = (int[])PhotonNetwork.CurrentRoom.CustomProperties["CharacterMeshOrder"];
            int myOrder = meshOrder[playerNumber];

            myCurrentBody = HumanBodies[myOrder];
            myCurrentBody.SetActive(true);

            if (this.photonView.IsMine)
            {
                string meshName = myCurrentBody.gameObject.name;
                string colorCode = meshName.Substring(meshName.Length - 1);
                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Color", colorCode } });
            }
        }
    }

    [PunRPC]
    void SetPlayerNameActive(int viewID, bool isActive)
    {
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            PlayerNameText.gameObject.SetActive(isActive);
        }
    }

    [PunRPC]
    void DressPlayer(int viewID, int playerNumber, string bodyType)
    {
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            Animator animator = this.gameObject.GetComponent<Animator>();
            string[] playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
            Dictionary<int, string> playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

            string myRole = playerRoles[playerNumber];
            string myStatus = playerStatuses[playerNumber+1];

            int mySinnerID = -1;
            if (myRole != "Reaper")
            {
                mySinnerID = GetSinnerID(myRole);
                ReaperBody.gameObject.SetActive(false);
                ReaperRoot.transform.name = "DisableRoot";
                RevivedBody.gameObject.SetActive(false);
                RevivedSinnerWeapons[mySinnerID].SetActive(false);
            }

            int[] meshOrder = (int[])PhotonNetwork.CurrentRoom.CustomProperties["CharacterMeshOrder"];
            int myHumanID = meshOrder[playerNumber];
            switch (bodyType)
            {
                case "Human":
                    rootToEnable = HumanRoot;
                    if (myRole == "Reaper")
                    {
                        myPreviousBody = ReaperBody;
                        rootToDisable = ReaperRoot;
                    }
                    else
                    {
                        myPreviousBody = SinnerBodies[mySinnerID];
                        if (myRole == "Knight") { rootToDisable = ReaperRoot; }
                        else { rootToDisable = SinnerRoot; }
                    }
                    myCurrentBody = HumanBodies[myHumanID];
                    animator.avatar = FantasyAvatar;
                    break;

                case "Role":
                    rootToDisable = HumanRoot;
                    if (myRole == "Reaper")
                    {
                        myPreviousBody = HumanBodies[myHumanID];
                        myCurrentBody = ReaperBody;
                        animator.avatar = DungeonAvatar;
                        rootToEnable = ReaperRoot;
                    }
                    else
                    {
                        myPreviousBody = HumanBodies[myHumanID];
                        myCurrentBody = SinnerBodies[mySinnerID];
                        if (myRole == "Knight")
                        {
                            animator.avatar = DungeonAvatar;
                            rootToEnable = ReaperRoot;
                        }
                        else
                        {
                            animator.avatar = FantasyAvatar;
                            rootToEnable = SinnerRoot;
                        }
                    }
                    break;

                case "Revived":
                    if (myRole == "Reaper")
                    {
                        myPreviousBody = ReaperBody;
                        myCurrentBody = ReaperBody;
                    }
                    else
                    {
                        myPreviousBody = SinnerBodies[mySinnerID];
                        myCurrentBody = RevivedBody;
                    }
                    rootToDisable = SinnerRoot;
                    rootToEnable = ReaperRoot;
                    break;
            }

            currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
            if (currentState == "Truth")
            {
                if (myRole == "Reaper")
                {
                    string reaperWizardAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperWizardAbility"];
                    if (reaperWizardAbility != "" && reaperWizardAbility != null)
                    {
                        if (reaperWizardAbility.Contains("Swap"))
                        {
                            string swappedRole = reaperWizardAbility.Replace("Swap", "");
                            int swappedSinnerID = GetSinnerID(swappedRole);
                            SinnerBodies[swappedSinnerID].gameObject.SetActive(false);
                            SinnerRoot.transform.name = "DisableRoot";
                            StolenReaperWeapon.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (myRole == "Wizard")
                    {
                        for (int i = 0; i < SinnerBodies.Length; i++)
                        {
                            SinnerBodies[i].gameObject.SetActive(false);
                            RevivedSinnerWeapons[mySinnerID].gameObject.SetActive(false);
                        }
                    }
                    else if (myStatus.Contains("Soul"))
                    {
                        RevivedBody.gameObject.SetActive(false);
                        SinnerBodies[5].gameObject.SetActive(false); // Make sure wizard skill is dismissed
                        SinnerRoot.gameObject.name = "DisableRoot";

                        switch (myRole)
                        {
                            case "Reaper":
                                ReaperWeapon.gameObject.SetActive(false);
                                break;

                            default:
                                RevivedSinnerWeapons[mySinnerID].gameObject.SetActive(false);
                                break;
                        }
                    }
                }
            }

            myPreviousBody.gameObject.SetActive(false);
            rootToDisable.gameObject.name = "DisableRoot";
            rootToEnable.gameObject.name = "Root";
            myCurrentBody.gameObject.SetActive(true);

            animator.Rebind();
            animator.applyRootMotion = true;
        }
    }
    int GetSinnerID(string role)
    {
        Dictionary<int, string> sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
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
    void BurnSuspect(int viewID, bool burning)
    {
        if (burning) 
        {
            BurnEffect.SetActive(true);
            //audioSource.clip = BurnSound;
            //audioSource.volume = 0.5f;
            //audioSource.loop = true;
            //audioSource.Play();
        }
        else
        {
            BurnEffect.SetActive(false);
            //audioSource.Stop();
        }
        // PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerParticle", "Burning" }  });
    }

    [PunRPC]
    void HoldWeapon(int viewID, int playerNumber, bool isActive)
    {
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            string[] playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
            Dictionary<int, string> playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
            string myRole = playerRoles[playerNumber];
            string myStatus = playerStatuses[playerNumber + 1];

            int mySinnerID = -1;
            if (myRole != "Reaper") 
            {
                Dictionary<int, string> sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
                for (int i = 0; i < sinnerRoles.Count; i++)
                {
                    if (myRole == sinnerRoles[i])
                    {
                        mySinnerID = i;
                    }
                }

                if (myStatus.Contains("Revived") || myStatus.Contains("Soul"))
                {
                    myWeapon = RevivedSinnerWeapons[mySinnerID];
                }
                else
                {
                    myWeapon = SinnerWeapons[mySinnerID];
                }
            }
            else
            {
                myWeapon = ReaperWeapon;
            }

            myWeapon.gameObject.SetActive(isActive);
        }
    }

    [PunRPC]
    void ChangeToSoulColor(int viewID, string soulType)
    {
        if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
        {
            SkinnedMeshRenderer playerRenderer = myCurrentBody.transform.GetComponent<SkinnedMeshRenderer>();
            Material[] rendererMaterials = playerRenderer.materials;
            switch (soulType)
            {
                case "TargetSoul":
                    rendererMaterials[0] = TargetSoulMaterial;
                    TargetSoulAura.SetActive(true);
                    break;

                case "Soul":
                    rendererMaterials[0] = SoulMaterial;
                    TargetSoulAura.SetActive(false);
                    break;

                case "TakenSoul":
                    rendererMaterials[0] = TakenSoulMaterial;
                    TargetSoulAura.SetActive(false);
                    break;
            }
            playerRenderer.materials = rendererMaterials;
        }
    }

    void InitializePlayer()
    {
        PlayerCameraWork _cameraWork = gameObject.GetComponent<PlayerCameraWork>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
        }

        // Create the UI
        //if (this.playerUiPrefab != null)
        //{
        //    GameObject _uiGo = Instantiate(this.playerUiPrefab);
        //    _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        //}
        //else
        //{
        //    Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
        //}
    }

    public override void OnDisable()
	{
		// Always call the base to remove callbacks
		base.OnDisable ();

#if UNITY_5_4_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
    }

    [PunRPC]
    void NameClone(int instanceID, string infoName)
    {
        if (this.gameObject.name.Contains("(Clone)"))
        {
            Dictionary<int, string> cloneInfo = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties[infoName];
            foreach (KeyValuePair<int, string> info in cloneInfo)
            {
                // Debug.Log(this.gameObject.name + " " + info.Key + " " + instanceID + " ");
                if (info.Key == instanceID)
                {
                    string cloneName = this.gameObject.name.Replace("Player", "CloneFor" + info.Value);
                    this.gameObject.name = cloneName.Replace("(Clone)", "");
                }
            }
        }
    }

    [PunRPC]
    void Disappear()
    {
        Destroy(this.gameObject);
    }

    public void Update()
    {
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];

        if (this.gameObject.name.Contains("CloneFor"))
        {
            if (!destroyedComponents)
            {
                destroyedComponents = true;
                Destroy(this.gameObject.GetComponent<PlayerCameraWork>());
                Destroy(this.gameObject.GetComponent<CharacterController>());
                Destroy(this.gameObject.GetComponent<PlayerAnimatorManagerSa>());
            }

            if (currentState != "Threat")
            {
                this.photonView.RPC("Disappear", RpcTarget.AllViaServer);
            }
        }

        if (!this.photonView.IsMine) { return; }

        //if (!handledClone)
        //{
        //    handledClone = true;
        //    if ((string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"] != null
        //        && (string)PhotonNetwork.CurrentRoom.CustomProperties["CloneStatus"] != null)
        //    {
        //        stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
        //        string cloneStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["CloneStatus"];

        //        // Debug.Log("cloneStatus"+cloneStatus);
        //        if (stateStatus == "BegunThreat"
        //            || cloneStatus.Contains("RenamedClonesFor"))
        //        {
        //            if (this.gameObject.name.Contains("Clone"))
        //            {
        //                string thisRole = "";
        //                if (stateStatus == "BegunThreat") { thisRole = "Reaper"; }
        //                else
        //                {
        //                    string previousRole = cloneStatus.Replace("RenamedClonesFor", "");
        //                    switch (previousRole)
        //                    {
        //                        case "Reaper": thisRole = "Knight"; break;
        //                        case "Knight": thisRole = "Shaman"; break;
        //                        case "Shaman": thisRole = "Stalker"; break;
        //                    }
        //                }

        //                this.photonView.RPC("NameCloneInstance", RpcTarget.All, this.photonView.ViewID, thisRole);
        //            }
        //            return;
        //        }
        //    }
        //}

        // playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        //this.photonView.RPC("LetsDebug", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber + " " + playerNumber);
        // we only process Inputs and check health if we are the local player
        //if (photonView.IsMine)
        //{
        //    this.ProcessInputs();

        //    if (this.Health <= 0f && !this.leavingRoom)
        //    {
        //        this.leavingRoom = TrustedGameManager.Instance.LeaveRoom();
        //    }
        //}

        //if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy)
        //{
        //    this.beams.SetActive(this.IsFiring);
        //}

        // Sa's Test
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Debug.Log("Moving all players to same position");

        //    if (PhotonNetwork.IsMasterClient)
        //    {
        //        this.photonView.RPC("MoveAllToSamePosition", RpcTarget.All);
        //    }
        //}

        //if (photonView.IsMine)
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha2))
        //    {
        //        MoveOnlyLocalPlayer();
        //        //this.photonView.RPC("MoveOnlyLocalPlayer", RpcTarget.All);
        //    }

        //}

        // --
    }

    //public override void OnLeftRoom()
    //{
    //    this.leavingRoom = false;
    //}

    /// <summary>
    /// MonoBehaviour method called when the Collider 'other' enters the trigger.
    /// Affect Health of the Player if the collider is a beam
    /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
    /// One could move the collider further away to prevent this or check if the beam belongs to the player.
    /// </summary>
    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }


        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam"))
        {
            return;
        }

        //this.Health -= 0.1f;
    }

    /// <summary>
    /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
    /// We're going to affect health while the beams are interesting the player
    /// </summary>
    /// <param name="other">Other.</param>
    public void OnTriggerStay(Collider other)
    {
        // we dont' do anything if we are not the local player.
        if (!photonView.IsMine)
        {
            return;
        }

        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam"))
        {
            return;
        }

        // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        //this.Health -= 0.1f*Time.deltaTime;
    }


    /// <summary>
    /// MonoBehaviour method called after a new level of index 'level' was loaded.
    /// We recreate the Player UI because it was destroy when we switched level.
    /// Also reposition the player if outside the current arena.
    /// </summary>
    /// <param name="level">Level index loaded</param>
    //void CalledOnLevelWasLoaded(int level)
    //{
    //    // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
    //    //if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
    //    //{
    //    //    transform.position = new Vector3(0f, 5f, 0f);
    //    //}

    //    //GameObject _uiGo = Instantiate(this.playerUiPrefab);
    //    //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    //}

    #endregion

    #region Private Methods


#if UNITY_5_4_OR_NEWER
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        // this.CalledOnLevelWasLoaded(scene.buildIndex);
    }
#endif

#if UNITY_5_4_OR_NEWER
    /// <summary> See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
    void OnLevelWasLoaded(int level)
    {
        // this.CalledOnLevelWasLoaded(level);
    }
#endif

    /// <summary>
    /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
    /// </summary>
    void ProcessInputs()
    {
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
        //    // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
        //    if (EventSystem.current.IsPointerOverGameObject())
        //    {
        //        //	return;
        //    }

        //    if (!this.IsFiring)
        //    {
        //        this.IsFiring = true;
        //    }
        //}

        //if (Input.GetButtonUp("Fire1"))
        //{
        //    if (this.IsFiring)
        //    {
        //        this.IsFiring = false;
        //    }
        //}
    }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting)
        //{
        //    // We own this player: send the others our data
        //    stream.SendNext(this.IsFiring);
        //    stream.SendNext(this.Health);
        //}
        //else
        //{
        //    // Network player, receive data
        //    this.IsFiring = (bool)stream.ReceiveNext();
        //    this.Health = (float)stream.ReceiveNext();
        //}
    }

    #endregion

    //[PunRPC]
    //public void MoveAllToSamePosition()
    //{
    //    this.GetComponent<CharacterController>().Move(new Vector3(5f, 5f, 0f));
    //}

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (!this.photonView.IsMine) { return; }

        Dictionary<int, string> playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
        string myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];

        foreach (var prop in propertiesThatChanged)
        {
            switch (Convert.ToString(prop.Key))
            {
                case "CloneStatus":
                    cloneStatus = Convert.ToString(prop.Value);
                    switch (cloneStatus)
                    {

                        case "SpawnedClonesForReaper":
                            if (!checkedReaperClones)
                            {
                                checkedReaperClones = true;
                                this.photonView.RPC("NameClone", RpcTarget.AllViaServer, this.gameObject.GetPhotonView().ViewID, "ReaperCloneInfo");
                            }
                            break;
                        case "SpawnedClonesForKnight":
                            if (!checkedKnightClones)
                            {
                                checkedKnightClones = true;
                                this.photonView.RPC("NameClone", RpcTarget.AllViaServer, this.gameObject.GetPhotonView().ViewID, "KnightCloneInfo");
                            }
                            break;
                        case "SpawnedClonesForShaman":
                            if (!checkedShamanClones)
                            {
                                checkedShamanClones = true;
                                this.photonView.RPC("NameClone", RpcTarget.AllViaServer, this.gameObject.GetPhotonView().ViewID, "ShamanCloneInfo");
                            }
                            break;
                        case "SpawnedClonesForStalker":
                            if (!checkedStalkerClones)
                            {
                                checkedStalkerClones = true;
                                this.photonView.RPC("NameClone", RpcTarget.AllViaServer, this.gameObject.GetPhotonView().ViewID, "StalkerCloneInfo");
                            }
                            break;
                    }
                    break;

                case "GameStatus":
                    if (this.gameObject.name.Contains("Clone")) { return; }
                    currentState = Convert.ToString(prop.Value);
                    int myPlayerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
                    switch (currentState)
                    {
                        case "Threat":
                            this.photonView.RPC("SetPlayerNameActive", RpcTarget.All, this.photonView.ViewID, true);
                            switch (myStatus)
                            {
                                case "Alive":
                                    this.photonView.RPC("DressPlayer", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, "Human");
                                    break;

                                case "Soul":
                                case "TakenSoul":
                                    this.photonView.RPC("DressPlayer", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, "Role");
                                    this.photonView.RPC("ChangeToSoulColor", RpcTarget.AllViaServer, this.photonView.ViewID, myStatus);
                                    break;
                            }
                            break;
                        case "RevealTargetSoul":
                            this.photonView.RPC("SetPlayerNameActive", RpcTarget.All, this.photonView.ViewID, false);
                            if (!myStatus.Contains("Revived"))
                            {
                                this.photonView.RPC("DressPlayer", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, "Role");
                                switch (myStatus)
                                {
                                    case "Alive":
                                        this.photonView.RPC("HoldWeapon", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, true);
                                        break;


                                    case "Soul":
                                    case "TakenSoul":
                                        this.photonView.RPC("ChangeToSoulColor", RpcTarget.AllViaServer, this.photonView.ViewID, myStatus);
                                        break;
                                }
                            }
                            else
                            {
                                this.photonView.RPC("DressPlayer", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, "Revived");
                                this.photonView.RPC("HoldWeapon", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, true);
                            }
                            break;

                        case "Trust":
                            this.photonView.RPC("SetPlayerNameActive", RpcTarget.All, this.photonView.ViewID, false);
                            if (myStatus == "TargetSoul")
                            {
                                this.photonView.RPC("ChangeToSoulColor", RpcTarget.AllViaServer, this.photonView.ViewID, "TargetSoul");
                            }
                            break;
                        case "Truth":
                            this.photonView.RPC("SetPlayerNameActive", RpcTarget.All, this.photonView.ViewID, true);
                            this.photonView.RPC("HoldWeapon", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, false);
                            switch (myStatus)
                            {
                                case "Alive":
                                    this.photonView.RPC("DressPlayer", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, "Human");
                                    break;

                                case "Soul":
                                case "TakenSoul":
                                    this.photonView.RPC("DressPlayer", RpcTarget.All, this.photonView.ViewID, myPlayerNumber, "Role");
                                    this.photonView.RPC("ChangeToSoulColor", RpcTarget.AllViaServer, this.photonView.ViewID, myStatus);
                                    break;
                            }
                            break;
                    }
                    break;

                case "StateStatus":
                    if (this.gameObject.name.Contains("Clone")) { return; }
                    stateStatus = Convert.ToString(prop.Value);
                    switch (stateStatus)
                    {
                        case "EndedTrust":
                            if (myStatus == "TargetSoul")
                            {
                                this.photonView.RPC("ChangeToSoulColor", RpcTarget.AllViaServer, this.photonView.ViewID, "Soul");
                                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Soul" } });

                                playerStatuses[PhotonNetwork.LocalPlayer.GetPlayerNumber() + 1] = "Soul";
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerStatuses", playerStatuses } });
                            }
                            else if (myStatus.Contains("Revived"))
                            {
                                string soulStatus = myStatus.Replace("Revived", "");
                                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", soulStatus } });

                                playerStatuses[PhotonNetwork.LocalPlayer.GetPlayerNumber()+1] = "Soul";
                                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerStatuses", playerStatuses } });
                            }
                            break;
                        case "EndedTruth":
                            if (myStatus == "BurnedSoul")
                            {
                                this.photonView.RPC("BurnSuspect", RpcTarget.AllViaServer, this.photonView.ViewID, false);
                                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Soul" } });
                            }
                            break;
                    }
                    break;

                case "WinnerTeam":
                    this.photonView.RPC("ShowTheWinner", RpcTarget.All);
                    break;
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        foreach (var prop in changedProps)
        {
            //if ((Convert.ToString(prop.Key) == "PlayerStatus")) 
            //{
            //    switch (Convert.ToString(prop.Value))
            //    {
                    
            //    }
            //}
        }
    }

    
}