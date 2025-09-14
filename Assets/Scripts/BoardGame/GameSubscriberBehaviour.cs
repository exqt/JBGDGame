using System;
using UnityEngine;

public abstract class GameSubscriberBehaviour<T> : MonoBehaviour where T : GameBase<T> {
    protected T game;

    public void SetGame(T game) {
        this.game = game;
    }

    public virtual void Awake() {
        if (game == null) {
            Debug.LogError("Game is not set");
            return;
        }

        game.OnStateChange += OnStateChange;
    }

    public abstract void OnStateChange(GameBase<T>.BoardGameCommand command);

    protected void OnDestroy() {
        game.OnStateChange -= OnStateChange;
    }
}
