using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconForKilled : MonoBehaviour
{
    public static IconForKilled Instance;

    [SerializeField]
    public GameObject[] objectsToDisplay;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
