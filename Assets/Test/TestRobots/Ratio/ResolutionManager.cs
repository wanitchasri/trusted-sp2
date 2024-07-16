using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    public void SetResolution(int width, int height, bool fullScreen)
    {
        Screen.SetResolution(width, height, fullScreen);
    }
}
