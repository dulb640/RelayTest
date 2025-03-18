using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnAnnouncer : NetworkBehaviour {
    void Start() {
        Debug.Log("I have started.");
    }

    void Update() {
        
    }

    public override void OnNetworkSpawn() {
        Debug.Log("I have spawned.");
    }
}
