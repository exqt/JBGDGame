using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Title : MonoBehaviour {
    private UIDocument uiDoc;
    private VisualElement root;

    Button hostButton, joinButton, settingsButton, quitButton, singleButton;

    [SerializeField] GameObject uiLobby, uiLobbyList, uiSettings;

    void Start() {
        uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        singleButton = root.Q<Button>("SingleButton");
        hostButton = root.Q<Button>("HostButton");
        joinButton = root.Q<Button>("JoinButton");
        settingsButton = root.Q<Button>("SettingsButton");
        quitButton = root.Q<Button>("QuitButton");

        singleButton.clicked += OnSingleButtonClicked;
        hostButton.clicked += OnHostButtonClicked;
        joinButton.clicked += OnJoinButtonClicked;
        settingsButton.clicked += OnSettingsButtonClicked;
        quitButton.clicked += OnQuitButtonClicked;

        AudioManager audioManager = AudioManager.Instance;
        root.Query<Button>().ForEach((button) => {
            button.clicked += () => audioManager.PlaySound("ui_click");
        });

    }

    private void OnSingleButtonClicked() {
        UILobby lobby = uiLobby.GetComponent<UILobby>();
        lobby.SetSinglePlayer(true);
        lobby.SetDisplay(true);
    }

    private void OnHostButtonClicked() {
        UILobby lobby = uiLobby.GetComponent<UILobby>();
        lobby.SetSinglePlayer(false);
        lobby.SetDisplay(true);
    }

    private void OnJoinButtonClicked() {
        // Change Scene
        // UnityEngine.SceneManagement.SceneManager.LoadScene("NumberMahjong");
        UILobbyList lobbyList = uiLobbyList.GetComponent<UILobbyList>();
        lobbyList.SetDisplay(true);
    }

    private void OnSettingsButtonClicked() {
        var popup = uiSettings.GetComponent<UISettings>();
        popup.SetDisplay(true);
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
