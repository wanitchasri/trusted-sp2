using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardSkillTest : MonoBehaviour
{
    public static WizardSkillTest Instance;

    [SerializeField]
    public GameObject[] WizardSkillTestToDisplay;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
    }
}