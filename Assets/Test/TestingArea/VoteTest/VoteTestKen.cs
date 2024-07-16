using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class VoteTestKen : MonoBehaviour
{
    public TMP_Text voteText;
    public int voteAmount;
    private bool isVoted;

    public GameObject clickablePlayer;
    //public UnityEvent OnClick = new UnityEvent(); //onClick Event

    private void Awake()
    {
        voteText = voteText.GetComponent<TMP_Text>();
    }

    // Use t$$anonymous$$s for initialization
    void Start()
    {
        voteAmount = 0;
        clickablePlayer = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(voteAmount);

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit Hit;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == gameObject)
            {

                switch (isVoted)
                {
                    case true:
                        RemoveVote();
                        break;
                    case false:
                        AddVote();
                        break;

                }
            }
        }

        voteText.gameObject.SetActive(isVoted);
        voteText.text = voteAmount.ToString();
    }

    public void AddVote()
    {
        Debug.Log("AddVote()");
        voteAmount += 1;
        isVoted = true;
    }

    public void RemoveVote()
    {
        Debug.Log("RemoveVote()");
        voteAmount--;
        isVoted = false;
    }

}
