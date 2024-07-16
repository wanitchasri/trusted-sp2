using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

//#pragma warning disable 649

public class RoleRevealer : MonoBehaviourPunCallbacks
{

	static public RoleRevealer Instance;

	[Header("Team Panel")]
	public GameObject TeamPanel;
	public TMP_Text TeamTextFront;
	public TMP_Text TeamTextBack;
	// public TMP_Text GoalText;

	public GameObject RolePanel; 
	public TMP_Text BlackTextMiddle;
	public TMP_Text RedTextMiddle;
	public TMP_Text WhiteTextMiddleBack;

	[Header("Player Object")]
	[SerializeField] private GameObject PlayerPrefab;
	[SerializeField] GameObject PlayerInstance;

	[Header("Spawners")]
	public GameObject RoleRevealHouse;
	public GameObject[] ReaperRevealBoxes;
	public GameObject[] SinnerRevealBoxes;
	[SerializeField] public GameObject MyRevealBox;

	[Header("Player Info")]
	string[] playerRoles;
	Dictionary<int, string> sinnerRoles;
	string myRole;

	[Header("Flags")]
	bool showedTeam;
	[HideInInspector] public bool showedRole;
	bool spawnedPrefab;

    #region MonoBehavmiour CallBacks

    private void Awake()
    {
		Instance = this;
	}

	void Start()
	{
		TeamTextFront = TeamTextFront.GetComponent<TMP_Text>();
		TeamTextBack = TeamTextBack.GetComponent<TMP_Text>();
		// GoalText = GoalText.GetComponent<TMP_Text>();
		BlackTextMiddle = BlackTextMiddle.GetComponent<TMP_Text>();
		RedTextMiddle = RedTextMiddle.GetComponent<TMP_Text>();
		WhiteTextMiddleBack = WhiteTextMiddleBack.GetComponent<TMP_Text>();

		if (PhotonNetwork.IsMasterClient)
		{
			InitGameStatuses();
		}
	}

    #endregion

