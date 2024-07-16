using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ChatContentManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField ChatInput;
    public TextMeshProUGUI ChatContent;
    private PhotonView _photon;
    private List<string> _messages = new List<string>();
    private float _buildDelay = 0f;
    //private int _maximumMessages = 14;

    string currentState;

    void Start()
    {
        //ChatContent = GetComponent<TMP_Text>();
        _photon = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            //ChatContent.maxVisibleLines = _maximumMessages;

            //if (_messages.Count > _maximumMessages)
            //{
            //    _messages.RemoveAt(0);
            //}

            if (_buildDelay < Time.time)
            {
                BuildChatContents();
                _buildDelay = Time.time + 0.25f;
            }
        }
        else if (_messages.Count > 0)
        {
            _messages.Clear();
            ChatContent.text = "";
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"] != null)
        {
            currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
            if (currentState != "SetupGame" && currentState != "EndGame")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SubmitChat();
                }
            }
        }
    }

    [PunRPC]
    void RPC_AddNewMessage(string msg)
    {
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        if (currentState != "SetupGame" && currentState != "EndGame")
        {
            _messages.Add(msg);
        }
        else
        {
        }
        // _messages.Add(msg);
    }

    public void SendChat(string msg)
    {
        string NewMessage = PhotonNetwork.NickName + ": " + msg;
        _photon.RPC("RPC_AddNewMessage", RpcTarget.All, NewMessage);
    }

    public void SubmitChat()
    {
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        if (currentState != "SetupGame" && currentState != "EndGame")
        {
            string blankCheck = ChatInput.text;
            blankCheck = Regex.Replace(blankCheck, @"\s", "");
            if (blankCheck == "")
            {
                ChatInput.ActivateInputField();
                ChatInput.text = "";
                return;
            }

            //SendChat(ChatInput.text);
            //ChatInput.ActivateInputField();
            //ChatInput.text = "";
            //Debug.Log("send message");

            SendChat(ChatInput.text);
            ChatInput.ActivateInputField();
            ChatInput.text = "";
            Debug.Log("send message");
        }
        else
        {
        }
    }


    void BuildChatContents()
    {
        currentState = (string)PhotonNetwork.CurrentRoom.CustomProperties["GameStatus"];
        if (currentState != "SetupGame" && currentState != "EndGame")
        {
            string NewContents = "";
            foreach (string s in _messages)
            {
                NewContents += s + "\n";
            }
            ChatContent.text = NewContents;
        }
        else
        {
        }
        //string NewContents = "";
        //foreach (string s in _messages)
        //{
        //    NewContents += s + "\n";
        //}
        //ChatContent.text = NewContents;
    }

}
