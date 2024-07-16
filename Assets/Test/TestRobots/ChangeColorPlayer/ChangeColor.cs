using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChangeColor : MonoBehaviour
{
    public static ChangeColor Instance;
    public Renderer robotRenderer; // assign this variable in the Inspector by dragging the robot character onto it
    //public Color newColorRandom1;
    //public Color newColorRandom2;
    //public Color newColorRandom3;
    //public Color newColorRandom4;
    //public Color newColor;
    public Color newColor1;
    public Color newColor2;
    public Color newColor3;
    public Color newColor4;
    public PhotonView photonView;

    //public Color[] playerColors; // assign this variable in the Inspector by adding a list of colors
    //private const string COLOR_PREF_KEY = "playerColor"; // a key to use with PlayerPrefs to store the player's color

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        photonView = this.gameObject.GetComponent<PhotonView>();
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        //{
        //    photonView.RPC("ChangeRobotColor1", RpcTarget.All);
        //}
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        //{
        //    photonView.RPC("ChangeRobotColor2", RpcTarget.All);
        //}
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
        //{
        //    photonView.RPC("ChangeRobotColor3", RpcTarget.All);
        //}
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
        //{
        //    photonView.RPC("ChangeRobotColor3", RpcTarget.All);
        //}
    }
         

    void Update()
    {
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        //{
        //    photonView.RPC("ChangeRobotColor1", RpcTarget.All);
        //}
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        //{
        //    photonView.RPC("ChangeRobotColor2", RpcTarget.All);
        //}
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
        //{
        //    photonView.RPC("ChangeRobotColor3", RpcTarget.All);
        //}
        //if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
        //{
        //    photonView.RPC("ChangeRobotColor3", RpcTarget.All);
        //}
    }
    //[PunRPC]
    //public void ChangeRobotColor()
    //{
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColor;
    //}
    //[PunRPC]
    //public void ChangeRobotColor(int playerId)
    //{
    //    Random.InitState(playerId);
    //    Color newColor = new Color(Random.value, Random.value, Random.value);
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColor;
    //}

    //[PunRPC]
    //public void ChangeRobotColor(int playerId)
    //{
    //    int colorIndex = playerId % playerColors.Length;
    //    Color newColor = playerColors[colorIndex];
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColor;
    //    PlayerPrefs.SetString(COLOR_PREF_KEY + playerId, newColor.ToString()); // save the color information
    //}

    //[PunRPC]
    //public void ChangeRandomRobotColor1()
    //{
    //    Color newColorRandom1 = new Color(Random.value, Random.value, Random.value);
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColorRandom1;
    //}
    //[PunRPC]
    //public void ChangeRandomRobotColor2()
    //{
    //    Color newColorRandom2 = new Color(Random.value, Random.value, Random.value);
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColorRandom2;
    //}
    //[PunRPC]
    //public void ChangeRandomRobotColor3()
    //{
    //    Color newColorRandom3 = new Color(Random.value, Random.value, Random.value);
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColorRandom3;
    //}
    //[PunRPC]
    //public void ChangeRandomRobotColor4()
    //{
    //    Color newColorRandom4 = new Color(Random.value, Random.value, Random.value);
    //    Material robotMaterial = robotRenderer.material;
    //    robotMaterial.color = newColorRandom4;
    //}
    [PunRPC]
    public void ChangeRobotColor1()
    {
        Material robotMaterial = robotRenderer.material;
        robotMaterial.color = newColor1;
    }
    [PunRPC]
    public void ChangeRobotColor2()
    {
        Material robotMaterial = robotRenderer.material;
        robotMaterial.color = newColor2;
    }
    [PunRPC]
    public void ChangeRobotColor3()
    {
        Material robotMaterial = robotRenderer.material;
        robotMaterial.color = newColor3;
    }
    [PunRPC]
    public void ChangeRobotColor4()
    {
        Material robotMaterial = robotRenderer.material;
        robotMaterial.color = newColor4;
    }
}