    #region Role Manager
    // STATUSES
    void InitGameStatuses()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			//Debug.LogError("PhotonNetwork : Only master client can initialize status, but you are not.");
			return;
		}
		Debug.Log("MasterClient: Initializing Statuses");

		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "StartSetup" } });
		// SetupStatusList = {"StartSetup", "InitializedRoles", "ManagedRoles", "AssignedRoles", "ShowingTeams", "SpawnTime", "ShowingRoles", "ShowedRoles", "FinishedSetup"}		
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "GameStatus", "SetupGame" } });
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "Day", 1 } });

		int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "MaxPlayers", maxPlayers } });
		for (int i = 0; i <= maxPlayers; i++)
        {
			PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { {  "VoteRecord"+i , 0 } });
        }
	}

	// ABOUT ROLES
	void InitRoles()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			//Debug.LogError("PhotonNetwork : Only master client can initialize roles, but you are not.");
			return;
		}

		// Debug.Log("MasterClient: Initializing Roles");

		Dictionary<int, string> minorRoles = new Dictionary<int, string>();
		minorRoles.Add(0, "Reaper");
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "MinorRoles", minorRoles } });

		sinnerRoles = new Dictionary<int, string>();
		sinnerRoles.Add(0, "Butcher");
		sinnerRoles.Add(1, "Druid");
		sinnerRoles.Add(2, "Knight");
		sinnerRoles.Add(3, "Shaman");
		sinnerRoles.Add(4, "Stalker");
		sinnerRoles.Add(5, "Wizard");
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SinnerRoles", sinnerRoles } });

		Dictionary<int, string> extraSinnerRoles = new Dictionary<int, string>();
		extraSinnerRoles.Add(0, "Witch");
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ExtraGoodRoles", extraSinnerRoles } });

		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperAbility", "" } });
		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "ReaperWizardAbility", "" } });

		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "InitializedRoles" } });
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "CloneStatus", "Unspawned"} });

		Dictionary<int, string> cloneInfo = new Dictionary<int, string>();
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ReaperCloneInfo", cloneInfo } });
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "KnightCloneInfo", cloneInfo } });
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "ShamanCloneInfo", cloneInfo } });
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "StalkerCloneInfo", cloneInfo } });
	}

	void ManageRolesForAll()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			//Debug.LogError("PhotonNetwork : Only master client can manage roles, but you are not.");
			return;
		}

		// Debug.Log("MasterClient: Managing Roles");

		// Get CustomProperties
		Dictionary<int, string> getMinorRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["MinorRoles"];
		string minorRole = getMinorRoles[0];
		Dictionary<int, string> getSinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];

		// Create roleArr for Role Assignment
		int maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];
		string[] roleArr = new string[maxPlayers];

		// Calculate badCount
		float minorRatio = 0.3f;
		int minorCount = (int)(maxPlayers * minorRatio);
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "AliveReapers", minorCount } }); //minorCount
		int sinnerCount = maxPlayers - minorCount;
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "AliveSinners", sinnerCount } }); //sinnerCount

		int roleAssigned = 0;
		while (roleAssigned < maxPlayers)
		{
			int possibility = UnityEngine.Random.Range(0, 10);
			int sinnerIndex = UnityEngine.Random.Range(0, getSinnerRoles.Count);
			string sinnerRole = getSinnerRoles[sinnerIndex];
			

			if ((possibility <= 3)
					&& (!roleArr.Contains(sinnerRole))
					&& (sinnerCount != 0))
			{
				roleArr[roleAssigned] = sinnerRole;
				sinnerCount--;
				roleAssigned++;
			}
			// Bad
			else if ((possibility >= 8)
				&& (minorCount != 0))
			{
				roleArr[roleAssigned] = minorRole;
				minorCount--;
				roleAssigned++;
			}
			// Or.. Skip
		}

  //      // Just for Testing !!
  //      roleArr[0] = minorRole;
		//roleArr[1] = minorRole;
		//roleArr[2] = "Butcher";
  //      roleArr[3] = "Stalker";
  //      roleArr[4] = "Shaman";
		//roleArr[5] = "Knight";
		//roleArr[6] = "Druid";
		//roleArr[7] = "Wizard";

		//for (int i = 0; i < getSinnerRoles.Count; i++)
  //      {
		//	Debug.Log("Sinner: " + (i + 1) + "Sinner: " + getSinnerRoles[i]);
  //      }

		//Debug.Log(roleArr.Length);
		for (int i = 0; i < roleArr.Length; i++)
        {
            Debug.Log("Player: " + (i+1) + ", Role: " + roleArr[i]);
        }

        Hashtable hashRoleAssignment = new Hashtable() { { "RoleAssignment", roleArr } };
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashRoleAssignment);

		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "ManagedRoles" } });
	}

	void AssignRolesToEach()
	{
		if (!PhotonNetwork.LocalPlayer.IsLocal) { return;  }

		// Debug.Log(PhotonNetwork.LocalPlayer.IsLocal);
		
		int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
		// Debug.Log("Player Number:" + playerNumber);

		string[] getRoleAssignment = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
		string playerRole = getRoleAssignment[playerNumber];

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Role", playerRole } });
		Debug.Log("My Role: " + playerRole);

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "AssignedRole" } });
		// playerStatus = { "AssignedRole", "Spawned" , "Alive", "TargetSoul" / "Soul" / "TakenSoul" }

		if (playerRole == "Reaper") 
		{ 
			PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Team", "Reaper" } });
		}
		else 
		{ 
			PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "Team", "Sinner" } }); 
		}

		if (PhotonNetwork.IsMasterClient)
        {
			PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "AssignedRoles" } });
		}
	}

	void ShowTeam()
	{
		if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }
		// Debug.Log("PhotonView.IsMine: ShowTeam");

		string playerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
		if (playerTeam == "Reaper") { TeamPanel.transform.Find("Background").GetComponent<Image>().color = new Color32(102, 0, 0, 255); }

		TeamPanel.SetActive(true);
		TeamTextFront.text = playerTeam;
		TeamTextBack.text = TeamTextFront.text;

		//string goal = "";
		//switch (playerTeam)
  //      {
		//	case "Sinner":
		//		goal = "Protect All Souls";
		//		break;
		//	case "Reaper":
		//		goal = "Hunt All Souls";
		//		break;
  //      }
		//GoalText.text = goal;

		showedTeam = true;
	}

	#endregion

	#region Reveal Player's Role

	void SpawnPlayerPrefab()
	{
		if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }
		spawnedPrefab = true;

		if (PlayerPrefab == null)
		{ // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.

			Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
		}
		else
		{
			if (PlayerManagerSa.LocalPlayerInstance == null)
			{
				// Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

				myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
				switch (myRole)
                {
					case "Reaper":
						int reaperID = GetReaperID(PhotonNetwork.LocalPlayer.GetPlayerNumber());
						MyRevealBox = ReaperRevealBoxes[reaperID];
						break;

					default:
						int sinnerID = GetSinnerID(myRole);
						Debug.Log("DEBUG "+myRole+" "+sinnerID);
						MyRevealBox = SinnerRevealBoxes[sinnerID];
						break;
                }
				Transform SpawnObj = MyRevealBox.transform.Find("Spawners").transform.Find("Spawner");

				// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
				PlayerInstance = PhotonNetwork.Instantiate(this.PlayerPrefab.name, SpawnObj.position, SpawnObj.rotation);
				PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Spawned" } });
			}
			else
			{
				Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
			}
		}
	}
	int GetReaperID(int playerNumber)
	{
		playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["RoleAssignment"];
		int reaperCount = 0;
		for (int i = 0; i < playerRoles.Length; i++)
		{
			if (playerRoles[i] == "Reaper")
			{
				if (i == playerNumber)
                {
					return reaperCount;
                }
				reaperCount += 1;
			}
		}
		return -1;
	}
	int GetSinnerID(string role)
    {
		sinnerRoles = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["SinnerRoles"];
		for (int i=0; i<sinnerRoles.Count; i++)
        {
			if (role == sinnerRoles[i])
            {
				return i;
            }
        }
		return -1;
	}

	#endregion

	// Check PropertiesUpdate
	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
		if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }
		myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

		foreach (var prop in propertiesThatChanged)
        {
			// Debug.Log("UPDATE RoomProperty: " + "Key=" + prop.Key + " " + "Value=" + prop.Value);

			switch (Convert.ToString(prop.Key))
			{
                case "MaxPlayers":
					if (PhotonNetwork.IsMasterClient)
                    {
						Dictionary<int, string> playerStatuses = new Dictionary<int, string>();
						int maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"];
						for (int i = 1; i <= maxPlayers; i++)
						{
							playerStatuses.Add(i, "Alive");
						}
						Hashtable hashPlayerStatuses = new Hashtable() { { "PlayerStatuses", playerStatuses } };
						PhotonNetwork.CurrentRoom.SetCustomProperties(hashPlayerStatuses);
					}
					break;

                case "GameStatus":
					switch (Convert.ToString(prop.Value))
                    {
						case "Threat":
							break;
                    }
					break;

				case "SetupStatus":
					if (PhotonNetwork.LocalPlayer.IsLocal)
					{
						switch (Convert.ToString(prop.Value))
						{
							case "StartSetup":
								InitRoles();
								break;
							case "InitializedRoles":
								ManageRolesForAll();
								break;
							case "ManagedRoles":
								AssignRolesToEach();
								break;
							case "ShowingTeams":
								if (!showedTeam)
                                {
									ShowTeam();
                                }
								break;
							case "SpawnTime":
								TeamPanel.SetActive(false);
								if (!spawnedPrefab)
                                {
									spawnedPrefab = true;
									SpawnPlayerPrefab();
								}
								break;
							case "ShowingHumanBody":
								if (!spawnedPrefab)
								{
									SpawnPlayerPrefab();
								}
								break;
							case "ShowedHumanBody":
								GameObject RevealMirror = MyRevealBox.transform.Find("RevealMirror").gameObject;
								if (!showedRole)
                                {
									showedRole = true;
									StartCoroutine(BlackOutMirror(RevealMirror));
								}
								break;
							case "ShowingRoles":
								RevealMirror = MyRevealBox.transform.Find("RevealMirror").gameObject;
								if (showedRole)
                                {
									showedRole = false;
									StartCoroutine(ShowRole(RevealMirror, myRole));
									//switch (myRole)
									//{
									//    case "Reaper":
									//        ReaperRoleText.text = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
									//        ReaperRoleText.gameObject.SetActive(true);
									//        break;

									//    default:
									//        SinnerRoleText.text = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
									//        SinnerRoleText.gameObject.SetActive(true);
									//        break;
									//}
								}
								break;
						}
					}
						break;

			}
        }
    }

	IEnumerator BlackOutMirror(GameObject RevealMirror)
	{
		RevealMirror.transform.Find("HumanMirror").gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		RevealMirror.transform.Find("HumanMirror").gameObject.SetActive(false);
		RevealMirror.transform.Find("MirrorBlackout").gameObject.SetActive(true);
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "ShowingRoles" } });
		}
		yield return new WaitForSeconds(0f);
	}

	IEnumerator ShowRole(GameObject RevealMirror, string myRole)
	{
		Debug.Log("Showing Role Text");
		RevealMirror.transform.Find("MirrorBlackout").gameObject.SetActive(false);
		RevealMirror.transform.Find("RoleMirror").gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		TMP_Text RoleText = RedTextMiddle;

		if (myRole == "Reaper") { RoleText = BlackTextMiddle; }
		RolePanel.SetActive(true);
        RoleText.text = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
		RoleText.gameObject.SetActive(true);
		WhiteTextMiddleBack.text = RoleText.text;
		WhiteTextMiddleBack.gameObject.SetActive(true);
		yield return new WaitForSeconds(2f);
		RoleText.gameObject.SetActive(false);
		WhiteTextMiddleBack.gameObject.SetActive(false);
		RolePanel.SetActive(false);
		yield return new WaitForSeconds(2f);
		RoleRevealHouse.SetActive(false);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

		foreach (var prop in changedProps)
		{

			switch (Convert.ToString(prop.Key))
			{
				case "Team":
					ShowTeam();
					if (PhotonNetwork.IsMasterClient)
					{
						PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "ShowingTeams" } });
					}
					break;
			}
		}

	}

}
