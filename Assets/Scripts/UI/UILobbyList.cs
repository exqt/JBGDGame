using UnityEngine;
using UnityEngine.UIElements;

public class UILobbyList : MonoBehaviour {
    VisualElement root;
    ScrollView scrollView;
    InGamePopupWindow popup;

    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] VisualTreeAsset lobbyItemAsset;

    void InitializeElements() {
        // Assert.IsNotNull(lobbyManager);

        root = GetComponent<UIDocument>().rootVisualElement;
        popup = root.Q<InGamePopupWindow>();
        // Assert.IsNotNull(popup);
        scrollView = root.Q<ScrollView>("ScrollView");

        var refreshButton = root.Q<Button>("RefreshButton");
        refreshButton.clicked += RefreshLobbyList;
    }

    void Awake() {
        InitializeElements();
        SetDisplay(false);
    }

    public void SetDisplay(bool display) {
        popup.SetDisplay(display);
    }

    async void RefreshLobbyList() {
        // var lobbies = (await lobbyManager.ListLobbiesAsync()).Results;

        // foreach (var lobby in lobbies) {
        //     Debug.Log("Lobby: " + lobby.Name + " Players: " + lobby.Players + "/" + lobby.MaxPlayers);
        // }

        scrollView.Clear();
        // foreach (var lobby in lobbies) {
        for (int i = 0; i < 10; i++) {
            var item = lobbyItemAsset.Instantiate();
            var lobbyName = item.Q<Label>("LobbyName");
            var gameName = item.Q<Label>("GameName");
            var numberOfPlayers = item.Q<Label>("NumberOfPlayers");

            lobbyName.text = "Lobby " + i;
            gameName.text = "숫자마작";
            numberOfPlayers.text = $"{UnityEngine.Random.Range(1, 5)}/4";

            item.RegisterCallback<ClickEvent>(evt => {
                int idx = i;
                OnClickLobby(idx);
            });

            scrollView.Add(item);
        }
    }

    private void OnClickLobby(int id) {
        Debug.Log("Clicked lobby: " + id);
    }
}
