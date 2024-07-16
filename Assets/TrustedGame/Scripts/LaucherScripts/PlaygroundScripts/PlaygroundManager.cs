using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

//#pragma warning disable 649

public class PlaygroundManager : MonoBehaviourPunCallbacks
{

	static public PlaygroundManager Instance;

	[Header("Status Panel")]
	public TMP_Text StatusText;

	[Header("Team Panel")]
	public GameObject TeamPanel;
	public TMP_Text TeamText;
	public TMP_Text GoalText;

    [SerializeField]
    private GameObject playerPrefab;
	public GameObject playerInstance;
	private float spawnPosX;

	[Header("Flags")]
	bool showedTeam;
	public bool showedRole;

    #region MonoBehaviour CallBacks

    private void Awake()
    {
		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "MaxPlayers", 8 } });
	}

    void Start()
	{
		// Sa's Test
		//Debug.Log("--- Playground ---");
		//Debug.Log("PlayerCount = " + PhotonNetwork.CurrentRoom.PlayerCount);
		//Debug.Log("IsMasterClient: " + PhotonNetwork.IsMasterClient);

		Instance = this;

		StatusText = StatusText.GetComponent<TMP_Text>();
		TeamText = TeamText.GetComponent<TMP_Text>();
		GoalText = GoalText.GetComponent<TMP_Text>();

		SpawnPlayerPrefab();

		if (PhotonNetwork.IsMasterClient)
		{
			InitGameStatuses();
		}
	}

    void SpawnPlayerPrefab()
    {
		//Dictionary<int, GameObject> getPlayerPrefabs = (Dictionary<int, GameObject>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerPrefabs"];
		//getPlayerPrefabs.Add(PhotonNetwork.LocalPlayer.GetPlayerNumber(), playerPrefab);
		//PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerPrefabs", getPlayerPrefabs } });

		if (playerPrefab == null)
		{ // #Tip Never assume public properties of Components are filled up properly, always check and inform the developer of it.

			Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
		}
		else
		{
			if (PlayerManagerSa.LocalPlayerInstance == null)
			{
				Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);

				spawnPosX = (float)PhotonNetwork.LocalPlayer.GetPlayerNumber() - 5f;

				//Debug.Log(spawnPosX);
				// we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
				playerInstance = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(spawnPosX, 5f, 0f), Quaternion.identity, 0);

				//Dictionary<int, GameObject> getPlayerInstances = (Dictionary<int, GameObject>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerInstances"];
				//getPlayerInstances.Add(PhotonNetwork.LocalPlayer.GetPlayerNumber(), playerInstance);
				//PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "PlayerInstances", getPlayerInstances } });
			}
			else
			{
				Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
			}
		}
	}
	
	#endregion

	// STATUSES
	void InitGameStatuses()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			//Debug.LogError("PhotonNetwork : Only master client can initialize status, but you are not.");
			return;
		}

		Debug.Log("MasterClient: Initializing Statuses");

		Hashtable hashSetupStatus = new Hashtable() { { "SetupStatus", "Waiting" } };
		// SetupStatusList = {"Waiting", "InitializedRoles", "ManagedRoles", "AssignedRoles", "ShowingTeams", "ShowedTeams", "ShowingRoles", "ShowedRoles", "FinishedSetup"}

		Hashtable hashGameStatus = new Hashtable() { { "GameStatus", "SetupGame" } };
		// GameStatusList = {"SetupGame", "StartGame",<LOOP> "Threat", "Trust", "Truth" <LOOP>, "EndGame"}

		Hashtable hashDay = new Hashtable() { { "Day", 1 } };

		PhotonNetwork.CurrentRoom.SetCustomProperties(hashSetupStatus);
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashGameStatus);
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashDay);
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

		Dictionary<int, string> basicSinnerRoles = new Dictionary<int, string>();
		basicSinnerRoles.Add(0, "Butcher");
		basicSinnerRoles.Add(1, "Druid");
		basicSinnerRoles.Add(2, "Knight");
		basicSinnerRoles.Add(3, "Shaman");
		basicSinnerRoles.Add(4, "Stalker");
		basicSinnerRoles.Add(5, "Wizard");

		Dictionary<int, string> extraSinnerRoles = new Dictionary<int, string>();
		extraSinnerRoles.Add(0, "Witch");

		Hashtable hashMinorRoles = new Hashtable() { { "MinorRoles", minorRoles } };
		Hashtable hashBasicSinnerRoles = new Hashtable() { { "SinnerRoles", basicSinnerRoles } };
		Hashtable hashExtraGoodRoles = new Hashtable() { { "ExtraGoodRoles", extraSinnerRoles } };
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashMinorRoles);
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashBasicSinnerRoles);
		PhotonNetwork.CurrentRoom.SetCustomProperties(hashExtraGoodRoles);

		PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "InitializedRoles" } });
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
		Dictionary<int, string> tempSinnerRoles = getSinnerRoles;

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
			int sinnerIndex = UnityEngine.Random.Range(0, tempSinnerRoles.Count);
			string sinnerRole = tempSinnerRoles[sinnerIndex];
			

			if ((possibility <= 3)
					&& (sinnerRole != null)
					&& (sinnerCount != 0))
			{
				roleArr[roleAssigned] = sinnerRole;
				tempSinnerRoles[sinnerIndex] = null;
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

        // Just for Testing !!
        roleArr[0] = minorRole;
		roleArr[1] = minorRole;
		roleArr[2] = "Butcher";
        roleArr[3] = "Stalker";
        roleArr[4] = "Shaman";
		roleArr[5] = "Knight";
		roleArr[6] = "Druid";
		roleArr[7] = "Wizard";


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

		Hashtable hashPlayerRole = new Hashtable() { { "Role", playerRole } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(hashPlayerRole);
		Debug.Log("My Role: " + playerRole);

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "AssignedRole" } });

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
		// Debug.Log("PhotonView.IsMine: ShowTeam");

		PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() { { "PlayerStatus", "Alive" } });
		string playerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
		TeamText.text = playerTeam;

		string goal = "";
		switch (playerTeam)
        {
			case "Sinner":
				goal = "Protect All Souls";
				break;
			case "Reaper":
				goal = "Hunt All Souls";
				break;
        }
		GoalText.text = goal;

		TeamPanel.SetActive(true);
		showedTeam = true;
	}

	// Check PropertiesUpdate
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

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
					break;
				case "SetupStatus":
					if (PhotonNetwork.LocalPlayer.IsLocal)
					{
						switch (Convert.ToString(prop.Value))
						{
							case "Waiting":
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
							case "ShowedTeams":
								TeamPanel.SetActive(false);
								PlayerIconManager.Instance.ShowRole(true);
								if (PhotonNetwork.IsMasterClient)
								{
									PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { "SetupStatus", "ShowingRoles" } });
								}
								break;
							case "ShowingRoles":
								if (!showedRole)
                                {
									TeamPanel.SetActive(false);
									PlayerIconManager.Instance.ShowRole(true);
								}
								break;
							case "ShowedRoles":
								PlayerIconManager.Instance.ShowRole(false);
								break;
						}
					}
						break;

			}
        }
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
