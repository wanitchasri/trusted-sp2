using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ken.Test;
using Photon.Pun;

public class Skills : MonoBehaviourPun
{
    public static Skills Instance;
    //PhotonView thisPhotonView;

    private void Start()
    {
        Instance = this;
	}

    void Update()
	{
        //if (PlayerAnimatorManager.Instance.gameObject.GetComponent<PhotonView>() != null)
        //{
        //    thisPhotonView = PlayerAnimatorManager.Instance.gameObject.GetComponent<PhotonView>();
        //}

    }

 //   public void SkillButtonClick()
 //   {
 //       //if (thisPhotonView.IsMine)
 //       //{
 //       //    thisPhotonView.RPC("Stunt", RpcTarget.All);
 //       //}
 //       // thisPhotonView.RPC("Stunt", RpcTarget.All);
 //       PlayerAnimatorManagerKen.Instance.StuntButton();
 //   }

 //   public void HitButtonClick()
 //   {
 //       PlayerAnimatorManagerKen.Instance.HitButton();
 //   }

 //   public void StuntButtonClick()
 //   {
 //       //if (thisPhotonView.IsMine)
 //       //{
 //       //    thisPhotonView.RPC("Stunt", RpcTarget.All);
 //       //}
 //       // thisPhotonView.RPC("Stunt", RpcTarget.All);
 //       PlayerAnimatorManagerKen.Instance.StuntButton();
 //   }

	//public void SlowButtonClick()
 //   {
 //       //if (thisPhotonView.IsMine)
 //       //{
 //       //    thisPhotonView.RPC("Slow", RpcTarget.All);
 //       //}
 //       // thisPhotonView.RPC("Slow", RpcTarget.All);
 //       PlayerAnimatorManagerKen.Instance.SlowButton();
 //   }
}
