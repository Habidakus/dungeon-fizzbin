using Godot;
using System;

public partial class sms_menu : state_machine_state
{
    public override void EnterState()
    {
        GetTree().Paused = false;
    }

    public override void Update(double delta)
    {
        //GD.Print($"Menu update {delta}");
        //throw new Exception("no printf?");
    }

    public override void ExitState()
    {
    }
}
