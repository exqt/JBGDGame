using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour {
    // public static LobbyManager instance;

    // string playerName = "Player";

    // void Awake() {
    //     if (instance == null) instance = this;
    //     else Destroy(gameObject);
    // }

    // async void Start() {
    //     return;

    //     await UnityServices.InitializeAsync();
    //     playerName += Random.Range(0, 1000);

    //     var authService = AuthenticationService.Instance;
    //     authService.SignedIn += () => {
    //         Debug.Log("Signed in" + authService.PlayerId);
    //     };

    //     await authService.SignInAnonymouslyAsync();
    // }

    // private async void CreateLobby() {
    //     string lobbyName = "My Lobby";
    //     int gameId = 0;
    //     int maxPlayers = 4;
    //     var lobbyService = LobbyService.Instance;

    //     CreateLobbyOptions options = new() {
    //         IsPrivate = false,
    //         Player = GetPlayer(),
    //         Data = new Dictionary<string, DataObject> {
    //             {
    //                 "GameMode",
    //                 new DataObject(DataObject.VisibilityOptions.Public, gameId.ToString())
    //             }
    //         }
    //     };

    //     try {
    //         hostLobby = await lobbyService.CreateLobbyAsync(lobbyName, maxPlayers, options);
    //     } catch (LobbyServiceException e) {
    //         Debug.LogError("Failed to create lobby: " + e.Message);
    //     }
    // }

    // public Task<QueryResponse> ListLobbiesAsync()
    // {
    //     QueryLobbiesOptions query = new()
    //     {
    //         Count = 10,
    //         Order = new List<QueryOrder> {
    //             new(true, QueryOrder.FieldOptions.Created)
    //         }
    //     };

    //     return Lobbies.Instance.QueryLobbiesAsync(query);
    // }

    // async void JoinLobby(string lobbyId) {
    //     var lobbies = Lobbies.Instance;
    //     string playerName = "Player" + Random.Range(0, 1000);

    //     JoinLobbyByIdOptions options = new() {
    //         Player = GetPlayer()
    //     };
    //     try {
    //         Lobby lobby = await lobbies.JoinLobbyByIdAsync(lobbyId, options);
    //     } catch (LobbyServiceException e) {
    //         Debug.LogError("Failed to join lobby: " + e.Message);
    //     }
    // }

    // async void UpdateLobbyGameMode(int gameId) {
    //     var lobbies = Lobbies.Instance;

    //     try {
    //         var data = new Dictionary<string, DataObject> {{
    //             "GameMode",
    //             new DataObject(DataObject.VisibilityOptions.Public, gameId.ToString())
    //         }};
    //         hostLobby = await lobbies.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
    //             Data = data
    //         });
    //     } catch (LobbyServiceException e) {
    //         Debug.LogError("Failed to update lobby: " + e.Message);
    //     }
    // }

    // Player GetPlayer() {
    //     return new Player {
    //         Data = new Dictionary<string, PlayerDataObject> {
    //             { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
    //         }
    //     };
    // }

    // readonly float HEARTBEAT_TIMER_MAX = 15;
    // float heartbeatTimer = 0;
    // async void HandleLobbyHeartbeat() {
    //     if (hostLobby == null) return;

    //     heartbeatTimer -= Time.deltaTime;
    //     if (heartbeatTimer < 0) {
    //         heartbeatTimer = HEARTBEAT_TIMER_MAX;
    //         var lobbyService = LobbyService.Instance;
    //         Debug.Log("Sending heartbeat ping");
    //         await lobbyService.SendHeartbeatPingAsync(hostLobby.Id);
    //     }
    // }

    // void Update() {
    //     HandleLobbyHeartbeat();
    // }
}
