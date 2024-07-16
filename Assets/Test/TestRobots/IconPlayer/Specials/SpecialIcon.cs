using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialIcon : MonoBehaviour
{
    public static SpecialIcon Instance;

    [SerializeField]
    public GameObject[] specialsToDisplay;

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
