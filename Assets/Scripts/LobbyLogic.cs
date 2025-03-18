using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyLogic : NetworkBehaviour {

    public TMPro.TMP_Text lobbyName;
    public TMPro.TMP_Text joinCode;
    public LobbyConfig lobbyConfig;
    public PlayerList playerList;
    public bool configFound = false;

    void Start() {
        TryToApplyLobbyConfig();
    }

    public override void OnNetworkSpawn() {
        Debug.Log("LobbyLogic.OnNetworkSpawn.");

        if (IsHost) {
            var hostConfigObject = GameObject.FindWithTag("HostConfig");
            if (hostConfigObject == null) {
                Debug.Log("Host missing host config.");
            } else {
                var hostConfig = hostConfigObject.GetComponent<HostConfig>();
                lobbyConfig.lobbyName.Value = hostConfig.lobbyName.ToString();
                lobbyConfig.joinCode.Value = hostConfig.joinCode.ToString();
                Debug.Log("Host has set lobby config.");
            }
        }

        TryToApplyLobbyConfig();


        if (IsClient) {
            var clientConfigObject = GameObject.FindWithTag("ClientConfig");
            if (clientConfigObject == null) {
                Debug.Log("Client missing host config.");
            } else {
                var clientConfig = clientConfigObject.GetComponent<ClientConfig>();
                playerList.AppendPlayer(clientConfig.playerName.ToString());
                Debug.Log("Client has added their name to the list.");
            }
        }
    }

    /// <summary>
    /// TryToApplyLobbyConfig applies the lobby config to the relevant ui
    /// components. It will be called twice. Start and OnNetworkSpawn can be
    /// called in either order. 
    /// </summary> 
    void TryToApplyLobbyConfig() {
        if (lobbyConfig.lobbyName.Value.ToString() != "") {
            lobbyName.text = lobbyConfig.lobbyName.Value.ToString();
        }
        if (lobbyConfig.joinCode.Value.ToString() != "") {
            joinCode.text = lobbyConfig.joinCode.Value.ToString();
        }
    }

    void Update() {
    }
}
