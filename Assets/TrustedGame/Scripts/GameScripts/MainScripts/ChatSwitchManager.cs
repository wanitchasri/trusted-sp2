using Ken.Test;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChatSwitchManager : MonoBehaviour
{
	public static ChatSwitchManager Instance;

	[Header("Chatbox")]
	[SerializeField]
	public GameObject AllChat;
	public GameObject SinnersChat;
	public GameObject ReapersChat;

	[Header("Game Info")]
	string currentState;

	[Header("Player Info")]
	string myRole;

	void Start()
	{
		Instance = this;
	}

	// Update is called once per frame
	void Update()
	{
		if (!PhotonNetwork.LocalPlayer.IsLocal) { return; }

		if (PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null)
		{
			currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
			switch (currentState)
            {
				case "Threat":
					AllChat.SetActive(false);
					SeparateChat();
					break;

				case "Truth":
					CombineChat();
					break;

				default:
					DisableChat();
					break;
            }
		}
	}

	void SeparateChat()
	{
		myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
		if (myRole == "Reaper")
		{
			SinnersChat.SetActive(false);
			ReapersChat.SetActive(true);
		}
	}

	void CombineChat()
    {
		SinnersChat.SetActive(false);
		ReapersChat.SetActive(false);
		AllChat.SetActive(true);
	}

	void DisableChat()
    {
		SinnersChat.SetActive(false);
		ReapersChat.SetActive(false);
		AllChat.SetActive(false);
	}
}
