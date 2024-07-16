using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Camera work. Follow a target
/// </summary>
public class PlayerCameraWork : MonoBehaviourPunCallbacks
{
	#region Private Fields

	[Header("Camera Work Flags")]
	[SerializeField] private bool followOnStart = false;            //Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.
	bool startCamera;
	bool isFollowing;
	// maintain a flag internally to reconnect if target is lost or camera is switched

	[Header("Camera Settings")]
	Vector3 cameraOffset = Vector3.zero;                            // Cache for camera offset
	[SerializeField] private float distance = 3.0f;					//distance in the local x-z plane to the target >> Positive = Back, Negative = Front               // height above target
	[SerializeField] private float height = 3.0f;
	[SerializeField] private Vector3 centerOffset = new Vector3(0, 3f, 0);   //Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground
	[SerializeField] private float smoothSpeed = 5f;

	[Header("Camera Info")]
	Transform cameraTransform;
	[SerializeField] Transform playerTransform;
	[SerializeField] Transform targetObj;

	[Header("Game Info")]
	string setupStatus;
	string currentState;
	string stateStatus;

	[Header("Player Info")]
	string myTeam;
	string myRole;
	GameObject myCurrentBody;
	string[] ThreatAbilitySinners = { "Knight", "Shaman", "Stalker" };
	Dictionary<int, string> playerStatuses;

	[Header("Overall Camera Transform")]
	Vector3 ThreatCameraPos;
	Quaternion ThreatCameraRot;
	Vector3 TruthCameraPos;
	Quaternion TruthCameraRot;

	[Header("Camera Control")]
	int cameraView;
	bool setFirstPersonView;
	bool setThirdPersonView;
	bool setBirdEyeView;
	bool shaked;
	bool setCamera;
	bool setFollowing;

	[Header("Network Message")]
	bool sentCameraMessage;
	string messageToDebug;

	#endregion

	#region MonoBehaviour Callbacks

	void Awake()
	{
		playerTransform = this.gameObject.transform;
	}

	/// <summary>
	/// MonoBehaviour method called on GameObject by Unity during initialization phase
	/// </summary>
	void Start()
	{
		// Start following the target if wanted.
		if (followOnStart)
		{
			OnStartFollowing();
		}
	}

	private void Update()
	{
		if (!this.photonView.IsMine) { return; }
		if (this.gameObject.name.Contains("CloneFor")) { return; }

		myCurrentBody = this.gameObject.GetComponent<PlayerManagerSa>().myCurrentBody;

		if ((string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null) 
		{
			setupStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["SetupStatus"];
			currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
			stateStatus = (string)PhotonNetwork.CurrentRoom.CustomProperties["StateStatus"];
			myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
			myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];

			switch (currentState)
            {
				case "SetupGame":
					if (RoleRevealer.Instance != null)
					{
						Transform stopper = RoleRevealer.Instance.MyRevealBox.transform.Find("Spawners").transform.Find("Stopper");
						if (playerTransform.position.z < stopper.position.z)
						{
							distance = 2f;
							height = 1f;
							centerOffset = new Vector3(0, 1, 0);
						}
						else
						{
							if (PhotonNetwork.IsMasterClient
								&& !sentCameraMessage)
							{
								sentCameraMessage = true;
								PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "SetupStatus", "ShowingHumanBody" } });
							}

							distance = 0f;
							height = 1.7f;
							centerOffset = new Vector3(0, 1.7f, 0);

							//switch (setupStatus)
							//                     {
							//	case "ShowingHumanBody":
							//                             distance = 1.2f;
							//                             height = 1.7f;
							//                             centerOffset = new Vector3(-0.05f, 1.7f, 0);
							//                             smoothSpeed = 1f;
							//		break;

							//	case "ShowingRoles":
							//		//if (!shaked)
							//  //                           {
							//		//	shaked = true;
							//		//	StartCoroutine(Shake(2f, 0.2f));
							//		//}
							//		break;

							//	case "ShowedHumanBody":
							//	case "ShowedRoles":
							//                             distance = 0f;
							//                             height = 1.7f;
							//                             centerOffset = new Vector3(0, 1.7f, 0);
							//		break;
							//}

							//this.photonView.RPC("LetsDebug", RpcTarget.All, "I am..." + this.gameObject.name + "distance=" + distance + "height" + height + "centerOffset" + centerOffset);
						}
					}
					break;

