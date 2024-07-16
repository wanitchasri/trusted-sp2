using Ken.Test;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChatSwitcher : MonoBehaviour
{
	public static ChatSwitcher Instance;

	[SerializeField]
	public GameObject SinnersChat;
	public GameObject ReapersChat;

	string myRole;

	void Start()
	{
		Instance = this;
	}

	// Update is called once per frame
	void Update()
	{
		if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] != null)
		{

			myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
			ChooseChat();
		}
	}
	public void ChooseChat()
	{
		if (myRole == "Sinner")
		{
			SinnersChat.SetActive(true);
			ReapersChat.SetActive(false);
		}
		else if (myRole == "Reaper")
		{
			SinnersChat.SetActive(false);
			ReapersChat.SetActive(true);
		}
	}
}
