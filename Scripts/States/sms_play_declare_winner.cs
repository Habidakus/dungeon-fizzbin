using Godot;
using System;

#nullable enable

public partial class sms_play_declare_winner : state_machine_state
{
    double delay = 5;
    double wait;

    public override void EnterState()
    {
        GetMainNode().AwardWinner();
        wait = delay;
    }

    public override void Update(double delta)
    {
        wait -= delta;
        if (wait  < 0)
        {
            GetStateMachine().SwitchState("Menu");
        }
    }

    public override void ExitState()
    {
    }
}
