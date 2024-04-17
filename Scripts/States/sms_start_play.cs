using Godot;
using System;

#nullable enable

public partial class sms_start_play : state_machine_state
{
    public override void EnterState()
    {
        //GetTree().Paused = true;
        GetMainNode().StartFreshDeal();
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
