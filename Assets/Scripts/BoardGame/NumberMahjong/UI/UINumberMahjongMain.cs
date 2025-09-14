using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class UINumberMahjongMain : NumberMahjongGameSubscriberBehaviour {
    UIDocument uiDocument;
    VisualElement root;
    VisualElement delcareWinPanel, roundInfoPanel;
    VisualElement cardCountContainer;
    ClientPlayer clientPlayer;

    Label roundLabel, turnLabel;

    void Start() {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        cardCountContainer = root.Q<VisualElement>("CardCountContainer");

        delcareWinPanel = root.Q<VisualElement>("DeclareWinPanel");
        delcareWinPanel.visible = false;

        clientPlayer = ClientPlayer.instance;

        var winButton = delcareWinPanel.Q<Button>("WinButton");
        var passButton = delcareWinPanel.Q<Button>("PassButton");
        winButton.RegisterCallback<ClickEvent>(OnClickWinButton);
        passButton.RegisterCallback<ClickEvent>(OnClickPassButton);

        roundInfoPanel = root.Q<VisualElement>("RoundInfoPanel");
        roundLabel = roundInfoPanel.Q<Label>("RoundLabel");
        turnLabel = roundInfoPanel.Q<Label>("TurnLabel");

        // initialize card count container
        var children = cardCountContainer.Children().ToList();
        for (int i = 0; i < children.Count; i++) {
            var cardIndicator = children[i];
            cardIndicator.RemoveFromClassList("CardCountColored");
            cardIndicator.RemoveFromClassList("CardDiscarded");
            cardIndicator.Q<Label>("Number").text = i.ToString();
        }
    }

    public void OnClickWinButton(ClickEvent ev) {
        delcareWinPanel.visible = false;
        NumberMahjong.DeclareWinOrSkipAction declareWinAction = new(game, clientPlayer.playerId, true);
        declareWinAction.Execute();
    }

    public void OnClickPassButton(ClickEvent ev) {
        delcareWinPanel.visible = false;
        NumberMahjong.DeclareWinOrSkipAction declareWinAction = new(game, clientPlayer.playerId, false);
        declareWinAction.Execute();
    }

    StyleColor neutralColor = new(new Color(0.95f, 0.95f, 0.95f));
    void ClearCardCounter() {
        var children = cardCountContainer.Children().ToList();
        for (int i = 0; i < children.Count; i++) {
            var cardIndicator = children[i];
            cardIndicator.style.backgroundColor = neutralColor;
            cardIndicator.RemoveFromClassList("CardCountColored");
            cardIndicator.RemoveFromClassList("CardDiscarded");
        }
    }

    void UpdateCardCounter() {
        StyleColor[] playerColors = new StyleColor[] {
            new(new Color(0.9f, 0.4f, 0.4f)),
            new(new Color(0.1f, 0.5f, 0.9f)),
            new(new Color(0.4f, 0.8f, 0.1f)),
            new(new Color(0.9f, 0.5f, 0.1f)),
        };

        var clientPlayerId = clientPlayer.playerId;
        var children = cardCountContainer.Children().ToList();

        Dictionary<int, int> discardMap = new();
        Dictionary<int, int> callMap = new();
        Dictionary<int, int> clientHandMap = new();

        for (int i = 0; i < 4; i++) {
            var cards = game.GetPlayerDiscards(i);
            for (int j = 0; j < cards.Count; j++) {
                if (j == cards.Count - 1 && clientPlayerId != i) {
                    discardMap[cards[j].number] = i+4;
                }
                else {
                    discardMap[cards[j].number] = i;
                }
            }
            var calls = game.GetPlayerCalls(i);
            for (int j = 0; j < calls.Count; j++) {
                callMap[calls[j].number] = i;
            }
        }

        var hand = game.GetPlayerHand(clientPlayerId);
        for (int i = 0; i < hand.Count; i++) {
            clientHandMap[hand[i].number] = clientPlayerId;
        }

        for (int i = 0; i < children.Count; i++) {
            var cardIndicator = children[i];
            cardIndicator.style.backgroundColor = neutralColor;

            if (callMap.TryGetValue(i, out int p0)) {
                cardIndicator.style.backgroundColor = playerColors[p0];
                cardIndicator.AddToClassList("CardCountColored");
                if (clientPlayerId != p0) {
                    cardIndicator.AddToClassList("CardDiscarded");
                }
            }
            else if (clientHandMap.TryGetValue(i, out int p1)) {
                cardIndicator.style.backgroundColor = playerColors[p1];
                cardIndicator.AddToClassList("CardCountColored");
            }
            else if (discardMap.TryGetValue(i, out int playerId)) {
                if (0 <= playerId && playerId < 4) {
                    cardIndicator.AddToClassList("CardDiscarded");
                }
                cardIndicator.style.backgroundColor = playerColors[playerId % 4];
                cardIndicator.AddToClassList("CardCountColored");
            }
            else {
                cardIndicator.RemoveFromClassList("CardDiscarded");
            }
        }
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand command) {
        roundLabel.text = $"Round: {game.RoundIndex + 1} - {game.RoundExtensionCount}";
        turnLabel.text = $"Turn {game.Turn}";

        delcareWinPanel.visible = false;

        if (command is NumberMahjong.DealCommand) {
            ClearCardCounter();
        }

        else if (game.Phase == NumberMahjong.PhaseType.Draw) {
            turnLabel.text = $"Turn {game.Turn}";
            delcareWinPanel.visible = false;
        }

        // Tsumo
        else if (command is NumberMahjong.DrawAction) {
            if (game.Turn == clientPlayer.playerId && game.IsWin(clientPlayer.playerId)) {
                var localizedTsumo = new LocalizedString("UI", "tsumo");
                delcareWinPanel.visible = true;
                delcareWinPanel.Q<Button>("WinButton").text = localizedTsumo.GetLocalizedString();
            }
        }

        // Ron
        else if (command is NumberMahjong.DiscardAction) {
            if (game.Turn != clientPlayer.playerId && game.IsWin(clientPlayer.playerId)) {
                var localizedRon = new LocalizedString("UI", "ron");
                delcareWinPanel.visible = true;
                delcareWinPanel.Q<Button>("WinButton").text = localizedRon.GetLocalizedString();
            }
        }

        UpdateCardCounter();
    }
}
