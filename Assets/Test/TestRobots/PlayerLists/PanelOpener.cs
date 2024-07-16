using Ken.Test;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelOpener : MonoBehaviour
{
    public static PanelOpener Instance;
    public GameObject Panel;
    //public bool isActive;
    private bool isOpen = false;

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenPanel();
        }
    }
    public void OpenPanel()
    {

        if (!Ken.Test.GameManagerKen.gameIsPaused) { 
            if (Panel != null)
            {
                if (!isOpen)
                {
                    Panel.SetActive(true);
                    isOpen = true;
                }
                else
                {
                    Panel.SetActive(false);
                    isOpen = false;
                }
                //isActive = Panel.activeSelf;
                //Panel.SetActive(!isActive);
            }
        }
    }
}
