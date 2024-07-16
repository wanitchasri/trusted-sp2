﻿using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Ken.Asteroids
{
    public class LobbyTopPanelKen : MonoBehaviour
    {
        private readonly string connectionStatusMessage = "    Connection Status: ";

        [Header("UI References")]
        public Text ConnectionStatusText;

        #region UNITY

        public void Update()
        {
            ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
        }

        #endregion
    }
}