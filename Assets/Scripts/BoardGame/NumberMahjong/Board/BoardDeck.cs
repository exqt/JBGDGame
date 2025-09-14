using UnityEngine;

public class BoardDeck : NumberMahjongGameSubscriberBehaviour, ISelectableBoardItem {
    public GameObject managerObject;
    public GameObject cardPrefab;

    private ClientPlayer clientPlayer;

    private MeshRenderer meshRenderer;
    private int nCards = 20;

    AudioSource audioSource;

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        nCards = game.GetDeckSize();
        clientPlayer = ClientPlayer.instance;
        audioSource = GetComponent<AudioSource>();
    }

    public void SetSelectable(bool selectable) {
        var material = meshRenderer.material;
        material.SetColor("_BaseColor", selectable ? Color.green : Color.white);
    }

    public int GetCount() {
        return nCards;
    }

    void Draw() {
        nCards = game.GetDeckSize();
        transform.localScale = new Vector3(1.0f, nCards, 1.0f);
        meshRenderer.enabled = nCards > 0;

        var tooltip = GetComponent<ToolTip>();
        var cardQuantityDescription = nCards == 1 ? "card" : "cards";
        tooltip.SetText($"Deck ({nCards} {cardQuantityDescription} left)");
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand action) {
        SetSelectable(game.Phase == NumberMahjong.PhaseType.Draw && game.Turn == clientPlayer.playerId);
        Draw();
    }

    public void OnSelect() {
        var clientPlayer = ClientPlayer.instance;
        if (
            clientPlayer.playerId != game.Turn ||
            game.Phase != NumberMahjong.PhaseType.Draw
        ) return;

        NumberMahjong.DrawAction drawAction = new(game, clientPlayer.playerId, -1);
        drawAction.Execute();
        audioSource.Play();
    }

    void Update() {

    }
}
