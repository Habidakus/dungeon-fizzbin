using Godot;
using System;

#nullable enable

public partial class sms_play_someone_passes : state_machine_state
{
    private double _wait = 0;
    private const double _delay = 0.5;
    public override void EnterState()
    {
        _wait = _delay;
    }

    public override void Update(double delta)
    {
        _wait -= delta;
        if (_wait < 0)
        {
            GetMainNode().ForceSomeoneToPass();
        }
    }

    public override void ExitState()
    {
    }
}
