using UnityEngine;

public class CanvasSorter : MonoBehaviour
{
    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.sortingOrder = 2;
        if (canvas == null)
        {
            Debug.LogError("There is no Canvas component attached to the game object");
        }
    }
}
