using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

#nullable enable

public partial class sms_play_someone_bets : state_machine_state
{
    private double _wait = 0.2;
    private int _positionId = -1;
    private double _betFloor = 0;

    public override void EnterState(Object? additionalInfo = null)
    {
        _positionId = -1;
        _betFloor = 0;
        _wait = 0.2;
    }

    internal void ConfirmBetPlaced(int positionId, double newBetFloor)
    {
        _positionId = positionId;
        _betFloor = newBetFloor;
    }

    public override void Update(double delta)
    {
        if (_wait >= 0)
        {
            // Wait a little bit, and then force the next person in queue to bet.
            // This will allow the HUD to update the arrow drawing.
            _wait -= delta;
            if (_wait < 0)
            {
                GetMainNode().ForceNextBet(ConfirmBetPlaced);
            }

            return;
        }

        if (_positionId < 0)
        {
            // We've issued the call to force the bet, but the player hasn't yet decided
            // how much to bet or fold.
            return;
        }

        GetMainNode().ForceNextBet_Post(_betFloor, _positionId);

        _positionId = -1;
        _betFloor = 0;

        GetStateMachine().SwitchState("Play_Loop");
    }

    public override void ExitState()
    {
    }
}
