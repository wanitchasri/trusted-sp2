// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerAnimatorManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player Animator Component controls.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun;
using System.Collections;
using System;
using Photon.Realtime;
using TMPro;

namespace Ken.Test
{
	public class PlayerAnimatorManagerKen : MonoBehaviourPunCallbacks
	{
		//[SerializeField] private float _rightBoundary = 0f;
		//[SerializeField] private float _leftBoundary = 0f;
		public float speed = 1;
		public static PlayerAnimatorManagerKen Instance;
		public float h;
		public float v;
		public int count1 = 0;
		public int count2 = 0;
		public int count3 = 0;
		public int count4 = 0;
		public string player = "";
		public string myRole = "";
		public string mySpecial = "";
		public GameObject Robot;
		public bool SpeedReaperAssign = false;
		public bool SpeedSinnerAssign = false;
		public bool SpeedSoulAssign = false;
		public bool SoulCharacterAssign = false;

		public ParticleSystem burnEffect;
		public AudioClip burnEffectClip;
		public ParticleSystem bloodEffect;

		public GameObject TargetSoulMark;

		public ParticleSystem TargetEffect;

		public AudioClip walkEffectClip;
		public bool nowWalk = false;

		//For All Roles
		public float AttackCooldown = 5.0f;
		public AudioClip attackEffectClip;
		public bool CanAttack = true;

		//Got Hit Sound
		public AudioClip reaperGotAttackEffectClip;
		public AudioClip sinnerGotAttackEffectClip;

		//For All Reapers & Reaper Nurse
		public float effectDurationSlowed = 5.0f;
		public bool GotSlow = false;

		//For All Sinners & Nurse
		public float effectDurationStunted = 5.0f;
		public bool GotStunt = false;

		//For Reaper Butcher
		public float effectDurationReaperFaster = 5.0f;
		public bool RunReaperFaster = false;

		//For Butcher
		public float effectDurationSinnerFaster = 5.0f;
		public bool RunSinnerFaster = false;

		//private float stuntDuration;
		//private float slowDuration;
		// public float rangeSkills = 10;
		// public Transform target;

		#region Private Fields

		[SerializeField]
	    private float directionDampTime = 0.25f;
		public Animator animator;
		public static bool Slowed;
		public static bool Stunted;
		public static bool Teleported;
		public static bool WasKilled;
		public static bool WasBurned;
		public static bool WizardSwap;
		public static bool SoulSwap;
		public static bool showTargetSoulAlready;
		public static bool showReaperAlready;
		// public static bool WasAttacked;

		public GameObject Robot1;
		public GameObject Robot2;
		public GameObject Girl1;
		public GameObject Girl2;
		public Avatar Avatar1;
		public Avatar Avatar2;
		public GameObject Character;
        private int CharacterIndex;
        private static object Characters;

        #endregion

        #region MonoBehaviour CallBacks

        private void Awake()
        {
			Instance = this;
			
		}
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
		
        void Start () 
	    {
			animator = GetComponent<Animator>();
            PhotonView photonView = this.gameObject.GetComponent<PhotonView>();
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
		void Update () 
	    {
			
			// Prevent control is connected to Photon and represent the localPlayer
	        if( photonView.IsMine == false && PhotonNetwork.IsConnected == true )
	        {
				return;
	        }

			// failSafe is missing Animator component on GameObject
	        if (!animator)
	        {
				return;
			}

            // deal with Jumping
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);			

            // only allow jumping if we are running.
            if (stateInfo.IsName("Base Layer.Run"))
            {
				//photonView.RPC("WalkOrNot", RpcTarget.All);
				// When using trigger parameter
				if (Input.GetButtonDown("Fire2"))
                {
					animator.SetTrigger("Jump");
				}	
            }

			// deal with movement
			h = Input.GetAxis("Horizontal");
			v = Input.GetAxis("Vertical");

            // prevent negative speed.
            if (v < 0)
            {
                v = 0;
            }

			// set the Animator Parameters
			animator.SetFloat("Speed", h * h+ v * v);
			// Debug.Log("h " +  h);
			// Debug.Log("v " + v);
			animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);

			//if (transform.position.x >= _rightBoundary)
			//{
			//	transform.position = new Vector3(_rightBoundary, transform.position.y, 0);
			//}
			//else if (transform.position.x <= _leftBoundary)
			//{
			//	transform.position = new Vector3(_leftBoundary, transform.position.y, 0);
			//}
			// Debug.Log("Start Speed");
			//if (animator.speed > 0)
			//         {
			//	// get the AudioSource component and configure it
			//	AudioSource walkSound = GetComponent<AudioSource>();
			//	walkSound.volume = 0.5f;
			//	walkSound.clip = walkEffectClip;
			//	walkSound.loop = true;
			//	walkSound.Play();
			//}
			//else
			//{
			//	// get the AudioSource component and configure it
			//	AudioSource walkSound = GetComponent<AudioSource>();
			//	walkSound.Stop();
			//}

			if (Input.GetKeyDown(KeyCode.T))
			{
				TeleportButton();
				//if (PhotonNetwork.IsMasterClient)
				//{
				//    photonView.RPC("TeleportButton", RpcTarget.All);
				//}
			}

