using System.Collections.Generic;
using UnityEngine;

public partial class NumberMahjong {
#region Turn Actions
    public class DrawAction : TurnAction {
        public int from;
        public Card card;

        public DrawAction(NumberMahjong game, int player, int from) {
            this.game = game;
            this.playerId = player;
            this.from = from;
        }

        public override bool Validate() {
            if (game.Phase != PhaseType.Draw || game.Turn != playerId) {
                return false;
            }
            return true;
        }

        protected override void OnExecute() {
            card = game.Draw(from, playerId);
            Debug.Log($"DrawAction: {card} p:{playerId} f:{from} {card != null}");
        }
    }

    public class DiscardAction : TurnAction {
        public Card card;

        public DiscardAction(NumberMahjong game, int playerId, Card card) {
            this.game = game;
            this.playerId = playerId;
            this.card = card;
        }

        public override bool Validate() {
            if (game.Phase != PhaseType.Discard || game.Turn != playerId) {
                return false;
            }
            if (game.playerHands[game.Turn].FindIndex((Card c) => c.uid == card.uid) == -1) {
                return false;
            }
            return true;
        }

        protected override void OnExecute() {
            game.Discard(card, playerId);
            Debug.Log($"DiscardAction: {card} p:{playerId}");
        }
    }

    public class DeclareWinOrSkipAction : TurnAction {
        public bool win;
        public DeclareWinOrSkipAction(NumberMahjong game, int playerId, bool win) {
            this.game = game;
            this.playerId = playerId;
            this.win = win;
        }

        public override bool Validate() {
            if (game.Phase == PhaseType.Draw || game.Phase == PhaseType.End) return false;

            // Tsumo
            if (game.Phase == PhaseType.Discard && playerId == game.Turn) {
                if (!win) return true; // cancel
                if (game.IsWin(playerId)) return true;
                return false;
            }

            // Ron
            if (game.declareWinState[playerId] != DeclareWinState.Waiting) return false;

            return true;
        }

        protected override void OnExecute() {
            game.DeclareOrSkipWin(playerId, win);
            Debug.Log($"DeclareWinAction: {playerId} {win}");
        }
    }
#endregion

#region Round Command

    public class RoundEndAction : BoardGameCommand {
        public RoundResult result;
        public RoundEndAction(NumberMahjong game, RoundResult result) {
            this.game = game;
            this.result = result;
        }

        protected override void OnExecute() {
        }
    }

    public class DealCommand : BoardGameCommand {
        public List<IReadOnlyList<Card>> playerHands;
        public DealCommand(NumberMahjong game) {
            this.game = game;
        }

        protected override void OnExecute() {
            playerHands = game.Deal();
        }
    }

#endregion
}
