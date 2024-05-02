using Godot;
using System;

#nullable enable

public partial class sms_play_player_leaves_game : state_machine_state
{
    private double _wait = 0;
    private const double _delay = 7.5;
    public override void EnterState()
    {
        GetMainNode().HavePlayerLeave();
        _wait = _delay;
    }

    public override void Update(double delta)
    {
        _wait -= delta;
        if (_wait < 0)
        {
            GetStateMachine().SwitchState("Play_Loop");
        }
    }

    public override void ExitState()
    {
    }
}
