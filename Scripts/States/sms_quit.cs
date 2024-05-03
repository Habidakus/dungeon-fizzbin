using Godot;
using System;

public partial class sms_quit : state_machine_state
{
    public override void EnterState()
    {
        GetTree().Quit();
    }

    public override void Update(double delta)
    {
    }

    public override void ExitState()
    {
    }
}
