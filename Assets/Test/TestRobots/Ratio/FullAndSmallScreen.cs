using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullAndSmallScreen : MonoBehaviour
{
    public static bool resolutionChanged;
    public ResolutionManager resolutionManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResolutionChangeOrNot();
        }
    }

    public void ResolutionChangeOrNot()
    {
        resolutionChanged = !resolutionChanged;
        ChangeResolution();
    }

    public void ChangeResolution()
    {
        if (resolutionChanged)
        {
            Debug.Log("Change Resolution to Minimized");
            resolutionManager.SetResolution(800, 600, false);
            Debug.Log(resolutionManager);
        }
        else
        {
            Debug.Log("Change Resolution to Fullscreen");
            resolutionManager.SetResolution(1920, 1080, true);
            Debug.Log(resolutionManager);
        }
    }
}
