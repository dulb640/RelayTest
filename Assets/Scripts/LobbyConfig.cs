using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyConfig : NetworkBehaviour {

    public NetworkVariable<FixedString128Bytes> lobbyName = new NetworkVariable<FixedString128Bytes>();
    public NetworkVariable<FixedString32Bytes> joinCode = new NetworkVariable<FixedString32Bytes>();

    void Start() {
        
    }

    void Update() {
        
    }
}
