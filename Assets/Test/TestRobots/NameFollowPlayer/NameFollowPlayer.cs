using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameFollowPlayer : MonoBehaviour
{
    public TMP_Text nameText;
    public Transform targetTransform;
    public Camera mainCamera;
    public Vector3 offset;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = nameText.GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetTransform.position + offset);
        rectTransform.position = screenPosition;
    }
}
