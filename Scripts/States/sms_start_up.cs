using Godot;
using System;

public partial class sms_start_up : state_machine_state
{
    private Timer _timer = new Timer();
    private double _countDown = 2;

    public override void EnterState()
    {
        GetTree().Paused = true;
        _countDown = 2;
    }

    public override void Update(double delta)
    {
        if (_countDown > 0)
        {
            _countDown -= delta;
            if (_countDown < 0)
            {
                GetStateMachine().SwitchState("Menu");
            }
        }
    }

    public override void ExitState()
    {
    }
}
