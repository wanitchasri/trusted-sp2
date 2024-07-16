using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class TargetRevealer : MonoBehaviourPunCallbacks
{
    public static TargetRevealer Instance;

    [Header("Spawners")]
    public Transform TargetSoulSpawner;
    public Transform[] ReaperSpawners;
    public Transform[] SinnerSpawners;
    public Transform MySpawner;
    public GameObject[] HumanMeshes;

    [Header("Text")]
    public TMP_Text RoleText;
    public TMP_Text NameText;

    [Header("Objects")]
    public Transform CameraStartPos;
    public Transform Mirror;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        RoleText = RoleText.GetComponent<TMP_Text>();
        NameText = NameText.GetComponent<TMP_Text>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
