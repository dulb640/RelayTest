using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RelayTest : MonoBehaviour {

    public GameObject mainMenu;

    public GameObject hostMenu;
    public TMPro.TMP_InputField lobbyNameInput;
    public TMPro.TMP_InputField hostPlayerNameInput;
    public Button hostButton;

    public GameObject joinMenu;
    public TMPro.TMP_InputField joinPlayerNameInput;
    public TMPro.TMP_InputField joinCodeInput;
    public Button joinButton;
    
    public HostConfig hostConfigPrefab;
    public ClientConfig clientConfigPrefab;

    void Start() {
        
    }

    void Update() {
        
    }

    public void ShowHostMenu() {
        mainMenu.SetActive(false);
        hostMenu.SetActive(true);
        joinMenu.SetActive(false);
    }

    public void ShowJoinMenu() {
        mainMenu.SetActive(false);
        hostMenu.SetActive(false);
        joinMenu.SetActive(true);
    }

    public async void Host() {
        Debug.Log("RelayTest.Host");
        hostButton.interactable = false;
        var joinCode = await StartHostWithRelay();

        var hostConfig = Instantiate(hostConfigPrefab).GetComponent<HostConfig>();
        hostConfig.lobbyName = lobbyNameInput.text;
        hostConfig.joinCode = joinCode;
        hostConfig.gameObject.tag = "HostConfig";
        DontDestroyOnLoad(hostConfig);

        var clientConfig = Instantiate(clientConfigPrefab).GetComponent<ClientConfig>();
        clientConfig.playerName = hostPlayerNameInput.text;
        clientConfig.gameObject.tag = "ClientConfig";
        DontDestroyOnLoad(clientConfig);

        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    public async void Join() {
        Debug.Log("RelayTest.Join");
        joinButton.interactable = false;
        var result = await StartClientWithRelay(joinCodeInput.text);
        if (result) {
            var clientConfig = Instantiate(clientConfigPrefab).GetComponent<ClientConfig>();
            clientConfig.playerName = joinPlayerNameInput.text;
            clientConfig.gameObject.tag = "ClientConfig";
            DontDestroyOnLoad(clientConfig);
        }
    }

    /// <summary>
    /// Starts a game host with a relay allocation: it initializes the Unity services, signs in anonymously and starts the host with a new relay allocation.
    /// </summary>
    /// <param name="maxConnections">Maximum number of connections to the created relay.</param>
    /// <returns>The join code that a client can use.</returns>
    /// <exception cref="ServicesInitializationException"> Exception when there's an error during services initialization </exception>
    /// <exception cref="UnityProjectNotLinkedException"> Exception when the project is not linked to a cloud project id </exception>
    /// <exception cref="CircularDependencyException"> Exception when two registered <see cref="IInitializablePackage"/> depend on the other </exception>
    /// <exception cref="AuthenticationException"> The task fails with the exception when the task cannot complete successfully due to Authentication specific errors. </exception>
    /// <exception cref="RequestFailedException"> See <see cref="IAuthenticationService.SignInAnonymouslyAsync"/></exception>
    /// <exception cref="ArgumentException">Thrown when the maxConnections argument fails validation in Relay Service SDK.</exception>
    /// <exception cref="RelayServiceException">Thrown when the request successfully reach the Relay Allocation service but results in an error.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the UnityTransport component cannot be found.</exception>
    public async Task<string> StartHostWithRelay(int maxConnections=5) {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log($"Hosting with join code = {joinCode}");
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    /// <summary>
    /// Joins a game with relay: it will initialize the Unity services, sign in anonymously, join the relay with the given join code and start the client.
    /// </summary>
    /// <param name="joinCode">The join code of the allocation</param>
    /// <returns>True if starting the client was successful</returns>
    /// <exception cref="ServicesInitializationException"> Exception when there's an error during services initialization </exception>
    /// <exception cref="UnityProjectNotLinkedException"> Exception when the project is not linked to a cloud project id </exception>
    /// <exception cref="CircularDependencyException"> Exception when two registered <see cref="IInitializablePackage"/> depend on the other </exception>
    /// <exception cref="AuthenticationException"> The task fails with the exception when the task cannot complete successfully due to Authentication specific errors. </exception>
    /// <exception cref="RequestFailedException">Thrown when the request does not reach the Relay Allocation service.</exception>
    /// <exception cref="ArgumentException">Thrown if the joinCode has the wrong format.</exception>
    /// <exception cref="RelayServiceException">Thrown when the request successfully reach the Relay Allocation service but results in an error.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the UnityTransport component cannot be found.</exception>
    public async Task<bool> StartClientWithRelay(string joinCode) {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
