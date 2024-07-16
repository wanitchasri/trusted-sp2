using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vote : MonoBehaviour
{
    public Text vote;
    public int voteAmount;
    //public Material[] materials;
    //public Renderer rend;
    //public static bool Choose;

    //public int index = 1;

    // Start is called before the first frame update
    void Start()
    {
        voteAmount = 0;
        vote = GetComponent<Text>();
        //rend = GetComponent<Renderer>();
        //rend.enabled = true;
    }

    // Update is called once per frame
    public void Update()
    {
        vote.text = voteAmount.ToString();
        //if (Input.GetMouseButtonDown(0))
        //{
        //    index += 1;
        //    if (index == materials.Length + 1)
        //    {
        //        index = 1;
        //    }
        //    print(index);
        //    rend.sharedMaterial = materials[index - 1];
            //Choose = !Choose;
            //ChooseOrNot();
        //} 
    }
    
    public void AddVote()
    {
        voteAmount += 1;
    }
    //void ChooseOrNot()
    //{
    //    if (Choose)
    //    {
    //        voteAmount += 1;
    //    }
    //    else
    //    {
    //        voteAmount -= 1;
    //    }
    //}
}
