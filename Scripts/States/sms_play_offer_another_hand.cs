using Godot;
using System;

#nullable enable

public partial class sms_play_offer_another_hand : state_machine_state
{
    public override void EnterState()
    {
        GetMainNode().AdvanceDealer();
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
