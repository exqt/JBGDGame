using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NumberMahjongAI : NumberMahjongGameSubscriberBehaviour {
    int playerId = -1;
    CharacterHandControl handControl;

    void Start() {
        playerId = GetComponent<BoardPlayer>().playerId;
        handControl = GetComponent<CharacterHandControl>();
    }

    public override void OnStateChange(GameBase<NumberMahjong>.BoardGameCommand command) {
        if (game.Phase == NumberMahjong.PhaseType.End) return;
        if (game.Phase == NumberMahjong.PhaseType.Waiting &&
            game.GetDeclareWinState(playerId) == NumberMahjong.DeclareWinState.Waiting
        ) {
            StartCoroutine(CheckWinCoroutine());
            return;
        }

        if (game.Turn != playerId) return;

        if (game.Phase == NumberMahjong.PhaseType.Draw) {
            StartCoroutine(OnDrawAction());
        } else if (game.Phase == NumberMahjong.PhaseType.Discard) {
            StartCoroutine(OnDiscardAction());
        }
    }

    IEnumerator CheckWinCoroutine() {
        var declareWinAction = new NumberMahjong.DeclareWinOrSkipAction(game, playerId, true);
        yield return new WaitForSeconds(1);
        declareWinAction.Execute();
        yield break;
    }

    IEnumerator OnDrawAction() {
        var drawAction = GetDrawAction();
        if (!drawAction.Validate()) {
            Debug.LogError($"AI Action Failed {playerId}");
            yield break;
        }

        yield return new WaitForSeconds(1.0f);

        // draw from deck
        if (drawAction.from == -1) {
            if (handControl) {
                yield return handControl.AnimateDrawDeck(() => {
                    drawAction.Execute();
                    return drawAction.card;
                });
            }
            else {
                drawAction.Execute();
                yield return new WaitForSeconds(1.0f);
            }
        // draw from other player
        } else {
            var manager = NumberMahjongManager.instance;
            var discardArea = manager.playerDiscardAreas[drawAction.from];
            var callArea = manager.playerCallAreas[playerId];
            var discardCard = discardArea.GetLatestDiscardCard();

            if (handControl) {
                yield return handControl.AnimateCall(
                discardCard.transform.position,
                callArea.GetNextCallPosition(),
                () => {
                    drawAction.Execute(false);
                    discardArea.RemoveCardFromDiscardArea(drawAction.card);
                    return drawAction.card;
                }, () => {
                    callArea.AddCardToCallArea(drawAction.card);
                });
            }
            else {
                drawAction.Execute(false);
                discardArea.RemoveCardFromDiscardArea(drawAction.card);
                callArea.AddCardToCallArea(drawAction.card);
                yield return new WaitForSeconds(1.0f);
            }
            drawAction.SendCommand();
        }
    }

    IEnumerator OnDiscardAction() {
        if (game.IsWin(playerId)) {
            var declareWinAction = new NumberMahjong.DeclareWinOrSkipAction(game, playerId, true);
            declareWinAction.Execute();
            yield break;
        }

        var discardAction = GetDiscardAction();
        if (!discardAction.Validate()) {
            Debug.LogError($"AI Action Failed {playerId}");
            yield break;
        }
        yield return new WaitForSeconds(1.0f);

        var manager = NumberMahjongManager.instance;
        var discardArea = manager.playerDiscardAreas[playerId];
        var pos = discardArea.GetNextDiscardPosition();
        if (handControl) {
            yield return handControl.AnimateDiscard(
                pos, discardAction.card,
                () => {
                    discardArea.AddCardToDiscardArea(discardAction.card);
                    discardAction.Execute();
                }
            );
        }
        else {
            discardArea.AddCardToDiscardArea(discardAction.card);
            discardAction.Execute();
        }

        var shanten = GetShanten();

        Debug.Log($"{playerId} AI Turn End {shanten.shanten} {shanten.a0} {shanten.d}");
    }

#region Implemention
    public NumberMahjong.DrawAction GetDrawAction() {
        var hands = game.GetPlayerHand(playerId);
        var calls = game.GetPlayerCalls(playerId);
        var cards = hands.Concat(calls).ToList();
        var discards = game.GetPlayerDiscards(playerId);
        var playerSideCards = cards.Concat(discards).ToList();

        // Assert.AreEqual(cards.Count, 5);

        var availables = GetAvailables();
        var possibleShantens = new List<Tuple<int, ShantenInfo>>();

        for (int pid = 0; pid < 4; pid++) {
            if (pid == playerId) continue;
            var othersDiscards = game.GetPlayerDiscards(pid);
            if (othersDiscards.Count == 0) continue;
            var pick = othersDiscards[^1];
            var newShanten = GetShanten(additional: pick.number);
            possibleShantens.Add(new Tuple<int, ShantenInfo>(pid, newShanten));
        }

        if (possibleShantens.Count == 0) {
            NumberMahjong.DrawAction action = new(game, playerId, -1);
            return action;
        }

        possibleShantens.Sort((a, b) => a.Item2.shanten - b.Item2.shanten);

        var bestShantenNumber = possibleShantens[0].Item2.shanten;
        possibleShantens = possibleShantens.Where((x) => x.Item2.shanten == bestShantenNumber).ToList();

        var idx = RandomManager.Instance.Next(0, possibleShantens.Count);
        var callShantenInfo = possibleShantens[idx];

        float expectedDrawShanten = 0;
        foreach (int i in availables) {
            if (playerSideCards.Exists((x) => x.number == i)) continue;
            var newShanten = GetShanten(additional: i);
            expectedDrawShanten += newShanten.shanten;
        }
        expectedDrawShanten /= availables.Count;

        if (expectedDrawShanten <= callShantenInfo.Item2.shanten || callShantenInfo.Item2.shanten == 3) {
            NumberMahjong.DrawAction action = new(game, playerId, -1);
            return action;
        }
        else {
            NumberMahjong.DrawAction action = new(game, playerId, callShantenInfo.Item1);
            return action;
        }
    }

    public NumberMahjong.DiscardAction GetDiscardAction() {
        var hands = game.GetPlayerHand(playerId);
        var calls = game.GetPlayerCalls(playerId);
        var cards = hands.Concat(calls).ToList();

        // Assert.AreEqual(cards.Count, 6);

        var availables = GetAvailables();
        var possibleShantens = new List<Tuple<int, ShantenInfo>>();

        foreach (var card in hands) {
            var newShanten = GetShanten(removal: card.number);
            possibleShantens.Add(new Tuple<int, ShantenInfo>(card.number, newShanten));
        }

        possibleShantens.Sort((a, b) => a.Item2.shanten - b.Item2.shanten);

        var bestShantenNumber = possibleShantens[0].Item2.shanten;
        possibleShantens = possibleShantens.Where((x) => x.Item2.shanten == bestShantenNumber).ToList();

        var idx = RandomManager.Instance.Next(0, possibleShantens.Count);
        var discardCardNumber = possibleShantens[idx];
        var discardCard = cards.Find((x) => x.number == discardCardNumber.Item1);

        NumberMahjong.DiscardAction action = new(game, playerId, discardCard);
        return action;
    }
#endregion

    private List<NumberMahjong.Card> GetPickableDiscards() {
        List<NumberMahjong.Card> ret = new();
        for (int i = 0; i < 4; i++) {
            if (i == playerId) continue;
            var discards = game.GetPlayerDiscards(i);
            if (discards.Count > 0) ret.Add(discards[^1]);
        }
        return ret;
    }

    public List<int> GetAvailables() {
        bool[] notAvailables = new bool[50];
        List<int> availables = new();

        for (int i = 0; i < 4; i++) {
            if (i == playerId) continue;

            var discards = game.GetPlayerDiscards(i);
            for (int j = 0; j < discards.Count - 1; j++) {
                notAvailables[discards[j].number] = true;
            }

            var calls = game.GetPlayerCalls(i);
            foreach (var call in calls) {
                notAvailables[call.number] = true;
            }
        }

        for (int i = 0; i < 50; i++) {
            if (!notAvailables[i]) availables.Add(i);
        }

        return availables;
    }

#region Utility
    public struct ShantenInfo {
        public int shanten;
        public int a0;
        public int d;

        public ShantenInfo(int shanten, int a0, int d) {
            this.shanten = shanten;
            this.a0 = a0;
            this.d = d;
        }
    }

    public ShantenInfo GetShanten(int additional = -1, int removal = -1) {
        var ownings = game.GetPlayerCards(playerId).Select((x) => x.number).ToList();
        var availables = GetAvailables();

        byte[] map = new byte[50];

        foreach (var x in availables) map[x] = 2;
        foreach (var x in ownings) map[x] = 1;
        if (additional != -1) map[additional] = 1;
        if (removal != -1) map[removal] = 0;

        int bestShanten = 100;
        int bestA0 = -1;
        int bestD = -1;

        Func<int, int, int> gcd = (a, b) => {
            while (b != 0) { int t = b; b = a % b; a = t; }
            return a;
        };

        for (int a0 = 0; a0 < 50; a0++) {
            // the maximun difference is 12 (0, 12, 24, 36, 48)
            for (int d = 1; d <= 12; d++) {
                // except hard to recognize patterns
                // ex) 9, 17, 25, 33, 41 (a0 = 9, d = 8)
                if (d >= 7 && gcd(a0 % d, d) == 1 ) continue;

                if (map[a0] == 0) continue;
                if (a0 + d*4 >= 50) break;
                int a1 = a0 + d;
                if (map[a1] == 0) continue;
                int a2 = a1 + d;
                if (map[a2] == 0) continue;
                int a3 = a2 + d;
                if (map[a3] == 0) continue;
                int a4 = a3 + d;
                if (map[a4] == 0) continue;

                int shanten = 5;
                if (map[a0] == 1) shanten--;
                if (map[a1] == 1) shanten--;
                if (map[a2] == 1) shanten--;
                if (map[a3] == 1) shanten--;
                if (map[a4] == 1) shanten--;

                if (shanten < bestShanten) {
                    bestShanten = shanten;
                    bestA0 = a0;
                    bestD = d;
                }
            }
        }

        return new ShantenInfo(bestShanten, bestA0, bestD);
    }
    #endregion
}
