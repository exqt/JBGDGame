using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UIElements;

public partial class UINumberMahjongRoundResult : NumberMahjongGameSubscriberBehaviour {
    public delegate void OnClickOkButtonCallback();
    public event OnClickOkButtonCallback OnClickOkButtonEvent;

    VisualElement root;
    Button okButton;
    Label title;
    PlayerComponent[] playerComponents = new PlayerComponent[4];
    VisualElement[] cardContainers = new VisualElement[4];
    VisualElement[,] arrows = new VisualElement[4,4];
    AudioSource audioSource;

    void InitializeElements() {
        root = GetComponent<UIDocument>().rootVisualElement;
        title = root.Q<Label>("Title");

        audioSource = GetComponent<AudioSource>();

        var manager = NumberMahjongManager.instance;
        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                if (i == j) continue;
                arrows[i, j] = root.Q<VisualElement>($"Arrow{i}{j}");
                Assert.IsNotNull(arrows[i, j]);
            }

            playerComponents[i] = new PlayerComponent(root.Q<VisualElement>($"Player{i}"));

            var characterInfo = manager.characterInfos[i];
            var image = characterInfo.charImage;
            playerComponents[i].SetImage(image);

            if (manager.playerTypes[i] == NumberMahjongManager.PlayerType.Human) {
                playerComponents[i].SetName("You");
            } else {
                playerComponents[i].SetName(characterInfo.charName.GetLocalizedString());
            }

            cardContainers[i] = root.Q<VisualElement>($"Player{i}Cards");

            Assert.IsNotNull(playerComponents[i]);
        }

        okButton = root.Q<Button>("OkButton");
        okButton.clicked += OnClickOkButton;
    }

    void Start() {
        InitializeElements();
        SetVisible(false);
    }

    IEnumerator UpdateResultCoroutine(
        int[] scores,
        int[] deltas,
        List<int[]> transfers,
        NumberMahjong.RoundResult result
    ) {
        int[] beforeRanks = new int[4] { game.GetRank(0), game.GetRank(1), game.GetRank(2), game.GetRank(3) };
        for (int i = 0; i < deltas.Length; i++) {
            playerComponents[i].Ranking = game.GetRank(i);
        }

        float sleep = 0.15f;
        for (int i = 0; i < deltas.Length; i++) {
            playerComponents[i].CurrentScore = scores[i];
            playerComponents[i].ScoreChange = deltas[i];
        }

        yield return new WaitForSeconds(2);

        int maxTransfer = transfers.Max(t => t[2]);
        int perTick = maxTransfer / 10;
        Debug.Log($"{maxTransfer}, {perTick}");
        int ticks = 0;

        audioSource.pitch = 1.0f;

        while (true) {
            bool done = true;

            foreach (var transfer in transfers) {
                var from = transfer[0];
                var to = transfer[1];
                var score = transfer[2];
                int d = Math.Min(score, perTick);

                scores[from] -= d;
                scores[to] += d;
                deltas[from] += d;
                deltas[to] -= d;
                transfer[2] -= d;

                playerComponents[from].CurrentScore = scores[from];
                playerComponents[to].CurrentScore = scores[to];
                playerComponents[from].ScoreChange = deltas[from];
                playerComponents[to].ScoreChange = deltas[to];
                if (transfer[2] != 0) done = false;
            }

            audioSource.Play();
            audioSource.pitch *= 1.05946309436f;

            if (done) break;
            yield return new WaitForSeconds(sleep);
            sleep *= 0.95f;
            ticks++;
        }

        Debug.Log($"{ticks} ticks done");

        game.ApplyRoundResult(result);
        int[] afterRank = new int[4] { game.GetRank(0), game.GetRank(1), game.GetRank(2), game.GetRank(3) };
        for (int i = 0; i < deltas.Length; i++) {
            playerComponents[i].Ranking = game.GetRank(i);
        }
    }

    Color ronColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);
    public void UpdateResult(NumberMahjong.RoundResult result) {
        int[] deltas = new int[4];
        int[] scores = new int[4];

        var titleBinding = title.GetBinding("text") as LocalizedString;
        titleBinding.Add("roundIndex", new IntVariable {Value = game.RoundIndex + 1});
        titleBinding.Add("roundExtensionCount", new IntVariable {Value = game.RoundExtensionCount});
        title.SetBinding("text", titleBinding);

        for (int i = 0; i < 4; i++) {
            scores[i] = game.GetScore(i);

            var cards = game.GetPlayerCards(i).ToList();
            cards.Sort((a, b) => a.number - b.number);
            var list = cardContainers[i].Query<Shadow>().ToList();

            var addedRonCard = false;
            if (result.Type == NumberMahjong.RoundResultType.Ron) {
                if (result.Winners.IndexOf(i) != -1) {
                    var lastCard = game.GetPlayerDiscards(game.Turn).Last();
                    cards.Add(lastCard);
                    addedRonCard = true;
                }
            }

            for (int j = 0; j < 6; j++) {
                var elem = list[j];
                var text = elem.Q<Label>();
                if (j < cards.Count) {
                    elem.style.display = DisplayStyle.Flex;
                    text.text = cards[j].number.ToString();
                    var content = elem.Q<VisualElement>("Content");
                    content.style.backgroundColor = addedRonCard && j == 5 ? ronColor : Color.white;
                }
                else {
                    elem.style.display = DisplayStyle.None;
                }
            };

            for (int j = 0; j < 4; j++) {
                if (i == j) continue;
                arrows[i, j].visible = false;
            }
        }

        List<int[]> scoreTransfers = new();
        foreach (var transfer in result.ScoreTransfers) {
            deltas[transfer.ToPlayerId] += transfer.Score;
            deltas[transfer.FromPlayerId] += -transfer.Score;
            arrows[transfer.FromPlayerId, transfer.ToPlayerId].visible = true;
            scoreTransfers.Add(new int[] { transfer.FromPlayerId, transfer.ToPlayerId, transfer.Score });
        }

        StartCoroutine(UpdateResultCoroutine(scores, deltas, scoreTransfers, result));
    }

    void OnClickOkButton() {
        OnClickOkButtonEvent.Invoke();
        SetVisible(false);
    }

    public void SetVisible(bool visible) {
        root.visible = visible;
        if (!visible) {
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (i == j) continue;
                    arrows[i, j].visible = false;
                }
            }
        }
    }

    void Update() {

    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand cmd) {
        if (cmd is NumberMahjong.RoundEndAction roundEndAction) {
            SetVisible(true);
            UpdateResult(roundEndAction.result);
        }
    }
}
