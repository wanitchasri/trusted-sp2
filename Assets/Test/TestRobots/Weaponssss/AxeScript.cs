using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//using UnityEditor.Animations;
//using System.Linq;

namespace Ken.Test
{
	public class AxeScript : MonoBehaviour
    {
        public static AxeScript Instance;
        public string myRole = "";
        PhotonView myPhotonView;

        //Axe Hit Sound
        public AudioClip sinnerGotAxeHitEffectClip;
        public AudioClip reaperGotAxeHitEffectClip;

        // Start is called before the first frame update
        void Start()
		{
            Instance = this;
            // Set the role custom property for the local player
        }

		// Update is called once per frame
		void Update()
		{
            myPhotonView = this.gameObject.GetComponentInParent<PhotonView>();
            if (!myPhotonView.IsMine) { return; }
            if ((string)PhotonNetwork.LocalPlayer.CustomProperties["Role"] != null)
            {
                myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["Role"];
            }

            //if (AxeScript.Instance.myRole != "") {
            //    if (AxeScript.Instance.myRole == "Reaper")
            //    {
            //        if (Input.GetKeyDown(KeyCode.L))
            //        {
            //            Debug.Log("Cancel Slowwwwwwwwwwwww");
            //            myPhotonView.RPC("SlowButton", RpcTarget.All);
            //            //SlowButton();
            //            //if (PhotonNetwork.IsMasterClient)
            //            //{
            //            //    photonView.RPC("SlowButton", RpcTarget.All);
            //            //}
            //        }
            //    }

            //    if (AxeScript.Instance.myRole == "Sinner")
            //    {
            //        if (Input.GetKeyDown(KeyCode.K))
            //        {
            //            Debug.Log("Cancel Stuntttttttttttttttt");
            //            myPhotonView.RPC("StuntButton", RpcTarget.All);
            //            //StuntButton();
            //            //if (PhotonNetwork.IsMasterClient)
            //            //{
            //            //    photonView.RPC("StuntButton", RpcTarget.All);
            //            //}
            //        }
            //    }
            //}   
        }

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

            //if (!this.gameObject.GetComponentsInParent<PhotonView>().IsMine)
            //{
            //	return;
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

            /// HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE


