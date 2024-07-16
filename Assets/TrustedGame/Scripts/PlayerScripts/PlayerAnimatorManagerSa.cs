using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;
using TMPro;

public class PlayerAnimatorManagerSa : MonoBehaviourPunCallbacks
{
	public static PlayerAnimatorManagerSa Instance;

	[Header("Animator")]
	[SerializeField] Animator animator;
	private float directionDampTime = 0.25f;
	[SerializeField] Vector3 playerDirection = Vector3.forward;
	[SerializeField] float moveSpeed = 1;
	public AnimatorStateInfo stateInfo;


	[Header("Audio Clips")]
	AudioSource audioSource;
	public AudioClip axeAttackSound;
	public AudioClip bloodSound;
	public AudioClip reaperScreamSound;
	[SerializeField] AudioClip sinnerScreamSound;
	//public AudioClip maleScreamSound;
	//public AudioClip femaleScreamSound;
	//public AudioClip walkingSound;
	// public AudioClip sinnerScreamSound;

	[Header("Particle Systems")]
	public ParticleSystem bloodEffect;

	[Header("CoolDown & Effect Duration")]
	[SerializeField] float attackCoolDownTime = 5.0f;
	[SerializeField] float slowEffectTime = 5.0f;
	[SerializeField] float stuntEffectTime = 5.0f;
	[SerializeField] float abilityEffectTime = 5.0f;

	[Header("Player Bodies")]
	public GameObject[] HumanBodies;
	public GameObject[] SinnerBodies;
	public GameObject ReaperBody;
	public GameObject SinnerRoot;
	public GameObject ReaperRoot;

	[Header("Objects")]
	public GameObject VotingNamePaper;
	public TMP_Text VotingNameText;

	[Header("Weapons")]
	public GameObject[] SinnerWeapons;
	public GameObject[] StolenSinnerWeapons;
	public GameObject WizardWeapon;
	public GameObject WizardWeaponReaper;
	public GameObject ReaperWeapon;
	public GameObject StolenReaperWeapon;
	GameObject CurrentBody;

	[Header("Game Info")]
	string setupStatus;
	string currentState;
	string stateStatus;
	string reaperAbility;
	string[] ThreatAbilitySinners = { "Knight", "Shaman", "Stalker" };
	string[] TrustAbilitySinners = { "Butcher", "Druid", "Wizard" };

	[Header("Time Info")]
	bool timerStarted;
	bool timeIsUp;

	[Header("Player Info")]
	string myTeam;
	string myRole;
	string myStatus;
	GameObject myCurrentBody;
	string[] playerRoles;
	Dictionary<int, string> playerStatuses;

	[Header("Control Flags")]
	[SerializeField] public bool enablePlayerControl = false;
	[SerializeField] bool enableTrustAttack = false;
	[SerializeField] bool enableTrustAbility = false;

	[Header("Main Ability Flags")]
	bool ICanAttack = true;
	bool IGotSlow;
	bool IGotStunt;

	[Header("Special Ability Flags")]
	bool usingAbility;
	public bool attackButtonClicked;
	public bool abilityButtonClicked;
	public bool usedAbility;
	bool IGotSwapped;
	bool swappedBack;
	IEnumerator stunteffectCoroutine;
	bool sentMessage;
	bool setSound;

	#region Unity Callbacks

	private void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		stunteffectCoroutine = StuntEffect();

