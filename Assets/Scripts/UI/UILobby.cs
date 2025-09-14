using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UILobby : MonoBehaviour {
    public UIGuide uiGuide;

    VisualElement root;

    VisualElement gameProfile;
    DropdownField gameList;
    VisualElement gamePreviewImage;
    Label gameName, gameDescription;

    VisualElement playerProfileContainer;
    VisualElement[] playerProfiles;

    VisualElement footer;
    Button startButton, leaveButton;

    readonly UILobbyPlayerProfileHandler[] playerProfileHandlers = new UILobbyPlayerProfileHandler[4];

    GameInfo[] gameInfos;
    int gameId = 0;
    bool isSinglePlayer = false;

    void InitializeElements() {
        root = GetComponent<UIDocument>().rootVisualElement;

        gameProfile = root.Q<VisualElement>("GameProfile");
        gameList = gameProfile.Q<DropdownField>("GameList");
        gamePreviewImage = gameProfile.Q<VisualElement>("GamePreviewImage");
        gameName = gameProfile.Q<Label>("GameName");
        gameDescription = gameProfile.Q<Label>("GameDescription");

        var guideButton = gameProfile.Q<Button>("GuideButton");
        guideButton.clicked += OnClickGuide;

        var readCartoonButton = gameProfile.Q<Button>("ReadCartoonButton");
        readCartoonButton.clicked += OnReadCartoon;

        // players
        playerProfileContainer = root.Q<VisualElement>("PlayerProfileContainer");
        playerProfiles = new VisualElement[4];
        var playerId = 0;
        foreach (var profile in playerProfileContainer.Children()) {
            playerProfiles[playerId++] = profile;
        }

        for (int i = 0; i < 4; i++) {
            int index = i;
            playerProfileHandlers[i] = new(
                playerProfiles[i],
                () => OnAddPlayer(index),
                () => OnRemovePlayer(index)
            );
        }

        gameList.RegisterValueChangedCallback(evt => {
            OnGameSelected(gameList.index);
        });

        // footer
        footer = root.Q<VisualElement>("Footer");
        startButton = footer.Q<Button>("StartButton");
        leaveButton = footer.Q<Button>("LeaveButton");
        startButton.clicked += OnGameStart;
        leaveButton.clicked += OnLeave;
    }

    void Start() {
        InitializeElements();

        gameInfos = Resources.LoadAll<GameInfo>("Data/GameInfo");
        gameList.choices = new List<string>();
        foreach (var gameInfo in gameInfos) {
            gameList.choices.Add(gameInfo.gameName.GetLocalizedString());
        }

        // sound
        AudioManager audioManager = AudioManager.Instance;
        root.Query<Button>().ForEach((button) => {
            button.clicked += () => audioManager.PlaySound("ui_click");
        });

        // Detect Localization locale change
        LocalizationSettings.SelectedLocaleChanged += (locale) => {
            gameList.choices = new List<string>();
            foreach (var gameInfo in gameInfos) {
                gameList.choices.Add(gameInfo.gameName.GetLocalizedString());
            }
        };

        OnGameSelected(0);
        SetDisplay(false);
    }

    public void SetDisplay(bool display) {
        root.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetSinglePlayer(bool single) {
        isSinglePlayer = single;
        if (single) {
            for (int i = 0; i < 4; i++) {
                if (i == 0) playerProfileHandlers[i].SetPlayerName("Player");
                else playerProfileHandlers[i].SetPlayerName("AI " + i);
                playerProfileHandlers[i].SetCharaterId(i);
                playerProfileHandlers[i].SetEditable(false);
                playerProfileHandlers[i].SetActive(true);
            }
        }
        else {
            for (int i = 0; i < 4; i++) {
                playerProfileHandlers[i].SetPlayerName("Player " + i);
                playerProfileHandlers[i].SetEditable(true);
                playerProfileHandlers[i].SetActive(false);
            }
        }
    }

    private void OnAddPlayer(int i) {
        Debug.Log("Add player");
        playerProfileHandlers[i].SetActive(true);
        playerProfileHandlers[i].SetCharaterId(i);
    }

    private void OnRemovePlayer(int i) {
        Debug.Log("Remove player");
        if (isSinglePlayer) return;
        playerProfileHandlers[i].SetActive(false);
    }

    private void OnGameStart() {
        Debug.Log("Game Start");
        SetDisplay(false);
        SceneLoadManager.Instance.GoToScene("NumberMahjong");
    }

    private void OnLeave() {
        Debug.Log("Leave");
        SetDisplay(false);
    }

    private void OnGameSelected(int index) {
        Debug.Log("Game selected: " + index);
        if (!(0 <= index && index < gameInfos.Length)) return;
        gameId = index;
        gameList.index = index;

        GameInfo gameInfo = gameInfos[index];
        gameName.text = gameInfo.gameName.GetLocalizedString();
        gameDescription.text = gameInfo.gameDescription.GetLocalizedString();
        gamePreviewImage.style.backgroundImage = gameInfo.gamePreviewImage;
    }

    private void OnClickGuide() {
        Debug.Log("Click guide");
        uiGuide.SetGameId(gameId);
        uiGuide.SetDisplay(true);
    }

    private void OnReadCartoon() {
        GameInfo gameInfo = gameInfos[gameList.index];
        Debug.Log("Read cartoon: " + gameInfo.cartoonUrl);
        Application.OpenURL(gameInfo.cartoonUrl);
    }

    void Update() {

    }
}
