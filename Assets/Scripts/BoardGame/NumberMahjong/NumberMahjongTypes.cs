using System.Collections.Generic;

public partial class NumberMahjong {
    public enum PhaseType { Draw, Discard, Waiting, End }

    public class Card : ItemBase {
        public int number;
        public override string ToString() {
            return $"[uid:{number}, number:{number}]";
        }
    };

    public enum CardLocationEnum { Deck, Hand, Discard, Call }

    public class CardLocation {
        public CardLocationEnum Location { get; }
        public int PlayerId { get; }
        public int CardIdx { get; }

        public CardLocation(CardLocationEnum location, int playerId, int cardIdx) =>
            (Location, PlayerId, CardIdx) = (location, playerId, cardIdx);
    }

    public class ScoreTransfer {
        public int FromPlayerId { get; set; }
        public int ToPlayerId { get; set; }
        public int Score { get; set; }
    }

    public enum RoundResultType { None, Ron, Tsumo, ExhaustiveDraw }
    public class RoundResult {
        public RoundResultType Type { get; set; }
        public List<int> Winners { get; set; }
        public ScoreTransfer[] ScoreTransfers { get; set; }
    }

    public enum DeclareWinState { None, Waiting, Win }
}
