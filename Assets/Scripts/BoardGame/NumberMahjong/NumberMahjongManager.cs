using UnityEngine;
using System.Collections;
using VInspector;
using System;

public class NumberMahjongManager : MonoBehaviour {
    public static NumberMahjongManager instance;

    public NumberMahjong game;

    public GameObject[] characters = new GameObject[4];
    public CharacterInfo[] characterInfos = new CharacterInfo[4];

    public enum PlayerType { Human, AI }
    public PlayerType[] playerTypes = new PlayerType[4]{PlayerType.Human, PlayerType.AI, PlayerType.AI, PlayerType.AI};

    public GameObject deckObject;
    public Transform gameBoard;

    public NumberMahjongAI[] AIs = new NumberMahjongAI[4];

    public UIPlayerHand uiPlayerHand;
    public UINumberMahjongRoundResult roundResultUI;
    public Canvas canvas;

    public BoardPlayerDiscardArea[] playerDiscardAreas = new BoardPlayerDiscardArea[4];
    public BoardPlayerCallArea[] playerCallAreas = new BoardPlayerCallArea[4];


    void Awake() {
        if (instance == null) {
            instance = this;
        }

        game = new NumberMahjong();

        Application.targetFrameRate = 60;
        roundResultUI.OnClickOkButtonEvent += () => {
            NewRound();
            canvas.gameObject.SetActive(true);
        };

        for (int i = 0; i < 4; i++) {
            playerDiscardAreas[i] = gameBoard.Find($"PlayerArea{i}/Discards").GetComponent<BoardPlayerDiscardArea>();
            playerCallAreas[i] = gameBoard.Find($"PlayerArea{i}/Calls").GetComponent<BoardPlayerCallArea>();
        }

        // chracaters
        var characterInfos = Resources.LoadAll<CharacterInfo>("Data/Characters");
        Array.Sort(characterInfos, (a, b) => a.charId - b.charId);
        this.characterInfos = characterInfos;
    }

    IEnumerator Func() {
        yield return new WaitForSeconds(0.1f);
        NumberMahjong.DealCommand dealAction = new(game);
        dealAction.Execute();
    }

    void NewRound() {
        game.SetupNewRound();

        for (int i = 0; i < NumberMahjong.N_PLAYERS; i++) {
        }

        uiPlayerHand.Clear();

        StartCoroutine(Func());
    }

    void Start() {
    }

    bool started = false;
    string input = "0";
    Rect windowRect = new Rect(10, 150, 100, 40);

    void OnGUI() {
        if (!started) {
            windowRect = GUI.Window(0, windowRect, InputSeed, "Input Seed");
        }
    }

    void InputSeed(int windowId) {
        // input a number
        if (!started) {
            input = GUI.TextField(new Rect(0,0,100, 20), input);
            if (GUI.Button(new Rect(0, 20, 100, 20), "Start")) {
                if (int.TryParse(input.Trim(), out int number)) {
                    if (number == 0) number = UnityEngine.Random.Range(1, 1000000);
                    RandomManager.Instance.SetSeed(number);
                    started = true;
                    NewRound();
                }
                else {
                    Debug.LogError("Invalid input. Please enter a valid number.");
                }
            }
        }
        // make this window draggable
        GUI.DragWindow();
    }

    [Button]
    public void PrintGameState() {
        Debug.Log(game.ToString());
    }

    /*
    [Rpc(SendTo.Server)]
    private void DrawCardServerRpc() {
        if (clientPlayerId != game.Turn) return;

        NumberMahjong.DrawAction action = new(game, game.Turn, 0);
        action.Execute();
    }

    [Rpc(SendTo.Server)]
    private void DiscardCardServerRpc(int cardNumber) {
        if (clientPlayerId != game.Turn) return;

        var hand = game.GetPlayerHand(game.Turn);
        NumberMahjong.DiscardAction action = new(game, game.Turn, hand[cardNumber]);
        action.Execute();
    }
    */

    void Update() {
    }

    [ContextMenu("ToBase64")]
    public void ToBase64() {
        Debug.Log(game.Serialize());
    }

    [ContextMenu("GetShantens")]
    public void GetShantens() {
        for (int i = 0; i < 4; i++) {
            if (AIs[i] == null) continue;
            var shanten = AIs[i].GetShanten();
            Debug.Log($"Player {i} Shanten: {shanten.shanten} {shanten.a0} {shanten.d}");
        }
    }

    void LateUpdate() {
    }
}
