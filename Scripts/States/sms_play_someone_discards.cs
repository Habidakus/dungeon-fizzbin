using Godot;
using System;

#nullable enable

public partial class sms_play_someone_discards : state_machine_state
{
    private double _wait = 1.0;
    private bool _hasRun = false;
    public override void EnterState()
    {
        _wait = 1.0;
        _hasRun = false;
    }

    public override void Update(double delta)
    {
        _wait -= delta;
        if (_wait < 0 && !_hasRun)
        {
            GetMainNode().ForceSomeoneToDiscard();
            _hasRun = true;
        }
    }

    public override void ExitState()
    {
    }
}
