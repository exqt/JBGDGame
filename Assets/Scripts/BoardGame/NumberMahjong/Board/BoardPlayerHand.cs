using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardPlayerHand : NumberMahjongGameSubscriberBehaviour {
    public GameObject boardCardPrefab;
    public int playerId;

    public NumberMahjongManager manager;

    void Start() {
    }

    public void Clear() {
        foreach (Transform child in gameObject.transform) {
            Destroy(child.gameObject);
        }
    }

    public void SetHandVisible(bool visible) {
        gameObject.SetActive(visible);
    }

    public void AddCardToHand(NumberMahjong.Card card) {
        var cardObject = Instantiate(boardCardPrefab);
        DataHolder.Attach(cardObject, card);

        cardObject.transform.SetParent(transform);
        cardObject.name = card.number.ToString();
        cardObject.GetComponent<BoardNumberCard>().SetGame(game);
        cardObject.GetComponent<BoardNumberCard>().isInHand = true;
        cardObject.transform.SetLocalPositionAndRotation(
            new Vector3(0, 0, 0),
            Quaternion.Euler(0, 0, 0)
        );
    }

    public void RemoveCardFromHand(NumberMahjong.Card card) {
        for (int i = 0; i < transform.childCount; i++) {
            var cardObject = transform.GetChild(i).gameObject;
            var _card = DataHolder.GetData<NumberMahjong.Card>(cardObject);
            if (_card.uid == card.uid) {
                var cardComp = cardObject.GetComponent<BoardNumberCard>();
                cardComp.isInHand = false;
                Destroy(cardObject);
                break;
            }
        }
    }

    [ContextMenu("Sort Hand")]
    public void SortHand() {
        List<GameObject> cards = new();
        for (int i = 0; i < transform.childCount; i++) {
            var o = transform.GetChild(i).gameObject;
            var comp = o.GetComponent<BoardNumberCard>();
            if (comp != null && comp.isInHand) {
                cards.Add(o);
            }
        }

#if UNITY_EDITOR
        cards.Sort((a, b) => {
            var da = DataHolder.GetData<NumberMahjong.Card>(a);
            var db = DataHolder.GetData<NumberMahjong.Card>(b);
            return da.uid - db.uid;
        });
#endif

        var nCards = cards.Count;
        float angle = 0.4f;
        float radius = 0.05f;

        for (int i = 0; i < nCards; i++) {
            var card = cards[i];
            // handle the case of a single card
            var p = nCards == 1 ? 0.5f : 1.0f * i / (nCards - 1);
            // scale [0, 1] to [-1, 1]
            p = 2 * p - 1;

            var a = -angle * p;
            var ta = Mathf.PI/2.0f;
            var x = Mathf.Cos(a + ta) * radius;
            var y = -radius + Mathf.Sin(a + ta) * radius;
            var cardComponent = cards[i].GetComponent<BoardNumberCard>();
            var ox = cardComponent.transform.localPosition.x;
            var oy = cardComponent.transform.localPosition.y;
            // if (ox == x && oy == y) { continue; }

            cardComponent.TweenTransform(new Vector3(x, 0.001f*i, y), -a, 0.5f);
        }
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand action) {
        if (action is NumberMahjong.DealCommand) {
            var callAction = action as NumberMahjong.DealCommand;
            var list = callAction.playerHands[playerId];
            Clear();
            foreach (var card in list) {
                AddCardToHand(card);
            }

            SortHand();
        }
    }
}
