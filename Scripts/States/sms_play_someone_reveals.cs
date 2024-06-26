using Godot;
using System;

#nullable enable

public partial class sms_play_someone_reveals : state_machine_state
{
    private double wait;
    private const double duration = 2;

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
                GetMainNode().RevealHand();
                GetStateMachine().SwitchState("Play_Loop");
            }
        }
    }

    public override void ExitState()
    {
    }
}
