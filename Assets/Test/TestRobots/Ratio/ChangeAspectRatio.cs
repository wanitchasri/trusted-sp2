using UnityEngine;
using UnityEngine.UI;

public class ChangeAspectRatio : MonoBehaviour
{
    public static bool ratioChanged;
    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
         mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RatioChangeOrNot();
        }
    }

    public void RatioChangeOrNot()
    {
        ratioChanged = !ratioChanged;
        ChangeRatio();
    }

    public void ChangeRatio()
    {
        if (ratioChanged)
        {
            Debug.Log("Change Ratio to New One");
            mainCamera.aspect = 16f / 10f;
            Debug.Log(mainCamera.aspect);
        }
        else
        {
            //Debug.Log("Change Ratio to Normal One");
            //mainCamera.aspect = freeaspect;
            //Debug.Log(mainCamera.aspect);
        }
    }
}
