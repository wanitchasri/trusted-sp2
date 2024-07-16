using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerListsManager : MonoBehaviour
{
    public static PlayerListsManager Instance;

    // public static TMP_InputField ChatInput;
    public TextMeshProUGUI ChatContent;
    // private PhotonView _photon;
    public List<string> _messages = new List<string>();
    // private float _buildDelay = 0f;
    //private int _maximumMessages = 14;

    void Start()
    {
        Instance = this;
    }

    public void showPlayerLists()
    {
        string name = PhotonNetwork.NickName;
        _messages.Add(name);
        Debug.Log(name);
        Debug.Log("Message1: " + _messages);

        string NewContents = "";
        foreach (string s in _messages)
        {
            NewContents += s + "\n";
            Debug.Log("Message2: " + _messages);
        }
        ChatContent.text = NewContents;
        Debug.Log(NewContents[0]);
    }

    //[PunRPC]
    //void RPC_AddNewMessage(string msg)
    //{
    //    _messages.Add(msg);
    //}

    //public static void SendChat(string msg)
    //{
    //    string NewMessage = PhotonNetwork.NickName;
    //    _photon.RPC("RPC_AddNewMessage", RpcTarget.All, NewMessage);
    //}

    //public static void SubmitChat()
    //{
    //    string blankCheck = ChatInput.text;
    //    blankCheck = Regex.Replace(blankCheck, @"\s", "");
    //    if (blankCheck == "")
    //    {
    //        ChatInput.ActivateInputField();
    //        ChatInput.text = "";
    //        return;
    //    }

    //    SendChat(ChatInput.text);
    //    ChatInput.ActivateInputField();
    //    ChatInput.text = "";
    //    Debug.Log("send message");
    //}

    //void BuildChatContents()
    //{
    //    string NewContents = "";
    //    foreach (string s in _messages)
    //    {
    //        NewContents += s + "\n";
    //    }
    //    ChatContent.text = NewContents;
        //string NewContents = "";
        //foreach (string s in _messages)
        //{
        //    NewContents += s + "\n";
        //}
        //ChatContent.text = NewContents;
    //}

    void Update()
    {
        //if (PhotonNetwork.InRoom)
        //{
        //    //ChatContent.maxVisibleLines = _maximumMessages;

        //    //if (_messages.Count > _maximumMessages)
        //    //{
        //    //    _messages.RemoveAt(0);
        //    //}
        //    if (_buildDelay < Time.time)
        //    {
        //        BuildChatContents();
        //        _buildDelay = Time.time + 0.25f;
        //    }
        //}
        //else if (_messages.Count > 0)
        //{
        //    _messages.Clear();
        //    ChatContent.text = "";
        //}
    }
}

