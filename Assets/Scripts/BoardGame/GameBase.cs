using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Analytics;


public class GameBase<T> where T : GameBase<T> {
    public abstract class BoardGameCommand {
        public T game;
        // return true if the action is successful
        protected abstract void OnExecute();
        public virtual bool Validate() { return true; }
        private bool _sent = false;

        public bool Execute(bool send = true) {
            var valid = Validate();
            if (!valid) return false;
            OnExecute();
            if (send) SendCommand();
            return valid;
        }

        public void SendCommand() {
            if (_sent) return;
            game.SendEvent(this);
            _sent = true;
        }
    }

    public abstract class TurnAction : BoardGameCommand {
        public int playerId;
    }

    int nextUid = 0;

    public delegate void StateChangeCallback(BoardGameCommand command);
    public event StateChangeCallback OnStateChange;

    public List<List<BoardGameCommand>> roundHistories = new();
    public int nStateChanged = 0;
    public List<BoardGameCommand> currentRoundHistory = new();

    public List<BoardGameCommand> PumpCommands() {
        List<BoardGameCommand> ret = new();
        for (int i = currentRoundHistory.Count - nStateChanged; i < currentRoundHistory.Count; i++) {
            ret.Add(currentRoundHistory[i]);
        }
        nStateChanged = 0;
        return ret;
    }

    public int GetNextUid() {
        return nextUid++;
    }

    public bool SendEvent(BoardGameCommand command) {
        currentRoundHistory.Add(command);
        nStateChanged += 1;
        OnStateChange.Invoke(command);
        return true;
    }
}
