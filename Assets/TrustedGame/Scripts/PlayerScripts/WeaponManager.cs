using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponManager : MonoBehaviourPunCallbacks
{
    public static WeaponManager Instance;

    [Header("Player Info")]
    string myTeam;
    string myStatus;

    [Header("Audio Clips")]
    [SerializeField] AudioSource audioSource;
    public AudioClip reaperScreamSound;
    public AudioClip sinnerScreamSound;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.photonView.IsMine && PhotonNetwork.IsConnected) { return; }

        if (PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null)
        {
            myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            myStatus = (string)PhotonNetwork.LocalPlayer.CustomProperties["PlayerStatus"];
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Axe")
        {
            string hitterName = other.gameObject.transform.parent.name;
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
}
