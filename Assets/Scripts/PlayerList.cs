using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerList : NetworkBehaviour {

    public TMPro.TMP_Text textField;

    private NetworkVariable<List<FixedString128Bytes>> players = new NetworkVariable<List<FixedString128Bytes>>(new List<FixedString128Bytes>());
    private FixedString128Bytes bufferedValue = new FixedString128Bytes();

    // Start is called before the first frame update
    void Start() {
        Debug.Log("PlayerList.Start");
        Initialise();
    }

    // Update is called once per frame
    void Update() {
        
    }

    public override void OnNetworkSpawn() {
        Debug.Log("PlayerList.OnNetworkSpawn");
        base.OnNetworkSpawn();
        Initialise();

        if (bufferedValue.Length > 0) {
            AppendPlayerRpc(bufferedValue);
        }

    }

    private void Initialise() {
        players.OnValueChanged += updateText;
        updateText(null, players.Value);
    }

    public void AppendPlayer(FixedString128Bytes playerName) {
        if (IsSpawned) {
            Debug.Log("Appending name immediately.");
            AppendPlayerRpc(playerName);
        } else {
            Debug.Log("Postponing name appending.");
            bufferedValue = playerName;
        }
    }

    [Rpc(SendTo.Server)]
    private void AppendPlayerRpc(FixedString128Bytes playerName) {
        Debug.Log($"Appending name \"{playerName}\".");
        players.Value.Add(playerName.ToString());
        players.SetDirty(true);
        players.IsDirty();
        players.OnValueChanged(null, players.Value);
    }

    public void updateText(List<FixedString128Bytes> _, List<FixedString128Bytes> newValue) {
        Debug.Log("Updating name list text.");
        if (newValue.Count == 0) {
            textField.text = "...";
        } else {
            textField.text = string.Join("\n", newValue);
        }
    }
}