		VotingNameText = VotingNameText.GetComponent<TMP_Text>();
	}

	void Update()
	{

		if (currentState == "Truth" && !this.gameObject.name.Contains("CloneFor"))
		{
			this.photonView.RPC("HandleTruthAnimation", RpcTarget.AllViaServer);
		}

        // Prevent control is connected to Photon and represent the localPlayer
        if (!this.photonView.IsMine) { return; }
		// failSafe is missing Animator component on GameObject
		//if (!animator) { return; }

		//myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
		//myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
		//if (!setSound
		//&& myTeam == "Sinner")
  //      {
		//	setSound = true;
		//	switch (myRole)
  //          {
		//		case "Druid":
		//		case "Knight":
		//		case "Shaman":
		//			sinnerScreamSound = femaleScreamSound;
		//			break;

		//		default:
		//			sinnerScreamSound = maleScreamSound;
		//			break;
  //          }
  //      }

		if (PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null)
		{
			currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
			setupStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["SetupStatus"];
			stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];

			if (currentState == "Threat" && this.name.Contains("Clone")) { return; }

			switch (currentState)
			{
				case "SetupGame":
					if (PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"] != null)
					{
						myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
						myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
						myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

						if (myStatus == "Spawned")
						{
							Transform Stopper = RoleRevealer.Instance.MyRevealBox.transform.Find("Spawners").transform.Find("Stopper");
							if (this.transform.position.z >= Stopper.position.z)
							{
								animator.SetBool("Careful Walk", false);
								this.transform.position = Stopper.position;
							}
							else
							{
								this.gameObject.GetComponent<CharacterController>().enabled = false;
								animator.SetBool("Careful Walk", true);
								this.transform.Translate(playerDirection * moveSpeed * Time.deltaTime);
							}
						}
					}
					break;

				//case "Truth":
				//	this.photonView.RPC("HandleTruthAnimation", RpcTarget.AllViaServer, this.photonView.ViewID);
				//	break;
			}

			//timeIsUp = StatusManager.Instance.timeIsUp;
			if (enablePlayerControl)
			{
				HandlePlayerControl();
			}

			if (currentState == "Trust")
			{
				timerStarted = StatusManager.Instance.timerStarted;
				if (!timerStarted) { return; }

				timeIsUp = StatusManager.Instance.timeIsUp;
				if (!timeIsUp)
				{
					if (enableTrustAttack)
					{
						HandleTrustAttack();
					}
					if (enableTrustAbility)
					{
						HandleTrustAbility();
					}

					ManageAbilityTime();
				}
				else
				{
					reaperAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperAbility"];

					if (PhotonNetwork.IsMasterClient)
					{
						if (stateStatus != "SoulTaken"
							&& reaperAbility == "")
						{
							// Alert something
							StartCoroutine(PlayerIconManager.Instance.Alert("Middle", "Soul is saved!", 5f));
							PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperAbility", "" } });
							PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "EndedTrust" } });
						}
					}
				}
			}
		}
	}

	[PunRPC]
	void SetActivePaper()
    {
		
    } 

	void HandlePlayerControl()
	{
		Time.timeScale = 1;

		// deal with Jumping
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		// only allow jumping if we are running.
		if (stateInfo.IsName("Base Layer.Run"))
		{
			//// When using trigger parameter
			//         if (Input.GetButtonDown("Fire2")) animator.SetTrigger("Jump"); 
		}


		// deal with movement
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		// prevent negative Speed.
		if (v < 0)
		{
			v = 0;
		}

		// set the Animator Parameters
		animator.SetFloat("Speed", h * h + v * v);
		animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);

		//if (animator.GetFloat("Speed") > 0.1)
  //      {
		//	if (currentState == "Threat")
  //          {
		//		audioSource.clip = walkingSound;
		//		audioSource.Play();
		//	}
  //      }
		//else
  //      {
		//	if (audioSource.isPlaying && audioSource.clip.name == "FootstepsConcrete")
  //          {
		//		audioSource.Stop();
  //          }
  //      }
	}

	void HandleTrustAttack()
	{
		bool clickedAttack = Input.GetMouseButtonDown(0);

		if (clickedAttack || attackButtonClicked)
		{
			attackButtonClicked = false;
			this.photonView.RPC("AttackButton", RpcTarget.AllViaServer);
		}
	}

	void HandleTrustAbility()
	{
		bool clickedAbility = Input.GetMouseButtonDown(1);

		string abilityRole = myRole;
		if (myRole == "Reaper")
		{
			reaperAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperAbility"];
			abilityRole = reaperAbility;
		}

		if ((clickedAbility || abilityButtonClicked) 
		&& (!usedAbility && !usingAbility))
		{
			abilityButtonClicked = false;
			switch (abilityRole)
			{
				case "Butcher":
				case "Stalker":
					this.photonView.RPC("Run", RpcTarget.AllViaServer);
					break;

				case "Druid":
				case "Knight":
					this.photonView.RPC("Heal", RpcTarget.AllViaServer);
					break;

				case "Wizard":
				case "Shaman":
					if (myRole != "Reaper")
					{
						this.photonView.RPC("SwapWizard", RpcTarget.AllViaServer);
						PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "WizardAbility", "Swap" } });
					}
					else
					{
						if (!usingAbility)
                        {
							usingAbility = true;
							playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
							playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];

							string reaperWizardAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperWizardAbility"];
							string swappedRole = "";
							if (reaperWizardAbility.Contains("Swap")) { swappedRole = reaperWizardAbility.Replace("Swap", ""); }
							else if (reaperWizardAbility.Contains("Unswap")) { swappedRole = reaperWizardAbility.Replace("Unswap", ""); }

							int randomNumber = UnityEngine.Random.Range(0, playerRoles.Length);
							string randomRole = playerRoles[randomNumber];
							do
							{
								randomNumber = UnityEngine.Random.Range(0, playerRoles.Length);
								randomRole = playerRoles[randomNumber];
								
								if (playerRoles[randomNumber] != "Reaper"
									&& playerStatuses[randomNumber+1] == "Alive"
									&& randomRole != swappedRole)
								{
									this.photonView.RPC("SwapReaper", RpcTarget.AllViaServer, randomRole);
									PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "Swap" + randomRole } });
									break;
								}
							}
							while (playerRoles[randomNumber] == "Reaper" || playerStatuses[randomNumber + 1] != "Alive" || randomRole == swappedRole);
						}

					}
					break;
			}
			if (!sentMessage)
			{
				sentMessage = true;
				StartCoroutine(PlayerIconManager.Instance.Alert("Middle", abilityRole + " Ability Activated!", 3f));
			}
		}
	}

	[PunRPC]
	void HandleTruthAnimation()
    {
			bool IGaveVote = this.gameObject.GetComponent<ThreatAbilityManager>().IGaveVote;

            animator.SetBool("Sit Talking", !IGaveVote);
            animator.SetBool("Stand Holding", IGaveVote);
            VotingNamePaper.SetActive(IGaveVote);
            if (IGaveVote) { VotingNameText.text = GetVotedPlayerName(); }
            else { VotingNameText.text = ""; }
            VotingNameText.gameObject.SetActive(IGaveVote);

            stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
            if (stateStatus == "EndedTruth")
            {
                animator.SetBool("Sit Talking", false);
                animator.SetBool("Stand Holding", false);
                VotingNamePaper.gameObject.SetActive(false);
                VotingNameText.gameObject.SetActive(false);
            }

		//bool IGaveVote = this.gameObject.GetComponent<ThreatAbilityManager>().IGaveVote;
		//GameObject VotingNamePaper = this.gameObject.GetComponent<PlayerAnimatorManagerSa>().VotingNamePaper;
		//TMP_Text VotingNameText = this.gameObject.GetComponent<PlayerAnimatorManagerSa>().VotingNameText;

		//animator.SetBool("Sit Talking", !IGaveVote);
		//animator.SetBool("Stand Holding", IGaveVote);
		//VotingNamePaper.SetActive(IGaveVote);
		//if (IGaveVote) { VotingNameText.text = GetVotedPlayerName(); }
		//else { VotingNameText.text = ""; }
		//VotingNameText.gameObject.SetActive(IGaveVote);

		//stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
		//if (stateStatus == "EndedTruth")
		//{
		//	animator.SetBool("Sit Talking", false);
		//	animator.SetBool("Stand Holding", false);
		//	VotingNamePaper.gameObject.SetActive(false);
		//	VotingNameText.gameObject.SetActive(false);
		//}
	}

	[PunRPC]
	void DisableTruthAnimation()
    {
		animator.SetBool("Sit Talking", false);
		animator.SetBool("Stand Holding", false);
		VotingNamePaper.gameObject.SetActive(false);
		VotingNameText.gameObject.SetActive(false);
	}

	string GetVotedPlayerName()
    {
		string votedName = this.gameObject.GetComponent<ThreatAbilityManager>().votedPlayerName;
		int votedNumber = Convert.ToInt32(votedName.Replace("Player ", ""));
		for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
		{
			if (PhotonNetwork.PlayerList[i].GetPlayerNumber() == (votedNumber-1))
            {
				return PhotonNetwork.PlayerList[i].NickName;
			}
		}
		return "";
    }

	void ManageAbilityTime()
    {
		float attackCoolDownTimer = TrustManager.Instance.AttackCoolDownTime;
		TMP_Text attackCoolDownText = TrustManager.Instance.AttackCoolDownText;
		if (Time.time < attackCoolDownTimer)
		{
			float remainingCooldownTime = attackCoolDownTimer - Time.time;
			attackCoolDownText.text = Convert.ToInt32(remainingCooldownTime).ToString();
			// attackCoolDownText.text = remainingCooldownTime.ToString("F1") + " s";
		}
		else
		{
			attackCoolDownText.text = "";
		}

		float abilityCoolDownTimer = TrustManager.Instance.AbilityCoolDownTime;
		TMP_Text abilityCoolDownText = TrustManager.Instance.AbilityCoolDownText;
		if (Time.time < abilityCoolDownTimer)
		{
			float remainingCooldownTime = abilityCoolDownTimer - Time.time;
			abilityCoolDownText.text = Convert.ToInt32(remainingCooldownTime).ToString();
			// abilityCoolDownText.text = remainingCooldownTime.ToString("F1") + " s";
		}
		else
		{
			abilityCoolDownText.text = "";
		}

	}

	#endregion

	#region Main Ability

	[PunRPC]
	public void AttackButton()
	{
		if (!ICanAttack)
		{
			Debug.Log("Attack on cooldown");
			return;
		}

		ICanAttack = false;
		animator.SetTrigger("Attack");
		audioSource.PlayOneShot(axeAttackSound, 0.5f);

		if (photonView.IsMine)
		{
			TrustManager.Instance.AttackCoolDownText.gameObject.SetActive(true);
			TrustManager.Instance.AttackCoolDownTime = Time.time + attackCoolDownTime;
		}

		StartCoroutine(AttackCoolDown());
	}
	IEnumerator AttackCoolDown()
	{
		// Debug.Log("Attack cooldown started");
		yield return new WaitForSeconds(attackCoolDownTime);
		ICanAttack = true;
		if (this.photonView.IsMine)
		{
			TrustManager.Instance.AttackCoolDownText.gameObject.SetActive(true);
		}
		Debug.Log("Attack cooldown finished, can attack again");
	}

	[PunRPC]
	public void TakeTargetSoul(int targetViewID)
	{
		if (this.photonView.ViewID == targetViewID)
		{
			audioSource.PlayOneShot(bloodSound, 0.5f);

			bloodEffect.Play();
			animator.SetTrigger("Die");

		}
	}

	[PunRPC]
	public void SlowPlayer(int targetViewID)
	{
		if (this.photonView.ViewID == targetViewID)
		{
			if (IGotSlow)
			{
				Debug.Log("You got slowed");
				return;
			}

			IGotSlow = true;
			animator.speed /= 3f;

			audioSource.PlayOneShot(this.bloodSound, 0.5f);
			bloodEffect.Play();

			StartCoroutine(SlowEffect());
		}
	}
	IEnumerator SlowEffect()
	{
		yield return new WaitForSeconds(slowEffectTime);
		IGotSlow = false;
		animator.speed = moveSpeed;
		// Debug.Log("Slow is over, you can get slowed again");
	}

	[PunRPC]
	public void StuntPlayer(int targetViewID)
	{
		if (this.photonView.ViewID == targetViewID)
		{
			if (IGotStunt)
			{
				Debug.Log("You Got Stunt");
				return;
			}
			IGotStunt = true;

			audioSource.PlayOneShot(this.bloodSound, 0.5f);
			bloodEffect.Play();
			animator.SetBool("GotHit", true);
            animator.speed = 0f;

            StartCoroutine(StuntEffect());
		}
	}
	IEnumerator StuntEffect()
	{
		yield return new WaitForSeconds(stuntEffectTime);
		animator.SetBool("GotHit", false);
		IGotStunt = false;
		animator.speed = moveSpeed;
		// Debug.Log("Stunt is over, you can get stunted again");
	}

	#endregion

	#region Special Ability

	// Butcher Ability
	[PunRPC]
	public void Run()
	{
		if (!usingAbility && (!IGotStunt || !IGotSlow))
		{
			usingAbility = true;
			animator.speed *= 1.5f;
			Debug.Log("Run Faster Sinner (Sinner Butcher)");

			if (photonView.IsMine)
			{
				TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
				TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
			}
			if (myRole == "Reaper")
            {
				StartCoroutine(AbilityEffect("Butcher", false));
			}
			else
            {
				StartCoroutine(AbilityEffect("Butcher", true));
			}
		}
		else if (IGotStunt)
		{
			Debug.Log("You cannot use because you got stunt");
		}
		else if (usingAbility)
		{
			float remainingCooldownTime = TrustManager.Instance.AbilityCoolDownTime - Time.time;
			Debug.Log("You cannot use because the skill is not end yet");
		}
	}

	// Druid Ability
	[PunRPC]
	public void Heal()
	{
		myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
		switch (myRole)
        {
			case "Reaper":
				if (!usingAbility && IGotSlow)
				{
					usingAbility = true;
					IGotSlow = false;
					animator.speed = moveSpeed;
					if (photonView.IsMine)
					{
						TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
						TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
					}
					Debug.Log("Remove got slow");
					StartCoroutine(AbilityEffect("Druid", false));
				}
				break;

			case "Druid":
				if (!usingAbility && IGotStunt)
				{
					usingAbility = true;
					IGotStunt = false;
					animator.speed = moveSpeed;
					animator.SetBool("GotHit", false);
					if (photonView.IsMine)
					{
						TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
						TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
					}
					Debug.Log("Remove got stunt");
					StartCoroutine(AbilityEffect("Druid", true));
				}
				break;
        }
	}

	//Wizard Ability
	[PunRPC]
	public void SwapWizard()
	{
		Dictionary<string, string> targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
		if (!usedAbility)
        {
			usedAbility = true;
			SwapWizardCharacter("Wizard", "TargetSoul");
			switch (targetSoulInfo["Role"])
            {
				case "Knight":
				case "Reaper":
					WizardWeapon.SetActive(false);
					WizardWeaponReaper.SetActive(true);
					break;
            }
			
			abilityEffectTime = 30f;
			if (photonView.IsMine)
			{
				TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
				TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
			}
			StartCoroutine(AbilityEffect("Wizard", true));
		}
		else
        {
			SwapWizardCharacter("TargetSoul", "Wizard");
			switch (targetSoulInfo["Role"])
			{
				case "Knight":
				case "Reaper":
					WizardWeapon.SetActive(true);
					WizardWeaponReaper.SetActive(false);
					break;
			}
		}
	}

	[PunRPC]
	public void SwapTargetSoul()
	{
		if (!IGotSwapped)
		{
			SwapWizardCharacter("TargetSoul", "Wizard");
			IGotSwapped = true;
			//abilityEffectTime = 30f;
			//if (photonView.IsMine)
			//{
			//	TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
			//	TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
			//}
		}
		else
		{
			SwapWizardCharacter("Wizard", "TargetSoul");
			// Alert something..
		}
	}

	//Wizard Ability
	[PunRPC]
	public void SwapReaper(string roleToSwap)
	{
		if (!usedAbility)
		{
			usingAbility = true;
			SwapStolenCharacter("Reaper", roleToSwap, true);

			abilityEffectTime = 30f;
			if (photonView.IsMine)
			{
				TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
				TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
			}
			StartCoroutine(AbilityEffect("Wizard", false));

		}
		else
		{
			SwapStolenCharacter(roleToSwap, "Reaper", true);
		}
	}

	[PunRPC]
	public void SwapSinner(string roleToSwap)
	{
		if (!IGotSwapped)
		{
			SwapStolenCharacter(roleToSwap, "Reaper", false);
			IGotSwapped = true;
			//abilityEffectTime = 30f;
			//if (photonView.IsMine)
			//{
			//	TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(true);
			//	TrustManager.Instance.AbilityCoolDownTime = Time.time + abilityEffectTime;
			//}
		}
		else
		{
			SwapStolenCharacter("Reaper", roleToSwap, false);
			// Alert something..
		}
	}

	public void SwapWizardCharacter(string bodyToDisable, string bodyToSwap)
	{
		int wizardBodyID = 5;
		Dictionary<string, string> targetSoulInfo = (Dictionary<string, string>)PhotonNetwork.CurrentRoom.CustomProperties["TargetSoulInfo"];
		if (targetSoulInfo["NickName"] == "") { Debug.Log("There is no target soul tonight?"); return; }
		string targetRole = targetSoulInfo["Role"];

		GameObject WizardBody = SinnerBodies[wizardBodyID];
		GameObject TargetBody;
		if (targetRole == "Reaper")
		{
			TargetBody = ReaperBody;
		}
		else
		{
			Dictionary<int, string> sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
			int targetSinnerID = -1;
			for (int i = 0; i < sinnerRoles.Count; i++)
			{
				if (targetRole == sinnerRoles[i])
				{
					targetSinnerID = i;
				}
			}
			TargetBody = SinnerBodies[targetSinnerID];
		}

		switch (bodyToDisable)
        {
			case "Wizard":
				WizardBody.gameObject.SetActive(false);
				break;

			default:
				TargetBody.gameObject.SetActive(false);
				if (targetRole == "Knight" || targetRole == "Reaper") { ReaperRoot.transform.name = "DisableRoot"; }
				break;
        }

		switch (bodyToSwap)
        {
			case "Wizard":
				WizardBody.gameObject.SetActive(true);
				SinnerRoot.gameObject.name = "Root";
				break;

			case "TargetSoul":
				TargetBody.gameObject.SetActive(true);
				if (targetRole == "Knight" || targetRole == "Reaper") { SinnerRoot.transform.name = "DisableRoot"; ReaperRoot.transform.name = "Root"; }
				else { SinnerRoot.transform.name = "Root"; }

				SkinnedMeshRenderer playerRenderer = TargetBody.transform.GetComponent<SkinnedMeshRenderer>();
				Material[] rendererMaterials = playerRenderer.materials;
				rendererMaterials[0] = PlayerManagerSa.Instance.TargetSoulMaterial;
				playerRenderer.materials = rendererMaterials;
				break;
        }

		animator.Rebind();
		animator.applyRootMotion = true;
	}

	public void SwapStolenCharacter(string bodyToDisable, string bodyToSwap, bool isReaper)
	{
		int sinnerIDToDisable = GetSinnerID(bodyToDisable);
		int sinnerIDToSwap = GetSinnerID(bodyToSwap);

		switch (bodyToDisable)
		{
			case "Reaper":
				ReaperBody.gameObject.SetActive(false);
				ReaperRoot.transform.name = "DisableRoot";

				if (isReaper) { ReaperWeapon.gameObject.SetActive(false); }
				else { StolenSinnerWeapons[sinnerIDToSwap].gameObject.SetActive(false); }
				break;

			default:
				SinnerBodies[sinnerIDToDisable].gameObject.SetActive(false);
				SinnerRoot.transform.name = "Disable";

				if (isReaper) { StolenReaperWeapon.gameObject.SetActive(false); }
				else { SinnerWeapons[sinnerIDToDisable].gameObject.SetActive(false); }
				break;
		}

		switch (bodyToSwap)
		{
			case "Reaper":
				ReaperBody.gameObject.SetActive(true);
				ReaperRoot.transform.name = "Root";

				if (isReaper) { ReaperWeapon.gameObject.SetActive(true); }
				else { StolenSinnerWeapons[sinnerIDToDisable].gameObject.SetActive(true); }
				break;

			default:
				SinnerBodies[sinnerIDToSwap].gameObject.SetActive(true);
				if (bodyToSwap == "Knight") 
				{ 
					ReaperRoot.transform.name = "Root";
					ReaperWeapon.gameObject.SetActive(true);
				}
				else 
				{ 
					SinnerRoot.transform.name = "Root";
				}

				if (isReaper) { StolenReaperWeapon.gameObject.SetActive(true); }
				else { SinnerWeapons[sinnerIDToSwap].gameObject.SetActive(true); }
				break;
		}

		animator.Rebind();
		animator.applyRootMotion = true;
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

	IEnumerator AbilityEffect(string abilityRole, bool isMyAbility)
	{
		yield return new WaitForSeconds(abilityEffectTime);

		//myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
		usingAbility = false;
		usedAbility = true;

		switch (abilityRole)
		{
			case "Butcher":
				animator.speed = moveSpeed;
				//if (isMyAbility) { animator.speed = 0.5f; }
				//else { animator.speed = 0.6f; }
				break;

			case "Druid":
				break;

			case "Wizard":
				if (isMyAbility) 
				{ 
					PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "WizardAbility", "Unswap" } }); 
				}
				else 
				{
					string command = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperWizardAbility"];
					string roleToUnswap = command.Replace("Swap", "");
					PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "Unswap" + roleToUnswap } });
					PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "Unswap" + roleToUnswap } }); 
				}
				break;
        }

		if (this.photonView.IsMine)
		{
			TrustManager.Instance.AbilityCoolDownText.gameObject.SetActive(false);
		}
	}

    #endregion

    #region OnTriggerEnter

    void OnTriggerEnter(Collider other)
	{
		if (currentState != "Trust") { return; }

		if (other.gameObject.tag == "Weapon")
		{
			string hitterName = other.gameObject.transform.root.name;
			int hitterNumber = Convert.ToInt32(hitterName.Replace("Player ", ""));
			PhotonView hitterPhotonView = other.gameObject.GetComponentInParent<PhotonView>();
			Animator hitterAnimator = other.gameObject.GetComponentInParent<Animator>();
			//Debug.Log(otherAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

			if (hitterAnimator != null && hitterPhotonView != null)
			{
				if (!hitterPhotonView.IsMine
					&& hitterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
				{
					string[] playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
					string hitterRole = playerRoles[hitterNumber - 1];
					string hitterTeam = "Sinner"; if (hitterRole == "Reaper") { hitterTeam = "Reaper"; }
					int myViewID = this.gameObject.GetComponentInParent<PhotonView>().ViewID;

					//Debug.Log("I got hit... myViewID=" + myViewID + " myRole: " + myRole);
					//Debug.Log("hitterRole: " + hitterRole);

					if (hitterRole != null)
					{
						switch (hitterTeam)
						{
							case "Reaper":
								if (myTeam == "Sinner")
								{
									if (myStatus == "TargetSoul")
									{
										this.photonView.RPC("TakeTargetSoul", RpcTarget.All, myViewID);
										audioSource.PlayOneShot(sinnerScreamSound, 0.5f);

										myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
										PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "TakenSoul" } });
										PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperAbility", myRole } });
										PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StateStatus", "SoulTaken" } });
									}
									else
									{
										this.photonView.RPC("StuntPlayer", RpcTarget.All, myViewID);
										audioSource.PlayOneShot(sinnerScreamSound, 0.5f);
									}
								}
								else if (myTeam == "Reaper")
								{
									// Alert something
								}
								break;

							case "Sinner":
								if (myTeam == "Reaper")
								{
									this.photonView.RPC("SlowPlayer", RpcTarget.All, myViewID);
									audioSource.PlayOneShot(reaperScreamSound, 0.5f);
								}
								else if (myTeam == "Sinner")
								{
									// Alert something
								}
								break;
						}
					}
				}
			}
		}
	}

	#endregion

	#region Photon Callbacks

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
		base.OnRoomPropertiesUpdate(propertiesThatChanged);
		if (!this.photonView.IsMine) { return; }

        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
        myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];

        foreach (var prop in propertiesThatChanged)
		{
			switch (Convert.ToString(prop.Key))
			{
				case "SetupStatus":
					switch (Convert.ToString(prop.Value))
					{
						case "BegunTrust":
							usingAbility = false;
							usedAbility = false;
							IGotSwapped = false;
							break;

						case "ShowingHumanBody":
							if (animator.GetBool("Careful Walk") == false) 
							{
								animator.SetTrigger("Look Around");
							}
							break;
					}
					break;

				case "GameStatus":
					switch (Convert.ToString(prop.Value))
					{
						case "SetupGame":
							enablePlayerControl = false;
							break;

						case "Threat":
							moveSpeed = 0.8f;
							if (myTeam == "Reaper") { enablePlayerControl = false; }
							else if (ThreatAbilitySinners.Contains(myRole)) 
							{
								string playerStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
								if (playerStatus == "Alive")
								{
									enablePlayerControl = false;
								}
								else
                                {
									enablePlayerControl = true;
								}
							}
							else { enablePlayerControl = true; }
							enableTrustAttack = false;
							break;

						case "Trust":
							enablePlayerControl = true;
							switch (myStatus)
                            {
								case "TargetSoul":
								case "Soul":
								case "TakenSoul":
									enableTrustAttack = false;
									enableTrustAbility = false;
									break;

								default:
									enableTrustAttack = true;
									switch (myTeam)
                                    {
										case "Reaper":
											reaperAbility = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperAbility"];
											moveSpeed = 1;
											if (reaperAbility != ""
											&& TrustAbilitySinners.Contains(reaperAbility))
                                            {
												enableTrustAbility = true;
											}
											else
                                            {
												enableTrustAbility = false;
											}
											break;

										case "Sinner":
											moveSpeed = 0.8f;
											enableTrustAbility = true;
											break;
                                    }
									break;
                            }
							if (PhotonNetwork.IsMasterClient) { PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "" } }); }
							if (myRole == "Wizard") { PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "" } }); }
							break;

						case "RevealTargetSoul":
						case "Truth":
							moveSpeed = 0.8f;
							enablePlayerControl = false;
							enableTrustAttack = false;
							break;
					}
					break;

				case "StateStatus":
					case "EndedTrust":
						StopAllCoroutines();
						PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "" } });
						PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "" } });
					break;

				case "WizardAbility":
					switch (Convert.ToString(prop.Value))
                    {
						case "Swap":
							if (myStatus == "TargetSoul")
							{
								StartCoroutine(PlayerIconManager.Instance.Alert("Upper", "Wizard has activated his ability!", 3f));
								this.photonView.RPC("SwapTargetSoul", RpcTarget.AllViaServer);
							}
							// PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "WizardAbility", "" } });
							break;

						case "Unswap":
							if (myStatus == "TargetSoul")
							{
								this.photonView.RPC("SwapTargetSoul", RpcTarget.AllViaServer);
							}
							else if (myRole == "Wizard")
                            {
								this.photonView.RPC("SwapWizard", RpcTarget.AllViaServer);
							}
                            break;

                    }
					break;

				case "ReaperWizardAbility":
					string command = Convert.ToString(prop.Value);
					if (command.Contains("Swap"))
                    {
						string roleToSwap = command.Replace("Swap", "");
						if (roleToSwap == myRole)
                        {
							this.photonView.RPC("SwapSinner", RpcTarget.AllViaServer, roleToSwap);
						}
                    }
					else if (command.Contains("Unswap"))
                    {
						string roleToUnswap = command.Replace("Unswap", "");
						if (roleToUnswap == myRole)
                        {
							this.photonView.RPC("SwapSinner", RpcTarget.AllViaServer, roleToUnswap);
						}
					}
					break;
			}
		}
	}

	public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
	{
		if (!this.photonView.IsMine) { return; }

		base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
		foreach (var prop in changedProps)
		{
			switch (Convert.ToString(prop.Key))
			{
				case "ReaperWizardAbility":
					string command = Convert.ToString(prop.Value);
					if (command.Contains("Unswap"))
					{
						if (myRole == "Reaper"
						&& usedAbility
						&& !swappedBack)
						{
							swappedBack = true;
							command = (string)PhotonNetwork.CurrentRoom.CustomProperties["ReaperWizardAbility"];
							string roleToUnswap = command.Replace("Unswap", "");
							this.photonView.RPC("SwapReaper", RpcTarget.AllViaServer, roleToUnswap);
						}
					}
					break;
			}
		}
	}

    #endregion
}
