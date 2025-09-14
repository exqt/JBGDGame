using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class NumberMahjong : GameBase<NumberMahjong> {
#region Constants
    public const int INITIAL_SCORE = 250;
    public const int N_CARDS = 50;
    public const int N_PLAYERS = 4;
    public const int HAND_SIZE = 5;
    public const int WIN_SCORE = 60;
    public const int DEALER_BONUS_SCORE = 30;
#endregion

    List<Card> deck = new();
    List<Card>[] playerHands = new List<Card>[4];
    List<Card>[] playerDiscards = new List<Card>[4];
    List<Card>[] playerCalls = new List<Card>[4];

    int[] scores = new int[4] { INITIAL_SCORE, INITIAL_SCORE, INITIAL_SCORE, INITIAL_SCORE };
    DeclareWinState[] declareWinState = new DeclareWinState[4] { DeclareWinState.None, DeclareWinState.None, DeclareWinState.None, DeclareWinState.None };

    public int DealerPlayerId { get; private set; } = 0;
    public int RoundIndex { get; private set; } = 0;
    public int RoundExtensionCount { get; private set; } = 0;
    public int Turn { get; private set; } = 0;
    public PhaseType Phase { get; private set; } = PhaseType.Draw;


    List<RoundResult> roundResults = new();

    public NumberMahjong() {
    }

    public void SetupNewRound() {
        ResetBoard();
        Shuffle();

        if (roundResults.Count != 0) {
            var prevRoundResult = roundResults[^1];

            // round extension
            if (prevRoundResult.Winners.IndexOf(DealerPlayerId) != -1) {
                RoundExtensionCount += 1;
            }
            else {
                DealerPlayerId = (DealerPlayerId + 1) % N_PLAYERS;
                RoundIndex += 1;
                RoundExtensionCount = 0;
            }
        }

        Turn = DealerPlayerId;
    }

    void ResetBoard() {
        deck.Clear();
        playerHands = new List<Card>[4];
        playerDiscards = new List<Card>[4];
        playerCalls = new List<Card>[4];

        for (int i = 0; i < N_PLAYERS; i++) {
            playerHands[i] = new List<Card>();
            playerDiscards[i] = new List<Card>();
            playerCalls[i] = new List<Card>();
        }

        Array.Fill(declareWinState, DeclareWinState.None);

        List<int> nums = new() {
            1, 2, 3, 4, 20, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
            0, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
            40, 41, 42, 43, 44, 45, 46, 47, 48, 49
        };

        nums.Reverse();

        for (int i = 0; i < N_CARDS; i++) {
            Card card = new() { uid = GetNextUid(), number = nums[i] };
            deck.Add(card);
        }
    }

    public void Shuffle() {
        RandomManager rng = RandomManager.Instance;
        if (rng.seed == -1) return;
        for (int i = 1; i < deck.Count; i++) {
            int j = rng.Next(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }

    public Card Pop(List<Card> list) {
        if (list.Count == 0) throw new Exception("List is empty");
        Card lastCard = list[^1];
        list.RemoveAt(list.Count - 1);
        return lastCard;
    }

#region Game Actions

    private List<IReadOnlyList<Card>> Deal() {
        var ret = new List<IReadOnlyList<Card>>();
        for (int i = 0; i < N_PLAYERS; i++) {
            for (int j = 0; j < HAND_SIZE; j++) {
                Card card = Pop(deck);
                playerHands[i].Add(card);
            }
            ret.Add(playerHands[i]);
        }

        Phase = PhaseType.Draw;
        return ret;
    }

    private Card Draw(int from, int playerId) {
        if (Phase != PhaseType.Draw || Turn != playerId) {
            return null;
        }

        Card card = null;
        if (from == -1) {
            if (deck.Count == 0) return null;

            card = Pop(deck);
            playerHands[Turn].Add(card);
        }
        else if (0 <= from && from < N_PLAYERS) {
            if (Turn == from) return null;
            if (playerDiscards[from].Count == 0) return null;

            card = Pop(playerDiscards[from]);
            playerCalls[Turn].Add(card);
        }

        Phase = PhaseType.Discard;
        return card;
    }

    private void Discard(Card card, int playerId) {
        playerHands[Turn].RemoveAll((c) => c.uid == card.uid);
        playerDiscards[Turn].Add(card);

        Phase = PhaseType.Waiting;
        var winners = GetWinners();
        if (winners.Count > 0) {
            winners.ForEach(winner => declareWinState[winner] = DeclareWinState.Waiting);
        }
        else {
            NextTurn();
        }
    }

    public void DeclareOrSkipWin(int playerId, bool win) {
        // Tsumo
        if (Phase == PhaseType.Discard && playerId == Turn) {
            if (!win) return;
            if (IsWin(playerId)) {
                EndRound();
                return;
            }
            return;
        }

        declareWinState[playerId] = win ? DeclareWinState.Win : DeclareWinState.None;

        var flag = declareWinState.All(state => state != DeclareWinState.Waiting);
        if (flag) {
            if (declareWinState.Any(state => state == DeclareWinState.Win)) {
                EndRound();
            }
            else {
                Turn = (Turn + 1) % N_PLAYERS;
                Phase = PhaseType.Draw;
            }
        }
    }
#endregion
    bool EndRound() {
        var result = GetRoundResult();
        // Assert.IsNotNull(result);
        RoundEndAction action = new(this, result);
        Phase = PhaseType.End;
        action.Execute();

        roundResults.Add(result);

        return true;
    }

    bool NextTurn() {
        if (deck.Count == 0) {
            Phase = PhaseType.End;
            EndRound();
            return false;
        }

        Turn = (Turn + 1) % N_PLAYERS;
        Phase = PhaseType.Draw;

        return true;
    }

    //  check if there's a player that can win
    List<int> GetWinners() {
        List<int> winners = new();
        for (int i = 0; i < N_PLAYERS; i++) {
            if (IsWin(i)) winners.Add(i);
        }
        return winners;
    }

    bool IsArithmeticProgression5(List<int> nums) {
        // Assert.IsTrue(nums.Count == 5);

        nums.Sort();

        int diff = nums[1] - nums[0];
        if (nums[2] - nums[1] == diff && nums[3] - nums[2] == diff && nums[4] - nums[3] == diff) {
            return true;
        }

        return false;
    }

    bool IsArithmeticProgression5OutOf6(List<int> nums) {
        // Assert.IsTrue(nums.Count == 6);

        nums.Sort();

        for (int i = 0; i < 6; i++) {
            List<int> t = new(nums);
            t.RemoveAt(i);
            if (IsArithmeticProgression5(t)) return true;
        }

        return false;
    }

    public bool IsWin(int playerId) {
        if (Phase == PhaseType.Draw) return false;

        List<int> nums = new();
        foreach (Card card in playerHands[playerId]) {
            nums.Add(card.number);
        }
        foreach (Card card in playerCalls[playerId]) {
            nums.Add(card.number);
        }

        // tsumo
        if (Phase == PhaseType.Discard && Turn == playerId) {
            // right after draw
            return IsArithmeticProgression5OutOf6(nums);
        }

        // ron
        if (playerId == Turn) return false; // you can't ron yourself
        var discards = GetPlayerDiscards(Turn);
        if (discards.Count == 0) return false;

        for (int i = 0; i < 5; i++) {
            var t = nums.ToList();
            t.RemoveAt(i);
            t.Add(discards[^1].number);
            if (IsArithmeticProgression5(t)) return true;
        }

        return false;
    }

    public bool IsTenapi(int playerId) {
        var nums = GetPlayerCards(playerId).Select(card => card.number).ToList();

        for (int i = 0; i < N_CARDS; i++) {
            var newHands = new List<int>(nums) { i };
            if (IsArithmeticProgression5OutOf6(newHands)) return true;
        }

        return false;
    }

    RoundResult GetRoundResultExhaustive() {
        if (deck.Count != 0) return null;

        List<ScoreTransfer> scoreTransfers = new();
        List<int> tenapiPlayerIds = new();
        List<int> nonTenapiPlayerIds = new();
        for (int i = 0; i < N_PLAYERS; i++) {
            if (IsTenapi(i)) tenapiPlayerIds.Add(i);
            else nonTenapiPlayerIds.Add(i);
        }

        if (tenapiPlayerIds.Count > 0) {
            int score = WIN_SCORE / tenapiPlayerIds.Count;
            if (tenapiPlayerIds.Count == 1) {
                foreach (int playerId in nonTenapiPlayerIds) {
                    scoreTransfers.Add(new ScoreTransfer() {
                        FromPlayerId = playerId,
                        ToPlayerId = tenapiPlayerIds[0],
                        Score = score
                    });
                }
            }
            else if (tenapiPlayerIds.Count == 2) {
                scoreTransfers.Add(new ScoreTransfer() {
                    FromPlayerId = nonTenapiPlayerIds[0],
                    ToPlayerId = tenapiPlayerIds[0],
                    Score = score
                });
                scoreTransfers.Add(new ScoreTransfer() {
                    FromPlayerId = nonTenapiPlayerIds[1],
                    ToPlayerId = tenapiPlayerIds[1],
                    Score = score
                });
            }
            else if (tenapiPlayerIds.Count == 3) {
                foreach (int playerId in tenapiPlayerIds) {
                    scoreTransfers.Add(new ScoreTransfer() {
                        FromPlayerId = nonTenapiPlayerIds[0],
                        ToPlayerId = playerId,
                        Score = score
                    });
                }
            }
        }

        return new RoundResult() {
            Type = RoundResultType.ExhaustiveDraw,
            Winners = new List<int>(),
            ScoreTransfers = scoreTransfers.ToArray(),
        };
    }

    public RoundResult GetRoundResult() {
        if (deck.Count == 0 && Phase == PhaseType.End) return GetRoundResultExhaustive();

        var scoreTransfers = new List<ScoreTransfer>();

        // Tsumo
        if (Phase == PhaseType.Discard) {
            var winner = Turn;
            for (int i = 0; i < N_PLAYERS; i++) {
                if (i == Turn) continue;
                int score = (WIN_SCORE + (winner == DealerPlayerId ? DEALER_BONUS_SCORE : 0)) / 3;
                scoreTransfers.Add(new ScoreTransfer() {
                    FromPlayerId = i,
                    ToPlayerId = winner,
                    Score = score
                });
            }

            return new RoundResult() {
                Type = RoundResultType.Tsumo,
                Winners = new List<int>() { winner },
                ScoreTransfers = scoreTransfers.ToArray(),
            };
        }

        // Ron
        var winners = declareWinState
            .Select((state, idx) => (state, idx))
            .Where(x => x.state == DeclareWinState.Win)
            .Select(x => x.idx)
            .ToList();

        if (winners.Count == 0) return null;

        int loser = Turn;
        foreach (int winner in winners) {
            int score = WIN_SCORE + (winner == DealerPlayerId ? DEALER_BONUS_SCORE : 0);
            scoreTransfers.Add(new ScoreTransfer() {
                FromPlayerId = Turn,
                ToPlayerId = winner,
                Score = score
            });
        }

        RoundResult ret = new() {
            Type = RoundResultType.Ron,
            Winners = winners,
            ScoreTransfers = scoreTransfers.ToArray(),
        };

        return ret;
    }

    public void ApplyRoundResult(RoundResult result) {
        foreach (ScoreTransfer transfer in result.ScoreTransfers) {
            scores[transfer.FromPlayerId] -= transfer.Score;
            scores[transfer.ToPlayerId] += transfer.Score;
        }

        roundHistories.Add(currentRoundHistory);
        currentRoundHistory = new();
    }

    public IReadOnlyList<Card> GetPlayerHand(int playerId) {
        return playerHands[playerId];
    }

    public IReadOnlyList<Card> GetPlayerDiscards(int playerId) {
        return playerDiscards[playerId];
    }

    /// <summary>
    /// Hand + Calls
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public IReadOnlyList<Card> GetPlayerCards(int playerId) {
        return playerHands[playerId].Concat(playerCalls[playerId]).ToList();
    }

    public List<Card> GetPlayerCalls(int playerId) {
        return playerCalls[playerId];
    }

    public int GetDeckSize() {
        return deck.Count;
    }

    public CardLocation GetCardLocation(int uid) {
        var deckIndex = deck.FindIndex(c => c.uid == uid);
        if (deckIndex != -1) return new CardLocation(CardLocationEnum.Deck, -1, deckIndex);

        for (int i = 0; i < N_PLAYERS; i++) {
            var handIndex = playerHands[i].FindIndex(c => c.uid == uid);
            if (handIndex != -1) return new CardLocation(CardLocationEnum.Hand, i, handIndex);

            var discardIndex = playerDiscards[i].FindIndex(c => c.uid == uid);
            if (discardIndex != -1) return new CardLocation(CardLocationEnum.Discard, i, discardIndex);

            var callIndex = playerCalls[i].FindIndex(c => c.uid == uid);
            if (callIndex != -1) return new CardLocation(CardLocationEnum.Call, i, callIndex);
        }

        return null;
    }

    public int GetScore(int playerId) {
        return scores[playerId];
    }

    public int GetRank(int playerId) {
        int rank = 1;
        for (int i = 0; i < 4; i++) {
            if (i == playerId) continue;
            if (scores[i] > scores[playerId]) rank += 1;
            else if (scores[i] == scores[playerId] && i < playerId) rank += 1;
        }
        return rank;
    }

    public DeclareWinState GetDeclareWinState(int playerId) {
        return declareWinState[playerId];
    }

    // Serialization
    public string Serialize() {
        MemoryStream stream = new();
        BinaryWriter writer = new(stream);

        writer.Write((byte)DealerPlayerId);
        writer.Write((byte)RoundIndex);
        writer.Write((byte)Turn);
        writer.Write((byte)Phase);

        writer.Write((byte)deck.Count);
        foreach (Card card in deck) {
            writer.Write((byte)card.uid);
            writer.Write((byte)card.number);
        }

        for (int i = 0; i < N_PLAYERS; i++) {
            writer.Write((byte)playerHands[i].Count);
            foreach (Card card in playerHands[i]) {
                writer.Write((byte)card.uid);
                writer.Write((byte)card.number);
            }

            writer.Write((byte)playerDiscards[i].Count);
            foreach (Card card in playerDiscards[i]) {
                writer.Write((byte)card.uid);
                writer.Write((byte)card.number);
            }

            writer.Write((byte)playerCalls[i].Count);
            foreach (Card card in playerCalls[i]) {
                writer.Write((byte)card.uid);
                writer.Write((byte)card.number);
            }
        }

        return Convert.ToBase64String(stream.ToArray());
    }

    public void Deserialize(string data) {
        byte[] bytes = Convert.FromBase64String(data);
        MemoryStream stream = new(bytes);
        BinaryReader reader = new(stream);

        DealerPlayerId = reader.ReadByte();
        RoundIndex = reader.ReadByte();
        Turn = reader.ReadByte();
        Phase = (PhaseType)reader.ReadByte();

        deck.Clear();
        int nDeck = reader.ReadByte();
        for (int i = 0; i < nDeck; i++) {
            Card card = new() {
                uid = reader.ReadByte(),
                number = reader.ReadByte()
            };
            deck.Add(card);
        }

        for (int i = 0; i < N_PLAYERS; i++) {
            playerHands[i].Clear();
            int nPlayerHands = reader.ReadByte();
            for (int j = 0; j < nPlayerHands; j++) {
                Card card = new() {
                    uid = reader.ReadByte(),
                    number = reader.ReadByte()
                };
                playerHands[i].Add(card);
            }

            playerDiscards[i].Clear();
            int nPlayerDiscards = reader.ReadByte();
            for (int j = 0; j < nPlayerDiscards; j++) {
                Card card = new() {
                    uid = reader.ReadByte(),
                    number = reader.ReadByte()
                };
                playerDiscards[i].Add(card);
            }

            playerCalls[i].Clear();
            int nPlayerCalls = reader.ReadByte();
            for (int j = 0; j < nPlayerCalls; j++) {
                Card card = new() {
                    uid = reader.ReadByte(),
                    number = reader.ReadByte()
                };
                playerCalls[i].Add(card);
            }
        }
    }

    // For Debug
    string ListToString(List<Card> cards) {
        return string.Join(", ", cards.Select(card => card.number.ToString()).ToList());
    }

    public override string ToString() {
        string ret = "";
        ret += $"Deck: {ListToString(deck)}\n";
        for (int i = 0; i < N_PLAYERS; i++) {
            ret += $"Player {i}\n";
            var list = playerHands[i].Concat(playerCalls[i]).ToList();
            list.Sort((a, b) => a.number - b.number);

            ret += $"Hand {i}: {ListToString(list)}\n";
            ret += $"Discards {i}: {ListToString(playerDiscards[i])}\n";
        }

        return ret;
    }
}
