using Godot;
using System;

#nullable enable

public partial class sms_play_animate_discards : state_machine_state
{
    private double wait;
    private const double duration = 0.5;
    public override void EnterState(Object? additionalInfo = null)
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
                if (GetMainNode().ProgressDiscardAnimation())
                {
                    wait = duration;
                }
            }
        }
        else
        {
            GetStateMachine().SwitchState("Play_Loop");
        }
    }

    public override void ExitState()
    {
    }
}
