using UnityEngine;

public class RotateWithMouse : MonoBehaviour
{

    public float Speed = 5;

    void Update()
    {
        if (Input.GetMouseButton(2))
        {
            Debug.Log("RotateWithMouse");
            transform.eulerAngles += Speed * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);

            //transform.Rotate(transform.up ,-Input.GetAxis("Mouse X") * Speed  ); //1
        }
    }
}