			// float dist = Vector3.Distance(target.position, transform.position);
			if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] != null)
			{
				myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
			}
			this.photonView.RPC("DebugRPC", RpcTarget.All, "My role is.." + myRole);

			if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Special"] != null)
			{
				mySpecial = (string)PhotonNetwork.LocalPlayer.CustomProperties["Special"];
			}
			this.photonView.RPC("DebugRPC", RpcTarget.All, "My special is.." + mySpecial);

			if (myRole != "")
            {
				SpecialSklls();
				PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SeeTargetSoul", "SawTargetSoul" } });
				if (myRole == "Reaper" && !SpeedReaperAssign)
                {
					animator.speed = 1.2f;
                    Debug.Log("Reaper Speed already assign");
					SpeedReaperAssign = true;
					player = "Robot";
				}
                if (myRole == "Sinner" && !SpeedSinnerAssign)
                {
                    animator.speed = 1f;
                    Debug.Log("Sinner Speed already assign");
					SpeedSinnerAssign = true;
					player = "Robot";
				}
				if (myRole == "TargetSoul" && !SpeedSoulAssign)
				{
					animator.speed = 1f;
					Debug.Log("TargetSoul Speed already assign");
					SpeedSoulAssign = true;
					player = "Robot";
				}
			}

			//         if (myRole == "TargetSoul" && !SoulCharacterAssign)
			//         {
			//             photonView.RPC("ChangeCharacter", RpcTarget.All);
			//             SoulCharacterAssign = true;
			//         }
			//         else
			//{
			//}

			if (myRole != "")
            {
                if (myRole == "Reaper")
                {
					//1.Reaper Nurse
					if (Input.GetKeyDown(KeyCode.K))
                    {
                        Debug.Log("Cancel Slow (Nurse Skill Reaper)");
						//photonView.RPC("SlowButton", RpcTarget.All);
						photonView.RPC("CancelGotSlow", RpcTarget.All);
						//SlowButton();
						//if (PhotonNetwork.IsMasterClient)
						//{
						//    photonView.RPC("SlowButton", RpcTarget.All);
						//}
					}
					//2.Reaper Butcher
					if (Input.GetKeyDown(KeyCode.L))
					{
						Debug.Log("Run faster (Reaper)");
						photonView.RPC("ReaperFaster", RpcTarget.All);
					}
				}

                if (myRole == "Sinner")
                {
					//1.Nurse
					if (Input.GetKeyDown(KeyCode.K))
                    {
                        Debug.Log("Cancel Stunt (Nurse Skill Sinner)");
                        //photonView.RPC("StuntButton", RpcTarget.All);
						photonView.RPC("CancelGotStunt", RpcTarget.All);
						//StuntButton();
						//if (PhotonNetwork.IsMasterClient)
						//{
						//    photonView.RPC("StuntButton", RpcTarget.All);
						//}
					}
					//2.Butcher
					if (Input.GetKeyDown(KeyCode.L))
					{
						Debug.Log("Run faster (Sinner)");
						photonView.RPC("SinnerFaster", RpcTarget.All);
					}
				}

				if (myRole == "TargetSoul")
				{
					//1.Nurse
					if (Input.GetKeyDown(KeyCode.K))
					{
						Debug.Log("You cannot use skill because you are targetsoul (Nurse)");
					}
					//2.Butcher
					if (Input.GetKeyDown(KeyCode.L))
					{
						Debug.Log("You cannot use skill because you are targetsoul (Butcher)");
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.B))
			{
				photonView.RPC("WasBurnButton", RpcTarget.All);
			}

			//if (Input.GetKeyDown(KeyCode.C) && mySpecial == "Wizard")
			//{
			//	GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			//	foreach (GameObject player in players)
			//	{
			//		string playerNumber = player.name.Replace("Player", "");
			//		int playerNumberConvert = Convert.ToInt32(playerNumber);
			//		string[] playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["Role"];
			//		string playerRole = playerRoles[playerNumberConvert - 1];

			//		if (playerRole == "TargetSoul")
			//		{
			//			player.GetComponent<PhotonView>().RPC("SwapCharacterWizard", RpcTarget.All);
			//		}
			//	}
			//	photonView.RPC("SwapCharacterWizard", RpcTarget.All);
			//}

			////AuraForPlayers
			//if (myRole == "Reaper")
			//{
			//	PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SeeTargetSoul", "SawTargetSoul" } });
			//	TargetEffect.Stop();
			//}
			//if (myRole == "Sinner")
			//{
			//	PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "SeeReaper", "SawReaper" } });
			//}
			//if (myRole == "TargetSoul")
			//{
			//	PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "TargetSoulCanSee", "TargetSoulCannotSee" } });
			//	TargetEffect.Stop();
			//}

			if (Input.GetKeyDown(KeyCode.C) && mySpecial == "Wizard")
            {
				//GameObject player = GameObject.Find(name);
				//if (player.name == "Player4")
				//{
				//	player.GetComponent<PhotonView>().RPC("SwapCharacterWizard", RpcTarget.All);
				//}
				photonView.RPC("SwapCharacterWizard", RpcTarget.AllViaServer);
				PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "WizardAbility", "Swapped" } });
            }

            if (!GameManagerKen.gameIsPaused)
			{
				if (mySpecial == "Wizard")
				{
					//if (Input.GetKeyDown(KeyCode.C))
					{
						//if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Special"] == "Wizard")
						//photonView.RPC("WizardUseOrNot", RpcTarget.All);
						//photonView.RPC("SwapCharacter", RpcTarget.All);
						//PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Ability", "Wizard1" } });
						//GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
						//foreach (GameObject player in players)
						//{
						//	// Only call ChangeCharacter on the current player
						//	if (player == gameObject)
						//	{
						//		string playerName = player.name;
						//		/*photonView.RPC("ChangeCharacter", RpcTarget.All, playerName);*/
						//	}
						//}
						//Debug.Log("Change Character Sinner to TaregtSoul");
						//photonView.RPC("ChangeCharacter", RpcTarget.All);
						//foreach (GameObject player in players)
						//{
						//	string playerNumber = player.name.Replace("Player", "");
						//	int playerNumberConvert = Convert.ToInt32(playerNumber);
						//	string[] playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerRoles"];
						//	string playerRole = playerRoles[playerNumberConvert - 1];
						//	string[] playerSpecials = (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerSpecials"];
						//	string playerSpecial = playerSpecials[playerNumberConvert - 1];

						//	Debug.Log("Role of the player " + player + " : " + playerRole);
						//	Debug.Log("Special of the player " + player + " : " + playerSpecial);
						//	////Only the working one
						//	//if (playerRole == "Sinner" || playerRole == "TargetSoul")
						//	if (playerSpecial == "Wizard" || playerRole == "TargetSoul")
						//                      {
						//                          Debug.Log("Change Character Sinner to TargetSoul");
						//		Debug.Log("Change Character TargetSoul to Sinner");
						//                          //player.GetComponent<PhotonView>().RPC("ChangeCharacter", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, PhotonNetwork.CurrentRoom.GetPlayer(playerNumberConvert - 1));
						//                          player.GetComponent<PhotonView>().RPC("ChangeCharacter", RpcTarget.All);
						//                          //player.GetComponent<PhotonView>().RPC("ChangeCharacter", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
						//                          //gameObject.GetComponent<PhotonView>().RPC("ChangeCharacter", RpcTarget.All, gameObject.GetComponent<PhotonView>().ViewID);
						//                          //if (playerSpecial == "Wizard" || playerSpecial == "NoSpecial")
						//                          //                        {
						//                          //	Debug.Log("Change Character Sinner to TargetSoul");
						//                          //	Debug.Log("Change Character TargetSoul to Sinner");
						//                          //	player.GetComponent<PhotonView>().RPC("ChangeCharacter", RpcTarget.All);
						//                          //}	
						//                      }
						//                      else
						//                      {

						//                      }
						//}
					}
					//if (myRole == "Sinner")
					//{
					//	Debug.Log("Change Character Sinner to TargetSoul");
					//	photonView.RPC("ChangeCharacter", RpcTarget.All);
					//	//Debug.Log("Change Character TargetSoul to Sinner");
					//	photonView.RPC("ChangeOtherPlayerCharacter", RpcTarget.All, "TargetSoul");
					//}
				}
			}

            if (!GameManagerKen.gameIsPaused)
            {
				//if (Input.GetMouseButtonDown(0))
				//{
				//	if (WeaponController.Instance.CanAttack)
				//	{
				//		photonView.RPC("SwordAttack", RpcTarget.All);
				//		//SwordAttack();
				//	}
				//}
				//if (Input.GetButtonDown("Fire1"))
				if (Input.GetMouseButtonDown(0))
				{
					if (myRole != "TargetSoul")
					{
						photonView.RPC("AttackButton", RpcTarget.All);
						//animator.SetTrigger("Attack");
					}
					else
					{
						Debug.Log("You are Target soul then cannot attack");
					}
				}
				
            }

            //if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            //         {
            //	photonView.RPC("ChangeRobotColor", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
            //}


            //if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            //         {
            //             photonView.RPC("ChangeRandomRobotColor1", RpcTarget.All);
            //             //count1 += 1;
            //         }
            //         if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            //         {
            //             photonView.RPC("ChangeRandomRobotColor2", RpcTarget.All);
            //             //count2 += 1;
            //         }
            //         if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
            //         {
            //             photonView.RPC("ChangeRandomRobotColor3", RpcTarget.All);
            //             //count3 += 1;
            //         }
            //         if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
            //         {
            //             photonView.RPC("ChangeRandomRobotColor4", RpcTarget.All);
            //             //count4 += 1;
            //         }

            if (Input.GetKeyDown(KeyCode.H) && PhotonNetwork.IsMasterClient)
			{
				Debug.Log("Reveal Who Got Killed");
				photonView.RPC("WasKillButton", RpcTarget.All);
				// IconForKilled.Instance.FlowKilledVictim();
			} else
            {
				Debug.Log("Cannot!!! Reveal Who Got Killed");
			}


			if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                photonView.RPC("ChangeRobotColor1", RpcTarget.All);
            }
            if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
            {
                photonView.RPC("ChangeRobotColor2", RpcTarget.All);
            }
            if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
            {
                photonView.RPC("ChangeRobotColor3", RpcTarget.All);
            }
            if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
            {
                photonView.RPC("ChangeRobotColor4", RpcTarget.All);
            }

			// UpdateEffectDurations();
			//Slow();
			//Stunt();

			////Slow
			//         if (GotSlow)
			//         {
			//             animator.speed /= 5f;
			//         }
			//else
			//{
			//	animator.speed = 1f;
			//}

			////Stunt
			//if (GotStunt)
			//         {
			//             animator.speed = 0f;
			//         }
			//         else
			//         {
			//             animator.speed = 1f;
			//         }


			if (photonView.IsMine)
            {
				//All Role Hit
				if (Time.time < GameManagerKen.Instance.hitCooldownEndTime)
				{
					float remainingCooldownTime = GameManagerKen.Instance.hitCooldownEndTime - Time.time;
					GameManagerKen.Instance.cooldownTextHit.text = remainingCooldownTime.ToString("F1") + " s";
				}
				else
				{
					GameManagerKen.Instance.cooldownTextHit.text = "";
				}

				//Reaper Skill
				if (myRole == "Reaper")
				{
					if (Time.time < GameManagerKen.Instance.reaperskillCooldownEndTime)
					{
						float remainingCooldownTime = GameManagerKen.Instance.reaperskillCooldownEndTime - Time.time;
						GameManagerKen.Instance.reapercooldownTextSkill.text = remainingCooldownTime.ToString("F1") + " s";
					}
					else
					{
						GameManagerKen.Instance.reapercooldownTextSkill.text = "";
					}
				}

				//Sinner Skill
				if (myRole == "Sinner")
				{
					if (Time.time < GameManagerKen.Instance.sinnerskillCooldownEndTime)
					{
						float remainingCooldownTime = GameManagerKen.Instance.sinnerskillCooldownEndTime - Time.time;
						GameManagerKen.Instance.sinnercooldownTextSkill.text = remainingCooldownTime.ToString("F1") + " s";
					}
					else
					{
						GameManagerKen.Instance.sinnercooldownTextSkill.text = "";
					}
				}
			}
		}

		//[PunRPC]
		//public void HitButton()
		//      {
		//	if (photonView.IsMine)
		//          {
		//		if (PlayerAnimatorManagerKen.Instance.myRole != "TargetSoul")
		//		{
		//			photonView.RPC("AttackButton", RpcTarget.All);
		//			//animator.SetTrigger("Attack");
		//		}
		//		else
		//		{
		//			Debug.Log("You are Target soul then cannot attack");
		//		}
		//	}
		//}

		//void ChangeCharacter(string name)
		//void ChangeCharacter(PhotonMessageInfo info = default)
		//Hereeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
		[PunRPC]
		public void showAura(bool showAuraAlready)
		{
			if (showAuraAlready)
			{
				TargetEffect.Play();
			}
            else
            {
				TargetEffect.Stop();
			}
		}

		[PunRPC]
		public void SwapCharacter(bool CharacterSwap)
		{
			if (CharacterSwap)
			{
				Debug.Log("ChangeToGirl");
				this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToRobot = " + this.gameObject.name);
				Animator characterAnim = Character.GetComponent<Animator>();
				characterAnim.avatar = Avatar2;
				//PhotonNetwork.Replace(oldChar, playerPrefab2);
				//viewedModel1.SetActive(false);
				// Robot1 = GameObject.FindWithTag("Part1");
				Robot1.SetActive(false);
				// Robot2 = GameObject.FindWithTag("Part2");
				Robot2.SetActive(false);
				// Girl1 = GameObject.FindWithTag("Part3");
				Girl1.SetActive(true);
				// Girl2 = GameObject.FindWithTag("Part4");
				Girl2.SetActive(true);
			}
			else
			{
				Debug.Log("ChangeToRobot");
				this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToGirl = " + this.gameObject.name);
				Animator characterAnim = Character.GetComponent<Animator>();
				characterAnim.avatar = Avatar1;

				//PhotonNetwork.Replace(oldChar, playerPrefab1);
				// GameObject Robot1 = GameObject.FindWithTag("Part1");
				Robot1.SetActive(true);
				// GameObject Robot2 = GameObject.FindWithTag("Part2");
				Robot2.SetActive(true);
				// GameObject Girl1 = GameObject.FindWithTag("Part3");
				Girl1.SetActive(false);
				// GameObject Girl2 = GameObject.FindWithTag("Part4");
				Girl2.SetActive(false);
			}
		}

		[PunRPC]
        public void ChangeCharacter()
        {
            myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
            mySpecial = (string)PhotonNetwork.LocalPlayer.CustomProperties["Special"];

            if (mySpecial == "Wizard" || myRole == "TargetSoul")
			{
				if (player == "Robot")
				{
					Debug.Log("ChangeToGirl");
					this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToRobot = " + this.gameObject.name);
					Animator characterAnim = Character.GetComponent<Animator>();
					characterAnim.avatar = Avatar2;

					//PhotonNetwork.Replace(oldChar, playerPrefab2);
					//viewedModel1.SetActive(false);
					// Robot1 = GameObject.FindWithTag("Part1");
					Robot1.SetActive(false);
					// Robot2 = GameObject.FindWithTag("Part2");
					Robot2.SetActive(false);
					// Girl1 = GameObject.FindWithTag("Part3");
					Girl1.SetActive(true);
					// Girl2 = GameObject.FindWithTag("Part4");
					Girl2.SetActive(true);
					player = "Girl";
				}

				else if (player == "Girl")
				{
					Debug.Log("ChangeToRobot");
					this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToGirl = " + this.gameObject.name);
					Animator characterAnim = Character.GetComponent<Animator>();
					characterAnim.avatar = Avatar1;

					//PhotonNetwork.Replace(oldChar, playerPrefab1);
					// GameObject Robot1 = GameObject.FindWithTag("Part1");
					Robot1.SetActive(true);
					// GameObject Robot2 = GameObject.FindWithTag("Part2");
					Robot2.SetActive(true);
					// GameObject Girl1 = GameObject.FindWithTag("Part3");
					Girl1.SetActive(false);
					// GameObject Girl2 = GameObject.FindWithTag("Part4");
					Girl2.SetActive(false);
					player = "Robot";
				}
			}
			

			////if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Special"] == "Wizard" || (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == "TargetSoul")
			////{
			//string role = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
			//string special = (string)PhotonNetwork.LocalPlayer.CustomProperties["Special"];

			//if (special == "Wizard" || role == "TargetSoul")




				//if (PlayerAnimScript.player == "Robot")
				//{
				//	Debug.Log("ChangeToGirl");
				//	this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToRobot = " + this.gameObject.name);
				//	Animator characterAnim = PlayerAnimScript.Character.GetComponent<Animator>();
				//	characterAnim.avatar = PlayerAnimScript.Avatar2;

				//	//PhotonNetwork.Replace(oldChar, playerPrefab2);
				//	//viewedModel1.SetActive(false);
				//	// Robot1 = GameObject.FindWithTag("Part1");
				//	PlayerAnimScript.Robot1.SetActive(false);
				//	// Robot2 = GameObject.FindWithTag("Part2");
				//	PlayerAnimScript.Robot2.SetActive(false);
				//	// Girl1 = GameObject.FindWithTag("Part3");
				//	PlayerAnimScript.Girl1.SetActive(true);
				//	// Girl2 = GameObject.FindWithTag("Part4");
				//	PlayerAnimScript.Girl2.SetActive(true);
				//	PlayerAnimScript.player = "Girl";
				//}
				//else if (PlayerAnimScript.player == "Girl")
				//            {
				//                Debug.Log("ChangeToRobot");
				//                this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToGirl = " + this.gameObject.name);
				//                Animator characterAnim = PlayerAnimScript.Character.GetComponent<Animator>();
				//                characterAnim.avatar = PlayerAnimScript.Avatar1;

				//                //PhotonNetwork.Replace(oldChar, playerPrefab1);
				//                // GameObject Robot1 = GameObject.FindWithTag("Part1");
				//                PlayerAnimScript.Robot1.SetActive(true);
				//                // GameObject Robot2 = GameObject.FindWithTag("Part2");
				//                PlayerAnimScript.Robot2.SetActive(true);
				//                // GameObject Girl1 = GameObject.FindWithTag("Part3");
				//                PlayerAnimScript.Girl1.SetActive(false);
				//                // GameObject Girl2 = GameObject.FindWithTag("Part4");
				//                PlayerAnimScript.Girl2.SetActive(false);
				//                PlayerAnimScript.player = "Robot";
				//            }
          
        }

		[PunRPC]
		void Swap(string name)
		{ 
			GameObject playerObj = GameObject.Find(name);
			PlayerAnimatorManagerKen scriptInstance = playerObj.GetComponent<PlayerAnimatorManagerKen>();
			string player = scriptInstance.player;
			GameObject Character = scriptInstance.Character;
			GameObject Robot1 = scriptInstance.Robot1;
			GameObject Robot2 = scriptInstance.Robot2;
			GameObject Girl1 = scriptInstance.Girl1;
			GameObject Girl2 = scriptInstance.Girl2;

			//this.photonView.RPC("DebugRPC", RpcTarget.All, "player in! = " + this.gameObject.name);
			//PlayerAnimatorManagerKen PlayerAnimScript = playerObj.gameObject.GetComponent<PlayerAnimatorManagerKen>();
			if (player == "Robot")
			{
				Debug.Log("ChangeToGirl");
				this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToRobot = " + this.gameObject.name);
				Animator characterAnim = Character.GetComponent<Animator>();
				characterAnim.avatar = Avatar2;

				//PhotonNetwork.Replace(oldChar, playerPrefab2);
				//viewedModel1.SetActive(false);
				// Robot1 = GameObject.FindWithTag("Part1");
				Robot1.SetActive(false);
				// Robot2 = GameObject.FindWithTag("Part2");
				Robot2.SetActive(false);
				// Girl1 = GameObject.FindWithTag("Part3");
				Girl1.SetActive(true);
				// Girl2 = GameObject.FindWithTag("Part4");
				Girl2.SetActive(true);
				player = "Girl";
			}

			else if (player == "Girl")
			{
				Debug.Log("ChangeToRobot");
				this.photonView.RPC("DebugRPC", RpcTarget.All, "ChangeToGirl = " + this.gameObject.name);
				Animator characterAnim = Character.GetComponent<Animator>();
				characterAnim.avatar = Avatar1;

				//PhotonNetwork.Replace(oldChar, playerPrefab1);
				// GameObject Robot1 = GameObject.FindWithTag("Part1");
				Robot1.SetActive(true);
				// GameObject Robot2 = GameObject.FindWithTag("Part2");
				Robot2.SetActive(true);
				// GameObject Girl1 = GameObject.FindWithTag("Part3");
				Girl1.SetActive(false);
				// GameObject Girl2 = GameObject.FindWithTag("Part4");
				Girl2.SetActive(false);
				player = "Robot";
			}

		}

        //Hereeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee

        public void CloseFlowKilledVictim()
		{
			for (int i = 0; i < IconForKilled.Instance.objectsToDisplay.Length; i++)
			{
				CloseDisplayObject(i);
			}
		}

		public void FlowKilledVictim()
		{
			for (int i = 0; i < IconForKilled.Instance.objectsToDisplay.Length; i++)
			{
				DisplayObject(i);
			}
		}

		public void CloseDisplayObject(int objectIndex)
		{
			IconForKilled.Instance.objectsToDisplay[objectIndex].SetActive(false);
		}

		public void DisplayObject(int objectIndex)
		{
			IconForKilled.Instance.objectsToDisplay[objectIndex].SetActive(true);
		}

		public void SpecialSklls()
		{
			for (int i = 0; i < SpecialIcon.Instance.specialsToDisplay.Length; i++)
			{
				if (mySpecial == "Wizard")
                {
					SpecialIcon.Instance.specialsToDisplay[5].SetActive(true);
				}
				else if (mySpecial == "Butcher")
				{
					SpecialIcon.Instance.specialsToDisplay[0].SetActive(true);
				}
				else if (mySpecial == "NoSpecial" && myRole != "TargetSoul")
				{
					SpecialIcon.Instance.specialsToDisplay[4].SetActive(false);
				}
				else if (mySpecial == "NoSpecial" && myRole == "TargetSoul")
				{
					SpecialIcon.Instance.specialsToDisplay[2].SetActive(false);
					SpecialIcon.Instance.specialsToDisplay[4].SetActive(false);
					SpecialIcon.Instance.specialsToDisplay[6].SetActive(false);
				}
			}
		}

		public void UseWizardSkll()
		{
			for (int i = 0; i < WizardSkillTest.Instance.WizardSkillTestToDisplay.Length; i++)
			{
				if (mySpecial == "Wizard" || myRole == "TargetSoul")
				{
					WizardSkillTest.Instance.WizardSkillTestToDisplay[0].SetActive(false);
					WizardSkillTest.Instance.WizardSkillTestToDisplay[1].SetActive(false);

					WizardSkillTest.Instance.WizardSkillTestToDisplay[2].SetActive(true);
					WizardSkillTest.Instance.WizardSkillTestToDisplay[3].SetActive(true);
				}
			}
		}

		public void StopUseWizardSkll()
		{
			for (int i = 0; i < WizardSkillTest.Instance.WizardSkillTestToDisplay.Length; i++)
			{
				if (mySpecial == "Wizard" || myRole == "TargetSoul")
				{
					WizardSkillTest.Instance.WizardSkillTestToDisplay[0].SetActive(true);
					WizardSkillTest.Instance.WizardSkillTestToDisplay[1].SetActive(true);

					WizardSkillTest.Instance.WizardSkillTestToDisplay[2].SetActive(false);
					WizardSkillTest.Instance.WizardSkillTestToDisplay[3].SetActive(false);
				}
			}
		}

	//public void WasAttack()
	//{
	//	if (WasAttacked)
	//	{
	//		animator.SetTrigger("Attack");
	//	}
	//	else
	//	{
	//	}
	//}

	public void WasBurn()
		{
			if (WasBurned)
			{
				burnEffect.Play();
				AudioSource burnSound = GetComponent<AudioSource>();
				burnSound.volume = 0.5f;
				burnSound.clip = burnEffectClip;
				burnSound.loop = true; // set loop to true
				burnSound.Play();
				//burnSound.PlayOneShot(burnEffectClip, 0.5f);
				Debug.Log("BurnSoundddddddddddddddddddddd");
			}
			else
			{
				burnEffect.Stop();
				AudioSource burnSound = GetComponent<AudioSource>();
				burnSound.Stop();
				Debug.Log("BurnSoundddddstop");
			}
		}

		public void WasKill()
		{
			if (WasKilled)
			{
				FlowKilledVictim();
			}
			else
			{
				CloseFlowKilledVictim();
			}
		}

		public void Slow()
        {
            if (Slowed)
            {
				animator.speed /= 5f;
				// Debug.Log("Slow Down");
			}
            else
            {
				animator.speed = 1f;
				// Debug.Log("Speed Normal");
			}
        }

		public void Stunt()
		{
			if (Stunted)
			{
				animator.speed = 0f;
				// Debug.Log("Stunt");
			}
			else
			{
				animator.speed = 1f;
				// Debug.Log("Stunt Cancel");
			}
		}

		public void Teleport()
		{
			gameObject.GetComponent<CharacterController>().enabled = false;
			if (Teleported)
			{
				gameObject.transform.position = new Vector3(0f, 5f, 5f);
				gameObject.GetComponent<CharacterController>().enabled = true;
				Debug.Log("Teleport to new position");
			}
			else
			{
				gameObject.transform.position = new Vector3(0f, 5f, 0f);
				gameObject.GetComponent<CharacterController>().enabled = true;
				Debug.Log("Teleport back");
			}
		}

  //      private void OnCollisionEnter(Collision collision)
  //      {
		//	if (collision.gameObject.CompareTag("Player"))
		//	{
		//		Debug.Log(collision.gameObject.name);
		//		Debug.Log(collision.gameObject.GetComponent<PhotonView>().IsMine);
		//		if (!collision.gameObject.GetComponent<PhotonView>().IsMine)
		//		{
		//			collision.gameObject.GetComponent<PhotonView>().RPC("TakeDamage1", RpcTarget.Others, 0.1f);
		//			Debug.Log("Hit" + collision.gameObject.name);
		//		}
		//	}
		//}

        void OnTriggerEnter(Collider other)
        {
            // Physics.IgnoreCollision(other.gameObject.GetComponent<Collider>(), GetComponent<Collider>());

            //if (!this.GetComponent<PhotonView>().IsMine)
            //         {
            //if (other.CompareTag("Enemy"))
            //{
            //    //animator.speed /= 10f;
            //    //Slowed = true;
            //    Debug.Log(other.name);
            //    Debug.Log("Got Hittttt");
            //}

            //if (!photonView.IsMine)
            //{
            //    return;
            //}

			//if (!other.name.Contains("Axe"))
			//{
			//    return;
			//}

			//PhotonView target = other.GetComponentInParent<PhotonView>();
			//if (target != null && target.IsMine && !other.name.Contains("Axe"))
			//{
			//    target.RPC("TakeDamage1", RpcTarget.Others, 0.1f);
			//    Debug.Log(target.name);

			//if (other.CompareTag("Player") && !other.gameObject.GetComponent<PhotonView>().IsMine)
			////if (other.name.Contains("Axe") && !photonView.IsMine)
			//{
			//	Debug.Log(other.name);
			//	Debug.Log(other.gameObject.GetComponent<PhotonView>().IsMine);
			//	photonView.RPC("TakeDamage1", RpcTarget.All, 0.1f);
			//	//if (!other.gameObject.GetComponent<PhotonView>().IsMine)
			//	//{
			//	//	photonView.RPC("TakeDamage1", RpcTarget.Others, 0.1f);
			//	//}
			//}

			//if (other.gameObject.tag == "Player")
			//{
			//	animator.speed /= 10f;
			//	Slowed = true;
			//	Debug.Log("ReaperGotHit");
			//	Debug.Log(other.gameObject.name);
			//}
			//}

			//if (other.gameObject.tag == "Player" && GameManager.role == "Sinner")
			//{
			//    animator.speed = 0f;
			//    Stunted = true;
			//    Debug.Log("SinnerGotHit");
			//}
		}

		// public static void IgnoreCollision(Collider collider1, Collider collider2, bool ignore = true);

		#endregion

		//[PunRPC]
		//public void SwapCharacter(string playerRole)
		//{
		//	playerRole = myRole;
		//	if (playerRole == "Sinner")
		//	{
		//		Debug.Log("Change Character Sinner to TargetSoul");
		//		ChangeCharacter();
		//	if (playerRole == "TargetSoul")
		//	{
		//		Debug.Log("Change Character TargetSoul to Sinner");
		//		ChangeCharacter();
		//		//photonView.RPC("ChangeCharacter", RpcTarget.All);
		//	}
		//	//Debug.Log("Change Character Sinner to TargetSoul");
		//	//photonView.RPC("ChangeCharacter", RpcTarget.All);
		//	//if (myRole == "TargetSoul") {
		//	//	Debug.Log("Change Character TargetSoul to Sinner");
		//	//	// ChangeCharacter();
		//	//	photonView.RPC("ChangeCharacter", RpcTarget.All);
		//	//}
		//	}
		//}

		[PunRPC]
		public void NowRun()
		{
			if (nowWalk)
			{
				nowWalk = false;
				AudioSource walkSound = GetComponent<AudioSource>();
				walkSound.volume = 0.5f;
				walkSound.clip = walkEffectClip;
				walkSound.loop = true; // set loop to true
				walkSound.Play();
				//burnSound.PlayOneShot(burnEffectClip, 0.5f);
				Debug.Log("WalkSoundddddddddddddddddddddd");
			}
			else
			{
				AudioSource walkSound = GetComponent<AudioSource>();
				walkSound.Stop();
				Debug.Log("Stop Walking Noww");
			}
		}

		//Reaper Nurse
		[PunRPC]
		public void CancelGotSlow()
        {
			if (GotSlow)
			{
				GotSlow = false;
				animator.speed = 1.2f;
				Debug.Log("Remove got slow");
			}
			else
			{
				Debug.Log("You didn't got slow");
			}
        }

		//Sinner Nurse
		[PunRPC]
		public void CancelGotStunt()
		{
			if (GotStunt)
			{
				GotStunt = false;
				animator.speed = 1f;
				Debug.Log("Remove got stunt");
			}
			else
			{
				Debug.Log("You didn't got stunt");
			}
		}

		//Reaper Butcher
		[PunRPC]
		public void ReaperFaster()
		{
			if (!RunReaperFaster && !GotSlow)
			{
				RunReaperFaster = true;
				animator.speed = 1.5f;
				Debug.Log("Run Faster Reaper (Reaper Butcher)");

				if (photonView.IsMine)
                {
					GameManagerKen.Instance.reapercooldownTextSkill.gameObject.SetActive(true);
					// Set the cooldown end time
					GameManagerKen.Instance.reaperskillCooldownEndTime = Time.time + effectDurationReaperFaster;
				}

				StartCoroutine(DisableRunFasterReaperSkill());
			}
			else if (GotSlow)
			{
				Debug.Log("You cannot use because you got slow");
			}
			else if (RunReaperFaster)
			{
				float remainingCooldownTime = GameManagerKen.Instance.reaperskillCooldownEndTime - Time.time;
				Debug.Log("You cannot use because the skill is not end yet");
			}
		}
		IEnumerator DisableRunFasterReaperSkill()
		{
			yield return new WaitForSeconds(effectDurationReaperFaster);
			RunReaperFaster = false;
			animator.speed = 1.2f;
			if (photonView.IsMine)
            {
				GameManagerKen.Instance.reapercooldownTextSkill.gameObject.SetActive(false);
			}
			Debug.Log("Run Faster is ready (Reaper), you can use again now");
		}

		//Butcher
		[PunRPC]
		public void SinnerFaster()
		{
			if (!RunSinnerFaster && !GotStunt)
			{
				RunSinnerFaster = true;
				animator.speed = 1.3f;
				Debug.Log("Run Faster Sinner (Sinner Butcher)");

				if (photonView.IsMine)
                {
					GameManagerKen.Instance.sinnercooldownTextSkill.gameObject.SetActive(true);
					// Set the cooldown end time
					GameManagerKen.Instance.sinnerskillCooldownEndTime = Time.time + effectDurationSinnerFaster;
				}
				StartCoroutine(DisableRunFasterSinnerSkill());
			}
			else if (GotStunt)
			{
				Debug.Log("You cannot use because you got stunt");
			}
			else if (RunSinnerFaster)
			{
				float remainingCooldownTime = GameManagerKen.Instance.sinnerskillCooldownEndTime - Time.time;
				Debug.Log("You cannot use because the skill is not end yet");
			}
		}

		IEnumerator DisableRunFasterSinnerSkill()
		{
			yield return new WaitForSeconds(effectDurationSinnerFaster);
			RunSinnerFaster = false;
			animator.speed = 1f;
			if (photonView.IsMine)
            {
				GameManagerKen.Instance.sinnercooldownTextSkill.gameObject.SetActive(false);
			}
			Debug.Log("Run Faster is ready (Sinner), you can use again now");
		}






		[PunRPC]
		public void ReaperAttack(int targetViewID)
        {
			if (this.photonView.ViewID == targetViewID)
            {
                if (GotStunt)
                {
                    Debug.Log("You Got Stunt");
                    return;
                }

				GotStunt = true;
				animator.speed = 0f;
				//Stunted = true;
				//stuntDuration = 5f;
				Debug.Log("Sinner Got Hittttt");
				Debug.Log("Stunted is Starting");
				AudioSource reaperGotAttackSound = GetComponent<AudioSource>();
				reaperGotAttackSound.PlayOneShot(reaperGotAttackEffectClip, 0.5f);
				bloodEffect.Play();
				animator.SetTrigger("GotHit");
				//SpawnBloodParticles();

				//GameObject otherPlayer = PhotonView.Find(otherViewID).gameObject;
				//Instantiate(GameManagerKen.Instance.bloodEffectObject,
				//	new Vector3(otherPlayer.transform.position.x, transform.position.y, otherPlayer.transform.position.z),
				//	otherPlayer.transform.rotation);

				//reaperGotAttackSound.volume = 0.5f;
				//reaperGotAttackSound.clip = reaperGotAttackEffectClip;
				//reaperGotAttackSound.Play();
				//photonView.RPC("UpdateEffectDurations", RpcTarget.All);
				StartCoroutine(DisableEffectAfterDurationStunted());
			}
        }

		IEnumerator DisableEffectAfterDurationStunted()
		{
			yield return new WaitForSeconds(effectDurationStunted);
			GotStunt = false;
			animator.speed = 1f;
			//Stunted = false;
			//Time.timeScale = 1.0f;
			Debug.Log("Stunted is over, you can got stunted again");
		}

		[PunRPC]
		public void SinnerAttack(int targetViewID)
		{
			if (this.photonView.ViewID == targetViewID)
			{
                if (GotSlow)
                {
                    Debug.Log("Slowed is over");
                    return;
                }

				GotSlow = true;
				animator.speed /= 3f;
				//Slowed = true;
				//slowDuration = 5f;
				Debug.Log("Reaper Got Hittttt");
				Debug.Log("Slowed is Starting");
				AudioSource sinnerGotAttackSound = GetComponent<AudioSource>();
				sinnerGotAttackSound.PlayOneShot(sinnerGotAttackEffectClip, 0.5f);
                //bloodEffect = GetComponentInChildren<ParticleSystem>();
				bloodEffect.Play();
				animator.SetTrigger("GotHit");
				//SpawnBloodParticles();

				//GameObject otherPlayer = PhotonView.Find(otherViewID).gameObject;
				//Instantiate(GameManagerKen.Instance.bloodEffectObject,
				//	new Vector3(otherPlayer.transform.position.x, transform.position.y, otherPlayer.transform.position.z),
				//	otherPlayer.transform.rotation);

				//sinnerGotAttackSound.volume = 0.5f;
				//sinnerGotAttackSound.clip = sinnerGotAttackEffectClip;
				//sinnerGotAttackSound.Play();
				//photonView.RPC("UpdateEffectDurations", RpcTarget.All);
				StartCoroutine(DisableEffectAfterDurationSlowed());
			}
		}

        IEnumerator DisableEffectAfterDurationSlowed()
        {
            yield return new WaitForSeconds(effectDurationSlowed);
			GotSlow = false;
			animator.speed = 1.2f;
			//Slowed = false;
			//Time.timeScale = 1.0f;
			Debug.Log("Slowed is over, you can got slowed again");
		}

		[PunRPC]
		public void SoulGotAttack(int targetViewID)
		{
			if (this.photonView.ViewID == targetViewID)
			{
				AudioSource sinnerGotAttackSound = GetComponent<AudioSource>();
				sinnerGotAttackSound.PlayOneShot(sinnerGotAttackEffectClip, 0.5f);
				//PlayerAnimatorManagerKen.Instance.bloodEffect = GetComponentInChildren<ParticleSystem>();
				bloodEffect.Play();
				animator.SetTrigger("Die");
				//animator.speed = 0.0f;
			}
		}


		[PunRPC]
		public void AttackButton()
		{
			if (!CanAttack)
			{
				float remainingCooldownTime = GameManagerKen.Instance.hitCooldownEndTime - Time.time;
				Debug.Log("Attack on cooldown");
				return;
			}

			CanAttack = false;
			Animator anim = Robot.GetComponent<Animator>();
			anim.SetTrigger("Attack");
			// Play a sound effect
			// Play a sound effect
			AudioSource attackSound = GetComponent<AudioSource>();
			attackSound.PlayOneShot(attackEffectClip, 0.5f);
			//attackSound.volume = 0.5f;
			//attackSound.clip = attackEffectClip;
			//attackSound.Play();
			//WasAttacked = !WasAttacked;
			//WasAttack();

			if (photonView.IsMine)
            {
				GameManagerKen.Instance.cooldownTextHit.gameObject.SetActive(true);
				// Set the cooldown end time
				GameManagerKen.Instance.hitCooldownEndTime = Time.time + AttackCooldown;
			}

			StartCoroutine(ResetAttackCooldown());
			Debug.Log("Attack started, cooldown started");
		}

		IEnumerator ResetAttackCooldown()
		{
			Debug.Log("Attack cooldown started");

			//// Wait for the attack animation to finish
			//Animator anim = Robot.GetComponent<Animator>();
			//float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
			
			//yield return new WaitForSeconds(animLength);
			yield return new WaitForSeconds(AttackCooldown);
			CanAttack = true;
			if (photonView.IsMine)
            {
				GameManagerKen.Instance.cooldownTextHit.gameObject.SetActive(false);
			}
			Debug.Log("Attack cooldown finished, can attack again");
		}

		//[PunRPC]
		//public void SpawnBloodParticles()
		//{
		//	GameManagerKen.Instance.bloodParticles.transform.position = transform.position;
		//	GameManagerKen.Instance.bloodParticles.Play();
		//}

		//[PunRPC]
		//public void bloodEffectShowUp(GameObject prey)
		//      {
		//	Instantiate(GameManagerKen.Instance.bloodEffectObject, prey.transform.position, prey.transform.rotation);
		//}
		[PunRPC]
		public void enabledTarget()
		{
			TargetSoulMark.SetActive(true);
		}
		
		[PunRPC]
		public void playAura()
		{
			TargetEffect.Play();
		}

		[PunRPC]
		public void stopAura()
		{
			TargetEffect.Stop();
		}

		[PunRPC]
		public void showTextTarget()
		{
			PlayerUIKen.Instance.playerNameText.enabled = true;
		}

		[PunRPC]
		public void ButtonShowTargetSoulAura()
		{
			showTargetSoulAlready = !showTargetSoulAlready;
			showAura(showTargetSoulAlready);
		}

		[PunRPC]
		public void ButtonShowReaperAura()
		{
			showReaperAlready = !showReaperAlready;
			showAura(showReaperAlready);
		}

		[PunRPC]
		public void SwapCharacterWizard()
		{
			Debug.Log("Player"+this.gameObject.name);

			WizardSwap = !WizardSwap;
			SwapCharacter(WizardSwap);
		}

		[PunRPC]
		public void SwapCharacterTargetSoul()
		{
			Debug.Log("Player" + this.gameObject.name);

			SoulSwap = !SoulSwap;
			SwapCharacter(SoulSwap);
		}

		[PunRPC]
		public void WalkOrNot()
		{
			nowWalk = !nowWalk;
			NowRun();
		}

		[PunRPC]
		public void WasBurnButton()
		{
			WasBurned = !WasBurned;
			WasBurn();
		}

		[PunRPC]
		public void WasKillButton()
		{
			WasKilled = !WasKilled;
			WasKill();
		}

		[PunRPC]
		public void TeleportButton()
		{
			Teleported = !Teleported;
			Teleport();
		}

		[PunRPC]
		public void StuntButton()
        {
			Stunted = !Stunted;
			Stunt();
		}

		[PunRPC]
		public void SlowButton()
		{
			Slowed = !Slowed;
			Slow();
		}

		[PunRPC]
		void ChangeOtherPlayerCharacter(string newRole)
		{
			// Find the player with the newRole and change their character
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject player in players)
			{
				if (player.GetComponent<PlayerAnimatorManagerKen>().myRole == newRole)
				{
					Debug.Log("Change Character " + newRole + " to Sinner");
					//player.GetComponent<PhotonView>().RPC("ChangeCharacter", RpcTarget.All);
				}
			}
		}

		public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			base.OnRoomPropertiesUpdate(propertiesThatChanged);
			if (!this.photonView.IsMine) { return; }

			myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
			foreach (var prop in propertiesThatChanged)
			{
				if (Convert.ToString(prop.Key) == "WizardAbility")
				{
					if (Convert.ToString(prop.Value) == "Swapped"
						&& myRole == "TargetSoul")
					{
						//this.photonView.RPC("DebugRPC", RpcTarget.All, "Role+" + myRole);
						this.photonView.RPC("SwapCharacterTargetSoul", RpcTarget.AllViaServer);
						//PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "WizardAbility", "" } });
					}
				}
				if (Convert.ToString(prop.Key) == "SeeTargetSoul")
				{
					if (Convert.ToString(prop.Value) == "SawTargetSoul"
						&& myRole == "TargetSoul")
					{
						//TargetEffect.Play();
						this.photonView.RPC("enabledTarget", RpcTarget.All);
						//this.photonView.RPC("playAura", RpcTarget.All);
						//this.photonView.RPC("showTextTarget", RpcTarget.All);
						//this.photonView.RPC("ButtonShowTargetSoulAura", RpcTarget.AllViaServer);
					}
				}
				//else if (Convert.ToString(prop.Key) == "SeeReaper")
				//{
				//	if (Convert.ToString(prop.Value) == "SawReaper"
				//	&& myRole == "Reaper")
				//	{
				//		//TargetEffect.Play();
				//		this.photonView.RPC("playAura", RpcTarget.All);
				//		//this.photonView.RPC("ButtonShowReaperAura()", RpcTarget.AllViaServer);
				//	}
				//	else if (Convert.ToString(prop.Value) == "SawReaper"
				//	&& myRole == "TargetSoul")
    //                {
				//		this.photonView.RPC("stopAura", RpcTarget.All);
				//	}
				//}
				//else if (Convert.ToString(prop.Key) == "TargetSoulCanSee")
				//{
				//	if (Convert.ToString(prop.Value) == "TargetSoulCannotSee" && myRole == "Reaper")
				//	{
				//		this.photonView.RPC("stopAura", RpcTarget.All);
				//	}
				//}
			}
		}

		//private IEnumerator DisableEffectAfterTime(float duration, Action onDisable)
		//{
		//    yield return new WaitForSeconds(duration);
		//    onDisable();
		//}

		// Call this method once in your MonoBehaviour's Update method
		//[PunRPC]
		//private void UpdateEffectDurations()
		//{
		//    if (Stunted)
		//    {
		//        stuntDuration -= Time.deltaTime;
		//        if (stuntDuration <= 0)
		//        {
		//            Stunted = false;
		//            StartCoroutine(DisableEffectAfterTime(0.5f, () =>
		//            {
		//                animator.speed = 1f; // revert the animation speed
		//            }));
		//            this.photonView.RPC("DebugRPC", RpcTarget.All, "Stunted out of time!!!!!!!!!");
		//        }
		//    }

		//    if (Slowed)
		//    {
		//        slowDuration -= Time.deltaTime;
		//        if (slowDuration <= 0)
		//        {
		//            Slowed = false;
		//            StartCoroutine(DisableEffectAfterTime(0.5f, () =>
		//            {
		//                animator.speed = 1f; // revert the animation speed
		//            }));
		//            this.photonView.RPC("DebugRPC", RpcTarget.All, "Slowed out of time!!!!!!!!!");
		//        }
		//    }
		//}

		//     [PunRPC]
		//     public void StuntButton()
		//     {
		//if (PlayerAnimatorManager.Stunted == false)
		//         {
		//             PlayerAnimatorManager.Instance.animator.speed = 0f;
		//             Debug.Log("StuntButton");
		//             PlayerAnimatorManager.Stunted = true;
		//         }
		//         else if (PlayerAnimatorManager.Stunted == true)
		//         {
		//             PlayerAnimatorManager.Instance.animator.speed = 1f;
		//             Debug.Log("CancelStunt");
		//             PlayerAnimatorManager.Stunted = false;
		//         }

		//     }

		//[PunRPC]
		//public void SlowButton()
		//{
		//    if (PlayerAnimatorManager.Slowed == false)
		//    {
		//        PlayerAnimatorManager.Instance.animator.speed /= 10f;
		//        Debug.Log("SlowButton");
		//        PlayerAnimatorManager.Slowed = true;
		//    }
		//    else if (PlayerAnimatorManager.Slowed == true)
		//    {
		//        PlayerAnimatorManager.Instance.animator.speed = 1f;
		//        Debug.Log("CancelSlow");
		//        PlayerAnimatorManager.Slowed = false;
		//    }

		//}
	}
}