using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UniVRM10;

[RequireComponent(typeof(BoardPlayer))]
public class CharacterHandControl : NumberMahjongGameSubscriberBehaviour {
    public AnimationClip reachClip;
    public GameObject boardCardPrefab;
    public GameObject deckObject;

    GameObject rightHandTarget, rightHandHint;
    Transform leftHand, rightHand;
    Transform rightThumb;
    Rig rig;
    Vrm10Instance vrmInstance;
    Animator animator;
    Vector3 rightHandTargetOriginPosition;

    Transform handContainer;
    BoardPlayerHand handComp;

    NumberMahjongManager manager;

    float handAnimationDuration = 2.0f;

    new void Awake() {
        manager = NumberMahjongManager.instance;
        SetGame(manager.game);
        base.Awake();

        var rightHandIK = transform.Find("RightHandIK");
        if (rightHandIK == null) {
            Debug.LogError("RightHandIK not found");
            return;
        }

        rig = rightHandIK.GetComponent<Rig>();
        animator = gameObject.GetComponent<Animator>();
        vrmInstance = gameObject.GetComponent<Vrm10Instance>();

        leftHand = transform.FindDescenedant("J_Bip_L_Hand");
        rightHand = transform.FindDescenedant("J_Bip_R_Hand");
        rightHandTarget = rightHandIK.transform.Find("Constraint/Target").gameObject;
        rightHandHint = rightHandIK.transform.Find("Constraint/Hint").gameObject;
        rightHandHint.transform.localPosition = new Vector3(0.4f, 0.4f, 0.0f);
        rightThumb = rightHand.Find("J_Bip_R_Thumb1");

        var boardPlayer = GetComponent<BoardPlayer>();

        handContainer = manager.gameBoard.transform.Find(
            $"PlayerArea{boardPlayer.playerId}/Hand"
        );
        handContainer.position = leftHand.position;
        handContainer.SetParent(leftHand);
        handContainer.localRotation = Quaternion.Euler(-180, 90, 60);
        handContainer.localPosition = new Vector3(-0.106f, -0.0154f, 0.0304f);

        handComp = handContainer.GetComponent<BoardPlayerHand>();

        vrmInstance.Runtime.Expression.SetWeight(ExpressionKey.Neutral, 1.0f);
    }

    void Start() {
        handAnimationDuration = reachClip.length;
        StartCoroutine(LazyStart());
    }

    IEnumerator LazyStart() {
        yield return new WaitForEndOfFrame();
        rightHandTarget.transform.position = rightHand.position;
    }

    // animation event
    // draw from deck : reach -> grab card -> pull back -> detatch card
    // draw from other player : reach -> grab card -> place on call -> pull back -> detatch card
    // discard : grab card -> place on discard -> pull back -> detatch card

    GameObject cardOnRightHand = null;
    GameObject GrabCard(NumberMahjong.Card cardData) {
        var cardObj = Instantiate(boardCardPrefab, rightHand);
        DataHolder.Attach(cardObj, cardData);

        var cardComp = cardObj.GetComponent<BoardNumberCard>();
        cardComp.isInHand = true;

        cardOnRightHand = cardObj;
        cardObj.transform.position = rightHand.position;
        cardObj.transform.localRotation = Quaternion.Euler(0, -90, -180);
        cardObj.transform.localPosition = new Vector3(0.102f, -0.015f, 0.0034f);

        return cardObj;
    }

    void DetatchCard() {
        if (cardOnRightHand == null) return;
        Destroy(cardOnRightHand);
    }

    IEnumerator Reach(Vector3 pos) {
        Vector3 dir = (pos - transform.position).normalized;
        pos -= new Vector3(dir.x, 0, dir.z) * 0.15f;

        rightHandTargetOriginPosition = rightHand.transform.position;
        animator.SetTrigger("Reach");

        var tween  = rightHandTarget.transform.DOMove(
            pos, handAnimationDuration
        );
        tween.onUpdate = () => {
            var t = tween.ElapsedPercentage();
            SetRigWeight(t);
        };
        yield return tween.WaitForCompletion();
    }

    IEnumerator MoveTo(Vector3 pos) {
        var tween = rightHandTarget.transform.DOMove(
            pos, handAnimationDuration
        );
        yield return tween.WaitForCompletion();
    }

    IEnumerator PullBack() {
        animator.SetTrigger("Pull");
        var tween = rightHandTarget.transform.DOMove(
            rightHandTargetOriginPosition, handAnimationDuration
        );
        tween.onUpdate = () => {
            var t = tween.ElapsedPercentage();
            SetRigWeight(1 - t);
        };
        yield return tween.WaitForCompletion();
    }


    // draw from deck : reach -> grab card -> pull back -> detatch card
    public IEnumerator AnimateDrawDeck(Func<NumberMahjong.Card> onDraw) {
        var deckPos = deckObject.transform.position;
        var deck = deckObject.GetComponent<BoardDeck>();
        deckPos += 0.001f * deck.GetCount() * Vector3.up;

        yield return Reach(deckPos);
        var card = onDraw();
        GrabCard(card);
        yield return PullBack();
        DetatchCard();
        handComp.AddCardToHand(card);
        handComp.SortHand();
    }

    // draw from other player : reach -> grab card -> place on call -> pull back -> detatch card
    public IEnumerator AnimateCall(Vector3 pos, Vector3 callPosition, Func<NumberMahjong.Card> onDraw, Action onDrop) {
        yield return Reach(pos);
        var card = onDraw();
        GrabCard(card);
        yield return MoveTo(callPosition);
        DetatchCard();
        onDrop();
        yield return PullBack();
    }

    // discard : grab card -> place on discard -> pull back -> detatch card
    public IEnumerator AnimateDiscard(Vector3 pos, NumberMahjong.Card card, Action onDrop) {
        yield return new WaitForSeconds(0.5f);
        GrabCard(card);
        handComp.RemoveCardFromHand(card);
        handComp.SortHand();
        yield return Reach(pos);
        DetatchCard();
        onDrop();
        yield return PullBack();
    }

    void SetRigWeight(float t) {
        rig.weight = t;
        var angle = 0;
        rightHandTarget.transform.localRotation = Quaternion.Euler(0, -90 + angle * t, 0);
    }

    /*
        var p1 = new Vector3(transform.position.x, 0, transform.position.z);
        var p2 = new Vector3(position.x, 0, position.z);
        var angle = Vector3.SignedAngle(transform.forward, p2 - p1, Vector3.up);
        // angle += transform.rotation.eulerAngles.y;
            rightHandTarget.transform.position =
                Vector3.Lerp(rightHandTargetOriginPosition, position, Mathf.Pow(p, 1f));
            rightHandTarget.transform.localRotation = Quaternion.Euler(0, -90 + angle * p, 0);
            */


    void Update() {
    }

    void LateUpdate() {
        // HandContainer.rotation = rightHand.rotation;
    }

    public override void OnStateChange(NumberMahjong.BoardGameCommand command) {
        // if (command is NumberMahjong.DrawAction drawAction) {
        //     StartCoroutine(AnimateDraw(deckObject.transform.position, drawAction.card));
        // }
    }
}
