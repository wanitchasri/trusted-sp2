using UnityEngine;
using Photon.Pun;

public class DontDestroy : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

    }
}
