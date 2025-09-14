using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BoardNumberCard : NumberMahjongGameSubscriberBehaviour, ISelectableBoardItem {
    // TODO: make it reactive
    public bool isInHand = false;
    NumberMahjong.CardLocation location;

    AudioSource audioSource;
    [SerializeField] AudioClip drawSound, discardSound;

    bool shouldPlayDiscardSound = false;
    TMP_Text numberText;
    ToolTip tooltip;

#region MonoBehaviour
    private void Start() {
        if (GetComponent<DataHolder>() == null) {
            // Debug.LogError("DataHolder not found");
            Destroy(this);
            return;
        }

        var card = DataHolder.GetData<NumberMahjong.Card>(gameObject);
        var numberTextObj = transform.Find("Text");
        numberText = numberTextObj.GetComponentInChildren<TMP_Text>();
        numberText.text = card.number.ToString();

        tooltip = GetComponent<ToolTip>();
        tooltip.SetText($"{card.number}");

        UpdateSelectable(card);

        audioSource = GetComponent<AudioSource>();
        if (shouldPlayDiscardSound) {
            audioSource.PlayOneShot(discardSound);
            shouldPlayDiscardSound = false;
        }
    }

    public void QueueDiscardSound() {
        shouldPlayDiscardSound = true;
    }

    void Update() {
        if (isInHand) {
            numberText.enabled = false;
            tooltip.enabled = false;
        }
    }

    private new void OnDestroy() {
        base.OnDestroy();
        if (tween != null && tween.IsActive()) tween.Kill();
    }
#endregion

    private Tween tween;
    public void TweenTransform(Vector3 pos, float angle, float duration) {
        tween = DOTween.Sequence()
            .Append(transform.DOLocalMove(pos, duration))
            .Join(transform.DOLocalRotate(new Vector3(0, angle*57.2958f, 0), duration));
    }

    bool isSelectable = false;
    public void UpdateSelectable(NumberMahjong.Card card) {
        bool flag;
        if (location == null) {
            location = game.GetCardLocation(card.uid);
            if (location == null) {
                Debug.LogError($"Card location not found {card.uid}");
                var t = game.GetCardLocation(card.uid);
                return;
            }
        }

        var clientPlayer = ClientPlayer.instance;

        if (clientPlayer.playerId != game.Turn) flag = false;
        else if (game.Phase != NumberMahjong.PhaseType.Draw) flag = false;
        else if (game.Turn == location.PlayerId) flag = false;
        else {
            var discards = game.GetPlayerDiscards(location.PlayerId);
            if (discards.Count == 0) flag = false;
            else if (discards[^1].uid == card.uid) flag = true;
            else flag = false;
        }

        var material = GetComponent<MeshRenderer>().material;
        material.SetColor("_BaseColor", flag ? Color.green : Color.white);

        isSelectable = flag;
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand action) {
        if (GetComponent<DataHolder>() == null) return;
        var card = DataHolder.GetData<NumberMahjong.Card>(gameObject);
        location ??= game.GetCardLocation(card.uid);
        UpdateSelectable(card);
    }

    public void OnSelect() {
        if (isSelectable) {
            var clientPlayer = ClientPlayer.instance;
            var location = game.GetCardLocation(DataHolder.GetData<NumberMahjong.Card>(gameObject).uid);

            NumberMahjong.DrawAction drawAction = new(game, game.Turn, location.PlayerId);
            drawAction.Execute();

            Destroy(gameObject);

            var manager = NumberMahjongManager.instance;
            var callArea = manager.playerCallAreas[clientPlayer.playerId];
            callArea.AddCardToCallArea(drawAction.card);
        }
    }
}
