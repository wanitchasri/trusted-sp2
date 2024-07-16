using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponController : MonoBehaviour
{
    public static WeaponController Instance;
    public GameObject Robot;
    public bool CanAttack = true;
    public float AttackCooldown = 1.0f;

    // Update is called once per frame
    void Update()
    {
        Instance = this;
    }

    [PunRPC]
    public void SwordAttack()
    {
        CanAttack = false;
        Animator anim = Robot.GetComponent<Animator>();
        anim.SetTrigger("Attack");
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }
}
