using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSkinnedMesh : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] Mesh changeSkinnedMeshRenderer;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeSkinnedMeshRenderer();
            Debug.Log("Change Skin");
        }
    }

    public void ChangeSkinnedMeshRenderer()
    {
        skinnedMeshRenderer.sharedMesh = changeSkinnedMeshRenderer;
    }
}
