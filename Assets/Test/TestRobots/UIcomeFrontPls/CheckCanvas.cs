using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCanvas : MonoBehaviour
{
    void Awake()
    {
        if (this.gameObject.GetComponent<Canvas>() == null)
        {
            this.gameObject.AddComponent<Canvas>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
