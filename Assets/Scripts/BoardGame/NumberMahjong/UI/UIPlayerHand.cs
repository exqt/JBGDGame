using System;
using System.Linq;
using UnityEngine;

public class UIPlayerHand : NumberMahjongGameSubscriberBehaviour {
    public GameObject cardPrefab;

    public int playerId = 0;

    float angle = 0.4f;
    float radius = 600.0f;
    bool shouldSort = false;

    void Start() {
        Clear();
    }

    public UICard[] GetCards(bool inHand) {
        var list = gameObject.GetComponentsInChildren<UICard>();
        if (inHand) list = list.Where(card => card.isInHand).ToArray();
        return list;
    }

    public void SetVisible(bool visible) {
        gameObject.SetActive(visible);
    }

    public void Clear() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    private void AddCard(NumberMahjong.Card card) {
        var o = Instantiate(cardPrefab, transform);
        DataHolder.Attach(o, card);
        var cardComponent = o.GetComponent<UICard>();
        cardComponent.isInHand = true;
        o.transform.SetParent(transform);
    }

    private void RemoveCard(NumberMahjong.Card card) {
        var cards = GetCards(false);
        foreach (var c in cards) {
            var data = DataHolder.GetData<NumberMahjong.Card>(c.gameObject);
            var cardComponent = c.GetComponent<UICard>();
            if (data.uid == card.uid) {
                cardComponent.isInHand = false;
                Destroy(c.gameObject);
                return;
            }
        }
    }

    public void SortCards() {
        var cards = GetCards(true);
        var nCards = cards.Length;

        if (cards.Length >= 5) radius = 450.0f;
        else if (cards.Length == 4) radius = 350.0f;
        else if (cards.Length == 3) radius = 200.0f;
        else if (cards.Length == 2) radius = 100.0f;
        else if (cards.Length == 1) radius = 50.0f;

        // sort
        Array.Sort(cards, (a, b) => {
            var da = DataHolder.GetData<NumberMahjong.Card>(a.gameObject);
            var db = DataHolder.GetData<NumberMahjong.Card>(b.gameObject);
            return db.number - da.number;
        });

        for (int i = 0; i < nCards; i++) {
            var card = cards[i];
            card.transform.SetSiblingIndex(i);
            // handle the case of a single card
            var p = nCards == 1 ? 0.5f : 1.0f * i / (nCards - 1);
            // scale [0, 1] to [-1, 1]
            p = 2 * p - 1;

            var a = angle * p;
            var ta = Mathf.PI/2.0f;
            var x = Mathf.Cos(a + ta) * radius;
            var y = -radius + Mathf.Sin(a + ta) * radius;
            var cardComponent = cards[i].GetComponent<UICard>();
            var ox = cardComponent.transform.localPosition.x;
            var oy = cardComponent.transform.localPosition.y;
            if (ox == x && oy == y) {
                continue;
            }

            cardComponent.TweenTransform(new Vector3(x, y, 0), a, 0.5f);
        }
    }

    public void SetSelectable() {
        var cards = GetCards(true);
        foreach (var card in cards) {
            // card.SetSelectable(true);
        }
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand action) {
        if (action is NumberMahjong.RoundEndAction) {
            SetVisible(false);
        }
        else if (action is NumberMahjong.DealCommand dealAction) {
            SetVisible(true);
            var list = dealAction.playerHands[playerId];
            foreach (var card in list) {
                AddCard(card);
            }
            shouldSort = true;
        }
        else if (action is NumberMahjong.DrawAction drawAction) {
            if (drawAction.playerId == playerId && drawAction.from == -1) {
                AddCard(drawAction.card);
                shouldSort = true;
            }
        }
        else if (action is NumberMahjong.DiscardAction discardAction) {
            if (discardAction.playerId == playerId) {
                RemoveCard(discardAction.card);
                shouldSort = true;
            }
        }
    }

#region
    void Update()
    {
        if (shouldSort) {
            SortCards();
            shouldSort = false;
        }
    }
#endregion
}
