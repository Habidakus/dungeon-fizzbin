using System;

#nullable enable

public partial class sms_play_someone_bets : state_machine_state
{
    private bool _started = false;
    private bool _gotCallback = false;
    private int _positionId = -1;
    private double _betFloor = 0;

    public override void EnterState(Object? additionalInfo = null)
    {
        _positionId = -1;
        _betFloor = 0;
        _started = false;
        _gotCallback = false;
    }

    internal void ConfirmBetPlaced(int positionId, double newBetFloor)
    {
        _positionId = positionId;
        _betFloor = newBetFloor;
        _gotCallback = true;
    }

    public override void Update(double delta)
    {
        if (_started == false)
        {
            _started = true;
            GetMainNode().ForceNextBet(ConfirmBetPlaced);
            return;
        }

        if (_gotCallback == true)
        {
            GetMainNode().ForceNextBet_Post(_betFloor, _positionId);
            GetStateMachine().SwitchState("Play_Loop");
            return;
        }
    }

    public override void ExitState()
    {
    }
}
