using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardPlayerDiscardArea : NumberMahjongGameSubscriberBehaviour {
    public int playerId;
    public GameObject boardCardPrefab;

    NumberMahjongManager manager;

    const float CARD_GAP = 0.045f;

    public void Clear() {
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public GameObject GetLatestDiscardCard() {
        if (transform.childCount == 0) return null;
        return transform.GetChild(transform.childCount - 1).gameObject;
    }

    public Vector3 GetNextDiscardPosition() {
        var n = transform.childCount;
        var localPos = transform.localPosition + CARD_GAP * n * Vector3.right;
        var worldPos = transform.parent.TransformPoint(localPos);
        return worldPos;
    }

    public void RemoveCardFromDiscardArea(NumberMahjong.Card card) {
        for (int i = 0; i < transform.childCount; i++) {
            var cardObject = transform.GetChild(i).gameObject;
            var _card = DataHolder.GetData<NumberMahjong.Card>(cardObject);
            if (_card.uid == card.uid) {
                Destroy(cardObject);
                break;
            }
        }
    }

    public void AddCardToDiscardArea(NumberMahjong.Card card) {
        var nCards = transform.childCount;
        var cardObject = Instantiate(boardCardPrefab, Vector3.zero, Quaternion.identity, transform);
        var comp = cardObject.GetComponent<BoardNumberCard>();
        comp.QueueDiscardSound();

        DataHolder.Attach(cardObject, card);
        cardObject.transform.SetLocalPositionAndRotation(new Vector3(CARD_GAP * nCards, 0.00001f * nCards, 0.0f), Quaternion.Euler(0, UnityEngine.Random.Range(-7, 8), 0));
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand action) {
        if (action is NumberMahjong.DealCommand) {
            Clear();
        }
    }
}
