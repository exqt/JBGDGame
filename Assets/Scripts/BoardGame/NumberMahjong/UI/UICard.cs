using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using VInspector;

public class UICard : NumberMahjongGameSubscriberBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {
    [HideInInspector] public bool isInHand = true;
    Image backgroundImage;
    RectTransform rectTransform;
    Canvas canvas;
    UIPlayerHand hand;

    Tween posTween, backgroundTween, hoverTween;

    public override void Awake() {
        base.Awake();

        rectTransform = transform.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        hand = GetComponentInParent<UIPlayerHand>();
        backgroundImage = transform.Find("Background").GetComponent<Image>();
    }

    void Start() {
        var textObject = transform.Find("Text");
        var textComponent = textObject.GetComponent<TextMeshProUGUI>();
        var card = DataHolder.GetData<NumberMahjong.Card>(gameObject);
        textComponent.text = card.number.ToString();
    }

    new void OnDestroy() {
        base.OnDestroy();
        if (posTween != null && posTween.IsActive()) posTween.Kill();
        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();
        if (backgroundTween != null && backgroundTween.IsActive()) backgroundTween.Kill();
    }

    static readonly Color hoverColor = new(0.95f, 0.95f, 0.95f, 1.0f);
    static readonly float tweenDuration = 0.2f;
    static readonly float hoverScale = 1.2f;

    public void OnPointerEnter(PointerEventData eventData) {
        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();
        if (backgroundTween != null && backgroundTween.IsActive()) backgroundTween.Kill();

        backgroundTween = backgroundImage.DOColor(hoverColor, tweenDuration);
        hoverTween = DOTween.Sequence()
            .Append(rectTransform.DOScale(hoverScale, tweenDuration));
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (hoverTween != null && hoverTween.IsActive()) hoverTween.Kill();
        if (backgroundTween != null && backgroundTween.IsActive()) backgroundTween.Kill();

        backgroundTween = backgroundImage.DOColor(Color.white, tweenDuration);
        hoverTween = DOTween.Sequence()
            .Append(rectTransform.DOScale(1.0f, tweenDuration));
    }

    public void OnPointerDown(PointerEventData eventData) {
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left) {
            // cancel drag
            eventData.pointerDrag = null;
            return;
        }

        isInHand = false;
        hand.SortCards();
    }

    public void OnEndDrag(PointerEventData eventData) {
        var parentRect = hand.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // check if is inside the hand
        if (parentRect.rect.Contains(localPoint)) {
            isInHand = true;
            hand.SortCards();
        } else {
            var manager = NumberMahjongManager.instance;
            var game = manager.game;
            var playerId = game.Turn;
            var card = DataHolder.GetData<NumberMahjong.Card>(gameObject);

            NumberMahjong.DiscardAction discardAction = new(game, playerId, card);
            var valid = discardAction.Validate();

            if (valid) {
                var discardArea = manager.playerDiscardAreas[playerId];
                discardArea.AddCardToDiscardArea(card);

                discardAction.Execute();
                isInHand = false;
                Destroy(gameObject);
            }
            else {
                isInHand = true;
                hand.SortCards();
            }
        }
    }

    public void OnDrag(PointerEventData eventData) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        rectTransform.position = canvas.transform.TransformPoint(localPoint);
    }

    public override void OnStateChange(GameBase<NumberMahjong>.BoardGameCommand action) {
    }

    public void TweenTransform(Vector3 pos, float angle, float duration) {
        posTween = DOTween.Sequence()
            .Append(rectTransform.DOLocalMove(pos, duration))
            .Join(rectTransform.DOLocalRotate(new Vector3(0, 0, angle*57.2958f), duration));
    }
}
