using Godot;
using System;

#nullable enable

public partial class sms_play_deal : state_machine_state
{
    bool _run = false;
    public override void EnterState()
    {
    }

    public override void Update(double delta)
    {
        if (!_run)
        {
            GetMainNode().StartFreshDeal();
            GetStateMachine().SwitchState("Play_Loop");
            _run = true;
        }
    }

    public override void ExitState()
    {
    }
}
