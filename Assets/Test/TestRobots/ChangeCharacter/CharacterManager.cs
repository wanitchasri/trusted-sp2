using Ken.Test;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterManager : MonoBehaviour
{
	//[SerializeField]
 //   public GameObject Robot1;
 //   public GameObject Robot2;
 //   public GameObject Girl1;
 //   public GameObject Girl2;
	//public Avatar Avatar1;
	//public Avatar Avatar2;

	//public GameObject Character;

	//// Update is called once per frame
	//void Update()
 //   {
 //       if (!GameManager.gameIsPaused) {
	//		if (Input.GetKeyDown(KeyCode.C))
	//		{
	//			Debug.Log("Change Character");
	//			ChangeCharacter();
	//		}
	//	}
	//}

	//void ChangeCharacter()
	//{
	//	if (GameManager.player == "Robot")
	//	{
	//		Debug.Log("ChangeToGirl");

	//		Animator characterAnim = Character.GetComponent<Animator>();
	//		characterAnim.avatar = Avatar2;

	//		//PhotonNetwork.Replace(oldChar, playerPrefab2);
	//		//viewedModel1.SetActive(false);
	//		// Robot1 = GameObject.FindWithTag("Part1");
	//		Robot1.SetActive(false);
	//		// Robot2 = GameObject.FindWithTag("Part2");
	//		Robot2.SetActive(false);
	//		// Girl1 = GameObject.FindWithTag("Part3");
	//		Girl1.SetActive(true);
	//		// Girl2 = GameObject.FindWithTag("Part4");
	//		Girl2.SetActive(true);
	//		GameManager.player = "Girl";
	//	}
	//	else if (GameManager.player == "Girl")
	//	{
	//		Debug.Log("ChangeToRobot");

	//		Animator characterAnim = Character.GetComponent<Animator>();
	//		characterAnim.avatar = Avatar1;

	//		//PhotonNetwork.Replace(oldChar, playerPrefab1);
	//		// GameObject Robot1 = GameObject.FindWithTag("Part1");
	//		Robot1.SetActive(true);
	//		// GameObject Robot2 = GameObject.FindWithTag("Part2");
	//		Robot2.SetActive(true);
	//		// GameObject Girl1 = GameObject.FindWithTag("Part3");
	//		Girl1.SetActive(false);
	//		// GameObject Girl2 = GameObject.FindWithTag("Part4");
	//		Girl2.SetActive(false);
	//		GameManager.player = "Robot";
	//	}
	//}
}
