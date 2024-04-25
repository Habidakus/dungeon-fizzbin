using Godot;
using System;

#nullable enable

public partial class sms_play_post_discard : state_machine_state
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
                GetMainNode().PerformPostDiscord();
                GetStateMachine().SwitchState("Play_Loop");
            }
        }
    }

    public override void ExitState()
    {
    }
}