            if (other.gameObject.name == "Axe")
            //if (other.tag == "Player" && !photonView.IsMine)
            {
                Animator otherAnimator = other.gameObject.GetComponentInParent<Animator>();
                PhotonView otherPhotonView = other.gameObject.GetComponentInParent<PhotonView>();
                Debug.Log(otherAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));
                //Debug.Log("Other animator found: " + (otherAnimator != null));
                //Debug.Log("Other animator found name: " + (otherAnimator.name));
                //Debug.Log("Other animator current state: " + otherAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash);
                // Debug.Log(other.gameObject.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack"));
                if (otherAnimator != null && otherPhotonView != null)
                {
                    if (!other.gameObject.GetComponentInParent<PhotonView>().IsMine
                    && otherAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    {
                        string[] playerRoles = (string[])PhotonNetwork.CurrentRoom.CustomProperties["PlayerRoles"];
                        string otherRole = playerRoles[otherPhotonView.OwnerActorNr - 1];
                        int targetViewID = this.gameObject.GetComponentInParent<PhotonView>().ViewID;
                        Debug.Log("Target that got hit... =" + targetViewID + " Role: " + myRole);
                        Debug.Log("Hitter Role: " + otherRole);

                        if (otherRole != null)
                        {
                            //bloodEffect = GetComponent<ParticleSystem>();
                            //Sinner Get Hit Code
                            if (otherRole == "Reaper" && myRole == "Sinner")
                            {
                                //this.gameObject.GetComponentInParent<PhotonView>().RPC("TakeDamage", RpcTarget.All, 0.1f, targetViewID);
                                myPhotonView.RPC("ReaperAttack", RpcTarget.All, targetViewID);
                                AudioSource sinnerGotAxeHit = GetComponent<AudioSource>();
                                sinnerGotAxeHit.PlayOneShot(sinnerGotAxeHitEffectClip, 0.5f);
                                //sinnerGotAxeHit.volume = 0.5f;
                                //sinnerGotAxeHit.clip = sinnerGotAxeHitEffectClip;
                                //sinnerGotAxeHit.Play();

                                // Spawn particle effect on the player that got hit
                                //Instantiate(GameManagerKen.Instance.bloodEffectObject, other.transform.position, other.transform.rotation);
                                // Play a particle effect
                                //bloodEffect.Play();
                                //Instantiate(GameManagerKen.Instance.bloodEffectObject, new Vector3(other.transform.position.x,
                                //transform.position.y, other.transform.position.z), other.transform.rotation);
                                //// Play a particle effect
                                //if (bloodEffect != null)
                                //{
                                //    bloodEffect.Play();
                                //}
                                //myPhotonView.RPC("UpdateEffectDurations", RpcTarget.All, targetViewID);
                            }

                            //Reaper Get Hit Code 
                            else if (otherRole == "Sinner" && myRole == "Reaper")
                            {
                                //this.gameObject.GetComponentInParent<PhotonView>().RPC("TakeDamage", RpcTarget.All, 0.1f, targetViewID);
                                myPhotonView.RPC("SinnerAttack", RpcTarget.All, targetViewID);
                                AudioSource reaperGotAxeHit = GetComponent<AudioSource>();
                                reaperGotAxeHit.PlayOneShot(reaperGotAxeHitEffectClip, 0.5f);
                                //reaperGotAxeHit.volume = 0.5f;
                                //reaperGotAxeHit.clip = reaperGotAxeHitEffectClip;
                                //reaperGotAxeHit.Play();

                                // Spawn particle effect on the player that got hit
                                //Instantiate(GameManagerKen.Instance.bloodEffectObject, other.transform.position, other.transform.rotation);
                                // Play a particle effect
                                //bloodEffect.Play();
                                //Instantiate(GameManagerKen.Instance.bloodEffectObject, new Vector3(other.transform.position.x,
                                //transform.position.y, other.transform.position.z), other.transform.rotation);
                                //Instantiate(bloodEffectObject, new Vector3(other.transform.position.x,
                                //transform.position.y, other.transform.position.z), other.transform.rotation);
                                //// Play a particle effect
                                //if (bloodEffect != null)
                                //{
                                //    bloodEffect.Play();
                                //}
                                //myPhotonView.RPC("UpdateEffectDurations", RpcTarget.All, targetViewID);
                            }

                            //Target Soul Get Hit Code
                            else if (otherRole == "Reaper" && myRole == "TargetSoul")
                            {
                                //this.gameObject.GetComponentInParent<PhotonView>().RPC("TakeDamage", RpcTarget.All, 0.1f, targetViewID);
                                myPhotonView.RPC("SoulTaken", RpcTarget.All, 0.0f, targetViewID);
                                myPhotonView.RPC("SoulGotAttack", RpcTarget.All, targetViewID);
                                AudioSource sinnerGotAxeHit = GetComponent<AudioSource>();
                                sinnerGotAxeHit.PlayOneShot(sinnerGotAxeHitEffectClip, 0.5f);
                                PlayerAnimatorManagerKen.Instance.TargetEffect.Play();
                                //sinnerGotAxeHit.volume = 0.5f;
                                //sinnerGotAxeHit.clip = sinnerGotAxeHitEffectClip;
                                //sinnerGotAxeHit.Play();

                                // Spawn particle effect on the player that got hit
                                //Instantiate(GameManagerKen.Instance.bloodEffectObject, other.transform.position, other.transform.rotation);
                                // Play a particle effect
                                //bloodEffect.Play();
                                //Instantiate(GameManagerKen.Instance.bloodEffectObject, new Vector3(other.transform.position.x,
                                //transform.position.y, other.transform.position.z), other.transform.rotation);
                                //Instantiate(bloodEffectObject, new Vector3(other.transform.position.x,
                                //transform.position.y, other.transform.position.z), other.transform.rotation);
                                //// Play a particle effect
                                //if (bloodEffect != null)
                                //{
                                //    bloodEffect.Play();
                                //}
                                //myPhotonView.RPC("UpdateEffectDurations", RpcTarget.All, targetViewID);
                            }

                            else if (otherRole == "Sinner" && myRole == "Sinner")
                            {
                                Debug.Log("You cannot hit the same team!!!");
                            }

                            else if (otherRole == "Sinner" && myRole == "TargetSoul")
                            {
                                Debug.Log("You need to protect this soul and not hit this soul!!!");
                            }

                            else
                            {

                            }
                        }
                    }
                }
            }

            /// HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

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
        //public void BloodEffect(Collider prey)
        //{
        //    // Check if the player that got hit is not us
        //    PhotonView otherPhotonView = other.GetComponent<PhotonView>();
        //    if (otherPhotonView != null && otherPhotonView.Owner != PhotonNetwork.LocalPlayer)
        //    {
        //        // Play a particle effect at the position of the other player
        //        Instantiate(GameManagerKen.Instance.bloodEffectObject, other.transform.position, other.transform.rotation);
        //    }
        //}


        //public void RoleManagement()
        //{
        //    if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        //    {
        //        role = "Sinner";
        //        //photonView.RPC("ChangeRobotColor1", RpcTarget.All);
        //    }
        //    if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        //    {
        //        role = "Sinner";
        //        //photonView.RPC("ChangeRobotColor2", RpcTarget.All);
        //    }
        //    if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
        //    {
        //        role = "Reaper";
        //        //photonView.RPC("ChangeRobotColor3", RpcTarget.All);
        //    }
        //    if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
        //    {
        //        role = "Reaper";
        //        //photonView.RPC("ChangeRobotColor3", RpcTarget.All);
        //    }
        //}
    }
}
