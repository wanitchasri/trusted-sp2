using UnityEngine;

public class GroupCanvas : MonoBehaviour
{
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.sortingOrder = -1;
    }
}
