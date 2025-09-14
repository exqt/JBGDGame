
public class TurnTimer {
    const float TURN_TIME = 10.0f;
    const float EXTRA_TIME = 20.0f;

    float leftTurnTime;
    float leftExtraTime;

    public TurnTimer() {
        Reset();
    }

    public void Reset() {
        leftTurnTime = TURN_TIME;
        leftExtraTime = EXTRA_TIME;
    }


    public void StartTurn() {
        leftTurnTime = TURN_TIME;
    }

    /// <summary>
    /// Tick the timer
    /// </summary>
    /// <param name="dt"></param>
    /// <returns>true means Timeout</returns>
    public bool Tick(float dt) {
        if (leftTurnTime > dt) {
            leftTurnTime -= dt;
            return false;
        }

        if (leftExtraTime > dt) {
            leftExtraTime -= dt;
            return false;
        }

        return true;
    }
};
