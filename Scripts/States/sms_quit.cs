using Godot;
using System;

public partial class sms_quit : state_machine_state
{
    public override void EnterState(Object additionalInfo = null)
    {
        Profile.Dump();
        GetTree().Quit();
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
