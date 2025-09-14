using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardPlayerCallArea : NumberMahjongGameSubscriberBehaviour {
    public int playerId;
    public GameObject boardCardPrefab;

    public NumberMahjongManager manager;

    void Start() {
    }

    public void Clear() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    private const float CARD_GAP = 0.045f;

    public Vector3 GetNextCallPosition() {
        var n = transform.childCount;
        var localPos = transform.localPosition + CARD_GAP * n * Vector3.right;
        var worldPos = transform.parent.TransformPoint(localPos);
        return worldPos;
    }

    public void AddCardToCallArea(NumberMahjong.Card card) {
        var nCards = transform.childCount;
        var cardObject = Instantiate(boardCardPrefab, Vector3.zero, Quaternion.identity, transform);
        cardObject.GetComponent<BoardNumberCard>().SetGame(game);
        DataHolder.Attach(cardObject, card);

        cardObject.transform.SetLocalPositionAndRotation(new Vector3(CARD_GAP * nCards, 0.000001f * nCards, 0.0f), Quaternion.Euler(0, UnityEngine.Random.Range(-7, 8), 0));
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand action) {
        if (action is NumberMahjong.DealCommand) {
            Clear();
        }
    }
}
