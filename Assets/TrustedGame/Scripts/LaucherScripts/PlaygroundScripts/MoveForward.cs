using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public static int moveSpeed = 1;
    public Vector3 userDirection = Vector3.forward;

    public Transform stopper;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.z <= stopper.transform.position.z)
        {
            transform.Translate(userDirection * moveSpeed * Time.deltaTime);
        }
    }
}
