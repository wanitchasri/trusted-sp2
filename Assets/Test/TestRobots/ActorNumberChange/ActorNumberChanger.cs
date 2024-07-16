using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ActorNumberChanger : MonoBehaviourPunCallbacks
{
    public static ActorNumberChanger Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeActorNumber(Player player, int newActorNumber)
    {
        if (player.ActorNumber == newActorNumber)
        {
            // The player already has this actor number, nothing to do.
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            // Only the master client can change actor numbers.
            Debug.LogWarning("Only the master client can change actor numbers.");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < newActorNumber)
        {
            // There are not enough players in the room to assign the new actor number.
            Debug.LogWarning("Not enough players in the room to assign actor number " + newActorNumber + ".");
            return;
        }

        // Get the current actor number for the player
        int currentActorNumber = (int)player.CustomProperties["ActorNumber"];

        // Update the actor number for the player
        ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable();
        customProps.Add("ActorNumber", newActorNumber);
        player.SetCustomProperties(customProps);

        // Update the actor numbers for the other players in the room
        foreach (Player otherPlayer in PhotonNetwork.PlayerList)
        {
            if (otherPlayer != player && (int)otherPlayer.CustomProperties["ActorNumber"] >= newActorNumber)
            {
                customProps = new ExitGames.Client.Photon.Hashtable();
                customProps.Add("ActorNumber", otherPlayer.ActorNumber + 1);
                otherPlayer.SetCustomProperties(customProps);
            }
        }

        // Update the local player's actor number if needed
        if (PhotonNetwork.LocalPlayer == player)
        {
            ExitGames.Client.Photon.Hashtable localProps = new ExitGames.Client.Photon.Hashtable();
            localProps.Add("ActorNumber", newActorNumber);
            PhotonNetwork.LocalPlayer.SetCustomProperties(localProps);
        }

        Debug.Log("Actor number changed from " + currentActorNumber + " to " + newActorNumber + " for player " + player.NickName + ".");
    }
    public void DecreasingActorNumber()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            Debug.Log("Cannot decrease actor number for player 1");
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
        {
            // Get the current player
            Player player = PhotonNetwork.LocalPlayer;
            // Declare and initialize the newActorNumber variable
            int newActorNumber = 1;
            // Call the ChangeActorNumber method with the player and newActorNumber parameters
            ChangeActorNumber(player, newActorNumber);
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 3)
        {
            // Get the current player
            Player player = PhotonNetwork.LocalPlayer;
            // Declare and initialize the newActorNumber variable
            int newActorNumber = 2;
            // Call the ChangeActorNumber method with the player and newActorNumber parameters
            ChangeActorNumber(player, newActorNumber);
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 4)
        {
            // Get the current player
            Player player = PhotonNetwork.LocalPlayer;
            // Declare and initialize the newActorNumber variable
            int newActorNumber = 3;
            // Call the ChangeActorNumber method with the player and newActorNumber parameters
            ChangeActorNumber(player, newActorNumber);
        }
        else if (PhotonNetwork.LocalPlayer.ActorNumber == 5)
        {
            // Get the current player
            Player player = PhotonNetwork.LocalPlayer;
            // Declare and initialize the newActorNumber variable
            int newActorNumber = 4;
            // Call the ChangeActorNumber method with the player and newActorNumber parameters
            ChangeActorNumber(player, newActorNumber);
        }
        else
        {
            Debug.Log("Cannot decrease actor number for player with actor number " + PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
}