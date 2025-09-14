using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class UILobbyPlayerProfileHandler {
    VisualElement root;
    readonly VisualElement activePanel;
    readonly VisualElement inactivePanel;
    readonly Button addButton;
    readonly Button removeButton;

    Label charName;
    VisualElement charImage;
    Label playerName;

    Action onAddButtonClicked, onRemoveButtonClicked;

    static CharacterInfo[] characterInfos = null;
    static void LoadCharacterInfos() {
        characterInfos = Resources.LoadAll<CharacterInfo>("Data/Characters");
        Array.Sort(characterInfos, (a, b) => a.charId.CompareTo(b.charId));
    }

    public UILobbyPlayerProfileHandler(VisualElement root, Action onAddButtonClicked, Action onRemoveButtonClicked) {
        this.root = root;

        addButton = root.Q<Button>("AddButton");
        removeButton = root.Q<Button>("RemoveButton");

        activePanel = root.Q<VisualElement>("Active");
        inactivePanel = root.Q<VisualElement>("Inactive");

        charName = activePanel.Q<Label>("CharName");
        charImage = activePanel.Q<VisualElement>("CharImage");
        playerName = activePanel.Q<Label>("PlayerName");

        this.onAddButtonClicked = onAddButtonClicked;
        this.onRemoveButtonClicked = onRemoveButtonClicked;

        addButton.clicked += onAddButtonClicked;
        removeButton.clicked += onRemoveButtonClicked;

        AudioManager audioManager = AudioManager.Instance;
        activePanel.RegisterCallback<MouseEnterEvent>(evt => {
            audioManager.PlaySound("ui_hover");
        });
    }

    ~UILobbyPlayerProfileHandler() {
        addButton.clicked -= onAddButtonClicked;
        removeButton.clicked -= onRemoveButtonClicked;
    }

    public void SetCharaterId(int charId) {
        if (characterInfos == null) {
            LoadCharacterInfos();
        }

        charName.text = characterInfos[charId].charName.GetLocalizedString();
        charImage.style.backgroundImage = characterInfos[charId].charImage;
    }

    public void SetEditable(bool flag) {
        if (flag) {
            addButton.style.display = DisplayStyle.Flex;
            removeButton.style.display = DisplayStyle.Flex;
        }
        else {
            addButton.style.display = DisplayStyle.None;
            removeButton.style.display = DisplayStyle.None;
        }
    }

    public void SetPlayerName(string name) {
        playerName.text = name;
    }

    public void SetActive(bool active) {
        if (active) {
            activePanel.style.display = DisplayStyle.Flex;
            inactivePanel.style.display = DisplayStyle.None;
        } else {
            activePanel.style.display = DisplayStyle.None;
            inactivePanel.style.display = DisplayStyle.Flex;
        }
    }
}
