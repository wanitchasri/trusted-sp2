// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to handle typical game management requirements
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Realtime;
using Photon.Pun;
using TMPro;
using System;
using System.Linq;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Ken.Test
{
	#pragma warning disable 649

	/// <summary>
	/// Game manager.
	/// Connects and watch Photon Status, Instantiate Player
	/// Deals with quiting the room and the game
	/// Deals with level loading (outside the in room synchronization)
	/// </summary>
	public class GameManagerKen : MonoBehaviourPunCallbacks
    {

		#region Public Fields

		static public GameManagerKen Instance;
		//public GameObject bloodEffectObject;
		//public ParticleSystem bloodParticles;

		#endregion

		#region Private Fields

		//int playerChar = 0;

		public int playerNow = 0;
		private string actor1Name;
		private string actor2Name;
		private string actor3Name;
		private string actor4Name;
		public string role = "";
		public string special = "";
		public GameObject playerInstance;
		// private int numActors;

		public TextMeshProUGUI playerNameUI;
		public TextMeshProUGUI playerRoleUI;
		public TextMeshProUGUI playerTeamUI;

		//All Roles
		public TextMeshProUGUI cooldownTextHit;
		public float hitCooldownEndTime;

		//Reaper
		public TextMeshProUGUI reapercooldownTextSkill;
		public float reaperskillCooldownEndTime;
		
		//Sinner
		public TextMeshProUGUI sinnercooldownTextSkill;
		public float sinnerskillCooldownEndTime;

		//// The tag of the game object that should have its name changed
		//public string targetTag = "Player";

		//// The name to assign to the game object for each actor number
		//public string[] names = { "Sinner1", "Sinner2", "Reaper", "Soul" };

		[Tooltip("The prefab to use for representing the player")]
        [SerializeField] public GameObject player1;
		[SerializeField] public GameObject player2;
		[SerializeField] public GameObject player3;
		[SerializeField] public GameObject player4;
		public static bool gameIsPaused;

		#endregion

		#region MonoBehaviour CallBacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{
			Instance = this;
			//var player2 = "";
			//var player3 = "";
			//var player4 = "";
			//GameObject playerCharacter = GameObject.FindWithTag("Player");
			//Physics.IgnoreCollision(playerCharacter.GetComponent<Collider>(), GetComponent<Collider>());

			// in case we started this demo with the wrong scene being active, simply load the menu scene
			if (!PhotonNetwork.IsConnected)
			{
				SceneManager.LoadScene("Lobby");
				return;
			}
			
			if (player1 == null || player2 == null || player3 == null || player4 == null) { // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.
				
				Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
			} else {

				if (PlayerManagerKen.LocalPlayerInstance == null)
				{
					Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
					// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
					//int numPlayers = PhotonNetwork.CountOfPlayers;
					//for (int i = 0; i < numPlayers; i++)
					//{
					//	GameObject photonPun2 = PhotonNetwork.Instantiate(this.playerPrefab1.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
					//	Renderer robotRenderer = photonPun2.GetComponent<Renderer>();
					//	Color newColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
					//	Material robotMaterial = robotRenderer.material;
					//	robotMaterial.color = newColor;
					//}
					//GameObject playerInstance = PhotonNetwork.Instantiate(this.playerPrefab1.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
					// PlayerListsManager.Instance.showPlayerLists();

					// Find the game object with the tag "Player"
					//GameObject myGameObject = GameObject.FindGameObjectWithTag("Player");
					//Debug.Log("Found game object with tag 'Player': " + myGameObject.name);
					string[] roleChoice = { "Sinner", "Reaper", "TargetSoul" };
					string[] specialCharacter = { "Wizard", "Butcher", "Reaper", "TargetSoul" };

					// Check the local player's actor number and rename the game object accordingly
					if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
					{
						// Get the role custom property for a remote player
						role = roleChoice[0];
						special = specialCharacter[0];
						playerRoleUI.text = "(" + special + ")";
						playerTeamUI.text = role;
						//playerInstance = PhotonNetwork.Instantiate(this.player1.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
						playerInstance = PhotonNetwork.Instantiate(this.player1.name, new Vector3(1.37f, 0.30f, 30f), Quaternion.identity, 0);
						// Set the name of the game object based on the actor number
						//myGameObject.name = "Sinner1";
						//Debug.Log("Renamed game object to 'Sinner1'");
					}
					else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
					{
						// Get the role custom property for a remote player
						role = roleChoice[0];
						special = specialCharacter[1];
						playerRoleUI.text = "(" + special + ")";
						playerTeamUI.text = role;
						playerInstance = PhotonNetwork.Instantiate(this.player2.name, new Vector3(1.37f, 0.30f, 30f), Quaternion.identity, 0);
						// Set the name of the game object based on the actor number
						//myGameObject.name = "Sinner2";
						//Debug.Log("Renamed game object to 'Sinner2'");
					}
					else if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
					{
						// Get the role custom property for a remote player
						role = roleChoice[1];
						special = specialCharacter[2];
						playerRoleUI.text = "(" + special + ")";
						playerTeamUI.text = role;
						playerInstance = PhotonNetwork.Instantiate(this.player3.name, new Vector3(1.37f, 0.30f, 30f), Quaternion.identity, 0);
						// Set the name of the game object based on the actor number
						//myGameObject.name = "Reaper";
						//Debug.Log("Renamed game object to 'Reaper'");
					}
					else if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
					{
						// Get the role custom property for a remote player
						role = roleChoice[2];
						special = specialCharacter[2];
						playerRoleUI.text = "(" + special + ")";
						playerTeamUI.text = role;
						playerInstance = PhotonNetwork.Instantiate(this.player4.name, new Vector3(1.37f, 0.30f, 30f), Quaternion.identity, 0);
						// Set the name of the game object based on the actor number
						//myGameObject.name = "Soul";
						//Debug.Log("Renamed game object to 'Soul'");
					}
					//This is only for the Computer owner
					PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Role", role } });
					PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Special", special } });

					if (PhotonNetwork.IsMasterClient)
                    {
						string[] playerRoles = { "Sinner", "Sinner", "Reaper", "TargetSoul" };
						string[] playerSpecials = { "Wizard", "Butcher", "Reaper", "TargetSoul" };
						// index = 0,1,2,3 // actornumber = 1,2,3,4
						//This is for the whole room not only one
						PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerRoles", playerRoles } });
						PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerSpecials", playerSpecials } });
					}
					

					//// The way to make spawn many characters for us in one times
					//if (player1=="" && player2=="" && player3 == "" && player4 == "")
					//	// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
					//	PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f,5f,0f), Quaternion.identity, 0);
					//	player1 = this.playerPrefab.name;
					//if (player1 != "" && player2 == "" && player3 == "" && player4 == "")
					//	// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
					//	PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(2f, 5f, 0f), Quaternion.identity, 0);
					//	player2 = this.playerPrefab.name;
					//if (player1 != "" && player2 != "" && player3 == "" && player4 == "")
					//	// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
					//	PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(4f, 5f, 0f), Quaternion.identity, 0);
					//	player3 = this.playerPrefab.name;
					//if (player1 != "" && player2 != "" && player3 != "" && player4 == "")
					//	// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
					//	PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(6f, 5f, 0f), Quaternion.identity, 0);
					//	player4 = this.playerPrefab.name;
				}
				else
				{

						Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
					}


			}
		}

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
		{
			//if (PlayerManager.LocalPlayerInstance != null && playerChar == 0)
			//         {
			//	Destroy(playerPrefab1);
			//	PhotonNetwork.Instantiate(this.playerPrefab2.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
			//	playerChar += 1;
			//}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				JustLeave();
			}

			if (Input.GetKeyDown(KeyCode.P))
			{
				gameIsPaused = !gameIsPaused;
				PauseGame();
			}

			//if (Input.GetKeyDown(KeyCode.C))
			//{
			//	Debug.Log("Change Character");
			//	ChangeCharacter();
				//}

				//if (Input.GetKeyDown(KeyCode.P))
				//{
				//	Time.timeScale = 0;
				//	Debug.Log("Paused and press C button to continue");
				//	// $ renpy.pause(5.0, hard = True) #will stop them from clicking
				//	// GetComponent<ChatManager>().enabled = false;
				//	// Cursor.visible = false;
				//}
				//if (Input.GetKeyDown(KeyCode.C))
				//{
				//	Time.timeScale = 1;
				//	Debug.Log("Continued and press P button to pause");

				//	// GetComponent<ChatManager>().enabled = true;
				//	// Cursor.visible = true;
				//}
				// "back" button of phone equals "Escape". quit app if that's pressed

				//if (Input.GetKeyDown(KeyCode.Escape))
				//{
				//	QuitApplication();
				//}
			//}
		}

		//      void ChangeCharacter()
		//{
		// oldChar = GameObject.FindGameObjectsWithTag("Player");
		//SkinnedMeshRenderer viewedModelFilter1 = (SkinnedMeshRenderer)gameObject.GetComponent("Robot2");
		//viewedModel1 = viewedModelFilter1;
		//SkinnedMeshRenderer viewedModelFilter2 = (SkinnedMeshRenderer)gameObject.GetComponent("Root");
		//viewedModel2 = viewedModelFilter2;
		//SkinnedMeshRenderer viewedModelFilter3 = (SkinnedMeshRenderer)gameObject.GetComponent("Body");
		//viewedModel3 = viewedModelFilter3;
		//SkinnedMeshRenderer viewedModelFilter4 = (SkinnedMeshRenderer)gameObject.GetComponent("Armature");
		//viewedModel4 = viewedModelFilter4;

		//if (player == "Robot")
		//{
		//	Debug.Log("ChangeToGirl");
		//             //PhotonNetwork.Replace(oldChar, playerPrefab2);
		//             //viewedModel1.SetActive(false);
		//             GameObject Robot1 = GameObject.FindWithTag("Part1");
		//             Robot1.SetActive(false);
		//             GameObject Robot2 = GameObject.FindWithTag("Part2");
		//             Robot2.SetActive(false);
		//             GameObject Girl1 = GameObject.FindWithTag("Part3");
		//             Girl1.SetActive(true);
		//             GameObject Girl2 = GameObject.FindWithTag("Part4");
		//             Girl2.SetActive(true);
		//             player = "Girl";
		//}
		//else if (player == "Girl")
		//{
		//	Debug.Log("ChangeToRobot");
		//	//PhotonNetwork.Replace(oldChar, playerPrefab1);
		//	GameObject Robot1 = GameObject.FindWithTag("Part1");
		//             Robot1.SetActive(true);
		//             GameObject Robot2 = GameObject.FindWithTag("Part2");
		//             Robot2.SetActive(true);
		//             GameObject Girl1 = GameObject.FindWithTag("Part3");
		//             Girl1.SetActive(false);
		//             GameObject Girl2 = GameObject.FindWithTag("Part4");
		//             Girl2.SetActive(false);
		//             player = "Robot";
		//}
		//}

		void PauseGame()
		{
			if (gameIsPaused)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = 1;
			}
		}
		
		#endregion

		#region Photon Callbacks

		/// <summary>
		/// Called when a Photon Player got connected. We need to then load a bigger scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerEnteredRoom( Player other  )
		{
			Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting
			Debug.Log("ActorNumber: " + other.ActorNumber);
			if ( PhotonNetwork.IsMasterClient )
			{
				Debug.LogFormat( "OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient ); // called before OnPlayerLeftRoom

				//PhotonNetwork.IsMessageQueueRunning = false;
				LoadArena();
				
			}

			//numActors = PhotonNetwork.CurrentRoom.PlayerCount;

			//// If the local player is the second actor to join the room, make them the new first actor
			//if (numActors == 2 && PhotonNetwork.LocalPlayer.ActorNumber == 2)
			//{
			//	PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "actorNumber", 1 } });
			//}
			//else
			//{
			//	// Update the actor numbers for the remaining players
			//	UpdateActorNumbers();
			//}

			// Find the player with actor number 1 and get their name
			Player actor1Player = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == 1);
			if (actor1Player != null)
			{
				actor1Name = actor1Player.NickName;
				Debug.Log("Actor 1 name is: " + actor1Name);
			}
			else
			{
				Debug.Log("No player with actor number 1 in the room.");
			}

			// Find the player with actor number 2 and get their name
			Player actor2Player = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == 2);
			if (actor2Player != null)
			{
				actor2Name = actor2Player.NickName;
				Debug.Log("Actor 2 name is: " + actor2Name);
			}
			else
			{
				Debug.Log("No player with actor number 2 in the room.");
			}

			// Find the player with actor number 3 and get their name
			Player actor3Player = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == 3);
			if (actor3Player != null)
			{
				actor3Name = actor3Player.NickName;
				Debug.Log("Actor 3 name is: " + actor3Name);
			}
			else
			{
				Debug.Log("No player with actor number 3 in the room.");
			}

			// Find the player with actor number 4 and get their name
			Player actor4Player = PhotonNetwork.PlayerList.FirstOrDefault(player => player.ActorNumber == 4);
			if (actor4Player != null)
			{
				actor4Name = actor4Player.NickName;
				Debug.Log("Actor 4 name is: " + actor4Name);
			}
			else
			{
				Debug.Log("No player with actor number 4 in the room.");
			}
		}

		/// <summary>
		/// Called when a Photon Player got disconnected. We need to load a smaller scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPlayerLeftRoom( Player other  )
		{
			Debug.Log("OnPlayerLeftRoom() " + other.NickName ); // seen when other disconnects
			Debug.Log("ActorNumber: " + other.ActorNumber);
			if (PhotonNetwork.IsMasterClient)
			{
				Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
				LoadArena();
			}

			if (ActorNumberChanger.Instance != null)
			{
				ActorNumberChanger.Instance.DecreasingActorNumber();
			}

			//numActors = PhotonNetwork.CurrentRoom.PlayerCount;

			//// Update the actor numbers for the remaining players
			//UpdateActorNumbers();
		}

		//public override void OnJoinedRoom()
		//      {
		//	// Find the game object with the target tag
		//	GameObject target = GameObject.FindGameObjectWithTag(targetTag);

		//	// Get the local player's actor number
		//	int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

		//	// Set the name of the game object based on the actor number
		//	target.name = names[actorNumber - 1];
		//}

		//public override void OnJoinedRoom()
		//{
		//	numActors = PhotonNetwork.CurrentRoom.PlayerCount;

		//	// Set the initial actor number for the local player
		//	int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
		//	PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "actorNumber", actorNumber } });
		//}

		/// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene("Lobby");
		}

		//private void UpdateActorNumbers()
		//{
		//	// Get the current actor number for the local player
		//	int actorNumber = (int)PhotonNetwork.LocalPlayer.CustomProperties["actorNumber"];

		//	// Update the actor numbers for the remaining players
		//	foreach (Player player in PhotonNetwork.PlayerList)
		//	{
		//		if (player.ActorNumber > actorNumber)
		//		{
		//			player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "actorNumber", player.ActorNumber - 1 } });
		//		}
		//	}
		//}

		#endregion

		#region Public Methods

		//public void OnLeaveRoomButtonClicked()
		//{
		//	LeaveRoomScript leaveRoomScript = GetComponent<LeaveRoomScript>();
		//	leaveRoomScript.LeaveRoom();
		//}

		//public void LeaveRoom()
		//{
		//    PhotonNetwork.LeaveRoom();
		//}

		public void JustLeave()
        {
			PhotonNetwork.LeaveRoom();
        }

        public bool LeaveRoom()
        {
            return PhotonNetwork.LeaveRoom();
        }



        public void QuitApplication()
		{
			Application.Quit();
		}

        #endregion

        #region Private Methods
        public void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }

            //Debug.LogFormat( "PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount );
            // PhotonNetwork.LoadLevel("Room for "+PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", 1);
			//PhotonNetwork.LoadLevel("Room for " + 1);
			PhotonNetwork.LoadLevel("Trusted");
			playerNow = PhotonNetwork.CurrentRoom.PlayerCount;
            Debug.Log("playerNow: " + playerNow);

        }

  //      private void OnLevelWasLoaded(int level)
  //      {
		//	PhotonNetwork.IsMessageQueueRunning = true;
		//}
        #endregion

    }

}