				case "RevealTargetSoul":
                    if (!setCamera)
                    {
                        setCamera = true;
                        cameraTransform.position = TargetRevealer.Instance.CameraStartPos.position;
                        cameraTransform.rotation = TargetRevealer.Instance.CameraStartPos.rotation;
                    }
                    break;

				case "Trust":
					setFirstPersonView = Input.GetKeyDown(KeyCode.Alpha1);
					setThirdPersonView = Input.GetKeyDown(KeyCode.Alpha2);
					setBirdEyeView = Input.GetKeyDown(KeyCode.Alpha3);

					if (setFirstPersonView) { cameraView = 1; }
					else if (setThirdPersonView) { cameraView = 2; }
					else if (setBirdEyeView) { cameraView = 3; }
					break;
			}
		}
	}


    void LateUpdate()
	{
		if (!this.photonView.IsMine) { return; }

		// The transform target may not destroy on level load, 
		// so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
		if (cameraTransform == null && startCamera)
		{
			OnStartFollowing();
		}

		switch (cameraView)
		{
			case 1: //"FirstPersonView"
				distance = 0.1f;
				height = 1.7f;
				centerOffset = new Vector3(0, 1.7f, 0);
				smoothSpeed = 5f;
				break;

			case 2: //"ThirdPersonView"
				distance = 2;
				height = 2;
				centerOffset = new Vector3(0, 1.7f, 0);
				smoothSpeed = 5f;
				break;

			case 3: //"BirdeyeView"
				distance = 7;
				height = 6;
				centerOffset = new Vector3(0, 3, 0);
				smoothSpeed = 5f;
				break;
		}

		if (startCamera)
        {
			switch (currentState)
			{
				case "SetupGame":
				case "Trust":
					isFollowing = true;
					break; 

				case "Threat":
					if (stateStatus == "MovedPlayersToTheirPosition") 
					{
						if (myTeam == "Reaper")
						{
							isFollowing = false;
						}
						else if (ThreatAbilitySinners.Contains(myRole))
                        {
							string playerStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
							if (playerStatus == "Alive")
							{
								isFollowing = false;
							}
							else
							{
								isFollowing = true;
							}
						}
						else { isFollowing = true; }
					}
					break;

				case "RevealTargetSoul":
				case "Truth":
					isFollowing = false;
					break;

				default:
					isFollowing = true;
					break;
			}

			// we don't smooth anything, we go straight to the right camera shot
			Cut();
		}

		// only follow is explicitly declared
		if (isFollowing) {
			Follow ();
		}

	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Raises the start following event. 
	/// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
	/// </summary>
	public void OnStartFollowing()
	{	      
		cameraTransform = Camera.main.transform;
		startCamera = true;

		//messageToDebug = "[State" + currentState + "][myTeam" + myTeam + "][" + this.gameObject.name + "] : OnStartFollowing()";
		//this.photonView.RPC("LetsDebug", RpcTarget.All, messageToDebug);
	}
		
	#endregion

	#region Private Methods

	/// <summary>
	/// Follow the target smoothly
	/// </summary>
	void Follow()
	{
		cameraOffset.z = -distance;
		cameraOffset.y = height;
			
		cameraTransform.position = Vector3.Lerp(cameraTransform.position, playerTransform.position + playerTransform.TransformVector(cameraOffset), smoothSpeed*Time.deltaTime);
		cameraTransform.LookAt(playerTransform.position + centerOffset);

		//messageToDebug = "[State" + currentState + "][myTeam" + myTeam + "][" + this.gameObject.name + "] : Follow()";
		//this.photonView.RPC("LetsDebug", RpcTarget.All, messageToDebug);
	}

	   
	void Cut()
	{
		cameraOffset.z = -distance;
		cameraOffset.y = height;

		if (currentState == "Threat"
			&& (myTeam == "Reaper" || ThreatAbilitySinners.Contains(myRole)))
        {
			switch (myTeam)
            {
				case "Reaper":
					cameraTransform.position = StatusManager.Instance.ThreatCameraForReapers.position;
					cameraTransform.rotation = StatusManager.Instance.ThreatCameraForReapers.rotation;
					break;

				default:
					if (isFollowing)
                    {
						cameraTransform.position = playerTransform.position + playerTransform.TransformVector(cameraOffset);
						cameraTransform.LookAt(playerTransform.position + centerOffset);
					}
					else
                    {
						switch (myRole)
						{
							case "Knight":
								cameraTransform.position = StatusManager.Instance.ThreatCameraForKnight.position;
								cameraTransform.rotation = StatusManager.Instance.ThreatCameraForKnight.rotation;
								break;

							case "Shaman":
								cameraTransform.position = StatusManager.Instance.ThreatCameraForShaman.position;
								cameraTransform.rotation = StatusManager.Instance.ThreatCameraForShaman.rotation;
								break;

							case "Stalker":
								cameraTransform.position = StatusManager.Instance.ThreatCameraForStalker.position;
								cameraTransform.rotation = StatusManager.Instance.ThreatCameraForStalker.rotation;
								break;
						}
					}
					break;
			}
		}
        else if (currentState == "RevealTargetSoul")
        {
            RotateAroundObj(0.6f);
        }
        else if (currentState == "Truth")
        {
			cameraTransform.position = StatusManager.Instance.TruthCamera.position;
			cameraTransform.rotation = StatusManager.Instance.TruthCamera.rotation;
		}
		else
        {
			cameraTransform.position = playerTransform.position + playerTransform.TransformVector(cameraOffset);
			cameraTransform.LookAt(playerTransform.position + centerOffset);
		}

		//messageToDebug = "[State" + currentState + "][myTeam" + myTeam + "][" + this.gameObject.name + "] : Cut() + " + location;
		//this.photonView.RPC("LetsDebug", RpcTarget.All, messageToDebug);
	}

	bool HasSomeoneToVote()
    {
		playerStatuses = (Dictionary<int, string>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerStatuses"];
		switch (myRole)
        {
            case "Shaman":
				foreach (var status in playerStatuses)
                {
					if (status.Value.Contains("Soul"))
                    {
						return true;
                    }
                }
                break;
        }
		return false;
    }

	void RotateAroundObj(float rotateSpeed)
    {
		targetObj = TargetRevealer.Instance.Mirror;
		cameraTransform.RotateAround(targetObj.position, Vector3.up, rotateSpeed);
	}

	IEnumerator Shake(float duration, float magnitude)
	{
		Vector3 originalPos = cameraTransform.position;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			float x = originalPos.x + UnityEngine.Random.Range(-1f, 1f) * magnitude;
			// float y = originalPos.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;

			cameraTransform.position = new Vector3(x, originalPos.y, originalPos.z);
			elapsed += Time.deltaTime;
			yield return 0;
		}

		cameraTransform.position = originalPos;
	}
	#endregion

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
		if (!this.photonView.IsMine) { return; }

		base.OnRoomPropertiesUpdate(propertiesThatChanged);

		foreach (var prop in propertiesThatChanged)
        {
			switch (Convert.ToString(prop.Key))
			{
				case "GameStatus":
					switch (Convert.ToString(prop.Value))
                    {
						case "Threat":
						case "Trust":
							cameraView = 2;
							break;

						case "RevealTargetSoul":
							setCamera = false;
							break;

						case "Truth":
							break;
					}
					break;
			}
		}
    }
}
