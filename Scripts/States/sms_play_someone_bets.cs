using Godot;
using System;

#nullable enable

public partial class sms_play_someone_bets : state_machine_state
{
    private double wait;
    private const double duration = 0.5;
    public override void EnterState()
    {
        wait = duration;
    }

    public override void Update(double delta)
    {
        if (wait > 0)
        {
            wait -= delta;
            if (wait <= 0)
            {
                GetMainNode().ForceNextBet();
                GetStateMachine().SwitchState("Play_Loop");
            }
        }
    }

    public override void ExitState()
    {
    }
}
