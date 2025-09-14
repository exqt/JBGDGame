using UnityEngine;

abstract public class NumberMahjongGameSubscriberBehaviour : GameSubscriberBehaviour<NumberMahjong> {
    public override void Awake() {
        SetGame(NumberMahjongManager.instance.game);
        base.Awake();
    }
}
