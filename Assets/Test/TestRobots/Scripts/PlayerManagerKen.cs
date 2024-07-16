// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using System;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

namespace Ken.Test
{
	#pragma warning disable 649

    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManagerKen : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields

        public static PlayerManagerKen Instance;

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public static int count = 0;

        public TextMeshProUGUI playerNameText;

        #endregion

        #region Private Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject playerUiPrefab;

        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        public GameObject axe;
        // public GameObject beams;
        // public GameObject beamsLatifa;
        private GameObject playerUI;
        // private GameObject playerUILatifa;
        //True, when the user is firing
        bool IsFiring;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {
            Instance = this;

            //if (this.axe == null)
            //{
            //    Debug.LogError("<Color=Red><b>Missing</b></Color> Axe Reference.", this);
            //}
            //else
            //{
            //    this.axe.SetActive(true);
            //}

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                this.photonView.RPC("NamePlayerInstance", RpcTarget.AllBuffered, this.photonView.ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
            //RectTransform rectTransform = playerUiPrefab.GetComponent<RectTransform>();

            //// Check if the Canvas component is attached to the player UI prefab
            //Canvas canvas = rectTransform.GetComponent<Canvas>();
            //if (canvas != null)
            //{
            //    // Adjust the Order in Layer property of the Canvas component
            //    canvas.sortingOrder = -1;
            //}

            //playerUiPrefab.GetComponent<RectTransform>().SetSiblingIndex(0);

            //// Set the health bar as a child of the parent panel
            //playerUiPrefab.transform.SetParent(Panel, false);
            //// Set the parent panel's sorting order to control the position of the health bar relative to other UI elements
            //PanelOpener.Instance.Panel.GetComponent<Canvas>().sortingOrder = 1;

            //int playerLayer = LayerMask.NameToLayer("Player");

            // ignore collisions between the player's layer and the child objects' layer
            //Physics.IgnoreLayerCollision(playerLayer, gameObject.layer, true);

            CameraWorkKen _cameraWork = gameObject.GetComponent<CameraWorkKen>();

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

            if (this.playerUiPrefab != null)
            {
                //playerUiPrefab.AddComponent<Canvas>();
                //playerUiPrefab.GetComponent<Canvas>().sortingOrder = -1;
                GameObject _uiGo = Instantiate(this.playerUiPrefab);
                // _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
                playerUI = _uiGo;
                playerUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            #if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif
        }

        public override void OnDisable()
		{
			// Always call the base to remove callbacks
			base.OnDisable ();

			#if UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
			#endif
		}

        private bool leavingRoom;

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Show and hide the beams
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {
            //if (playerUI != null)
            //{
            //    DeactivateByTag();
            //}

            // we only process Inputs and check health if we are the local player
            if (photonView.IsMine)
            {
                //this.ProcessInputs();
                //playerUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
                if (this.Health <= 0f && !this.leavingRoom)
                {
                    this.leavingRoom = GameManagerKen.Instance.LeaveRoom();
                }
            }

            //if (this.axe != null && this.IsFiring != this.axe.activeInHierarchy)
            //{
            //    this.axe.SetActive(this.IsFiring);
            //}

            //if (GameManagerKen.player == "Robot")
            //{
            //    // playerUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            //    if (this.axe != null && this.IsFiring != this.axe.activeInHierarchy)
            //    {
            //        this.axe.SetActive(this.IsFiring);
            //    }
            //}
            //if (GameManager.player == "Girl")
            //{
            //    // playerUI.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            //    if (this.beamsLatifa != null && this.IsFiring != this.beamsLatifa.activeInHierarchy)
            //    {
            //        this.beamsLatifa.SetActive(this.IsFiring);
            //    }
            //}
        }
        public void DeactivateByTag()
        {
            if (PanelOpener.Instance.Panel.activeSelf == true)
            {
                playerUI.SetActive(false);
                Debug.Log("isActive");
            }
            else
            {
                playerUI.SetActive(true);
                Debug.Log("isNotActive");
            }
        }

        public override void OnLeftRoom()
        {
            this.leavingRoom = false;
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        //public void OnTriggerEnter(Collider other)
        //{
        //    if (!photonView.IsMine)
        //    {
        //        return;
        //    }

        //    //// Ignore collisions with self
        //    //if (other.transform.parent != null && other.transform.parent.gameObject == gameObject)
        //    //{
        //    //    return;
        //    //}

        //    // We are only interested in Beamers
        //    // we should be using tags but for the sake of distribution, let's simply check by name.
        //    if (!other.name.Contains("Axe"))
        //    {
        //        return;
        //    }

        //    //Debug.Log(other.name);
        //    //photonView.RPC("TakeDamage1", RpcTarget.All, 0.1f);

        //    // photonView.RPC("TakeDamage1", RpcTarget.All, 0.1f);

        //    // Call TakeDamage method on target player using PhotonView component
        //    //PhotonView target = other.GetComponent<PhotonView>();
        //    //if (target != null && target.IsMine)
        //    //{
        //    //    target.RPC("TakeDamage1", RpcTarget.All);
        //    //    Debug.Log(other.name);
        //    //}

        //    //if (!this.GetComponent<PhotonView>().IsMine)
        //    //{
        //    //    photonView.RPC("TakeDamage1", RpcTarget.All);
        //    //}

        //    PhotonView target = other.GetComponentInParent<PhotonView>();
        //    if (target != null && target.IsMine)
        //    {
        //        target.RPC("TakeDamage1", RpcTarget.Others, 0.1f);
        //        Debug.Log(target.name);
        //    }
        //}

        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are interesting the player
        /// </summary>
        /// <param name="other">Other.</param>
        //public void OnTriggerStay(Collider other)
        //{
        //    // we dont' do anything if we are not the local player.
        //    if (!photonView.IsMine)
        //    {
        //        return;
        //    }

        //    //// Ignore collisions with self
        //    //if (other.transform.parent != null && other.transform.parent.gameObject == gameObject)
        //    //{
        //    //    return;
        //    //}

        //    // We are only interested in Beamers
        //    // we should be using tags but for the sake of distribution, let's simply check by name.
        //    if (!other.name.Contains("Axe"))
        //    {
        //        return;
        //    }

        //    // Call TakeDamage method on target player using PhotonView component
        //    PhotonView target = other.GetComponent<PhotonView>();
        //    if (target != null && target.IsMine)
        //    {
        //        target.RPC("TakeDamage2", RpcTarget.All, 0.1f);
        //    }

        //    //if (!this.GetComponent<PhotonView>().IsMine)
        //    //{
        //    //    photonView.RPC("TakeDamage2", RpcTarget.All);
        //    //}

        //    // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        //    //this.Health -= 0.1f*Time.deltaTime;
        //    //Debug.Log(other.name);
        //}

        [PunRPC]
        void NamePlayerInstance(int viewID, int playerNumber) 
        {
            Debug.Log("viewID"+viewID);
            Debug.Log("playerNumber" + playerNumber);
            if (this.gameObject.GetComponent<PhotonView>().ViewID == viewID)
            {
                this.gameObject.name = "Player" + playerNumber;
                if (playerNameText != null)
                {
                    playerNameText.text = this.photonView.Owner.NickName;
                    GameManagerKen.Instance.playerNameUI.text = this.photonView.Owner.NickName;
                }

                //if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
                //{
                //    this.gameObject.name = "Sinner1";
                //}
                //else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
                //{
                //    this.gameObject.name = "Sinner2";
                //}
                //else if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
                //{
                //    this.gameObject.name = "Reaper";
                //}
                //else if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
                //{
                //    this.gameObject.name = "TargetSoul";
                //}
            }
        }

        [PunRPC]
        public void TakeDamage(float damage, int targetViewID)
        {
            if (this.photonView.ViewID == targetViewID)
            {
                Debug.Log(targetViewID + " I take damage");
                this.Health -= damage;
            }
        }

        [PunRPC]
        public void SoulTaken(float damage, int targetViewID)
        {
            if (this.photonView.ViewID == targetViewID)
            {
                Debug.Log(targetViewID + " has been taken soul!!!");
                this.Health -= damage;
            }
        }

        [PunRPC]
        void DebugRPC(string ms)
        {
            Debug.Log(ms);
        }

        //[PunRPC]
        //public void TakeDamage2()
        //{
        //    this.Health -= 0.1f * Time.deltaTime;
        //}

#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


        /// <summary>
        /// MonoBehaviour method called after a new level of index 'level' was loaded.
        /// We recreate the Player UI because it was destroy when we switched level.
        /// Also reposition the player if outside the current arena.
        /// </summary>
        /// <param name="level">Level index loaded</param>
        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Private Methods


		#if UNITY_5_4_OR_NEWER
		void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
		{
			this.CalledOnLevelWasLoaded(scene.buildIndex);
		}
#endif

        /// <summary>
        /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
        /// </summary>
        //void ProcessInputs()
        //{
        //    if (!GameManagerKen.gameIsPaused)
        //    {
        //        if (Input.GetButtonDown("Fire1"))
        //        {
        //            // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
        //            // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
        //            if (EventSystem.current.IsPointerOverGameObject())
        //            {
        //                //	return;
        //            }

        //            if (!this.IsFiring)
        //            {
        //                this.IsFiring = true;
        //            }
        //        }

        //        if (Input.GetButtonUp("Fire1"))
        //        {
        //            if (this.IsFiring)
        //            {
        //                this.IsFiring = false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //    }
        //}

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                //stream.SendNext(this.IsFiring);
                stream.SendNext(this.Health);
            }
            else
            {
                // Network player, receive data
                //this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}